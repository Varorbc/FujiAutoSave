using System.Net;

namespace FujiAutoSave.Core.Messaging;

public sealed class RegisterMessage : AutoSaveMessageBase
{
    public required IPAddress Host { get; set; }

    public int Port { get; set; }

    public required string Name { get; set; }

    public required string Model { get; set; }
}
