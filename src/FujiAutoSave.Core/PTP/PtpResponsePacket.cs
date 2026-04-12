using FujiAutoSave.Core.Enums;

namespace FujiAutoSave.Core.PTP;

public class PtpResponsePacket : PtpPacket
{
    public PtpResponseCode ResponseCode => (PtpResponseCode)Code;
}
