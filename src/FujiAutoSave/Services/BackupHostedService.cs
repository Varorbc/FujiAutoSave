using FujiAutoSave.Core;

namespace FujiAutoSave.Services;

public class BackupHostedService(IServiceProvider sp, IFujiCameraClient client) : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        client.StartConnection();

        client.StartDiscovery();

        client.CameraConnecting += camera =>
        {
            var scope = sp.CreateScope();

            var fujiCameraService = scope.ServiceProvider.GetRequiredService<IFujiCameraService>();

            return fujiCameraService.BackupAsync(camera.Host, camera.Name, cancellationToken);
        };

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        client.Dispose();

        return Task.CompletedTask;
    }
}
