using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Core.PTP;

public class PtpDataPacket : PtpPacket
{
    public byte[] Payload { get; set; } = [];

    public PtpDataPacket() => Type = PtpPacketType.Data;
}
