using FujiAutoSave.Core.Models;

namespace FujiAutoSave.Core;

public interface IFujiCameraClient : IDisposable
{
    event Func<CameraInfo, Task> CameraRegistered;

    event Func<CameraInfo, Task> CameraConnecting;

    void StartDiscovery();

    void StopDiscovery();

    void StartConnection();

    void StopConnection();
}
