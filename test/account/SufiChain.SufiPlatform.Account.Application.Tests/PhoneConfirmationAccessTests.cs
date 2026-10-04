using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.UI.Abstractions.Account;
using Volo.Abp;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class PhoneConfirmationAccessTests
{
    [Fact]
    public async Task Anonymous_Caller_Without_A_Token_Is_Rejected()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            PhoneConfirmationAccess.ResolveUserIdAsync(AnonymousUser(), Store(), null));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
    }

    [Fact]
    public async Task Unknown_Token_Is_Rejected()
    {
        var sessions = Store();
        sessions.FindUserIdAsync("missing", Arg.Any<CancellationToken>()).Returns((Guid?)null);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            PhoneConfirmationAccess.ResolveUserIdAsync(AnonymousUser(), sessions, "missing"));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
    }

    [Fact]
    public async Task Anonymous_Caller_With_A_Valid_Token_Acts_For_That_User()
    {
        var userId = Guid.NewGuid();
        var sessions = Store();
        sessions.FindUserIdAsync("owned", Arg.Any<CancellationToken>()).Returns(userId);

        var resolved = await PhoneConfirmationAccess.ResolveUserIdAsync(AnonymousUser(), sessions, "owned");

        resolved.ShouldBe(userId);
    }

    [Fact]
    public async Task Signed_In_Caller_Without_A_Token_Acts_Only_For_Themselves()
    {
        var userId = Guid.NewGuid();

        var resolved = await PhoneConfirmationAccess.ResolveUserIdAsync(SignedIn(userId), Store(), null);

        resolved.ShouldBe(userId);
    }

    [Fact]
    public async Task Signed_In_Caller_Cannot_Use_Another_Users_Token()
    {
        var sessions = Store();
        sessions.FindUserIdAsync("other", Arg.Any<CancellationToken>()).Returns(Guid.NewGuid());

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            PhoneConfirmationAccess.ResolveUserIdAsync(SignedIn(Guid.NewGuid()), sessions, "other"));

        exception.Code.ShouldBe(IdentitySecurityErrorCodes.PhoneConfirmationSessionInvalid);
    }

    [Fact]
    public async Task Signed_In_Caller_Can_Use_Their_Own_Token()
    {
        var userId = Guid.NewGuid();
        var sessions = Store();
        sessions.FindUserIdAsync("mine", Arg.Any<CancellationToken>()).Returns(userId);

        var resolved = await PhoneConfirmationAccess.ResolveUserIdAsync(SignedIn(userId), sessions, "mine");

        resolved.ShouldBe(userId);
    }

    private static ICurrentUser AnonymousUser()
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns((Guid?)null);
        return currentUser;
    }

    private static ICurrentUser SignedIn(Guid userId)
    {
        var currentUser = Substitute.For<ICurrentUser>();
        currentUser.Id.Returns(userId);
        return currentUser;
    }

    private static IPhoneConfirmationSessionStore Store()
    {
        return Substitute.For<IPhoneConfirmationSessionStore>();
    }
}
