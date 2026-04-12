using System.Net;
using FujiAutoSave.Core.Enums;
using FujiAutoSave.Core.PTP;

namespace FujiAutoSave.Core.Tests.PTP;

public sealed class PtpPacketFactoryTests
{
    [Fact]
    public void CreateInitPacket_FromDeviceNameAndHost_ShouldReturn82BytePacket()
    {
        var host = IPAddress.Parse("192.168.137.89");
        var deviceName = "FUJIFILM-X-S10-0EA7";

        var buffer = PtpPacketFactory.CreateInitPacket(host, deviceName);

        Assert.Equal("5200000001000000F2E4538FADA5485D87B27F0BD3D5DED000000000C0A88959460055004A004900460049004C004D002D0058002D005300310030002D003000450041003700000000000000000000000000", Convert.ToHexString(buffer));
    }

    [Fact]
    public void CreateStatusPacket_WithCurrentIndexAndTotalCount_ShouldReturnEncodedStatusBuffer()
    {
        var buffer = PtpPacketFactory.CreateStatusPacket(0, 1);

        Assert.Equal("0430002F0031000000", Convert.ToHexString(buffer));
    }

    [Fact]
    public void ParseCameraModel_FromInitResponseData_ShouldReturnModelName()
    {
        var buffer = Convert.FromHexString("4400000002000000000000000870B0610A8B4593B2E79357DD36E05058002D0053003100300000");

        var cameraModel = PtpPacketFactory.ParseCameraModel(buffer);

        Assert.Equal("X-S10", cameraModel);
    }

    [Fact]
    public void ParseImageInfo_FromFujiObjectInfoData_ShouldReturnImageInfo()
    {
        var buffer = Convert.FromHexString("0100001001380000DA54D800083800000000000000000000000060180000401000000000000000000000000000000000000000000D440053004300460037003000330034002E004A005000470000001032003000320036003000330032003800540030003100330033003400330000000E4F007200690065006E0074006100740069006F006E003A0031000000");

        var imageInfo = PtpPacketFactory.ParseImageInfo(buffer);

        Assert.Equal(268435457u, imageInfo.StorageId);
        Assert.Equal(ImageFormat.Jpeg, imageInfo.Format);
        Assert.Equal(0, imageInfo.Protection);
        Assert.Equal(14177498u, imageInfo.CompressedSize);
        Assert.Equal(14344u, imageInfo.MaxPartialSize);
        Assert.Equal(6240u, imageInfo.Width);
        Assert.Equal(4160u, imageInfo.Height);
        Assert.Equal(0u, imageInfo.BitDepth);
        Assert.Equal(0u, imageInfo.ParentObject);
        Assert.Equal("DSCF7034.JPG", imageInfo.FileName);
        Assert.Equal(new DateTimeOffset(2026, 3, 28, 1, 33, 43, TimeSpan.Zero), imageInfo.CreateTime);
    }
}
