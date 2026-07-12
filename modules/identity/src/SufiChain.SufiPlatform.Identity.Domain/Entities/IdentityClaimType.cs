using System;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityClaimType : AuditedAggregateRoot<Guid>
{
    public virtual string Name { get; protected set; } = null!;

    public virtual bool Required { get; set; }

    public virtual bool IsStatic { get; set; }

    [CanBeNull]
    public virtual string? Regex { get; set; }

    [CanBeNull]
    public virtual string? RegexDescription { get; set; }

    [CanBeNull]
    public virtual string? Description { get; set; }

    public virtual IdentityClaimValueType ValueType { get; set; }

    protected IdentityClaimType()
    {

    }

    public IdentityClaimType(
        Guid id,
        [NotNull] string name,
        bool required = false,
        bool isStatic = false,
        string? regex = null,
        string? regexDescription = null,
        string? description = null,
        IdentityClaimValueType valueType = IdentityClaimValueType.String)
    {
        Check.NotNull(name, nameof(name));

        Id = id;
        Name = name;
        Required = required;
        IsStatic = isStatic;
        Regex = regex;
        RegexDescription = regexDescription;
        Description = description;
        ValueType = valueType;
    }
}

public enum IdentityClaimValueType
{
    String = 0,
    Int = 1,
    Boolean = 2,
    DateTime = 3
}
