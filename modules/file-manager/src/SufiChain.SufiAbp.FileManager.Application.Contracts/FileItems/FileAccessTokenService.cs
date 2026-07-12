using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.FileManager.Configuration;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.FileManager.FileItems;

public class FileAccessTokenService : IFileAccessTokenService, ITransientDependency
{
    private readonly FileManagerOptions _options;

    public FileAccessTokenService(IOptions<FileManagerOptions> options)
    {
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.FileAccessTokenSecret);

    public string GenerateToken(Guid fileId)
    {
        var secret = GetSecret();
        var expiry = DateTimeOffset.UtcNow.AddMinutes(_options.FileAccessTokenValidityMinutes).ToUnixTimeSeconds();
        var payload = $"{fileId:N}|{expiry}";
        var signature = ComputeHmac(secret, payload);
        var payloadBase64 = ToBase64Url(Encoding.UTF8.GetBytes(payload));
        var signatureBase64 = ToBase64Url(signature);
        return $"{payloadBase64}.{signatureBase64}";
    }

    public bool TryGenerateToken(Guid fileId, out string token)
    {
        token = string.Empty;
        if (!IsConfigured)
        {
            return false;
        }

        token = GenerateToken(fileId);
        return true;
    }

    public bool TryValidateToken(string? token, out Guid fileId)
    {
        fileId = default;
        if (!IsConfigured || string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        var parts = token.Split('.');
        if (parts.Length != 2)
        {
            return false;
        }

        byte[] payloadBytes;
        try
        {
            payloadBytes = FromBase64Url(parts[0]);
        }
        catch
        {
            return false;
        }

        var payload = Encoding.UTF8.GetString(payloadBytes);
        var payloadParts = payload.Split('|');
        if (payloadParts.Length != 2)
        {
            return false;
        }

        if (!Guid.TryParse(payloadParts[0], out fileId))
        {
            return false;
        }

        if (!long.TryParse(payloadParts[1], out var expiryUnix) || DateTimeOffset.UtcNow.ToUnixTimeSeconds() > expiryUnix)
        {
            return false;
        }

        var secret = GetSecret();
        var expectedSignature = ComputeHmac(secret, payload);
        var actualSignature = FromBase64Url(parts[1]);

        return CryptographicOperations.FixedTimeEquals(expectedSignature, actualSignature);

    }

    private string GetSecret()
    {
        if (!IsConfigured)
        {
            throw new InvalidOperationException("FileAccessTokenSecret is not configured.");
        }

        return _options.FileAccessTokenSecret!.Trim();
    }

    private byte[] ComputeHmac(string secret, string data)
    {
        var keyBytes = Encoding.UTF8.GetBytes(secret);
        var dataBytes = Encoding.UTF8.GetBytes(data);
        using var hmac = new HMACSHA256(keyBytes);
        return hmac.ComputeHash(dataBytes);
    }

    private string ToBase64Url(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    private byte[] FromBase64Url(string base64Url)
    {
        var base64 = base64Url
            .Replace('-', '+')
            .Replace('_', '/');
        
        switch (base64.Length % 4)
        {
            case 2: base64 += "=="; break;
            case 3: base64 += "="; break;
        }
        
        return Convert.FromBase64String(base64);
    }
}
