using System;
using SufiChain.SufiPlatform.Application.Dtos;

namespace SufiChain.SufiPlatform.ShortLinks;

public class GetShortUrlListDto : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    
    public string? CreatedByModule { get; set; }
    
    public bool? IsActive { get; set; }
    
    public DateTime? CreatedAfter { get; set; }
    
    public DateTime? CreatedBefore { get; set; }
}

