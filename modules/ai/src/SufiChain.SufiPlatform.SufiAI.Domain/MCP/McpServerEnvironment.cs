using System.Text.Json;
using System.Text.RegularExpressions;
using SufiChain.SufiPlatform.SufiAI.MCP.Entities;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Security.Encryption;

namespace SufiChain.SufiPlatform.SufiAI.MCP;

/// <summary>
/// sufi-chain#557: stdio process environment. Values are encrypted in the server's extra properties.
/// Names may be shown. Values are never returned to the UI, arguments, or logs.
/// An empty update clears the map. A null update leaves it unchanged.
/// </summary>
public static class McpServerEnvironment
{
    public const string PropertyName = "McpEnvironment";

    private static readonly Regex KeyPattern = new("^[A-Za-z_][A-Za-z0-9_]{0,127}$", RegexOptions.Compiled);

    public static void Apply(
        MCPServer server,
        IStringEncryptionService encryptor,
        IReadOnlyDictionary<string, string?>? updates)
    {
        ArgumentNullException.ThrowIfNull(server);
        ArgumentNullException.ThrowIfNull(encryptor);
        if (updates is null)
        {
            return;
        }

        if (updates.Count == 0)
        {
            server.RemoveProperty(PropertyName);
            return;
        }

        var ciphers = ReadCiphers(server);
        foreach (var pair in updates)
        {
            if (!KeyPattern.IsMatch(pair.Key))
            {
                throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                    .WithData("Error", "An MCP environment name must be a single identifier (letters, digits, underscore).");
            }

            if (string.IsNullOrEmpty(pair.Value))
            {
                ciphers.Remove(pair.Key);
                continue;
            }

            if (pair.Value.Contains('\0') || pair.Value.Contains('\r') || pair.Value.Contains('\n') || pair.Value.Length > 4096)
            {
                throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                    .WithData("Error", $"MCP environment value for {pair.Key} is empty, too long, or contains a line break.");
            }

            var cipher = encryptor.Encrypt(pair.Value);
            if (string.IsNullOrEmpty(cipher))
            {
                throw new BusinessException(AIErrorCodes.InvalidProviderConfiguration)
                    .WithData("Error", $"MCP environment value for {pair.Key} could not be encrypted.");
            }

            ciphers[pair.Key] = cipher;
        }

        if (ciphers.Count == 0)
        {
            server.RemoveProperty(PropertyName);
            return;
        }

        server.SetProperty(PropertyName, JsonSerializer.Serialize(ciphers));
    }

    public static IReadOnlyList<string> KeyNames(MCPServer server)
    {
        return ReadCiphers(server).Keys.OrderBy(key => key, StringComparer.Ordinal).ToList();
    }

    public static IReadOnlyDictionary<string, string> Decrypt(MCPServer server, IStringEncryptionService? encryptor)
    {
        var ciphers = ReadCiphers(server);
        if (ciphers.Count == 0)
        {
            return new Dictionary<string, string>();
        }

        if (encryptor is null)
        {
            throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                .WithData("Error", "MCP stdio environment is configured but string encryption is unavailable.");
        }

        var plain = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in ciphers)
        {
            string? value;
            try
            {
                value = encryptor.Decrypt(pair.Value);
            }
            catch (Exception exception)
            {
                throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed, innerException: exception)
                    .WithData("Error", $"MCP stdio environment value for {pair.Key} could not be decrypted.");
            }

            if (string.IsNullOrEmpty(value))
            {
                throw new BusinessException(AIErrorCodes.MCPServerConnectionFailed)
                    .WithData("Error", $"MCP stdio environment value for {pair.Key} could not be decrypted.");
            }

            plain[pair.Key] = value;
        }

        return plain;
    }

    private static Dictionary<string, string> ReadCiphers(MCPServer server)
    {
        var json = server.GetProperty<string>(PropertyName);
        if (string.IsNullOrWhiteSpace(json))
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }

        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json)
                ?? new Dictionary<string, string>(StringComparer.Ordinal);
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>(StringComparer.Ordinal);
        }
    }
}
