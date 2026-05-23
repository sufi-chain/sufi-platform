
using SufiChain.SufiAbp.Ddd.Dtos;

namespace SufiChain.SufiAbp.Application.Dtos;

/// <summary>
/// Simply implements <see cref="IPagedAndSortedResultRequest"/>.
/// </summary>
[Serializable]
public class PagedAndSortedResultRequestDto : PagedResultRequestDto, IPagedAndSortedResultRequest
{
    public virtual string? Sorting { get; set; }
}

/// <summary>
/// Extensible paged and sorted result request DTO.
/// </summary>
[Serializable]
public class ExtensiblePagedAndSortedResultRequestDto : ExtensiblePagedResultRequestDto, IPagedAndSortedResultRequest
{
    public virtual string? Sorting { get; set; }
}
