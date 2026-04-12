using System.Net;

namespace FujiAutoSave.Core.Models;

public sealed class CameraInfo
{
    public required string Name { get; set; }

    public string? Model { get; set; }

    public required IPAddress Host { get; set; }

    public required int MaxTimeout { get; set; }

    public required string Target { get; set; }
}
