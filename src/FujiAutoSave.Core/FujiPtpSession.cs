using System.Buffers.Binary;
using System.Net;
using System.Net.Sockets;
using FujiAutoSave.Core.Enums;
using FujiAutoSave.Core.Logging;
using FujiAutoSave.Core.Models;
using FujiAutoSave.Core.PTP;
using Microsoft.Extensions.Logging;

namespace FujiAutoSave.Core;

public sealed class FujiPtpSession(ILogger<FujiPtpSession> logger) : IFujiPtpSession
{
    private const int DefaultTimeout = 5000;
    private const uint ChunkSize = 1 * 1024 * 1024;
    private TcpClient? _commandClient;
    private NetworkStream? _commandStream;
    private uint _transactionId;
    private bool _disposedValue;

    public int CommandPort { get; } = 55740;

    public async Task ConnectAsync(IPAddress host, string deviceName, CancellationToken cancellationToken = default)
    {
        logger.ConnectingToPtpPort(host, CommandPort);

        await Task.Delay(500, cancellationToken);

        _commandClient = new TcpClient();
        var connectTask = _commandClient.ConnectAsync(host, CommandPort, cancellationToken).AsTask();
        var timeoutTask = Task.Delay(DefaultTimeout, cancellationToken);

        if (await Task.WhenAny(connectTask, timeoutTask) != connectTask)
        {
            throw new TimeoutException("Failed to connect to camera PTP port");
        }

        await connectTask;
        _commandStream = _commandClient.GetStream();
        _commandStream.ReadTimeout = 30000;
        _commandStream.WriteTimeout = 30000;

        logger.ConnectedToPtpPort();

        await SendInitAsync(host, deviceName, cancellationToken);
    }

    public async Task<ImageInfo?> GetImageInfoAsync(uint index, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        logger.GettingImageInfo(index);

        var packet = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.GetImageInfo,
            TransactionId = ++_transactionId
        };
        var buffer = packet.Create([index]);
        await _commandStream!.WriteAsync(buffer, cancellationToken);

        var dataPacket = await ReadDataPacketAsync(DefaultTimeout, cancellationToken);
        if (dataPacket == null)
        {
            logger.NoImageInfoDataPacket(index);

            return null;
        }

        logger.ReceivedImageInfoDataPacket(dataPacket.Payload.Length);

        var response = await ReadResponseAsync(DefaultTimeout, cancellationToken);
        if (response.ResponseCode != PtpResponseCode.OK)
        {
            logger.FailedToGetImageInfo(index, response.ResponseCode);

            return null;
        }

        logger.ImageInfoReceived(index);

