using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Xunit;

namespace SufiChain.SufiPlatform.Account.Verification;

public class NormalizePhoneTests
{
    [Theory]
    [InlineData("09120322854")]
    [InlineData("9120322854")]
    [InlineData("00989120322854")]
    [InlineData("+989120322854")]
    [InlineData("+98 912 032 2854")]
    [InlineData("0912-032-2854")]
    [InlineData("۰۹۱۲۰۳۲۲۸۵۴")]
    [InlineData("٠٩١٢٠٣٢٢٨٥٤")]
    public void NormalizePhone_maps_iranian_forms_to_e164(string input)
    {
        VerificationIdentifierHelper.NormalizePhone(input).ShouldBe("+989120322854");
    }

    [Fact]
    public void NormalizePhone_keeps_non_iranian_plus_digits()
    {
        VerificationIdentifierHelper.NormalizePhone("+44 20 7946 0958").ShouldBe("+442079460958");
    }

    [Fact]
    public void PhoneLookupVariants_cover_legacy_stored_forms()
    {
        var variants = VerificationIdentifierHelper.PhoneLookupVariants("+989120322854");

        variants.ShouldBe(new[]
        {
            "+989120322854",
            "09120322854",
            "9120322854",
            "00989120322854",
            "989120322854"
        });
    }

    [Fact]
    public async Task FindByPhoneNumberAsync_matches_legacy_national_storage()
    {
        var user = CreateUser("09120322854");
        var repo = CreateRepo(user);

        var found = await VerificationIdentifierHelper.FindByPhoneNumberAsync(repo, "+989120322854");

        found.ShouldNotBeNull();
        found!.Id.ShouldBe(user.Id);
    }

    [Fact]
    public async Task FindByPhoneNumberAsync_matches_e164_storage_from_national_input()
    {
        var user = CreateUser("+989120322854");
        var repo = CreateRepo(user);

        var found = await VerificationIdentifierHelper.FindByPhoneNumberAsync(repo, "09120322854");

        found.ShouldNotBeNull();
        found!.Id.ShouldBe(user.Id);
    }

    private static IdentityUser CreateUser(string phone)
    {
        var user = new IdentityUser(Guid.NewGuid(), "otp-user", "otp@example.com");
        user.SetPhoneNumber(phone, confirmed: true);
        return user;
    }

    private static IIdentityUserRepository CreateRepo(IdentityUser user)
    {
        var repo = Substitute.For<IIdentityUserRepository>();
        repo.GetListAsync(
                sorting: Arg.Any<string?>(),
                maxResultCount: Arg.Any<int>(),
                skipCount: Arg.Any<int>(),
                filter: Arg.Any<string?>(),
                includeDetails: Arg.Any<bool>(),
                roleId: Arg.Any<Guid?>(),
                organizationUnitId: Arg.Any<Guid?>(),
                userName: Arg.Any<string?>(),
                phoneNumber: Arg.Any<string?>(),
                emailAddress: Arg.Any<string?>(),
                name: Arg.Any<string?>(),
                surname: Arg.Any<string?>(),
                isLockedOut: Arg.Any<bool?>(),
                notActive: Arg.Any<bool?>(),
                emailConfirmed: Arg.Any<bool?>(),
                isExternal: Arg.Any<bool?>(),
                maxCreationTime: Arg.Any<DateTime?>(),
                minCreationTime: Arg.Any<DateTime?>(),
                maxModificationTime: Arg.Any<DateTime?>(),
                minModificationTime: Arg.Any<DateTime?>(),
                cancellationToken: Arg.Any<CancellationToken>())
            .Returns(call =>
            {
                var phone = call.ArgAt<string?>(8);
                return !string.IsNullOrWhiteSpace(phone)
                       && user.PhoneNumber!.Contains(phone, StringComparison.Ordinal)
                    ? [user]
                    : [];
            });

        return repo;
    }
}
