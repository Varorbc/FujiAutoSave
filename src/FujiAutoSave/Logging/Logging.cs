using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Logging;

internal static partial class Logging
{
    [LoggerMessage(101, LogLevel.Warning, "Failed to get image info for index {Index}")]
    public static partial void FailedToGetImageInfo(this ILogger logger, Exception exception, uint index);

    [LoggerMessage(102, LogLevel.Warning, "Failed to get image info for index {Index}, code: {Code}")]
    public static partial void FailedToGetImageInfo(this ILogger logger, uint index, PtpResponseCode code);

    [LoggerMessage(103, LogLevel.Information, "Found {Count} images")]
    public static partial void FoundImages(this ILogger logger, int count);

    [LoggerMessage(104, LogLevel.Error, "Download size mismatch for {FileName}: expected {ExpectedSize} bytes, got {ActualSize} bytes")]
    public static partial void DownloadSizeMismatch(this ILogger logger, string fileName, int actualSize, uint expectedSize);

    [LoggerMessage(105, LogLevel.Information, "Downloading image {FileName} to {Path}")]
    public static partial void DownloadingImage(this ILogger logger, string fileName, string path);

    [LoggerMessage(106, LogLevel.Information, "Downloaded image {FileName} successfully")]
    public static partial void DownloadedImage(this ILogger logger, string fileName);

    [LoggerMessage(107, LogLevel.Warning, "Failed to download image {FileName}")]
    public static partial void FailedToDownloadImage(this ILogger logger, string fileName);

    [LoggerMessage(108, LogLevel.Information, "Skipping existing file {FileName}")]
    public static partial void SkippingExistingFile(this ILogger logger, string fileName);
}
