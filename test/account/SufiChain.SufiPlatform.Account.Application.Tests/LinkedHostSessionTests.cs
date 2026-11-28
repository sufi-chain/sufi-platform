using System.Security.Claims;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class LinkedHostSessionTests
{
    [Fact]
    public void AvatarMenu_LinkedSession_ShowsBackToHost()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
        [
            new Claim(LinkedHostSession.SourceUserIdClaim, Guid.NewGuid().ToString("D")),
            new Claim(LinkedHostSession.SourceIsHostClaim, "true")
        ],
        "test"));

        LinkedHostSession.IsReturnToHostAvailable(
            principal.FindFirst(LinkedHostSession.SourceUserIdClaim)?.Value,
            principal.FindFirst(LinkedHostSession.SourceIsHostClaim)?.Value)
            .ShouldBeTrue();
    }

    [Fact]
    public void AvatarMenu_DirectSession_HidesBackToHost()
    {
        LinkedHostSession.IsReturnToHostAvailable(null, null).ShouldBeFalse();
    }
}
