using System.Net;
using System.Net.Sockets;
using System.Text;
using FujiAutoSave.Core.Logging;
using FujiAutoSave.Core.Messaging;
using FujiAutoSave.Core.Models;
using Microsoft.Extensions.Logging;

namespace FujiAutoSave.Core;

public sealed class FujiCameraClient(ILogger<FujiCameraClient> logger) : IFujiCameraClient
{
    private const int DefaultTimeout = 5000;
    private readonly Encoding _encoding = Encoding.ASCII;
    private readonly Lock _lock = new();
    private UdpClient? _discoveryClient;
    private UdpClient? _connectionClient;
    private TcpListener? _registerListener;
    private TcpListener? _importListener;
    private CancellationTokenSource? _discoveryCts;
    private CancellationTokenSource? _connectionCts;
    private Task? _discoveryTask;
    private Task? _connectionTask;
    private bool _disposed;

    public event Func<CameraInfo, Task>? CameraRegistered;
    public event Func<CameraInfo, Task>? CameraConnecting;

    public int NotifyPort { get; set; } = 51540;
    public int ConnectPort { get; } = 51541;
    public int RegisterPort { get; } = 51542;
    public int DiscoverPort { get; } = 51542;

    public void StartDiscovery()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_discoveryClient != null)
            {
                throw new InvalidOperationException("Already discovering");
            }

            _discoveryCts = new CancellationTokenSource();
            _discoveryClient = new UdpClient();
            _discoveryClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _discoveryClient.Client.Bind(new IPEndPoint(IPAddress.Any, DiscoverPort));

            logger.StartedListeningDiscovery(DiscoverPort);

            _discoveryTask = Task.Run(() => DiscoveryLoopAsync(_discoveryCts.Token));
        }
    }

    public void StopDiscovery()
    {
        lock (_lock)
        {
            if (_discoveryCts is null)
            {
                return;
            }

            _discoveryCts.Cancel();
            _ = _discoveryTask?.WaitAsync(TimeSpan.FromSeconds(5));

            _discoveryClient?.Close();
            _discoveryClient?.Dispose();
            _discoveryClient = null;

            _registerListener?.Stop();
            _registerListener?.Dispose();
            _registerListener = null;

            _discoveryCts.Dispose();
            _discoveryCts = null;
            _discoveryTask = null;

            logger.StoppedListeningDiscovery();
        }
    }

    public void StartConnection()
    {
        ThrowIfDisposed();

        lock (_lock)
        {
            if (_connectionClient != null)
            {
                throw new InvalidOperationException("Already connecting");
            }

            _connectionCts = new CancellationTokenSource();
            _connectionClient = new UdpClient();
            _connectionClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _connectionClient.Client.Bind(new IPEndPoint(IPAddress.Any, ConnectPort));

            logger.StartedListeningConnection(ConnectPort);

            _connectionTask = Task.Run(() => ConnectionLoopAsync(_connectionCts.Token));
        }
    }

    public void StopConnection()
    {
        lock (_lock)
        {
            if (_connectionCts is null)
            {
                return;
            }

            _connectionCts.Cancel();
            _ = _connectionTask?.WaitAsync(TimeSpan.FromSeconds(5));

            _connectionClient?.Close();
            _connectionClient?.Dispose();
            _connectionClient = null;

            _importListener?.Stop();
            _importListener?.Dispose();
            _importListener = null;

            _connectionCts.Dispose();
            _connectionCts = null;
            _connectionTask = null;

            logger.StoppedListeningConnection();
        }
    }

    private async Task<bool> SendNotifyAsync(IPAddress host, string clientName, CancellationToken cancellationToken)
    {
        try
        {
            logger.SendingNotify(host, NotifyPort);

            using var tcpClient = new TcpClient();
            var connectTask = tcpClient.ConnectAsync(host, NotifyPort, cancellationToken).AsTask();
            var timeoutTask = Task.Delay(DefaultTimeout, cancellationToken);

            if (await Task.WhenAny(connectTask, timeoutTask) != connectTask)
            {
                logger.FailedToConnectNotify(host, NotifyPort);

                return false;
            }

            await connectTask;

            using var stream = tcpClient.GetStream();
            stream.ReadTimeout = DefaultTimeout;
            stream.WriteTimeout = DefaultTimeout;

            var notifyMessage = AutoSaveMessageFactory.BuildNotify(host, NotifyPort, clientName);
            var buffer = _encoding.GetBytes(notifyMessage);
            await stream.WriteAsync(buffer, cancellationToken);

            var responseBuffer = new byte[512];
            var bytesRead = await stream.ReadAsync(responseBuffer, cancellationToken);
            var response = _encoding.GetString(responseBuffer, 0, bytesRead);

            logger.NotifyResponse(response);

            if (AutoSaveMessageFactory.IsHttpOk(response))
            {
                logger.NotifySuccessful();

                return true;
            }

            logger.NotifyFailed(response);

            return false;
        }
        catch
        {
            return false;
        }
    }

    private async Task WaitForRegistrationAsync(DiscoverMessage discoverMessage, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var tcpListener = new TcpListener(IPAddress.Any, RegisterPort);
        tcpListener.Start();

        logger.StartedListeningRegistration(RegisterPort);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            var tcpClient = await tcpListener.AcceptTcpClientAsync(timeoutCts.Token);

            using (tcpClient)
            {
                var stream = tcpClient.GetStream();
                stream.ReadTimeout = DefaultTimeout;
                stream.WriteTimeout = DefaultTimeout;

                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                var message = _encoding.GetString(buffer, 0, bytesRead);

                logger.ReceivedRegisterMessage(message);

                var registerMessage = AutoSaveMessageFactory.ParseRegister(message);
                if (registerMessage is null)
                {
                    logger.InvalidRegisterMessage(message);

                    throw new InvalidOperationException("Invalid REGISTER message");
                }

                var response = AutoSaveMessageFactory.BuildRegisterResponse();
                var responseBuffer = _encoding.GetBytes(response);
                await stream.WriteAsync(responseBuffer, cancellationToken);

                var cameraInfo = new CameraInfo
                {
                    Name = registerMessage.Name,
                    Model = registerMessage.Model,
                    Host = discoverMessage.CameraHost,
                    MaxTimeout = discoverMessage.MaxTimeout,
                    Target = registerMessage.Target
                };

                logger.RegisteredCamera(cameraInfo.Name, cameraInfo.Model);

                CameraRegistered?.Invoke(cameraInfo);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.RegistrationTimeout();
        }
        catch (Exception ex)
        {
            logger.ErrorAcceptingRegistrationClient(ex);

            throw;
        }
        finally
        {
            logger.StoppedListeningRegistration();
        }
    }

    private async Task DiscoveryLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _discoveryClient!.ReceiveAsync(cancellationToken);
                var message = _encoding.GetString(result.Buffer);
                logger.ReceivedRawDiscoveryMessage(message);

                var discoverMessage = AutoSaveMessageFactory.ParseDiscover(message);
                if (discoverMessage != null)
                {
                    logger.DiscoveredCamera(discoverMessage.CameraHost);

                    var handshakeSuccess = await SendNotifyAsync(discoverMessage.CameraHost, Environment.MachineName, cancellationToken);

                    if (!handshakeSuccess)
                    {
                        logger.NotifyFailed("Handshake failed");

                        continue;
                    }

                    try
                    {
                        await WaitForRegistrationAsync(discoverMessage, TimeSpan.FromSeconds(30), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorAcceptingRegistrationClient(ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.ErrorReceivingDiscoveryMessage(ex);
            }
        }
    }

    private async Task WaitForConnectionAsync(DiscoverMessage discoverMessage, TimeSpan timeout, CancellationToken cancellationToken)
    {
        using var tcpListener = new TcpListener(IPAddress.Any, ConnectPort);
        tcpListener.Start();

        logger.StartedWaitingForConnection(ConnectPort);

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(timeout);

            var tcpClient = await tcpListener.AcceptTcpClientAsync(timeoutCts.Token);

            using (tcpClient)
            {
                var stream = tcpClient.GetStream();
                stream.ReadTimeout = DefaultTimeout;
                stream.WriteTimeout = DefaultTimeout;

                var buffer = new byte[1024];
                var bytesRead = await stream.ReadAsync(buffer, cancellationToken);
                var message = _encoding.GetString(buffer, 0, bytesRead);

                logger.ReceivedImportMessage(message);

                var importMessage = AutoSaveMessageFactory.ParseImport(message);
                if (importMessage is null)
                {
                    logger.InvalidImportMessage(message);

                    throw new InvalidOperationException("Invalid IMPORT message");
                }

                var response = AutoSaveMessageFactory.BuildImportResponse();
                var responseBuffer = _encoding.GetBytes(response);
                await stream.WriteAsync(responseBuffer, cancellationToken);

                logger.ReceivedImportRequest(importMessage.Name, importMessage.Target);

                var cameraInfo = new CameraInfo
                {
                    Name = importMessage.Name,
                    Host = discoverMessage.CameraHost,
                    MaxTimeout = discoverMessage.MaxTimeout,
                    Target = importMessage.Target
                };

                logger.ConnectedToCamera();

                CameraConnecting?.Invoke(cameraInfo);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            logger.ConnectionTimeout();
        }
        catch (Exception ex)
        {
            logger.ErrorAcceptingConnectionClient(ex);

            throw;
        }
        finally
        {
            logger.StoppedWaitingForConnection();
        }
    }

    private async Task ConnectionLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = await _connectionClient!.ReceiveAsync(cancellationToken);
                var message = _encoding.GetString(result.Buffer);
                logger.ReceivedRawConnectionMessage(message);

                var discoverMessage = AutoSaveMessageFactory.ParseDiscover(message);
                if (discoverMessage != null)
                {
                    logger.DiscoveredCamera(discoverMessage.CameraHost);

                    var handshakeSuccess = await SendNotifyAsync(discoverMessage.CameraHost, Environment.MachineName, cancellationToken);

                    if (!handshakeSuccess)
                    {
                        logger.NotifyFailed("Handshake failed");

                        continue;
                    }

                    try
                    {
                        await WaitForConnectionAsync(discoverMessage, TimeSpan.FromSeconds(30), cancellationToken);
                    }
                    catch (OperationCanceledException)
                    {
                        return;
                    }
                    catch (Exception ex)
                    {
                        logger.ErrorAcceptingConnectionClient(ex);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (SocketException ex) when (ex.SocketErrorCode == SocketError.Interrupted)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.ErrorReceivingDiscoveryMessage(ex);
            }
        }
    }

    private void ThrowIfDisposed() => ObjectDisposedException.ThrowIf(_disposed, this);

    private void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                StopDiscovery();
                StopConnection();
            }

            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
