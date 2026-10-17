using System;
using System.Security.Cryptography;
using System.Text;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>
/// Signed, unguessable Quick Share tokens (sufi-chain#436).
/// Payload: <c>share|fileId|tenantId|expiryUnix|version</c>, HMAC-SHA256 signed.
/// The version is stored on the file; bumping it revokes every link issued before.
/// </summary>
public static class FileShareTokens
{
    public const string VersionPropertyName = "QuickShareVersion";
    private const string Kind = "share";

    public readonly record struct Payload(Guid FileId, Guid? TenantId, DateTimeOffset ExpiresAt, int Version);

    public static string Create(string secret, Payload payload)
    {
        var raw = string.Join('|',
            Kind,
            payload.FileId.ToString("N"),
            payload.TenantId?.ToString("N") ?? "-",
            payload.ExpiresAt.ToUnixTimeSeconds(),
            payload.Version);
        return ToBase64Url(Encoding.UTF8.GetBytes(raw)) + "." + ToBase64Url(Sign(secret, raw));
    }

    /// <summary>Checks signature and expiry only; tenant and version are checked against the file.</summary>
    public static FileShareLinkStatus Validate(string secret, string? token, DateTimeOffset now, out Payload payload)
    {
        payload = default;
        if (string.IsNullOrWhiteSpace(token) || token.Length > 512)
        {
            return FileShareLinkStatus.Invalid;
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return FileShareLinkStatus.Invalid;
        }

        string raw;
        byte[] signature;
        try
        {
            raw = Encoding.UTF8.GetString(FromBase64Url(parts[0]));
            signature = FromBase64Url(parts[1]);
        }
        catch (FormatException)
        {
            return FileShareLinkStatus.Invalid;
        }

        if (!CryptographicOperations.FixedTimeEquals(Sign(secret, raw), signature))
        {
            return FileShareLinkStatus.Invalid;
        }

        var fields = raw.Split('|');
        if (fields.Length != 5
            || fields[0] != Kind
            || !Guid.TryParseExact(fields[1], "N", out var fileId)
            || !long.TryParse(fields[3], out var expiryUnix)
            || !int.TryParse(fields[4], out var version))
        {
            return FileShareLinkStatus.Invalid;
        }

        Guid? tenantId = null;
        if (fields[2] != "-")
        {
            if (!Guid.TryParseExact(fields[2], "N", out var parsedTenant))
            {
                return FileShareLinkStatus.Invalid;
            }

            tenantId = parsedTenant;
        }

        payload = new Payload(fileId, tenantId, DateTimeOffset.FromUnixTimeSeconds(expiryUnix), version);
        return now >= payload.ExpiresAt ? FileShareLinkStatus.Expired : FileShareLinkStatus.Valid;
    }

    /// <summary>A valid token still has to match the file's tenant and current share version.</summary>
    public static FileShareLinkStatus CheckAgainstFile(Payload payload, Guid? fileTenantId, int currentVersion)
    {
        if (payload.TenantId != fileTenantId)
        {
            return FileShareLinkStatus.Invalid;
        }

        return payload.Version == currentVersion ? FileShareLinkStatus.Valid : FileShareLinkStatus.Revoked;
    }

    private static byte[] Sign(string secret, string raw)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("sufi-file-share:" + secret));
        return hmac.ComputeHash(Encoding.UTF8.GetBytes(raw));
    }

    private static string ToBase64Url(byte[] data) =>
        Convert.ToBase64String(data).TrimEnd('=').Replace('+', '-').Replace('/', '_');

    private static byte[] FromBase64Url(string value)
    {
        var s = value.Replace('-', '+').Replace('_', '/');
        s = s.PadRight(s.Length + (4 - s.Length % 4) % 4, '=');
        return Convert.FromBase64String(s);
    }
}