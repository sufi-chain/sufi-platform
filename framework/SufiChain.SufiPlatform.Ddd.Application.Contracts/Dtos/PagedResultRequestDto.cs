using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace SufiChain.SufiPlatform.Application.Dtos;

/// <summary>
/// Simply implements <see cref="IPagedResultRequest"/>.
/// </summary>
[Serializable]
public class PagedResultRequestDto : LimitedResultRequestDto, IPagedResultRequest
{
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
}

/// <summary>
/// Extensible paged result request DTO.
/// </summary>
[Serializable]
public class ExtensiblePagedResultRequestDto : SufiExtensibleLimitedResultRequestDto, IPagedResultRequest
{
    [Range(0, int.MaxValue)]
    public virtual int SkipCount { get; set; }
}
