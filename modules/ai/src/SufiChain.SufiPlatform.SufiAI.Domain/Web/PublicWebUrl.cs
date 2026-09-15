using System;
using System.Net;
using System.Net.Sockets;

namespace SufiChain.SufiPlatform.SufiAI.Web;

public static class PublicWebUrl
{
    public static Uri Parse(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 4096 || !Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("http" or "https") || uri.Port is not (80 or 443) ||
            uri.UserInfo.Length != 0 || uri.HostNameType is UriHostNameType.Unknown ||
            uri.IsLoopback || uri.Host.EndsWith(".localhost", StringComparison.OrdinalIgnoreCase) ||
            (IPAddress.TryParse(uri.Host.Trim('[', ']'), out var address) && !IsPublic(address)))
            throw new WebResearchException("UrlNotAllowed");
        return new UriBuilder(uri) { Fragment = "" }.Uri;
    }

    public static bool IsPublic(IPAddress address)
    {
        if (address.IsIPv4MappedToIPv6) address = address.MapToIPv4();
        var b = address.GetAddressBytes();
        if (address.AddressFamily == AddressFamily.InterNetwork)
            return !(b[0] is 0 or 10 or 127 || b[0] >= 224 ||
                (b[0] == 100 && b[1] is >= 64 and <= 127) ||
                (b[0] == 169 && b[1] == 254) || (b[0] == 172 && b[1] is >= 16 and <= 31) ||
                (b[0] == 192 && (b[1] is 0 or 168 || (b[1] == 88 && b[2] == 99))) ||
                (b[0] == 198 && (b[1] is 18 or 19 || (b[1] == 51 && b[2] == 100))) ||
                (b[0] == 203 && b[1] == 0 && b[2] == 113));
        // Permit global unicast only; exclude transition, documentation, and special-use ranges.
        return address.AddressFamily == AddressFamily.InterNetworkV6 && (b[0] & 0xe0) == 0x20 &&
            !(b[0] == 0x20 && b[1] == 0x01 && (b[2] < 2 || (b[2] == 0x0d && b[3] == 0xb8))) &&
            !(b[0] == 0x20 && b[1] == 0x02) && !(b[0] == 0x3f && b[1] == 0xff);
    }
}

public sealed class WebResearchException(string code) : Exception(code)
{
    public string Code { get; } = code;
}
