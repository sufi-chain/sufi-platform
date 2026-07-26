using System;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Identity.LinkUsers;

public class IdentityLinkUser_Tests
{
    [Fact]
    public void Ctor_From_LinkUserInfo_Should_Set_Source_And_Target()
    {
        var source = new IdentityLinkUserInfo(Guid.NewGuid());
        var target = new IdentityLinkUserInfo(Guid.NewGuid(), Guid.NewGuid());

        var link = new IdentityLinkUser(Guid.NewGuid(), source, target);

        link.SourceUserId.ShouldBe(source.UserId);
        link.SourceTenantId.ShouldBeNull();
        link.TargetUserId.ShouldBe(target.UserId);
        link.TargetTenantId.ShouldBe(target.TenantId);
        link.TenantId.ShouldBe(source.TenantId);
    }
}
