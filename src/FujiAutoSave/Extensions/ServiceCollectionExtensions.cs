using FujiAutoSave.Core.Extensions;
using FujiAutoSave.Services;

namespace FujiAutoSave.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFujiCameraService(this IServiceCollection services)
    {
        services.AddHostedService<BackupHostedService>();
        services.AddTransient<IFujiCameraService, FujiCameraService>();
        services.AddFujiCameraClient();

        return services;
    }
}
