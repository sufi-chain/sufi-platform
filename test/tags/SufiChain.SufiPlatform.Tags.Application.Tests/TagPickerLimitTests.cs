using Shouldly;
using SufiChain.SufiPlatform.Tags.Tags;
using Xunit;

namespace SufiChain.SufiPlatform.Tags;

public class TagPickerLimitTests
{
    [Fact]
    public void Explicit_max_wins_over_the_policy()
    {
        TagPickerLimit.Resolve(3, 10).ShouldBe(3);
        TagPickerLimit.Resolve(null, 10).ShouldBe(10);
        TagPickerLimit.BlocksAnother(10, 10).ShouldBeTrue();
        TagPickerLimit.BlocksAnother(9, 10).ShouldBeFalse();
    }

    [Fact]
    public void AssignAsync_AtLimit_ThrowsMaxTagsPerEntityExceeded()
    {
        var error = Should.Throw<Volo.Abp.BusinessException>(() => TagPickerLimit.EnsureRoom(10, 10));
        error.Code.ShouldBe(TagsErrorCodes.MaxTagsPerEntityExceeded);
        error.Data["MaxTagsPerEntity"].ShouldBe(10);
        Should.NotThrow(() => TagPickerLimit.EnsureRoom(9, 10));
    }
}
