using System.Net;
using FujiAutoSave.Core.Models;

namespace FujiAutoSave.Core;

public interface IFujiPtpSession : IDisposable
{
    Task ConnectAsync(IPAddress host, string deviceName, CancellationToken cancellationToken = default);

    Task<int> DownloadImageAsync(uint index, Stream outputStream, CancellationToken cancellationToken = default);

    Task<int> DownloadPartialImageAsync(uint index, Stream outputStream, CancellationToken cancellationToken = default);

    Task<int> GetImageCountAsync(CancellationToken cancellationToken = default);

    Task<ImageInfo?> GetImageInfoAsync(uint index, CancellationToken cancellationToken = default);

    Task SetAutoSaveDatabaseStatusAsync(uint currentIndex, int totalCount, CancellationToken cancellationToken = default);
}
