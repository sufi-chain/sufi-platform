namespace SufiChain.SufiPlatform.Identity.OrganizationUnits;

public static class OuCreateDialogState
{
    public static bool ShowParentAlert(Guid? parentId, string? parentName) =>
        parentId.HasValue && !string.IsNullOrWhiteSpace(parentName);

    public static bool ShouldResetModel(bool wasOpen, bool isOpen) => !wasOpen && isOpen;
}
