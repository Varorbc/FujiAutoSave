using System.Net;
using FujiAutoSave.Core;
using FujiAutoSave.Core.Enums;
using FujiAutoSave.Core.Models;
using FujiAutoSave.Logging;

namespace FujiAutoSave.Services;

public sealed class FujiCameraService(ILogger<FujiCameraService> logger, IFujiPtpSession ptpSession) : IFujiCameraService
{
    private bool _disposedValue;

    public async Task BackupAsync(IPAddress host, string deviceName, CancellationToken cancellationToken = default)
    {
        await ptpSession.ConnectAsync(host, deviceName, cancellationToken);

        var imageCount = await ptpSession.GetImageCountAsync(cancellationToken);
        logger.FoundImages(imageCount);

        if (imageCount == 0)
        {
            return;
        }

        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var saveDirectory = Path.Combine(userProfile, "Photos");
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
        }

        for (var index = 1u; index <= imageCount; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await ptpSession.SetAutoSaveDatabaseStatusAsync(index - 1, imageCount, cancellationToken);

                var image = await ptpSession.GetImageInfoAsync(index, cancellationToken);
                if (image == null)
                {
                    await ptpSession.SetAutoSaveDatabaseStatusAsync(index, imageCount, cancellationToken);

                    logger.FailedToGetImageInfo(index, PtpResponseCode.InvalidImageIndex);

                    continue;
                }

                var path = Path.Combine(saveDirectory, image.FileName);
                if (File.Exists(path))
                {
                    await ptpSession.SetAutoSaveDatabaseStatusAsync(index, imageCount, cancellationToken);

                    logger.SkippingExistingFile(image.FileName);

                    continue;
                }

                await DownloadImageInternalAsync(image, path, index, cancellationToken);

                await ptpSession.SetAutoSaveDatabaseStatusAsync(index, imageCount, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.FailedToGetImageInfo(ex, index);
            }
        }
    }

    private async Task DownloadImageInternalAsync(ImageInfo image, string path, uint imageIndex, CancellationToken cancellationToken)
    {
        var tempPath = path + ".tmp";
        logger.DownloadingImage(image.FileName, tempPath);

        await using var fileStream = File.Create(tempPath);
        var bytesWritten = await ptpSession.DownloadPartialImageAsync(imageIndex, fileStream, cancellationToken);
        if (bytesWritten < 0)
        {
            logger.FailedToDownloadImage(image.FileName);

            await fileStream.DisposeAsync();
            File.Delete(tempPath);

            return;
        }

        if (bytesWritten != image.CompressedSize)
        {
            logger.DownloadSizeMismatch(image.FileName, bytesWritten, image.CompressedSize);

            await fileStream.DisposeAsync();
            File.Delete(tempPath);

            return;
        }

        await fileStream.DisposeAsync();
        File.Move(tempPath, path);

        logger.DownloadedImage(image.FileName);
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                ptpSession?.Dispose();
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
