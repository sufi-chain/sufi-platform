using Shouldly;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Menus;

public class MenuItemTests
{
    [Fact]
    public void SetParent_Should_Accept_Another_Item()
    {
        var item = new MenuItem(Guid.NewGuid(), Guid.NewGuid(), "home", "Home", "home");
        var parentId = Guid.NewGuid();
        item.SetParent(parentId);
        item.ParentId.ShouldBe(parentId);
    }

    [Fact]
    public void SetParent_Should_Reject_A_Circular_Reference()
    {
        var item = new MenuItem(Guid.NewGuid(), Guid.NewGuid(), "home", "Home", "home");
        var error = Should.Throw<BusinessException>(() => item.SetParent(item.Id));
        error.Code.ShouldBe(MenusErrorCodes.MenuItemCircularReference);
    }
}
