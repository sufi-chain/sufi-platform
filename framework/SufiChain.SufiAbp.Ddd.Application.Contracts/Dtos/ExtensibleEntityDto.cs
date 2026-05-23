using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base class for extensible entity DTOs with extra properties support.
/// </summary>
[Serializable]
public abstract class ExtensibleEntityDto : ExtensibleObject, IEntityDto
{
    protected ExtensibleEntityDto()
        : this(true)
    {
    }

    protected ExtensibleEntityDto(bool setDefaultsForExtraProperties)
        : base(setDefaultsForExtraProperties)
    {
    }

    public override string ToString()
    {
        return $"[DTO: {GetType().Name}]";
    }
}

/// <summary>
/// Base class for extensible entity DTOs with a primary key and extra properties support.
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
[Serializable]
public abstract class SufiAbpExtensibleEntityDto<TKey> : ExtensibleObject, IEntityDto<TKey>
{
    /// <summary>
    /// Id of the entity.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    protected SufiAbpExtensibleEntityDto()
        : this(true)
    {
    }

    protected SufiAbpExtensibleEntityDto(bool setDefaultsForExtraProperties)
        : base(setDefaultsForExtraProperties)
    {
    }

    public override string ToString()
    {
        return $"[DTO: {GetType().Name}] Id = {Id}";
    }

    public virtual string? GetObjectKey()
    {
        return Id?.ToString();
    }
}
