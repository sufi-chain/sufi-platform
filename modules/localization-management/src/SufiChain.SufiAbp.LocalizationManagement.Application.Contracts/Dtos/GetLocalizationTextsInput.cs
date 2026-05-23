using System.ComponentModel.DataAnnotations;
using SufiChain.SufiAbp.Application.Dtos;

namespace SufiChain.SufiAbp.LocalizationManagement.Dtos;

public class GetLocalizationTextsInput : PagedAndSortedResultRequestDto
{
    public string? ResourceName { get; set; }
    public string? CultureName { get; set; }
    public string? KeyFilter { get; set; }
    public string? ValueFilter { get; set; }
    public bool ShowMissingOnly { get; set; }
}

public class GetMergedLocalizationTextsInput : PagedAndSortedResultRequestDto
{
    [Required]
    public string ResourceName { get; set; } = default!;

    [Required]
    public string CultureName { get; set; } = default!;

    public string? KeyFilter { get; set; }
    public bool OnlyOverridden { get; set; }
}

public class GetLocalizationResourcesInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public bool? IsEnabled { get; set; }
}