        return PtpPacketFactory.ParseImageInfo(dataPacket.Payload);
    }

    public async Task<int> GetImageCountAsync(CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        logger.GettingImageCount();

        var payload = await GetDevicePropValueAsync(PtpPropertyCode.EventsList, cancellationToken);
        if (payload.Length < 2)
        {
            logger.FailedToGetImageCount();

            return 0;
        }

        var eventCount = BitConverter.ToUInt16(payload, 0);
        logger.ReceivedEvents(eventCount);

        var offset = 2;
        for (var i = 0; i < eventCount; i++)
        {
            if (offset + 6 > payload.Length)
            {
                break;
            }

            var code = BitConverter.ToUInt16(payload, offset);
            var value = BitConverter.ToUInt32(payload, offset + 2);

            if (code == (ushort)PtpPropertyCode.ImageCount)
            {
                logger.ReceivedImageCount((int)value);

                return (int)value;
            }

            offset += 6;
        }

        logger.FailedToFindImageCountInEvents();

        return 0;
    }

    public async Task SetAutoSaveDatabaseStatusAsync(uint currentIndex, int totalCount, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        logger.SettingAutoSaveDatabaseStatus(currentIndex, totalCount);

        var data = PtpPacketFactory.CreateStatusPacket(currentIndex, totalCount);

        await SetDevicePropValueAsync(PtpPropertyCode.AutoSaveDatabaseStatus, data, cancellationToken);
    }

    public async Task<int> DownloadImageAsync(uint index, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        logger.DownloadingImage(index);

        var packet = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.GetImage,
            TransactionId = ++_transactionId
        };

        var buffer = packet.Create([index]);
        await _commandStream!.WriteAsync(buffer, cancellationToken);

        var totalBytesWritten = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var dataPacket = await ReadDataPacketAsync(60000, cancellationToken);
            if (dataPacket != null)
            {
                logger.WritingDataToStream(dataPacket.Payload.Length);

                await outputStream.WriteAsync(dataPacket.Payload, cancellationToken);
                totalBytesWritten += dataPacket.Payload.Length;

                continue;
            }

            logger.DownloadedImage(index, totalBytesWritten);

            return totalBytesWritten;
        }
    }

    public async Task<int> DownloadPartialImageAsync(uint index, Stream outputStream, CancellationToken cancellationToken = default)
    {
        ThrowIfNotConnected();

        logger.DownloadingImage(index);

        var totalBytesWritten = 0;

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var packet = new PtpCommandPacket
            {
                Code = (ushort)PtpOperationCode.GetPartialImage,
                TransactionId = ++_transactionId
            };
            var buffer = packet.Create([index, (uint)totalBytesWritten, ChunkSize]);
            await _commandStream!.WriteAsync(buffer, cancellationToken);

            var bytesWritten = await ReadAndWritePartialImageDataAsync(outputStream, cancellationToken);
            totalBytesWritten += bytesWritten;
            if (bytesWritten == 0)
            {
                break;
            }
        }

        logger.DownloadedImage(index, totalBytesWritten);

        return totalBytesWritten;
    }

    private async Task<int> ReadAndWritePartialImageDataAsync(Stream outputStream, CancellationToken cancellationToken)
    {
        var totalBytesWritten = 0;
        var lastPacketSize = 0;

        while (true)
        {
            var responsePacket = await ReadPacketAsync(60000, cancellationToken);
            if (responsePacket == null)
            {
                logger.NoResponseForGetPartialImage();

                return totalBytesWritten;
            }

            var packetType = BinaryPrimitives.ReadUInt16LittleEndian(responsePacket.AsSpan(4));
            if (packetType is 65535)
            {
                logger.PartialImageDownloadCancelled();

                return totalBytesWritten;
            }

            var packetInfo = PtpPacket.FromByteArray(responsePacket);
            logger.ParsedPacketInfo(packetInfo.Length, packetInfo.Type, packetInfo.Code, packetInfo.TransactionId);

            if (packetInfo.Type == PtpPacketType.Response)
            {
                if (lastPacketSize < ChunkSize)
                {
                    return totalBytesWritten;
                }

                return totalBytesWritten;
            }

            if (packetInfo.Type == PtpPacketType.Data)
            {
                var payloadSize = (int)(packetInfo.Length - PtpPacket.HeaderSize);
                if (payloadSize > 0)
                {
                    var payload = new byte[payloadSize];
                    Array.Copy(responsePacket, PtpPacket.HeaderSize, payload, 0, payloadSize);
                    await outputStream.WriteAsync(payload, cancellationToken);
                }
                else
                {
                    payloadSize = 0;
                }

                totalBytesWritten += payloadSize;
                lastPacketSize = payloadSize;
            }
        }
    }

    private void ThrowIfNotConnected()
    {
        if (_commandStream is null || _commandClient?.Connected == false)
        {
            logger.PtpSessionNotConnected();

            throw new InvalidOperationException("PTP session is not connected. Call ConnectAsync first.");
        }
    }

    private async Task SendInitAsync(IPAddress host, string deviceName, CancellationToken cancellationToken)
    {
        logger.SendingPtpInitPacket();

        var buffer = PtpPacketFactory.CreateInitPacket(host, deviceName);
        await _commandStream!.WriteAsync(buffer, cancellationToken);

        var lengthBuffer = new byte[4];
        await _commandStream.ReadExactlyAsync(lengthBuffer, cancellationToken);
        var length = BinaryPrimitives.ReadUInt32LittleEndian(lengthBuffer);

        logger.ReceivedInitResponseLength(length);

        var responseBuffer = new byte[length];
        Array.Copy(lengthBuffer, responseBuffer, lengthBuffer.Length);
        await _commandStream.ReadExactlyAsync(responseBuffer.AsMemory(lengthBuffer.Length), cancellationToken);

        logger.ReceivedRawPtpInitResponse(BitConverter.ToString(responseBuffer));

        var cameraModel = PtpPacketFactory.ParseCameraModel(responseBuffer);
        logger.PtpInitSuccessful(cameraModel);

        await OpenSessionAsync(cancellationToken);
    }

    private async Task OpenSessionAsync(CancellationToken cancellationToken)
    {
        logger.OpeningPtpSession();

        var packet = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.OpenSession,
            TransactionId = ++_transactionId
        };

        var buffer = packet.Create([1]);
        await _commandStream!.WriteAsync(buffer, cancellationToken);
        logger.OpenSessionSent();

        var response = await ReadResponseAsync(DefaultTimeout, cancellationToken);
        if (response == null || response.ResponseCode == PtpResponseCode.OK || response.ResponseCode == PtpResponseCode.SessionAlreadyOpened)
        {
            return;
        }

        throw new InvalidOperationException($"OpenSession failed with code: {response.ResponseCode}");
    }

    private async Task<byte[]> GetDevicePropValueAsync(PtpPropertyCode propertyCode, CancellationToken cancellationToken = default)
    {
        logger.GettingDevicePropValue(propertyCode);

        var packet = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.GetDevicePropValue,
            TransactionId = ++_transactionId
        };

        var buffer = packet.Create([(ushort)propertyCode]);
        await _commandStream!.WriteAsync(buffer, cancellationToken);

        var dataPacket = await ReadDataPacketAsync(DefaultTimeout, cancellationToken);
        if (dataPacket == null)
        {
            logger.NoDevicePropValueReceived(propertyCode);

            return [];
        }

        var response = await ReadResponseAsync(DefaultTimeout, cancellationToken);
        if (response.ResponseCode != PtpResponseCode.OK)
        {
            logger.FailedToGetDevicePropValue(propertyCode, response.ResponseCode);

            throw new InvalidOperationException($"PTP response error: {response.ResponseCode} for property {propertyCode}.");
        }

        logger.ReceivedDevicePropValue(propertyCode, dataPacket.Payload.Length);

        return dataPacket.Payload;
    }

    private async Task SetDevicePropValueAsync(PtpPropertyCode propertyCode, byte[] data, CancellationToken cancellationToken = default)
    {
        logger.SettingDevicePropValue(propertyCode);

        var commandPacket = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.SetDevicePropValue,
            TransactionId = ++_transactionId
        };
        var commandBuffer = commandPacket.Create([(ushort)propertyCode]);
        await _commandStream!.WriteAsync(commandBuffer, cancellationToken);

        var dataPacket = new PtpDataPacket
        {
            Code = (ushort)PtpOperationCode.SetDevicePropValue,
            TransactionId = commandPacket.TransactionId,
            Payload = data
        };
        var dataBuffer = dataPacket.ToByteArray(payload: data);
        await _commandStream!.WriteAsync(dataBuffer, cancellationToken);

        var ackPacket = await ReadPacketAsync(DefaultTimeout, cancellationToken);
        logger.ReceivedRawData(BitConverter.ToString(ackPacket ?? []));

        logger.DevicePropValueSet(propertyCode);
    }

    private async Task<PtpResponsePacket> ReadResponseAsync(int timeout, CancellationToken cancellationToken)
    {
        var buffer = await ReadPacketAsync(timeout, cancellationToken);
        if (buffer == null)
        {
            throw new TimeoutException("Timeout waiting for response");
        }

        logger.ReceivedRawData(BitConverter.ToString(buffer));

        var packet = PtpPacket.FromByteArray(buffer);
        logger.ParsedPacketInfo(packet.Length, packet.Type, packet.Code, packet.TransactionId);

        if (packet.Type != PtpPacketType.Response)
        {
            logger.UnexpectedPacketType(packet.Type, packet.Code);
        }

        if (packet is not PtpResponsePacket responsePacket)
        {
            throw new InvalidCastException($"Expected PtpResponsePacket but got {packet.GetType().Name}. Type: {packet.Type}, Code: {packet.Code}");
        }

        return responsePacket;
    }

    private async Task<PtpDataPacket?> ReadDataPacketAsync(int timeout, CancellationToken cancellationToken)
    {
        logger.ReadingDataPacket(timeout);

        var buffer = await ReadPacketAsync(timeout, cancellationToken);
        if (buffer == null)
        {
            logger.ReadDataPacketTimeout();

            return null;
        }

        var packet = PtpPacket.FromByteArray(buffer);
        logger.ParsedPacketInfo(packet.Length, packet.Type, packet.Code, packet.TransactionId);

        if (packet.Type == PtpPacketType.Response)
        {
            logger.ReceivedResponseInsteadOfData(packet.Code);

            return null;
        }

        if (packet.Type == PtpPacketType.Data)
        {
            var payloadSize = (int)(packet.Length - PtpPacket.HeaderSize);
            var payload = new byte[payloadSize];

            Array.Copy(buffer, PtpPacket.HeaderSize, payload, 0, payloadSize);

            logger.ReceivedDataPacket(payloadSize, $"{payloadSize} bytes");

            return new PtpDataPacket
            {
                Length = packet.Length,
                Type = packet.Type,
                Code = packet.Code,
                TransactionId = packet.TransactionId,
                Payload = payload
            };
        }

        logger.UnexpectedPacketTypeInReadDataPacket(packet.Type, packet.Code);

        return null;
    }

    private async Task<byte[]?> ReadPacketAsync(int timeout, CancellationToken cancellationToken)
    {
        logger.WaitingForPacketLength(timeout);

        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            var lengthBuffer = new byte[4];
            var bytesRead = await _commandStream!.ReadAsync(lengthBuffer, timeoutCts.Token);
            if (bytesRead < 4)
            {
                logger.FailedToReadPacketLength(bytesRead);

                return null;
            }

            var length = BinaryPrimitives.ReadUInt32LittleEndian(lengthBuffer);
            logger.ReceivedPacketLength(length);

            if (length is < 8)
            {
                logger.InvalidPacketLength(length);

                return null;
            }

            var packetBuffer = new byte[length];
            BinaryPrimitives.WriteUInt32LittleEndian(packetBuffer, length);

            var remaining = length - 4;
            if (remaining > 0)
            {
                logger.ReadingPacketRemainingBytes((int)remaining, timeout);

                var totalRead = 0;
                while (totalRead < remaining)
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return null;
                    }

                    var readBytes = await _commandStream.ReadAsync(packetBuffer.AsMemory(4 + totalRead), timeoutCts.Token);
                    if (readBytes == 0)
                    {
                        logger.FailedToReadPacketData(totalRead, (int)remaining);

                        return null;
                    }

                    totalRead += readBytes;
                    logger.ReadPacketProgress(totalRead, (int)remaining);
                }
            }

            return packetBuffer;
        }
        catch (OperationCanceledException)
        {
            logger.ReadPacketCancelled();

            return null;
        }
        catch (Exception ex)
        {
            logger.ReadPacketError(ex);

            return null;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _commandStream?.Dispose();
                _commandClient?.Dispose();
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
