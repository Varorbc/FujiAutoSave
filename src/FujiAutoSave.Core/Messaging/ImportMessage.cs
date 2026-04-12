using System.Net;

namespace FujiAutoSave.Core.Messaging;

public sealed class ImportMessage : AutoSaveMessageBase
{
    public required IPAddress Host { get; set; }

    public int Port { get; set; }

    public required string Name { get; set; }
}
