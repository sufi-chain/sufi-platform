using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Projects;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Storage;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Tests.Storage;

public class LocalOnlyStorageProviderTests : IDisposable
{
    private readonly IHelpDeskProjectRepository _projectRepository;
    private readonly LocalOnlyStorageProvider _storageProvider;
    private readonly string _testStoragePath;

    public LocalOnlyStorageProviderTests()
    {
        _testStoragePath = Path.Combine(Path.GetTempPath(), "KBStorageTests", Guid.NewGuid().ToString());

        var options = Options.Create(new KBStorageOptions
        {
            LocalStoragePath = _testStoragePath,
            CreateDirectoriesIfNotExist = true
        });

        _projectRepository = Substitute.For<IHelpDeskProjectRepository>();
        _storageProvider = new LocalOnlyStorageProvider(
            options,
            _projectRepository,
            NullLogger<LocalOnlyStorageProvider>.Instance);
    }

    [Fact]
    public async Task WriteArticleContentAsync_Should_Create_File()
    {
        var projectId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var languageCode = "en";
        var content = "# Test Article\n\nThis is test content.";

        var project = new HelpDeskProject(projectId, "Test Project", "test-project");
        _projectRepository.GetAsync(projectId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(project);

        await _storageProvider.WriteArticleContentAsync(projectId, articleId, languageCode, content);

        var exists = await _storageProvider.ArticleContentExistsAsync(projectId, articleId, languageCode);
        exists.ShouldBeTrue();
    }

    [Fact]
    public async Task ReadArticleContentAsync_Should_Return_Content()
    {
        var projectId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var languageCode = "en";
        var content = "# Test Article\n\nThis is test content.";

        var project = new HelpDeskProject(projectId, "Test Project", "test-project");
        _projectRepository.GetAsync(projectId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(project);

        await _storageProvider.WriteArticleContentAsync(projectId, articleId, languageCode, content);

        var readContent = await _storageProvider.ReadArticleContentAsync(projectId, articleId, languageCode);

        readContent.ShouldBe(content);
    }

    [Fact]
    public async Task DeleteArticleContentAsync_Should_Remove_File()
    {
        var projectId = Guid.NewGuid();
        var articleId = Guid.NewGuid();
        var languageCode = "en";
        var content = "# Test Article";

        var project = new HelpDeskProject(projectId, "Test Project", "test-project");
        _projectRepository.GetAsync(projectId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns(project);

        await _storageProvider.WriteArticleContentAsync(projectId, articleId, languageCode, content);

        await _storageProvider.DeleteArticleContentAsync(projectId, articleId, languageCode);

        var exists = await _storageProvider.ArticleContentExistsAsync(projectId, articleId, languageCode);
        exists.ShouldBeFalse();
    }

    [Fact]
    public void GetStorageType_Should_Return_LocalOnly()
    {
        var storageType = _storageProvider.GetStorageType();
        storageType.ShouldBe(StorageType.LocalOnly);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testStoragePath))
        {
            try
            {
                Directory.Delete(_testStoragePath, true);
            }
            catch
            {
                // Ignore cleanup errors
            }
        }
    }
}
