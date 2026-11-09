using System;
using System.Security.Cryptography;
using System.Text;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// Deterministic Console <c>MCPServer.Id</c> values for iran-mcp services.
/// Matches the gateway resolver so stage and prod each get distinct per-server ids
/// without broadcasting one shared <c>IRAN_MCP_SERVER_ID</c>.
/// </summary>
public static class IranMcpServerId
{
    /// <summary>RFC 4122 DNS namespace UUID.</summary>
    public static readonly Guid Namespace = Guid.Parse("6ba7b810-9dad-11d1-80b4-00c04fd430c8");

    public const string NamePrefix = "sufi-iran-mcp/";

    /// <summary>
    /// Prefers an explicit host-row or config id. Otherwise derives from
    /// <paramref name="environment"/> + <paramref name="serverName"/>.
    /// </summary>
    public static Guid Resolve(string? explicitId, string environment, string serverName)
    {
        if (!string.IsNullOrWhiteSpace(explicitId) && Guid.TryParse(explicitId.Trim(), out var parsed))
        {
            return parsed;
        }

        return Derive(environment, serverName);
    }

    /// <summary>
    /// UUID v5 over <c>sufi-iran-mcp/{environment}/{serverName}</c>.
    /// Same inputs always yield the same id; different environments stay distinct.
    /// </summary>
    public static Guid Derive(string environment, string serverName)
    {
        var env = NormalizeEnvironment(environment);
        var server = NormalizeServerName(serverName);
        return CreateVersion5(Namespace, NamePrefix + env + "/" + server);
    }

    public static string NormalizeEnvironment(string? environment)
    {
        var value = (environment ?? string.Empty).Trim().ToLowerInvariant();
        return string.IsNullOrEmpty(value) ? "default" : value;
    }

    public static string NormalizeServerName(string? serverName)
    {
        var value = (serverName ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(value))
        {
            throw new ArgumentException("Iran MCP server name is required.", nameof(serverName));
        }

        return value;
    }

    /// <summary>RFC 4122 version-5 UUID (SHA-1). Matches Python <c>uuid.uuid5</c>.</summary>
    public static Guid CreateVersion5(Guid namespaceId, string name)
    {
        if (name is null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        var namespaceBytes = namespaceId.ToByteArray();
        SwapGuidByteOrder(namespaceBytes);

        var nameBytes = Encoding.UTF8.GetBytes(name);
        var data = new byte[namespaceBytes.Length + nameBytes.Length];
        Buffer.BlockCopy(namespaceBytes, 0, data, 0, namespaceBytes.Length);
        Buffer.BlockCopy(nameBytes, 0, data, namespaceBytes.Length, nameBytes.Length);

#pragma warning disable CA5350 // UUID v5 requires SHA-1.
        byte[] hash;
        using (var sha1 = SHA1.Create())
        {
            hash = sha1.ComputeHash(data);
        }
#pragma warning restore CA5350

        var bytes = new byte[16];
        Buffer.BlockCopy(hash, 0, bytes, 0, 16);

        bytes[6] = (byte)((bytes[6] & 0x0F) | 0x50);
        bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
        SwapGuidByteOrder(bytes);
        return new Guid(bytes);
    }

    private static void SwapGuidByteOrder(byte[] bytes)
    {
        Swap(bytes, 0, 3);
        Swap(bytes, 1, 2);
        Swap(bytes, 4, 5);
        Swap(bytes, 6, 7);
    }

    private static void Swap(byte[] bytes, int left, int right)
    {
        var temporary = bytes[left];
        bytes[left] = bytes[right];
        bytes[right] = temporary;
    }
}
