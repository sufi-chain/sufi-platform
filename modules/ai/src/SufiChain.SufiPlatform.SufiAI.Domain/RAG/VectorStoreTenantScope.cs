using System.Text;

namespace SufiChain.SufiPlatform.SufiAI.RAG;

/// <summary>
/// Builds per-tenant Qdrant collection / Pgvector schema names from a configured base.
/// Naming: <c>{base}_{tenantKey}</c> where <c>tenantKey</c> is <c>host</c> or tenant Guid <c>N</c>.
/// </summary>
public static class VectorStoreTenantScope
{
    public static string GetTenantKey(Guid? tenantId)
    {
        return tenantId?.ToString("N").ToLowerInvariant() ?? "host";
    }

    public static string BuildName(string baseName, string tenantKey)
    {
        var basePart = Sanitize(baseName);
        var keyPart = Sanitize(tenantKey);

        if (string.IsNullOrEmpty(basePart))
        {
            throw new ArgumentException("Base vector store name is required.", nameof(baseName));
        }

        if (string.IsNullOrEmpty(keyPart))
        {
            throw new ArgumentException("Tenant key is required.", nameof(tenantKey));
        }

        return $"{basePart}_{keyPart}";
    }

    public static string Sanitize(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length);
        foreach (var ch in value.Trim().ToLowerInvariant())
        {
            if ((ch >= 'a' && ch <= 'z') || (ch >= '0' && ch <= '9') || ch == '_')
            {
                builder.Append(ch);
            }
            else
            {
                builder.Append('_');
            }
        }

        return builder.ToString().Trim('_');
    }
}
