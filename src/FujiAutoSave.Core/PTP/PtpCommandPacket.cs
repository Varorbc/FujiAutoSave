using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Core.PTP;

public class PtpCommandPacket : PtpPacket
{
    public PtpCommandPacket() => Type = PtpPacketType.Command;

    public byte[] Create(uint[] parameters) => ToByteArray(parameters);
}
