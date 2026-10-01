using NSubstitute;
using Shouldly;
using Volo.Abp.Domain.Entities;
using Xunit;

namespace SufiChain.SufiPlatform.Users;

public class UserLookupServiceExtensionTests
{
    [Fact]
    public async Task GetById_Should_Return_The_User()
    {
        var id = Guid.NewGuid();
        var user = Substitute.For<IUser>();
        user.Id.Returns(id);
        var lookup = Substitute.For<IUserLookupService<IUser>>();
        lookup.FindByIdAsync(id, Arg.Any<CancellationToken>()).Returns(user);

        (await lookup.GetByIdAsync(id)).Id.ShouldBe(id);
    }

    [Fact]
    public async Task GetById_Should_Reject_A_Missing_User()
    {
        var id = Guid.NewGuid();
        var lookup = Substitute.For<IUserLookupService<IUser>>();
        lookup.FindByIdAsync(id, Arg.Any<CancellationToken>()).Returns((IUser?)null);

        await Should.ThrowAsync<EntityNotFoundException>(() => lookup.GetByIdAsync(id));
    }
}
