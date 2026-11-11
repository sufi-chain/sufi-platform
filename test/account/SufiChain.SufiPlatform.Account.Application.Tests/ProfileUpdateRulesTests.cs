using System.ComponentModel.DataAnnotations;
using System.Reflection;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Validation;
using Xunit;

namespace SufiChain.SufiPlatform.Account;

public class ProfileUpdateRulesTests
{
    [Theory]
    [InlineData(null, null)]
    [InlineData(null, "")]
    [InlineData(null, "   ")]
    [InlineData("0912-111-2233", "09121112233")]
    [InlineData("+98 912 111 2233", "+989121112233")]
    [InlineData("+", "")]
    public void SamePhone_Treats_Blank_And_Formatting_As_The_Same_Number(string? stored, string? submitted)
    {
        ProfileUpdateRules.SamePhone(stored, submitted).ShouldBeTrue();
    }

    [Fact]
    public void SamePhone_Rejects_A_Different_Number()
    {
        ProfileUpdateRules.SamePhone("09121112233", "09120000000").ShouldBeFalse();
    }

    [Fact]
    public void NormalizePhone_Drops_Formatting_That_Exceeds_The_Column_Length()
    {
        var formatted = "+98 (912) 345-6789";
        formatted.Length.ShouldBeGreaterThan(16);

        var normalized = ProfileUpdateRules.NormalizePhone(formatted);

        normalized.ShouldBe("+989123456789");
        ProfileUpdateRules.EnsurePhoneFits(normalized);
    }

    [Fact]
    public void EnsurePhoneFits_Rejects_A_Normalized_Number_Past_The_Column()
    {
        var exception = Should.Throw<AbpValidationException>(() =>
            ProfileUpdateRules.EnsurePhoneFits(new string('1', 17)));

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber)));
    }

    [Fact]
    public void Optional_Profile_Fields_Are_Nullable()
    {
        Nullability(typeof(UpdateProfileDto), nameof(UpdateProfileDto.Name)).ShouldBe(NullabilityState.Nullable);
        Nullability(typeof(UpdateProfileDto), nameof(UpdateProfileDto.Surname)).ShouldBe(NullabilityState.Nullable);
        Nullability(typeof(UpdateProfileDto), nameof(UpdateProfileDto.PhoneNumber)).ShouldBe(NullabilityState.Nullable);
        Nullability(typeof(UpdateProfileDto), nameof(UpdateProfileDto.ConcurrencyStamp)).ShouldBe(NullabilityState.Nullable);
        Nullability(typeof(ProfileDto), nameof(ProfileDto.Name)).ShouldBe(NullabilityState.Nullable);
        Nullability(typeof(ProfileDto), nameof(ProfileDto.PhoneNumber)).ShouldBe(NullabilityState.Nullable);
    }

    [Fact]
    public void Update_Accepts_A_Profile_With_No_Name_Phone_Or_Concurrency_Stamp()
    {
        var input = new UpdateProfileDto
        {
            UserName = "pooria_qa",
            Email = "pooria@example.com",
            PhoneNumber = "+98 (912) 345-6789"
        };

        var results = Validate(input);

        results.ShouldBeEmpty();
    }

    [Theory]
    [InlineData("not-a-phone")]
    [InlineData("abc!!!")]
    public void Invalid_phone_is_rejected_and_the_stored_number_stays(string submitted)
    {
        var user = new IdentityUser(Guid.NewGuid(), "demo", "demo@example.com");
        user.SetPhoneNumber("+12025550129", confirmed: true);
        var input = new UpdateProfileDto
        {
            UserName = "demo",
            Email = "demo@example.com",
            PhoneNumber = submitted
        };

        var exception = Should.Throw<AbpValidationException>(() => ProfileUpdateRules.NormalizePhone(submitted));

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber)));
        input.PhoneNumber.ShouldBe(submitted);
        user.PhoneNumber.ShouldBe("+12025550129");
        Validate(input).ShouldContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber)));
    }

    [Fact]
    public async Task Update_rejects_an_invalid_phone_before_the_profile_is_written()
    {
        var service = new ProfileAppService(null!, null!, Options.Create(new Microsoft.AspNetCore.Identity.IdentityOptions()));
        var input = new UpdateProfileDto
        {
            UserName = "demo",
            Email = "demo@example.com",
            PhoneNumber = "not-a-phone"
        };

        var exception = await Should.ThrowAsync<AbpValidationException>(() => service.UpdateAsync(input));

        exception.ValidationErrors.ShouldContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber)));
        input.PhoneNumber.ShouldBe("not-a-phone");
    }

    [Fact]
    public void Valid_phone_normalizes_for_save()
    {
        ProfileUpdateRules.NormalizePhone("+12025550129").ShouldBe("+12025550129");
        var input = new UpdateProfileDto
        {
            UserName = "demo",
            Email = "demo@example.com",
            PhoneNumber = "+12025550129"
        };

        Validate(input).ShouldBeEmpty();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_phone_still_clears(string? submitted)
    {
        ProfileUpdateRules.NormalizePhone(submitted).ShouldBeNull();
        var input = new UpdateProfileDto
        {
            UserName = "demo",
            Email = "demo@example.com",
            PhoneNumber = submitted
        };

        Validate(input).ShouldNotContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.PhoneNumber)));
    }

    [Fact]
    public void Update_Rejects_A_Missing_User_Name()
    {
        var results = Validate(new UpdateProfileDto
        {
            Email = "pooria@example.com"
        });

        results.ShouldContain(error => error.MemberNames.Contains(nameof(UpdateProfileDto.UserName)));
    }

    private static List<ValidationResult> Validate(UpdateProfileDto input)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true);
        return results;
    }

    private static NullabilityState Nullability(Type type, string propertyName)
    {
        var property = type.GetProperty(propertyName);
        property.ShouldNotBeNull();
        return new NullabilityInfoContext().Create(property).WriteState;
    }
}
