using System.Net;
using FujiAutoSave.Core.Messaging;

namespace FujiAutoSave.Core.Tests.Messaging;

public sealed class AutoSaveMessageFactoryTests
{
    [Fact]
    public void ParseDiscover_WithStarTarget_ShouldParseSuccessfully()
    {
        var message = "DISCOVER * HTTP/1.1\r\nMX:3\r\nHOST:192.168.137.255:\u0001\r\nDSCADDR:192.168.137.89\r\nSERVICE:PCAUTOSAVE/1.0\r\n";

        var discoverMessage = AutoSaveMessageFactory.ParseDiscover(message);

        Assert.NotNull(discoverMessage);
        Assert.Equal("1.1", discoverMessage.HttpVersion);
        Assert.Equal("*", discoverMessage.Target);
        Assert.Equal(3, discoverMessage.MaxTimeout);
        Assert.Equal(IPAddress.Parse("192.168.137.255"), discoverMessage.BroadcastHost);
        Assert.Equal(IPAddress.Parse("192.168.137.89"), discoverMessage.CameraHost);
        Assert.Equal("PCAUTOSAVE/1.0", discoverMessage.Service);
    }

    [Fact]
    public void ParseDiscover_WithDesktopTarget_ShouldParseSuccessfully()
    {
        var message = "DISCOVER desktop HTTP/1.1\r\nMX:3\r\nHOST:192.168.137.255:\u0001\r\nDSCADDR:192.168.137.89\r\nSERVICE:PCAUTOSAVE/1.0\r\n";

        var result = AutoSaveMessageFactory.ParseDiscover(message);

        Assert.NotNull(result);
        Assert.Equal("1.1", result.HttpVersion);
        Assert.Equal("desktop", result.Target);
        Assert.Equal(3, result.MaxTimeout);
        Assert.Equal(IPAddress.Parse("192.168.137.255"), result.BroadcastHost);
        Assert.Equal(IPAddress.Parse("192.168.137.89"), result.CameraHost);
        Assert.Equal("PCAUTOSAVE/1.0", result.Service);
    }

    [Fact]
    public void ParseDiscover_WithEmptyMessage_ShouldReturnNull()
    {
        var message = string.Empty;

        var discoverMessage = AutoSaveMessageFactory.ParseDiscover(message);

        Assert.Null(discoverMessage);
    }

    [Fact]
    public void ParseRegister_WithValidMessage_ShouldReturnRegisterMessage()
    {
        var message = "REGISTER * HTTP/1.1\r\nHOST:192.168.137.1:51542\r\nDSCNAME:FUJIFILM-X-S10-0EA7\r\nDSCMODEL:X-S10\r\n";

        var registerMessage = AutoSaveMessageFactory.ParseRegister(message);

        Assert.NotNull(registerMessage);
        Assert.Equal("1.1", registerMessage.HttpVersion);
        Assert.Equal("*", registerMessage.Target);
        Assert.Equal(IPAddress.Parse("192.168.137.1"), registerMessage.Host);
        Assert.Equal(51542, registerMessage.Port);
        Assert.Equal("FUJIFILM-X-S10-0EA7", registerMessage.Name);
        Assert.Equal("X-S10", registerMessage.Model);
    }

    [Fact]
    public void ParseRegister_WithEmptyMessage_ShouldReturnNull()
    {
        var message = string.Empty;

        var registerMessage = AutoSaveMessageFactory.ParseRegister(message);

        Assert.Null(registerMessage);
    }

    [Theory]
    [InlineData("IMPORT /guest HTTP/1.1\r\nHOST:192.168.137.1:51541\r\nDSCNAME:FUJIFILM-X-S10-0EA7\r\nDSCPORT:\r\n")]
    [InlineData("IMPORT /guest HTTP/1.1\r\nHOST:192.168.137.1:51541\r\nDSCNAME:FUJIFILM-X-S10-0EA7\r\nDSCPORT:\r\n\r\n")]
    [InlineData("IMPORT /guest HTTP/1.1\r\nHOST:192.168.137.1:51541\r\nDSCNAME:FUJIFILM-X-S10-0EA7\r\nDSCPORT:?\r\n")]
    public void ParseImport_WithGuestTarget_ShouldParseSuccessfully(string message)
    {
        var importMessage = AutoSaveMessageFactory.ParseImport(message);

        Assert.NotNull(importMessage);
        Assert.Equal("1.1", importMessage.HttpVersion);
        Assert.Equal("/guest", importMessage.Target);
        Assert.Equal(IPAddress.Parse("192.168.137.1"), importMessage.Host);
        Assert.Equal(51541, importMessage.Port);
        Assert.Equal("FUJIFILM-X-S10-0EA7", importMessage.Name);
    }

    [Fact]
    public void ParseImport_WithEmptyMessage_ShouldReturnNull()
    {
        var message = string.Empty;

        var importMessage = AutoSaveMessageFactory.ParseImport(message);

        Assert.Null(importMessage);
    }

    [Fact]
    public void BuildImportResponse_ShouldReturnHttpOk()
    {
        var result = AutoSaveMessageFactory.BuildImportResponse();

        Assert.Equal("HTTP/1.1 200 OK\r\n", result);
    }

    [Fact]
    public void BuildNotify_WithValidParameters_ShouldReturnCorrectFormat()
    {
        var host = IPAddress.Parse("192.168.1.100");
        var port = 51540;
        var clientName = Environment.MachineName;

        var message = AutoSaveMessageFactory.BuildNotify(host, port, clientName);

        Assert.NotNull(message);
        Assert.Contains(
            $"NOTIFY * HTTP/1.1\r\n" +
            $"HOST: {host}:{port}\r\n" +
            $"IMPORTER: {clientName}\r\n", message);
    }

    [Fact]
    public void BuildRegisterResponse_ShouldReturnCorrectFormat()
    {
        var message = AutoSaveMessageFactory.BuildRegisterResponse();

        Assert.NotNull(message);
        Assert.Contains(
            "HTTP/1.1 200 OK\r\n" +
            "FOLDER: guest\r\n" +
            "ServiceName: PCAUTOSAVE/1.0\r\n", message);
    }

    [Theory]
    [InlineData("HTTP/1.1 200 OK", true)]
    [InlineData("HTTP/1.1 200 OK\r\n\r\n", true)]
    [InlineData("HTTP/1.1 404 Not Found", false)]
    [InlineData("HTTP/1.1 500 Internal Server Error", false)]
    [InlineData("", false)]
    public void IsHttpOk_WithVariousResponses_ShouldReturnCorrectResult(string response, bool expected)
    {
        var result = AutoSaveMessageFactory.IsHttpOk(response);

        Assert.Equal(expected, result);
    }
}
