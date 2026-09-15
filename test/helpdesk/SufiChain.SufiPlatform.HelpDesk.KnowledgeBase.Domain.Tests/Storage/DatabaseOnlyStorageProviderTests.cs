using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Projects;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Storage;
using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Tests.Storage;

public class DatabaseOnlyStorageProviderTests
{
    private readonly DatabaseOnlyStorageProvider _storageProvider = new();

    [Fact]
    public void GetStorageType_Should_Return_None()
    {
        _storageProvider.GetStorageType().ShouldBe(StorageType.None);
    }

    [Fact]
    public async Task WriteArticleContentWithResultAsync_Should_Return_DatabaseOnly()
    {
        var result = await _storageProvider.WriteArticleContentWithResultAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en",
            "# Test");

        result.StorageType.ShouldBe(StorageType.None);
        result.CommitHash.ShouldBeNull();
    }

    [Fact]
    public async Task ArticleContentExistsAsync_Should_Return_False()
    {
        var exists = await _storageProvider.ArticleContentExistsAsync(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "en");

        exists.ShouldBeFalse();
    }

    [Fact]
    public async Task ReadArticleContentAsync_Should_Throw_ArticleContentNotFound()
    {
        var exception = await Should.ThrowAsync<BusinessException>(() =>
            _storageProvider.ReadArticleContentAsync(Guid.NewGuid(), Guid.NewGuid(), "en"));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleContentNotFound);
    }
}
