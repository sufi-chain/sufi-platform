using Shouldly;
using SufiChain.SufiPlatform.Identity.OrganizationUnits;
using Xunit;

namespace SufiChain.SufiPlatform.Identity;

public class OuCreateDialogStateTests
{
    [Fact]
    public void OUCreateModal_WithParentName_ShowsIt()
    {
        OuCreateDialogState.ShowParentAlert(Guid.NewGuid(), "Sales").ShouldBeTrue();
    }

    [Fact]
    public void OUCreateModal_WithoutParentName_HidesAlert()
    {
        OuCreateDialogState.ShowParentAlert(Guid.NewGuid(), "").ShouldBeFalse();
        OuCreateDialogState.ShowParentAlert(null, "Sales").ShouldBeFalse();
    }

    [Fact]
    public void OUCreateModal_Rerender_KeepsTypedName()
    {
        OuCreateDialogState.ShouldResetModel(wasOpen: false, isOpen: true).ShouldBeTrue();
        OuCreateDialogState.ShouldResetModel(wasOpen: true, isOpen: true).ShouldBeFalse();
    }
}
