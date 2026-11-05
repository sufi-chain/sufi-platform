using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Projects;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Services;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Storage;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using SufiChain.SufiPlatform.HelpDesk.Settings;
using SufiChain.SufiPlatform.Menus.Menus;
using SufiChain.SufiPlatform.SufiAI.MCP.Abstractions;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Volo.Abp.Timing;
using Volo.Abp.Users;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticlePublicationGateTests
{
    [Fact]
    public async Task Editor_without_publish_can_save_a_draft_but_cannot_publish()
    {
        var projectId = Guid.NewGuid();
        var article = Draft(projectId);
        var harness = Harness.Create(projectId, canPublish: false);
        harness.Bind(article);

        var saved = await harness.Articles.UpdateAsync(article.Id, Edit("Saved title", "saved body", "saved"));

        saved.Title.ShouldBe("Saved title");
        saved.Content.ShouldBe("saved body");
        article.GetTranslation("en").Content.ShouldBe("saved body");
        article.Status.ShouldBe(ArticleStatus.Draft);

        var denied = await Should.ThrowAsync<AbpAuthorizationException>(() =>
            harness.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" }));

        denied.Message.ShouldBe(KnowledgeBasePermissions.Articles.Publish);
        article.Status.ShouldBe(ArticleStatus.Draft);
    }

    [Fact]
    public async Task Publisher_can_publish()
    {
        var projectId = Guid.NewGuid();
        var article = Draft(projectId);
        var harness = Harness.Create(projectId, canPublish: true);
        harness.Bind(article);

        var published = await harness.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" });

        published.Status.ShouldBe(ArticleStatus.Published);
        article.Status.ShouldBe(ArticleStatus.Published);
    }

    [Theory]
    [InlineData(KnowledgeBaseAutomatedCaller.Import)]
    [InlineData(KnowledgeBaseAutomatedCaller.Sync)]
    [InlineData(KnowledgeBaseAutomatedCaller.AiModerator)]
    public async Task Automated_callers_cannot_publish_even_when_the_user_holds_publish(KnowledgeBaseAutomatedCaller caller)
    {
        var projectId = Guid.NewGuid();
        var article = Draft(projectId);
        var harness = Harness.Create(projectId, canPublish: true);
        harness.Bind(article);

        using (KnowledgeBasePublicationAmbient.Begin(caller))
        {
            var error = await Should.ThrowAsync<BusinessException>(() =>
                harness.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" }));
            error.Code.ShouldBe(KnowledgeBaseErrorCodes.AutomatedPublishForbidden);
        }

        article.Status.ShouldBe(ArticleStatus.Draft);

        var published = await harness.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" });
        published.Status.ShouldBe(ArticleStatus.Published);
    }

    [Fact]
    public async Task Agent_call_acting_for_a_publisher_cannot_publish_or_unpublish()
    {
        var projectId = Guid.NewGuid();
        var article = Draft(projectId);
        article.Publish(DateTime.UtcNow);
        var harness = Harness.Create(projectId, canPublish: true);
        harness.Bind(article);

        using (AgentCallAmbient.Begin(AgentCallContext.Anonymous(sessionId: "kb-qa")))
        {
            var unpublish = await Should.ThrowAsync<BusinessException>(() => harness.Articles.UnpublishAsync(article.Id));
            unpublish.Code.ShouldBe(KnowledgeBaseErrorCodes.AutomatedPublishForbidden);
            article.Status.ShouldBe(ArticleStatus.Published);

            var draft = Draft(projectId, slug: "other");
            harness.Bind(draft);
            var publish = await Should.ThrowAsync<BusinessException>(() =>
                harness.Articles.PublishAsync(draft.Id, new PublishKBArticleDto { CommitMessage = "release" }));
            publish.Code.ShouldBe(KnowledgeBaseErrorCodes.AutomatedPublishForbidden);
            draft.Status.ShouldBe(ArticleStatus.Draft);
        }

        AgentCallAmbient.Current.ShouldBeNull();
    }

    [Fact]
    public async Task Another_tenant_cannot_publish_this_tenants_article()
    {
        var projectId = Guid.NewGuid();
        var owner = Guid.NewGuid();
        var article = Draft(projectId, tenantId: owner);
        var harness = Harness.Create(projectId, canPublish: true, tenantId: Guid.NewGuid());
        harness.Bind(article);

        await Should.ThrowAsync<EntityNotFoundException>(() =>
            harness.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" }));

        article.Status.ShouldBe(ArticleStatus.Draft);
    }

    [Fact]
    public async Task With_approval_off_an_editor_edit_changes_the_public_page()
    {
        var projectId = Guid.NewGuid();
        var project = PublicProject(projectId);
        var article = Published(projectId, "Live title", "live-article", "live body");
        var harness = Harness.Create(projectId, canPublish: false, approval: "false");
        harness.Bind(article, project);

        var saved = await harness.Articles.UpdateAsync(article.Id, Edit("Edited title", "edited body", "live-article"));

        saved.HasPendingPublication.ShouldBeFalse();
        article.GetTranslation("en").Content.ShouldBe("edited body");

        var page = await harness.Articles.GetBySlugAsync(projectId, "en", "live-article");
        page.Content.ShouldBe("edited body");
        page.Title.ShouldBe("Edited title");
    }

    [Fact]
    public async Task With_approval_on_an_editor_edit_stays_off_the_public_page_until_publish()
    {
        var projectId = Guid.NewGuid();
        var project = PublicProject(projectId);
        var article = Published(projectId, "Live title", "live-article", "live body");
        var editor = Harness.Create(projectId, canPublish: false, approval: "true");
        editor.Bind(article, project);

        var saved = await editor.Articles.UpdateAsync(article.Id, Edit("Pending title", "pending body", "live-article"));

        saved.HasPendingPublication.ShouldBeTrue();
        saved.Title.ShouldBe("Pending title");
        saved.Content.ShouldBe("pending body");
        article.Status.ShouldBe(ArticleStatus.Published);
        article.GetTranslation("en").Title.ShouldBe("Live title");
        article.GetTranslation("en").Content.ShouldBe("live body");

        var before = await editor.Articles.GetBySlugAsync(projectId, "en", "live-article");
        before.Content.ShouldBe("live body");
        before.Title.ShouldBe("Live title");

        var publisher = Harness.Create(projectId, canPublish: true, approval: "true");
        publisher.Bind(article, project);
        var released = await publisher.Articles.PublishAsync(article.Id, new PublishKBArticleDto { CommitMessage = "release" });

        released.HasPendingPublication.ShouldBeFalse();
        released.Content.ShouldBe("pending body");
        article.GetTranslation("en").Content.ShouldBe("pending body");

        var after = await publisher.Articles.GetBySlugAsync(projectId, "en", "live-article");
        after.Content.ShouldBe("pending body");
        after.Title.ShouldBe("Pending title");
    }

    [Fact]
    public async Task With_approval_on_an_agent_edit_of_a_published_article_does_not_change_the_public_page()
    {
        var projectId = Guid.NewGuid();
        var project = PublicProject(projectId);
        var article = Published(projectId, "Live title", "live-article", "live body");
        var harness = Harness.Create(projectId, canPublish: true, approval: "true");
        harness.Bind(article, project);

        using (AgentCallAmbient.Begin(AgentCallContext.ForUser(null, Guid.NewGuid(), new ClaimsPrincipal(new ClaimsIdentity()))))
        {
            var saved = await harness.Articles.UpdateAsync(article.Id, Edit("Agent title", "agent body", "live-article"));
            saved.HasPendingPublication.ShouldBeTrue();
        }

        article.GetTranslation("en").Content.ShouldBe("live body");
        var page = await harness.Articles.GetBySlugAsync(projectId, "en", "live-article");
        page.Content.ShouldBe("live body");
    }

    [Fact]
    public async Task With_approval_on_restoring_a_published_article_without_publish_does_not_change_live_content()
    {
        var projectId = Guid.NewGuid();
        var article = Published(projectId, "Live title", "live-article", "live body");
        var previous = new KBArticleVersion(
            Guid.NewGuid(),
            article.Id,
            "en",
            1,
            "Old title",
            "old body",
            "saved");
        article.AddVersion(previous);

        var harness = Harness.Create(projectId, canPublish: false, approval: "true");
        harness.Bind(article);
        harness.Versions.FindByVersionNumberAsync(article.Id, 1, Arg.Any<string?>(), Arg.Any<CancellationToken>())
            .Returns(previous);

        var restored = await harness.VersionService.RestoreAsync(article.Id, 1);

        restored.Content.ShouldBe("old body");
        restored.CommitMessage.ShouldStartWith(KnowledgeBasePendingPublication.CommitPrefix);
        article.GetTranslation("en").Content.ShouldBe("live body");
    }

    [Fact]
    public void Pending_publication_round_trips_slug_visibility_and_category()
    {
        var categoryId = Guid.NewGuid();
        var commit = KnowledgeBasePendingPublication.FormatCommit("next-slug", ArticleVisibility.Internal, categoryId);
        commit.Length.ShouldBeLessThanOrEqualTo(KnowledgeBaseConsts.MaxCommitMessageLength);

        var article = Draft(Guid.NewGuid());
        var version = new KBArticleVersion(Guid.NewGuid(), article.Id, "en", 1, "Title", "Body", commit);
        article.AddVersion(version);

        var latest = KnowledgeBasePendingPublication.FindLatest(article, "en");
        latest.ShouldBe(version);
        KnowledgeBasePendingPublication.TryParse(version, "fallback", ArticleVisibility.Public, Guid.Empty, out var slug, out var visibility, out var category)
            .ShouldBeTrue();
        slug.ShouldBe("next-slug");
        visibility.ShouldBe(ArticleVisibility.Internal);
        category.ShouldBe(categoryId);
    }

    [Fact]
    public void Article_manager_rejects_sync_publish_and_unpublish()
    {
        var manager = new KBArticleManager(
            Substitute.For<IKBArticleRepository>(),
            Substitute.For<IMenuRepository>(),
            Substitute.For<IMenuItemRepository>(),
            new KBSlugGenerator());
        var article = Draft(Guid.NewGuid());

        using (KnowledgeBasePublicationAmbient.Begin(KnowledgeBaseAutomatedCaller.Sync))
        {
            var error = Should.Throw<BusinessException>(() => manager.Publish(article, DateTime.UtcNow));
            error.Code.ShouldBe(KnowledgeBaseErrorCodes.AutomatedPublishForbidden);
            article.Status.ShouldBe(ArticleStatus.Draft);
        }

        article.Publish(DateTime.UtcNow);
        using (KnowledgeBasePublicationAmbient.Begin(KnowledgeBaseAutomatedCaller.Import))
        {
            Should.Throw<BusinessException>(() => manager.Unpublish(article, DateTime.UtcNow));
        }

        article.Status.ShouldBe(ArticleStatus.Published);
    }

    [Theory]
    [InlineData("en.json", "Publish articles", "Knowledge Base")]
    [InlineData("fa.json", "انتشار مقاله", "پایگاه دانش")]
    public void Publish_permission_is_localized(string fileName, string publishText, string groupText)
    {
        var assembly = typeof(KnowledgeBaseErrorCodes).Assembly;
        var resourceName = assembly.GetManifestResourceNames().Single(name => name.EndsWith(fileName, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        using var document = JsonDocument.Parse(reader.ReadToEnd());
        var texts = document.RootElement.GetProperty("texts");

        texts.GetProperty("Permission:SufiHelpDesk.KnowledgeBase.Articles.Publish").GetString().ShouldBe(publishText);
        texts.GetProperty("Permission:SufiHelpDesk.KnowledgeBase").GetString().ShouldBe(groupText);
        texts.GetProperty(KnowledgeBaseErrorCodes.AutomatedPublishForbidden).GetString().ShouldNotBeNullOrWhiteSpace();
        texts.GetProperty("KnowledgeBase:PublishRequiresPermission").GetString().ShouldNotBeNullOrWhiteSpace();
        texts.GetProperty("KnowledgeBase:PendingPublication").GetString().ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void Import_and_user_guide_sync_do_not_publish_articles()
    {
        var root = FindRepositoryRoot();
        var import = File.ReadAllText(Path.Combine(
            root,
            "pro-modules",
            "helpdesk",
            "src",
            "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Application",
            "Files",
            "KBFileImportService.cs"));
        import.ShouldContain("KnowledgeBaseAutomatedCaller.Import");
        import.ShouldNotContain("ArticleStatus.Published");
        import.ShouldNotContain(".Publish(");

        var sync = File.ReadAllText(Path.Combine(
            root,
            "hosts",
            "SufiChane.SufiPlatform",
            "src",
            "SufiChane.SufiPlatform.Domain",
            "Data",
            "SufiPlatformKnowledgeBaseGuideDataSeedContributor.cs"));
        sync.ShouldContain("KnowledgeBaseAutomatedCaller.Sync");
        sync.ShouldNotContain("_articleManager.Publish");
    }

    [Fact]
    public void Publishing_approval_setting_defaults_off_and_is_visible_to_clients()
    {
        HelpDeskSettings.KnowledgeBasePublishingRequireApproval.ShouldBe("SufiHelpDesk.KnowledgeBase.Publishing.RequireApproval");

        var context = new SettingContext();
        new HelpDeskSettingDefinitionProvider().Define(context);
        var definition = context.GetOrNull(HelpDeskSettings.KnowledgeBasePublishingRequireApproval);

        definition.ShouldNotBeNull();
        definition.DefaultValue.ShouldBe("false");
        definition.IsVisibleToClients.ShouldBeTrue();
        definition.IsInherited.ShouldBeTrue();
    }

    private static UpdateKBArticleDto Edit(string title, string content, string slug)
    {
        return new UpdateKBArticleDto
        {
            LanguageCode = "en",
            Title = title,
            Content = content,
            Slug = slug,
            Visibility = ArticleVisibility.Public,
            CommitMessage = "save"
        };
    }

    private static KBArticle Draft(Guid projectId, Guid? tenantId = null, string slug = "draft-article")
    {
        var article = new KBArticle(Guid.NewGuid(), projectId, Guid.NewGuid(), tenantId);
        article.AddTranslation(Guid.NewGuid(), "en", "Draft", slug, "draft body");
        article.SetVisibility(ArticleVisibility.Public);
        return article;
    }

    private static KBArticle Published(Guid projectId, string title, string slug, string content)
    {
        var article = new KBArticle(Guid.NewGuid(), projectId, Guid.NewGuid());
        article.AddTranslation(Guid.NewGuid(), "en", title, slug, content);
        article.SetVisibility(ArticleVisibility.Public);
        article.Publish(DateTime.UtcNow);
        return article;
    }

    private static HelpDeskProject PublicProject(Guid projectId)
    {
        var project = new HelpDeskProject(projectId, "Public KB", "public-kb");
        project.SetIsKnowledgeBasePublic(true);
        return project;
    }

    private static string FindRepositoryRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var marker = Path.Combine(dir.FullName, "hosts", "SufiChane.SufiPlatform", "SufiChane.SufiPlatform.slnx");
            if (File.Exists(marker))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate the sufi-chain repository from the test output directory.");
    }

    private sealed class SettingContext : ISettingDefinitionContext
    {
        private readonly Dictionary<string, SettingDefinition> _settings = new(StringComparer.Ordinal);

        public SettingDefinition? GetOrNull(string name)
        {
            return _settings.TryGetValue(name, out var definition) ? definition : null;
        }

        public IReadOnlyList<SettingDefinition> GetAll() => _settings.Values.ToList();

        public void Add(params SettingDefinition[] definitions)
        {
            foreach (var definition in definitions)
            {
                _settings[definition.Name] = definition;
            }
        }
    }

    private sealed class Harness
    {
        private readonly IKBArticleRepository _articles;
        private readonly IHelpDeskProjectRepository _projects;
        public KBArticleAppService Articles { get; }
        public KBArticleVersionAppService VersionService { get; }
        public IKBArticleVersionRepository Versions { get; }

        private Harness(
            IKBArticleRepository articles,
            IHelpDeskProjectRepository projects,
            IKBArticleVersionRepository versions,
            KBArticleAppService articleService,
            KBArticleVersionAppService versionService)
        {
            _articles = articles;
            _projects = projects;
            Versions = versions;
            Articles = articleService;
            VersionService = versionService;
        }

        public static Harness Create(Guid projectId, bool canPublish, string? approval = null, Guid? tenantId = null)
        {
            var articles = Substitute.For<IKBArticleRepository>();
            var versions = Substitute.For<IKBArticleVersionRepository>();
            var projects = Substitute.For<IHelpDeskProjectRepository>();
            var clock = Substitute.For<IClock>();
            clock.Now.Returns(new DateTime(2026, 11, 3, 12, 0, 0, DateTimeKind.Utc));

            var policy = Substitute.For<IKBStoragePolicyChecker>();
            policy.GetEffectiveStorageTypeAsync(Arg.Any<StorageType>(), Arg.Any<CancellationToken>())
                .Returns(StorageType.None);
            var provider = Substitute.For<IKBStorageProvider>();
            provider.WriteArticleContentWithResultAsync(
                    Arg.Any<Guid>(),
                    Arg.Any<Guid>(),
                    Arg.Any<string>(),
                    Arg.Any<string>(),
                    Arg.Any<string?>(),
                    Arg.Any<CancellationToken>())
                .Returns(KBStorageWriteResult.DatabaseOnly());
            var resolver = Substitute.For<IKBStorageProviderResolver>();
            resolver.Resolve(Arg.Any<StorageType>()).Returns(provider);

            var storage = new KBStorageManager(
                projects,
                resolver,
                policy,
                clock,
                NullLogger<KBStorageManager>.Instance);
            var manager = new KBArticleManager(
                articles,
                Substitute.For<IMenuRepository>(),
                Substitute.For<IMenuItemRepository>(),
                new KBSlugGenerator(),
                new KnowledgeBasePublicationGuard());

            var articleService = new KBArticleAppService(articles, versions, projects, manager, storage);
            var versionService = new KBArticleVersionAppService(articles, versions, storage, manager);

            var permissions = Substitute.For<IPermissionChecker>();
            permissions.IsGrantedAsync(KnowledgeBasePermissions.Articles.Publish).Returns(canPublish);
            var settings = Substitute.For<ISettingProvider>();
            settings.GetOrNullAsync(HelpDeskSettings.KnowledgeBasePublishingRequireApproval).Returns(approval);
            var tenant = Substitute.For<ICurrentTenant>();
            tenant.Id.Returns(tenantId);

            var services = new ServiceCollection();
            services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
            services.AddSingleton(tenant);
            services.AddSingleton(Substitute.For<ICurrentUser>());
            services.AddSingleton(clock);
            services.AddSingleton(permissions);
            services.AddSingleton(settings);
            var lazy = new AbpLazyServiceProvider(services.BuildServiceProvider());
            articleService.LazyServiceProvider = lazy;
            versionService.LazyServiceProvider = lazy;

            return new Harness(articles, projects, versions, articleService, versionService);
        }

        public void Bind(KBArticle article, HelpDeskProject? project = null)
        {
            _articles.GetAsync(article.Id, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(article);
            var translation = article.GetTranslation("en");
            _articles.FindBySlugAsync(article.ProjectId, "en", translation.Slug, Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(article);

            project ??= new HelpDeskProject(article.ProjectId, "KB", "kb", article.TenantId);
            _projects.FindAsync(article.ProjectId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(project);
            _projects.GetAsync(article.ProjectId, Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(project);
        }
    }
}
