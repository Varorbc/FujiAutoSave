using FujiAutoSave.Extensions;

var builder = Host.CreateApplicationBuilder(args);

var appSettingsPath = Path.Combine(Environment.CurrentDirectory, "settings", "appsettings.json");
builder.Configuration.AddJsonFile(appSettingsPath, true);

builder.Services.AddFujiCameraService();

var host = builder.Build();
await host.RunAsync();
