using System.Net;

namespace FujiAutoSave.Core.Messaging;

public sealed class DiscoverMessage : AutoSaveMessageBase
{
    public int MaxTimeout { get; set; }

    public required IPAddress BroadcastHost { get; set; }

    public required IPAddress CameraHost { get; set; }

    public required string Service { get; set; }
}
