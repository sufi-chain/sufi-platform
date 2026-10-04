using Microsoft.AspNetCore.Identity;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using Volo.Abp.Data;
using Xunit;
using IdentityUser = SufiChain.SufiPlatform.Identity.IdentityUser;

namespace SufiChain.SufiPlatform.Account;

public class PasswordLoginDecisionTests
{
    [Fact]
    public void Missing_user_on_the_host_is_invalid_credentials()
    {
        PasswordLoginDecision.Decide(Facts(userFound: false, tenantSelected: false))
            .ShouldBe(PasswordLoginKind.InvalidCredentials);
    }

    [Fact]
    public void Missing_user_in_a_selected_workspace_is_a_tenant_mismatch()
    {
        PasswordLoginDecision.Decide(Facts(userFound: false, tenantSelected: true))
            .ShouldBe(PasswordLoginKind.TenantMismatch);
    }

    [Fact]
    public void Locked_account_is_reported_before_the_password_result()
    {
        PasswordLoginDecision.Decide(Facts(isLockedOut: true, passwordAccepted: false))
            .ShouldBe(PasswordLoginKind.LockedOut);
    }

    [Fact]
    public void External_account_without_a_password_asks_for_the_provider()
    {
        PasswordLoginDecision.Decide(Facts(hasPassword: false, isExternal: true, passwordAccepted: false))
            .ShouldBe(PasswordLoginKind.ExternalLoginOnly);
    }

    [Fact]
    public void Wrong_password_stays_invalid_credentials()
    {
        PasswordLoginDecision.Decide(Facts(passwordAccepted: false))
            .ShouldBe(PasswordLoginKind.InvalidCredentials);
    }

    [Fact]
    public void Accepted_password_for_a_disabled_account_is_inactive()
    {
        PasswordLoginDecision.Decide(Facts(isActive: false))
            .ShouldBe(PasswordLoginKind.Inactive);
    }

    [Fact]
    public void Accepted_password_with_unconfirmed_email_asks_for_confirmation()
    {
        var kind = PasswordLoginDecision.Decide(Facts(
            emailConfirmed: false,
            requireConfirmedEmail: true,
            canSignIn: false));

        kind.ShouldBe(PasswordLoginKind.EmailConfirmationRequired);
        PasswordLoginDecision.LocalizationKey(kind).ShouldBe("Sufi.Identity:EmailConfirmationRequired");
    }

    [Fact]
    public void Accepted_password_with_unconfirmed_phone_asks_for_phone_confirmation()
    {
        PasswordLoginDecision.Decide(Facts(phoneConfirmationRequired: true, phoneNumberConfirmed: false))
            .ShouldBe(PasswordLoginKind.PhoneConfirmationRequired);
    }

    [Fact]
    public void Accepted_password_with_two_factor_continues_to_the_second_step()
    {
        PasswordLoginDecision.Decide(Facts(twoFactorEnabled: true))
            .ShouldBe(PasswordLoginKind.TwoFactorRequired);
    }

    [Fact]
    public void Accepted_password_that_passes_policy_can_sign_in()
    {
        PasswordLoginDecision.Decide(Facts()).ShouldBe(PasswordLoginKind.Success);
        PasswordLoginDecision.IsPasswordAccepted(PasswordVerificationResult.SuccessRehashNeeded).ShouldBeTrue();
        PasswordLoginDecision.IsPasswordAccepted(PasswordVerificationResult.Failed).ShouldBeFalse();
    }

    [Fact]
    public void Malformed_password_hash_does_not_throw()
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var user = new IdentityUser(Guid.NewGuid(), "pooria_qa", "pooria_qa@example.com");
        SetPasswordHash(user, "not-a-hash");

        var result = PasswordLoginVerifier.Verify(hasher, user, "secret", out var hashFormatInvalid);

        result.ShouldBe(PasswordVerificationResult.Failed);
        hashFormatInvalid.ShouldBeTrue();
    }

    [Fact]
    public void Valid_password_hash_is_accepted_without_updating_the_user()
    {
        var hasher = new PasswordHasher<IdentityUser>();
        var user = new IdentityUser(Guid.NewGuid(), "pooria_qa", "pooria_qa@example.com");
        SetPasswordHash(user, hasher.HashPassword(user, "secret"));
        var hashBefore = user.PasswordHash;

        var result = PasswordLoginVerifier.Verify(hasher, user, "secret", out var hashFormatInvalid);

        PasswordLoginDecision.IsPasswordAccepted(result).ShouldBeTrue();
        hashFormatInvalid.ShouldBeFalse();
        user.PasswordHash.ShouldBe(hashBefore);
    }

    [Fact]
    public void Concurrency_and_tenant_errors_map_to_specific_keys()
    {
        LoginExceptionClassifier.LocalizationKey(new AbpDbConcurrencyException(), tenantSelected: false)
            .ShouldBe("LoginConcurrencyError");

        var tenantMissing = new BusinessException("Volo.AbpIo.MultiTenancy:010001");
        LoginExceptionClassifier.LocalizationKey(tenantMissing, tenantSelected: true)
            .ShouldBe("LoginTenantMismatch");

        LoginExceptionClassifier.LocalizationKey(new TimeoutException(), tenantSelected: true)
            .ShouldBe("LoginAccountStoreUnavailable");

        LoginExceptionClassifier.ReasonCode(tenantMissing)
            .ShouldBe("BusinessException:Volo.AbpIo.MultiTenancy:010001");
    }

    private static void SetPasswordHash(IdentityUser user, string hash)
    {
        typeof(IdentityUser).GetProperty(nameof(IdentityUser.PasswordHash))!.SetValue(user, hash);
    }

    private static PasswordLoginFacts Facts(
        bool userFound = true,
        bool tenantSelected = false,
        bool isActive = true,
        bool isLockedOut = false,
        bool hasPassword = true,
        bool isExternal = false,
        bool passwordAccepted = true,
        bool emailConfirmed = true,
        bool requireConfirmedEmail = false,
        bool canSignIn = true,
        bool phoneConfirmationRequired = false,
        bool phoneNumberConfirmed = true,
        bool twoFactorEnabled = false)
    {
        return new PasswordLoginFacts(
            userFound,
            tenantSelected,
            isActive,
            isLockedOut,
            hasPassword,
            isExternal,
            passwordAccepted,
            emailConfirmed,
            requireConfirmedEmail,
            canSignIn,
            phoneConfirmationRequired,
            phoneNumberConfirmed,
            twoFactorEnabled);
    }
}
