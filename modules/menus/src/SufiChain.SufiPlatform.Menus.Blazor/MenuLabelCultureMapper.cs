using SufiChain.SufiPlatform.Localization.Blazor.Public.Models;
using SufiChain.SufiPlatform.Menus.Menus;

namespace SufiChain.SufiPlatform.Menus.Blazor;

internal static class MenuLabelCultureMapper
{
    public static List<MultilingualCulture> Map(IEnumerable<MenuLabelCultureDto> cultures) =>
        cultures.Select(culture => new MultilingualCulture
        {
            CultureName = culture.CultureName,
            DisplayName = culture.DisplayName,
            IsRtl = culture.IsRtl,
            IsDefault = culture.IsDefault
        }).ToList();
}
