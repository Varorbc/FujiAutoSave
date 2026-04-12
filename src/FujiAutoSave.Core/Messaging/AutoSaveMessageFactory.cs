using System.Net;
using System.Text.RegularExpressions;

namespace FujiAutoSave.Core.Messaging;

internal static partial class AutoSaveMessageFactory
{
    [GeneratedRegex(
        $@"^DISCOVER (?<{nameof(DiscoverMessage.Target)}>\S+) HTTP/(?<{nameof(DiscoverMessage.HttpVersion)}>\d+\.\d+)\r\n" +
        $@"MX:(?<{nameof(DiscoverMessage.MaxTimeout)}>\d+)\r\n" +
        $@"HOST:(?<{nameof(DiscoverMessage.BroadcastHost)}>\d+\.\d+\.\d+\.\d+):(?<Port>[^\r\n]+)\r\n" +
        $@"DSCADDR:(?<{nameof(DiscoverMessage.CameraHost)}>\d+\.\d+\.\d+\.\d+)\r\n" +
        $@"SERVICE:(?<{nameof(DiscoverMessage.Service)}>[A-Z]+/[0-9\.]+)\r\n$",
        RegexOptions.Compiled)]
    private static partial Regex DiscoverRegex();

    [GeneratedRegex(
        $@"^REGISTER (?<{nameof(RegisterMessage.Target)}>\S+) HTTP/(?<{nameof(RegisterMessage.HttpVersion)}>\d+\.\d+)\r\n" +
        $@"HOST:(?<{nameof(RegisterMessage.Host)}>\d+\.\d+\.\d+\.\d+):(?<{nameof(RegisterMessage.Port)}>\d+)\r\n" +
        $@"DSCNAME:(?<{nameof(RegisterMessage.Name)}>[A-Za-z0-9\-]+)\r\n" +
        $@"DSCMODEL:(?<{nameof(RegisterMessage.Model)}>[A-Za-z0-9\-]+)\r\n$",
        RegexOptions.Compiled)]
    private static partial Regex RegisterRegex();

    [GeneratedRegex(
        $@"^IMPORT (?<{nameof(ImportMessage.Target)}>\S+) HTTP/(?<{nameof(ImportMessage.HttpVersion)}>\d+\.\d+)\r\n" +
        $@"HOST:(?<{nameof(ImportMessage.Host)}>\d+\.\d+\.\d+\.\d+):(?<{nameof(ImportMessage.Port)}>\d+)\r\n" +
        $@"DSCNAME:(?<{nameof(ImportMessage.Name)}>[A-Za-z0-9\-]+)\r\n" +
        $@"DSCPORT:(?<DscPort>[^\r\n]*)\r\n" +
        @"(\r\n)*$",
        RegexOptions.Compiled)]
    private static partial Regex ImportRegex();

    public static string BuildNotify(IPAddress host, int port, string clientName) =>
        $"NOTIFY * HTTP/1.1\r\n" +
        $"HOST: {host}:{port}\r\n" +
        $"IMPORTER: {clientName}\r\n\r\n";

    public static string BuildRegisterResponse() =>
        "HTTP/1.1 200 OK\r\n" +
        "FOLDER: guest\r\n" +
        "ServiceName: PCAUTOSAVE/1.0\r\n";

    public static string BuildImportResponse() =>
        "HTTP/1.1 200 OK\r\n";

    public static DiscoverMessage? ParseDiscover(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var match = DiscoverRegex().Match(message);
        if (!match.Success)
        {
            return null;
        }

        var broadcastHosStr = match.Groups[nameof(DiscoverMessage.BroadcastHost)].Value;
        var cameraHostStr = match.Groups[nameof(DiscoverMessage.CameraHost)].Value;
        if (!IPAddress.TryParse(broadcastHosStr, out var broadcastHost) ||
            !IPAddress.TryParse(cameraHostStr, out var cameraHost))
        {
            return null;
        }

        var msg = new DiscoverMessage
        {
            HttpVersion = match.Groups[nameof(DiscoverMessage.HttpVersion)].Value,
            BroadcastHost = broadcastHost,
            CameraHost = cameraHost,
            Service = match.Groups[nameof(DiscoverMessage.Service)].Value,
            Target = match.Groups[nameof(DiscoverMessage.Target)].Value
        };

        if (int.TryParse(match.Groups[nameof(DiscoverMessage.MaxTimeout)].Value, out var maxTimeout))
        {
            msg.MaxTimeout = maxTimeout;
        }

        return msg;
    }

    public static RegisterMessage? ParseRegister(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var match = RegisterRegex().Match(message);
        if (!match.Success)
        {
            return null;
        }

        var hostStr = match.Groups[nameof(RegisterMessage.Host)].Value;
        if (!IPAddress.TryParse(hostStr, out var host))
        {
            return null;
        }

        var msg = new RegisterMessage
        {
            HttpVersion = match.Groups[nameof(RegisterMessage.HttpVersion)].Value,
            Target = match.Groups[nameof(RegisterMessage.Target)].Value,
            Host = host,
            Name = match.Groups[nameof(RegisterMessage.Name)].Value,
            Model = match.Groups[nameof(RegisterMessage.Model)].Value
        };

        if (int.TryParse(match.Groups[nameof(RegisterMessage.Port)].Value, out var port))
        {
            msg.Port = port;
        }

        return msg;
    }

    public static ImportMessage? ParseImport(string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return null;
        }

        var match = ImportRegex().Match(message);
        if (!match.Success)
        {
            return null;
        }

        var hostStr = match.Groups[nameof(ImportMessage.Host)].Value;
        if (!IPAddress.TryParse(hostStr, out var host))
        {
            return null;
        }

        var msg = new ImportMessage
        {
            HttpVersion = match.Groups[nameof(ImportMessage.HttpVersion)].Value,
            Target = match.Groups[nameof(ImportMessage.Target)].Value,
            Host = host,
            Name = match.Groups[nameof(ImportMessage.Name)].Value
        };

        if (int.TryParse(match.Groups[nameof(ImportMessage.Port)].Value, out var port))
        {
            msg.Port = port;
        }

        return msg;
    }

    public static bool IsHttpOk(string response) => response.Contains("HTTP/1.1 200 OK");
}
