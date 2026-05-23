using Volo.Abp.Application.Dtos;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Implements <see cref="IListResult{T}"/>.
/// </summary>
/// <typeparam name="T">Type of the items in the Items list</typeparam>
[Serializable]
public class ListResultDto<T> : IListResult<T>
{
    /// <inheritdoc />
    public IReadOnlyList<T> Items
    {
        get { return _items ?? (_items = new List<T>()); }
        set { _items = value; }
    }
    private IReadOnlyList<T>? _items;

    /// <summary>
    /// Creates a new <see cref="ListResultDto{T}"/> object.
    /// </summary>
    public ListResultDto()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ListResultDto{T}"/> object.
    /// </summary>
    /// <param name="items">List of items</param>
    public ListResultDto(IReadOnlyList<T> items)
    {
        Items = items;
    }
}

/// <summary>
/// Extensible list result DTO with extra properties support.
/// </summary>
[Serializable]
public class ExtensibleListResultDto<T> : ExtensibleObject, IListResult<T>
{
    /// <inheritdoc />
    public IReadOnlyList<T> Items
    {
        get { return _items ?? (_items = new List<T>()); }
        set { _items = value; }
    }
    private IReadOnlyList<T>? _items;

    /// <summary>
    /// Creates a new <see cref="ExtensibleListResultDto{T}"/> object.
    /// </summary>
    public ExtensibleListResultDto()
    {
    }

    /// <summary>
    /// Creates a new <see cref="ExtensibleListResultDto{T}"/> object.
    /// </summary>
    /// <param name="items">List of items</param>
    public ExtensibleListResultDto(IReadOnlyList<T> items)
    {
        Items = items;
    }
}
