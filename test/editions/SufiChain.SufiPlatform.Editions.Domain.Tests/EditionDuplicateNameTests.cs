using System.Threading;
using NSubstitute;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.Editions;

public class EditionDuplicateNameTests
{
    [Fact]
    public async Task Duplicate_name_throws_before_insert()
    {
        var existing = new Edition(Guid.NewGuid(), "Scale", "Scale", "SCALE");
        var repository = Substitute.For<IEditionRepository>();
        repository.FindByNameAsync("Scale", Arg.Any<CancellationToken>()).Returns(existing);
        var manager = new EditionManager(repository);

        var exception = await Should.ThrowAsync<BusinessException>(() =>
            manager.CreateAsync("Scale", "QA B42 1111", "QA1111"));

        exception.Code.ShouldBe(EditionsErrorCodes.EditionNameAlreadyExists);
        await repository.DidNotReceive().InsertAsync(Arg.Any<Edition>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }
}
