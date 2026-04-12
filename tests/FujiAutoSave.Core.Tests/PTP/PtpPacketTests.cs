using FujiAutoSave.Core.Enums;
using FujiAutoSave.Core.PTP;

namespace FujiAutoSave.Core.Tests.PTP;

public sealed class PtpPacketTests
{
    [Fact]
    public void Create_OpenSessionCommand_WithSessionIdParameter_ShouldCreate16BytePacket()
    {
        var packet = new PtpCommandPacket
        {
            Code = (ushort)PtpOperationCode.OpenSession,
            TransactionId = 1
        };

        var buffer = packet.Create([1]);

        Assert.Equal("10000000010002100100000001000000", Convert.ToHexString(buffer));
    }

    [Fact]
    public void FromByteArray_12ByteResponsePacket_ShouldParseAllFields()
    {
        var buffer = Convert.FromHexString("0C0000000300012002000000");

        var packet = PtpPacket.FromByteArray(buffer);

        Assert.NotNull(packet);
        Assert.Equal(12u, packet.Length);
        Assert.Equal(PtpPacketType.Response, packet.Type);
        Assert.Equal(8193, packet.Code);
        Assert.Equal(2u, packet.TransactionId);
    }

    [Fact]
    public void FromByteArray_8ByteMinimumResponsePacket_ShouldParseWithDefaults()
    {
        var buffer = Convert.FromHexString("0800000003000000");

        var packet = PtpPacket.FromByteArray(buffer);

        Assert.NotNull(packet);
        Assert.Equal(8u, packet.Length);
        Assert.Equal(PtpPacketType.Response, packet.Type);
        Assert.Equal(0, packet.Code);
        Assert.Equal(0u, packet.TransactionId);
    }
}
