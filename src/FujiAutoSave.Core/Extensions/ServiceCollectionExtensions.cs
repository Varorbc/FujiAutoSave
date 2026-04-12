using Microsoft.Extensions.DependencyInjection;

namespace FujiAutoSave.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddFujiCameraClient(this IServiceCollection services)
    {
        services.AddTransient<IFujiCameraClient, FujiCameraClient>();
        services.AddTransient<IFujiPtpSession, FujiPtpSession>();

        return services;
    }
}
