using System.Net;
using FujiAutoSave.Core.Enums;
using Microsoft.Extensions.Logging;

namespace FujiAutoSave.Core.Logging;

internal static partial class Logging
{
    [LoggerMessage(1, LogLevel.Information, "Discovered camera: {Host}")]
    public static partial void DiscoveredCamera(this ILogger logger, IPAddress host);

    [LoggerMessage(2, LogLevel.Error, "Error receiving discovery message")]
    public static partial void ErrorReceivingDiscoveryMessage(this ILogger logger, Exception exception);

    [LoggerMessage(3, LogLevel.Debug, "Sending NOTIFY to {Host}:{Port}")]
    public static partial void SendingNotify(this ILogger logger, IPAddress host, int port);

    [LoggerMessage(4, LogLevel.Error, "Failed to connect to {Host}:{Port}")]
    public static partial void FailedToConnectNotify(this ILogger logger, IPAddress host, int port);

    [LoggerMessage(5, LogLevel.Debug, "NOTIFY response: {Response}")]
    public static partial void NotifyResponse(this ILogger logger, string response);

    [LoggerMessage(6, LogLevel.Debug, "NOTIFY successful")]
    public static partial void NotifySuccessful(this ILogger logger);

    [LoggerMessage(7, LogLevel.Warning, "NOTIFY failed with response: {Response}")]
    public static partial void NotifyFailed(this ILogger logger, string response);

    [LoggerMessage(8, LogLevel.Information, "Started listening for camera discovery on port {Port}")]
    public static partial void StartedListeningDiscovery(this ILogger logger, int port);

    [LoggerMessage(9, LogLevel.Information, "Stopped listening for camera discovery")]
    public static partial void StoppedListeningDiscovery(this ILogger logger);

    [LoggerMessage(10, LogLevel.Warning, "Registration timeout or canceled")]
    public static partial void RegistrationTimeout(this ILogger logger);

    [LoggerMessage(11, LogLevel.Error, "Error accepting registration client")]
    public static partial void ErrorAcceptingRegistrationClient(this ILogger logger, Exception exception);

    [LoggerMessage(12, LogLevel.Debug, "Received REGISTER message: {Message}")]
    public static partial void ReceivedRegisterMessage(this ILogger logger, string message);

    [LoggerMessage(13, LogLevel.Warning, "Invalid REGISTER message: {Message}")]
    public static partial void InvalidRegisterMessage(this ILogger logger, string message);

    [LoggerMessage(14, LogLevel.Information, "Registered camera: {Name} ({Model})")]
    public static partial void RegisteredCamera(this ILogger logger, string name, string? model);

    [LoggerMessage(15, LogLevel.Debug, "Connecting to camera PTP port {Host}:{Port}")]
    public static partial void ConnectingToPtpPort(this ILogger logger, IPAddress host, int port);

    [LoggerMessage(16, LogLevel.Debug, "Connected to camera PTP port")]
    public static partial void ConnectedToPtpPort(this ILogger logger);

    [LoggerMessage(17, LogLevel.Debug, "Sending PTP/IP init packet")]
    public static partial void SendingPtpInitPacket(this ILogger logger);

    [LoggerMessage(18, LogLevel.Information, "Init successful, camera model: {Model}")]
    public static partial void PtpInitSuccessful(this ILogger logger, string model);

    [LoggerMessage(19, LogLevel.Debug, "Opening PTP session")]
    public static partial void OpeningPtpSession(this ILogger logger);

    [LoggerMessage(20, LogLevel.Warning, "Failed to get image info for index {Index}")]
    public static partial void FailedToGetImageInfo(this ILogger logger, Exception exception, int index);

    [LoggerMessage(21, LogLevel.Warning, "Failed to get image info for index {Index}, code: {code}")]
    public static partial void FailedToGetImageInfo(this ILogger logger, uint index, PtpResponseCode code);

    [LoggerMessage(22, LogLevel.Debug, "Reading data packet with timeout {Timeout}ms")]
    public static partial void ReadingDataPacket(this ILogger logger, int timeout);

    [LoggerMessage(23, LogLevel.Warning, "Read data packet timeout")]
    public static partial void ReadDataPacketTimeout(this ILogger logger);

    [LoggerMessage(24, LogLevel.Debug, "Parsed packet: Length={Length}, Type={Type}, Code={Code}, TransactionId={TransactionId}")]
    public static partial void ParsedPacketInfo(this ILogger logger, uint length, PtpPacketType type, ushort code, uint transactionId);

    [LoggerMessage(25, LogLevel.Debug, "Received data packet: Size={Size}, Data={Data}")]
    public static partial void ReceivedDataPacket(this ILogger logger, int size, string data);

