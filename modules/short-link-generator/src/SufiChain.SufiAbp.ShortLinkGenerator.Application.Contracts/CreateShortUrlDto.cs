using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

public class CreateShortUrlDto : ExtensibleObject
{
    [Required]
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDestinationUrlLength)]
    public string DestinationUrl { get; set; } = string.Empty;
    
    public DateTime? ExpiresAt { get; set; }
    
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxDescriptionLength)]
    public string? Description { get; set; }
    
    [StringLength(ShortLinkGeneratorConsts.ShortUrl.MaxCreatedByModuleLength)]
    public string? CreatedByModule { get; set; }

    public CreateShortUrlDto()
        : base(setDefaultsForExtraProperties: false)
    {
    }
}
