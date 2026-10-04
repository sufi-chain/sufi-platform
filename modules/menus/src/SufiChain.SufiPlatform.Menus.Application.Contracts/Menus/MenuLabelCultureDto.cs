namespace SufiChain.SufiPlatform.Menus.Menus;

public class MenuLabelCultureDto
{
    public string CultureName { get; set; } = string.Empty;

    public string DisplayName { get; set; } = string.Empty;

    public bool IsRtl { get; set; }

    public bool IsDefault { get; set; }

    public int SortOrder { get; set; }
}
