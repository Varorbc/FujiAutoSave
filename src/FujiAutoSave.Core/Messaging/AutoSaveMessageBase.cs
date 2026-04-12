namespace FujiAutoSave.Core.Messaging;

public abstract class AutoSaveMessageBase
{
    public required string HttpVersion { get; set; }

    public required string Target { get; set; }
}