    [LoggerMessage(26, LogLevel.Information, "Writing {Bytes} bytes to stream")]
    public static partial void WritingDataToStream(this ILogger logger, int bytes);

    [LoggerMessage(27, LogLevel.Warning, "No data packet received for image info at index {Index}")]
    public static partial void NoImageInfoDataPacket(this ILogger logger, uint index);

    [LoggerMessage(28, LogLevel.Warning, "PTP session is not connected")]
    public static partial void PtpSessionNotConnected(this ILogger logger);

    [LoggerMessage(29, LogLevel.Information, "Received response instead of data packet: {Code}")]
    public static partial void ReceivedResponseInsteadOfData(this ILogger logger, ushort code);

    [LoggerMessage(30, LogLevel.Debug, "Received raw data: {Data}")]
    public static partial void ReceivedRawData(this ILogger logger, string data);

    [LoggerMessage(31, LogLevel.Warning, "Unexpected packet type in ReadDataPacket: Type={Type}, Code={Code}")]
    public static partial void UnexpectedPacketTypeInReadDataPacket(this ILogger logger, PtpPacketType type, ushort code);

    [LoggerMessage(32, LogLevel.Debug, "Image info response OK for index {Index}")]
    public static partial void ImageInfoReceived(this ILogger logger, uint index);

    [LoggerMessage(33, LogLevel.Debug, "Waiting for packet length with timeout {Timeout}ms")]
    public static partial void WaitingForPacketLength(this ILogger logger, int timeout);

    [LoggerMessage(34, LogLevel.Warning, "Failed to read packet length: bytesRead={BytesRead}")]
    public static partial void FailedToReadPacketLength(this ILogger logger, int bytesRead);

    [LoggerMessage(35, LogLevel.Debug, "Received packet length: {Length}")]
    public static partial void ReceivedPacketLength(this ILogger logger, uint length);

    [LoggerMessage(36, LogLevel.Warning, "Invalid packet length: {Length}")]
    public static partial void InvalidPacketLength(this ILogger logger, uint length);

    [LoggerMessage(37, LogLevel.Debug, "Reading {Remaining} bytes with timeout {Timeout}ms")]
    public static partial void ReadingPacketRemainingBytes(this ILogger logger, int remaining, int timeout);

    [LoggerMessage(38, LogLevel.Warning, "Failed to read packet data: read={Read}, expected={Expected}")]
    public static partial void FailedToReadPacketData(this ILogger logger, int read, int expected);

    [LoggerMessage(39, LogLevel.Debug, "Read packet progress: {Current}/{Total}")]
    public static partial void ReadPacketProgress(this ILogger logger, int current, int total);

    [LoggerMessage(40, LogLevel.Information, "Read packet cancelled")]
    public static partial void ReadPacketCancelled(this ILogger logger);

    [LoggerMessage(41, LogLevel.Error, "Read packet error")]
    public static partial void ReadPacketError(this ILogger logger, Exception exception);

    [LoggerMessage(42, LogLevel.Information, "Downloading image {Index}")]
    public static partial void DownloadingImage(this ILogger logger, uint index);

    [LoggerMessage(43, LogLevel.Debug, "Received image info data packet: {Size} bytes")]
    public static partial void ReceivedImageInfoDataPacket(this ILogger logger, int size);

    [LoggerMessage(44, LogLevel.Information, "Connected to camera successfully")]
    public static partial void ConnectedToCamera(this ILogger logger);

    [LoggerMessage(45, LogLevel.Information, "Started waiting for registration on port {Port}")]
    public static partial void StartedListeningRegistration(this ILogger logger, int port);

    [LoggerMessage(46, LogLevel.Information, "Stopped waiting for registration")]
    public static partial void StoppedListeningRegistration(this ILogger logger);

    [LoggerMessage(47, LogLevel.Information, "Started listening for camera connection on port {Port}")]
    public static partial void StartedListeningConnection(this ILogger logger, int port);

    [LoggerMessage(48, LogLevel.Information, "Stopped listening for camera connection")]
    public static partial void StoppedListeningConnection(this ILogger logger);

    [LoggerMessage(49, LogLevel.Information, "Started waiting for connection on port {Port}")]
    public static partial void StartedWaitingForConnection(this ILogger logger, int port);

    [LoggerMessage(50, LogLevel.Information, "Stopped waiting for connection")]
    public static partial void StoppedWaitingForConnection(this ILogger logger);

    [LoggerMessage(51, LogLevel.Debug, "Received IMPORT message: {Message}")]
    public static partial void ReceivedImportMessage(this ILogger logger, string message);

    [LoggerMessage(52, LogLevel.Warning, "Invalid IMPORT message: {Message}")]
    public static partial void InvalidImportMessage(this ILogger logger, string message);

