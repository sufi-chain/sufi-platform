using Volo.Abp.Application.Dtos;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Base class for entity DTOs.
/// </summary>
[Serializable]
public abstract class EntityDto : IEntityDto
{
    public override string ToString()
    {
        return $"[DTO: {GetType().Name}]";
    }
}

/// <summary>
/// Base class for entity DTOs with a primary key.
/// </summary>
/// <typeparam name="TKey">Type of the primary key</typeparam>
[Serializable]
public abstract class SufiAbpEntityDto<TKey> : EntityDto, IEntityDto<TKey>
{
    /// <summary>
    /// Id of the entity.
    /// </summary>
    public virtual TKey Id { get; set; } = default!;

    public override string ToString()
    {
        return $"[DTO: {GetType().Name}] Id = {Id}";
    }

    public virtual string? GetObjectKey()
    {
        return Id?.ToString();
    }
}
