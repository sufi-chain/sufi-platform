using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Services;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Storage;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.Menus.Menus;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleProjectNotFoundTests
{
    [Fact]
    public async Task CreateAsync_Should_Throw_Localized_ProjectNotFound_When_Project_Missing()
    {
        var projectId = Guid.NewGuid();
        var projectRepository = Substitute.For<IHelpDeskProjectRepository>();
        projectRepository.FindAsync(projectId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((HelpDeskProject?)null);

        var service = CreateService(projectRepository);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(new CreateKBArticleDto
            {
                ProjectId = projectId,
                CategoryId = Guid.NewGuid(),
                Title = "Draft",
                Slug = "draft",
                Content = "body",
                LanguageCode = "en"
            }));

        error.Code.ShouldBe(KnowledgeBaseErrorCodes.ProjectNotFound);
        error.Code.ShouldBe("SufiHelpDesk:KB:ProjectNotFound");
        error.Data["ProjectId"].ShouldBe(projectId);
    }

    [Fact]
    public async Task UpdateAsync_Should_Throw_Localized_ProjectNotFound_When_Project_Missing()
    {
        var articleId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var article = new KBArticle(articleId, projectId, Guid.NewGuid(), tenantId: null);
        article.AddTranslation(Guid.NewGuid(), "en", "Title", "title", "content");

        var articleRepository = Substitute.For<IKBArticleRepository>();
        articleRepository.GetAsync(articleId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(article);

        var projectRepository = Substitute.For<IHelpDeskProjectRepository>();
        projectRepository.FindAsync(projectId, Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns((HelpDeskProject?)null);

        var service = CreateService(projectRepository, articleRepository);

        var error = await Should.ThrowAsync<BusinessException>(() =>
            service.UpdateAsync(articleId, new UpdateKBArticleDto
            {
                LanguageCode = "en",
                Title = "Title",
                Content = "content",
                Visibility = ArticleVisibility.Public
            }));

        error.Code.ShouldBe("SufiHelpDesk:KB:ProjectNotFound");
    }

    private static KBArticleAppService CreateService(
        IHelpDeskProjectRepository projectRepository,
        IKBArticleRepository? articleRepository = null)
    {
        articleRepository ??= Substitute.For<IKBArticleRepository>();
        var clock = Substitute.For<IClock>();
        clock.Now.Returns(DateTime.UtcNow);

        var articleManager = new KBArticleManager(
            articleRepository,
            Substitute.For<IMenuRepository>(),
            Substitute.For<IMenuItemRepository>(),
            new KBSlugGenerator());

        var storageManager = new KBStorageManager(
            projectRepository,
            Substitute.For<IKBStorageProviderResolver>(),
            Substitute.For<IKBStoragePolicyChecker>(),
            clock,
            NullLogger<KBStorageManager>.Instance);

        var service = new KBArticleAppService(
            articleRepository,
            Substitute.For<IKBArticleVersionRepository>(),
            projectRepository,
            articleManager,
            storageManager);

        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        services.AddSingleton(Substitute.For<ICurrentTenant>());
        services.AddSingleton(Substitute.For<ICurrentUser>());
        services.AddSingleton(clock);
        var provider = services.BuildServiceProvider();
        service.LazyServiceProvider = new AbpLazyServiceProvider(provider);
        return service;
    }
}