    [LoggerMessage(53, LogLevel.Debug, "Received IMPORT request: {Name}, folder: {Folder}")]
    public static partial void ReceivedImportRequest(this ILogger logger, string name, string folder);

    [LoggerMessage(54, LogLevel.Warning, "Connection timeout or canceled")]
    public static partial void ConnectionTimeout(this ILogger logger);

    [LoggerMessage(55, LogLevel.Error, "Error accepting connection client")]
    public static partial void ErrorAcceptingConnectionClient(this ILogger logger, Exception exception);

    [LoggerMessage(56, LogLevel.Warning, "Unexpected packet type: {Type}, code: {Code}")]
    public static partial void UnexpectedPacketType(this ILogger logger, PtpPacketType type, ushort code);

    [LoggerMessage(57, LogLevel.Debug, "OpenSession command sent")]
    public static partial void OpenSessionSent(this ILogger logger);

    [LoggerMessage(58, LogLevel.Debug, "Received init response length: {Length}")]
    public static partial void ReceivedInitResponseLength(this ILogger logger, uint length);

    [LoggerMessage(59, LogLevel.Debug, "Setting device property value: {PropertyCode}")]
    public static partial void SettingDevicePropValue(this ILogger logger, PtpPropertyCode propertyCode);

    [LoggerMessage(60, LogLevel.Debug, "Device property {PropertyCode} set successfully")]
    public static partial void DevicePropValueSet(this ILogger logger, PtpPropertyCode propertyCode);

    [LoggerMessage(61, LogLevel.Debug, "Getting device property value: {PropertyCode}")]
    public static partial void GettingDevicePropValue(this ILogger logger, PtpPropertyCode propertyCode);

    [LoggerMessage(62, LogLevel.Information, "No device property value received for {PropertyCode}")]
    public static partial void NoDevicePropValueReceived(this ILogger logger, PtpPropertyCode propertyCode);

    [LoggerMessage(63, LogLevel.Warning, "Failed to get device property {PropertyCode}, code: {ResponseCode}")]
    public static partial void FailedToGetDevicePropValue(this ILogger logger, PtpPropertyCode propertyCode, PtpResponseCode responseCode);

    [LoggerMessage(64, LogLevel.Debug, "Received device property {PropertyCode} value: {Length} bytes")]
    public static partial void ReceivedDevicePropValue(this ILogger logger, PtpPropertyCode propertyCode, int length);

    [LoggerMessage(65, LogLevel.Debug, "Setting AutoSaveDatabaseStatus: {CurrentIndex}/{TotalCount}")]
    public static partial void SettingAutoSaveDatabaseStatus(this ILogger logger, uint currentIndex, int totalCount);

    [LoggerMessage(66, LogLevel.Debug, "Getting image count from EventsList")]
    public static partial void GettingImageCount(this ILogger logger);

    [LoggerMessage(67, LogLevel.Debug, "Received events count: {Count}")]
    public static partial void ReceivedEvents(this ILogger logger, int count);

    [LoggerMessage(68, LogLevel.Debug, "Received image count: {Count}")]
    public static partial void ReceivedImageCount(this ILogger logger, int count);

    [LoggerMessage(69, LogLevel.Warning, "Failed to get image count")]
    public static partial void FailedToGetImageCount(this ILogger logger);

    [LoggerMessage(70, LogLevel.Warning, "Failed to find image count in Fuji events")]
    public static partial void FailedToFindImageCountInEvents(this ILogger logger);

    [LoggerMessage(71, LogLevel.Debug, "Received raw discovery message: {Message}")]
    public static partial void ReceivedRawDiscoveryMessage(this ILogger logger, string message);

    [LoggerMessage(72, LogLevel.Debug, "Received raw connection message: {Message}")]
    public static partial void ReceivedRawConnectionMessage(this ILogger logger, string message);

    [LoggerMessage(73, LogLevel.Debug, "Received raw PTP init response: {Data}")]
    public static partial void ReceivedRawPtpInitResponse(this ILogger logger, string data);

    [LoggerMessage(74, LogLevel.Information, "Getting image info for index {Index}")]
    public static partial void GettingImageInfo(this ILogger logger, uint index);

    [LoggerMessage(75, LogLevel.Debug, "Downloaded image {Index}: {BytesWritten} bytes")]
    public static partial void DownloadedImage(this ILogger logger, uint index, int bytesWritten);

    [LoggerMessage(76, LogLevel.Warning, "No response from camera for GetPartialImage")]
    public static partial void NoResponseForGetPartialImage(this ILogger logger);

    [LoggerMessage(77, LogLevel.Information, "Partial image download cancelled by device")]
    public static partial void PartialImageDownloadCancelled(this ILogger logger);
}
