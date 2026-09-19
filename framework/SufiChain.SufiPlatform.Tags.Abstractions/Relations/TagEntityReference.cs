using System;

namespace SufiChain.SufiPlatform.Tags.Relations;

/// <summary>
/// Identifies an entity within the effective tenant. The type is a registered,
/// stable machine key, never a CLR type name or a client-provided tenant identity.
/// </summary>
public sealed class TagEntityReference : IEquatable<TagEntityReference>
{
    public const int MaxEntityTypeLength = 96;

    public string EntityType { get; }
    public Guid EntityId { get; }

    public TagEntityReference(string entityType, Guid entityId)
    {
        if (!IsValidEntityType(entityType))
        {
            throw new ArgumentException("A lowercase registered entity type key is required.", nameof(entityType));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException("An entity ID is required.", nameof(entityId));
        }

        EntityType = entityType;
        EntityId = entityId;
    }

    public static bool IsValidEntityType(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length > MaxEntityTypeLength ||
            value[0] < 'a' || value[0] > 'z')
        {
            return false;
        }

        var previousWasSeparator = false;
        foreach (var character in value)
        {
            var isSeparator = character == '.' || character == '-';
            if (isSeparator)
            {
                if (previousWasSeparator)
                {
                    return false;
                }
            }
            else if (!(character >= 'a' && character <= 'z') && !(character >= '0' && character <= '9'))
            {
                return false;
            }

            previousWasSeparator = isSeparator;
        }

        return !previousWasSeparator;
    }

    public bool Equals(TagEntityReference? other) => other != null &&
        EntityId == other.EntityId && string.Equals(EntityType, other.EntityType, StringComparison.Ordinal);

    public override bool Equals(object? obj) => obj is TagEntityReference other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(StringComparer.Ordinal.GetHashCode(EntityType), EntityId);
}
