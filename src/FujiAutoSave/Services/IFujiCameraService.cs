using System.Net;

namespace FujiAutoSave.Services;

public interface IFujiCameraService : IDisposable
{
    Task BackupAsync(IPAddress host, string deviceName, CancellationToken cancellationToken = default);
}
