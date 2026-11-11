using System.ComponentModel.DataAnnotations;
using Shouldly;
using SufiChain.SufiPlatform.Tags.Tags;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class CreateTagValidationTests
{
    [Fact]
    public void Empty_name_and_scope_fail_before_the_service_runs()
    {
        var results = Validate(new CreateTagDto());
        results.ShouldContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Name)));
        results.ShouldContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Scope)));
    }

    [Fact]
    public void Name_only_still_requires_scope()
    {
        var results = Validate(new CreateTagDto { Name = "QA", Scope = " " });
        results.ShouldNotContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Name)));
        results.ShouldContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Scope)));
    }

    [Fact]
    public void Scope_only_still_requires_name()
    {
        var results = Validate(new CreateTagDto { Name = "", Scope = "QA.B42" });
        results.ShouldContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Name)));
        results.ShouldNotContain(result => result.MemberNames.Contains(nameof(CreateTagDto.Scope)));
    }

    [Fact]
    public void Name_and_scope_pass_data_annotation_validation()
    {
        Validate(new CreateTagDto { Name = "QA", Scope = "QA.B42" }).ShouldBeEmpty();
    }

    private static List<ValidationResult> Validate(CreateTagDto dto)
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(dto, new ValidationContext(dto), results, validateAllProperties: true);
        return results;
    }
}
