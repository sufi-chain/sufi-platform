using System.Text.Json;
using Microsoft.Extensions.Localization;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Ai;
using SufiChain.SufiPlatform.HelpDesk.Features;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.AI;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Data;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Localization;
using SufiChain.SufiPlatform.SufiAI.Hooshvare;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KbArticleEditorHooshvareResolverTests
{
    [Fact]
    public async Task Feature_Off_Should_Say_Feature_Disabled_And_Not_Look_Up_By_Key()
    {
        var fixture = Fixture.Create(featureEnabled: false);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(Bound(fixture.PurposeHooshvareId));

        var exception = await Should.ThrowAsync<BusinessException>(
            () => fixture.Resolver.ResolveAsync(fixture.ProjectId));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleHooshvareFeatureDisabled);
        exception.Data["Feature"].ShouldBe(HelpDeskFeatures.Names.KnowledgeBaseArticleHooshvare);
        exception.Message.ShouldContain("KnowledgeBase:ArticleHooshvareFeatureDisabled");
        exception.Message.ShouldContain(HelpDeskFeatures.Names.KnowledgeBaseArticleHooshvare);
        await fixture.Catalog.DidNotReceive().GetByKeyAsync(Arg.Any<string>());
        await fixture.Catalog.DidNotReceive().GetAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Content_Editing_Purpose_Id_Should_Win_Over_The_Canonical_Key()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        var purpose = CatalogItem(fixture.PurposeHooshvareId, "custom.article.editor");
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(Bound(fixture.PurposeHooshvareId));
        fixture.Catalog.GetAsync(fixture.PurposeHooshvareId).Returns(purpose);

        var result = await fixture.Resolver.ResolveAsync(fixture.ProjectId);

        result.Id.ShouldBe(fixture.PurposeHooshvareId);
        result.Key.ShouldBe("custom.article.editor");
        await fixture.Catalog.DidNotReceive().GetByKeyAsync(Arg.Any<string>());
    }

    [Fact]
    public async Task Missing_Content_Editing_Assignment_Should_Fall_Back_To_The_Canonical_Key()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        var seeded = CatalogItem(fixture.SeededHooshvareId, PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(new HelpDeskAiBindingResultDto
            {
                ProjectId = fixture.ProjectId,
                Purpose = HelpDeskAiWorkspacePurpose.ContentEditing,
                Source = HelpDeskAiBindingSource.Missing,
                Readiness = HelpDeskAiBindingReadiness.MissingAssignment
            });
        fixture.Catalog.GetByKeyAsync(PlatformHooshvareKeys.HelpDeskKbArticleEditor).Returns(seeded);

        var result = await fixture.Resolver.ResolveAsync(fixture.ProjectId);

        result.Id.ShouldBe(fixture.SeededHooshvareId);
        result.Key.ShouldBe(PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        await fixture.Catalog.DidNotReceive().GetAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Default_Purpose_Assignment_Should_Not_Replace_The_Content_Editing_Key_Fallback()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        var otherId = Guid.NewGuid();
        var seeded = CatalogItem(fixture.SeededHooshvareId, PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(new HelpDeskAiBindingResultDto
            {
                ProjectId = fixture.ProjectId,
                Purpose = HelpDeskAiWorkspacePurpose.ContentEditing,
                Source = HelpDeskAiBindingSource.ProjectDefault,
                HooshvareId = otherId,
                Readiness = HelpDeskAiBindingReadiness.Ready
            });
        fixture.Catalog.GetByKeyAsync(PlatformHooshvareKeys.HelpDeskKbArticleEditor).Returns(seeded);

        var result = await fixture.Resolver.ResolveAsync(fixture.ProjectId);

        result.Id.ShouldBe(fixture.SeededHooshvareId);
        await fixture.Catalog.DidNotReceive().GetAsync(otherId);
    }

    [Fact]
    public async Task Missing_Purpose_Definition_Should_Fall_Back_To_The_Canonical_Key()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        var seeded = CatalogItem(fixture.SeededHooshvareId, PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(Bound(fixture.PurposeHooshvareId));
        fixture.Catalog.GetAsync(fixture.PurposeHooshvareId)
            .Throws(new EntityNotFoundException(typeof(HooshvareDefinition), fixture.PurposeHooshvareId));
        fixture.Catalog.GetByKeyAsync(PlatformHooshvareKeys.HelpDeskKbArticleEditor).Returns(seeded);

        var result = await fixture.Resolver.ResolveAsync(fixture.ProjectId);

        result.Id.ShouldBe(fixture.SeededHooshvareId);
    }

    [Fact]
    public async Task Unbound_Project_Without_A_Seeded_Key_Should_Say_Not_Bound()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(new HelpDeskAiBindingResultDto
            {
                Source = HelpDeskAiBindingSource.Missing,
                Readiness = HelpDeskAiBindingReadiness.MissingAssignment
            });
        fixture.Catalog.GetByKeyAsync(PlatformHooshvareKeys.HelpDeskKbArticleEditor)
            .Throws(new BusinessException(AIHooshvareErrorCodes.HooshvareNotFound));

        var exception = await Should.ThrowAsync<BusinessException>(
            () => fixture.Resolver.ResolveAsync(fixture.ProjectId));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleHooshvareNotBound);
        exception.Message.ShouldContain("KnowledgeBase:ArticleHooshvareNotBound");
        exception.Message.ShouldNotContain("HooshvareNotFound");
    }

    [Fact]
    public async Task Bound_Project_Without_A_Resolvable_Definition_Should_Say_Not_Seeded()
    {
        var fixture = Fixture.Create(featureEnabled: true);
        fixture.Workspace.ResolveAsync(
                fixture.ProjectId,
                HelpDeskAiWorkspacePurpose.ContentEditing,
                Arg.Any<CancellationToken>())
            .Returns(Bound(fixture.PurposeHooshvareId));
        fixture.Catalog.GetAsync(fixture.PurposeHooshvareId)
            .Throws(new BusinessException(AIHooshvareErrorCodes.HooshvareNotFound));
        fixture.Catalog.GetByKeyAsync(PlatformHooshvareKeys.HelpDeskKbArticleEditor)
            .Throws(new BusinessException(AIHooshvareErrorCodes.HooshvareNotFound));

        var exception = await Should.ThrowAsync<BusinessException>(
            () => fixture.Resolver.ResolveAsync(fixture.ProjectId));

        exception.Code.ShouldBe(KnowledgeBaseErrorCodes.ArticleHooshvareNotSeeded);
        exception.Data["Key"].ShouldBe(PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        exception.Message.ShouldContain("KnowledgeBase:ArticleHooshvareNotSeeded");
        exception.Message.ShouldContain(PlatformHooshvareKeys.HelpDeskKbArticleEditor);
    }

    [Fact]
    public void Article_Editor_Seed_Should_Require_The_ArticleHooshvare_Feature()
    {
        var contributor = new HelpDeskKbArticleEditorHooshvareDataSeedContributor(
            null!,
            new HooshvareLocalizedShortcutPromptsBuilder(
                Microsoft.Extensions.Options.Options.Create(new AbpLocalizationOptions())),
            null!);

        var snapshot = contributor.GetSnapshots().ShouldHaveSingleItem();

        snapshot.Key.ShouldBe(PlatformHooshvareKeys.HelpDeskKbArticleEditor);
        snapshot.Definition.RequiredFeatureName.ShouldBe(HelpDeskFeatures.Names.KnowledgeBaseArticleHooshvare);
        snapshot.Definition.DefaultEnabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData("en", "Article editor AI is unavailable because the feature {0} is disabled.")]
    [InlineData("en", "The article editor hooshvare {0} is not seeded.")]
    [InlineData("en", "This project has no Content Editing hooshvare bound.")]
    [InlineData("fa", "هوش مصنوعی ویرایشگر مقاله در دسترس نیست چون ویژگی {0} غیرفعال است.")]
    [InlineData("fa", "هوشواره ویرایشگر مقاله {0} بذرگذاری نشده است.")]
    [InlineData("fa", "برای این پروژه هیچ هوشواره‌ای به هدف ویرایش محتوا متصل نشده است.")]
    public void Article_Editor_Failure_Messages_Should_Be_Localized(string culture, string message)
    {
        var root = FindWorkspaceRoot();
        var path = Path.Combine(
            root,
            "pro-modules",
            "helpdesk",
            "src",
            "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Domain.Shared",
            "Localization",
            "KnowledgeBase",
            culture + ".json");
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var texts = document.RootElement.GetProperty("texts");
        var values = texts.EnumerateObject().Select(property => property.Value.GetString()).ToList();

        values.ShouldContain(message);
        values.ShouldNotContain("تعریف هوشواره یافت نشد.");
        values.ShouldNotContain("Hooshvare definition was not found.");
    }

    private static HelpDeskAiBindingResultDto Bound(Guid hooshvareId)
    {
        return new HelpDeskAiBindingResultDto
        {
            Source = HelpDeskAiBindingSource.Project,
            Purpose = HelpDeskAiWorkspacePurpose.ContentEditing,
            HooshvareId = hooshvareId,
            Readiness = HelpDeskAiBindingReadiness.Ready,
            WorkspaceId = Guid.NewGuid(),
            WorkspaceName = "content-editing"
        };
    }

    private static HooshvareCatalogItemDto CatalogItem(Guid id, string key)
    {
        return new HooshvareCatalogItemDto
        {
            Id = id,
            Key = key,
            DisplayName = key,
            WorkspaceId = Guid.NewGuid(),
            WorkspaceName = "content-editing",
            AllowComposerEmoji = true,
            ShortcutPromptsJson = "{\"keys\":[\"rewrite\"]}"
        };
    }

    private static string FindWorkspaceRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory != null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "AGENTS.md")) &&
                Directory.Exists(Path.Combine(directory.FullName, "pro-modules")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not locate the sufi-chain workspace from the test output directory.");
    }

    private sealed class Fixture
    {
        public required Guid ProjectId { get; init; }
        public required Guid PurposeHooshvareId { get; init; }
        public required Guid SeededHooshvareId { get; init; }
        public required IFeatureChecker Features { get; init; }
        public required IHelpDeskAiWorkspaceResolver Workspace { get; init; }
        public required IHooshvareCatalogAppService Catalog { get; init; }
        public required KbArticleEditorHooshvareResolver Resolver { get; init; }

        public static Fixture Create(bool featureEnabled)
        {
            var features = Substitute.For<IFeatureChecker>();
            features.IsEnabledAsync(HelpDeskFeatures.Names.KnowledgeBaseArticleHooshvare)
                .Returns(featureEnabled);
            var workspace = Substitute.For<IHelpDeskAiWorkspaceResolver>();
            var catalog = Substitute.For<IHooshvareCatalogAppService>();
            var localizer = Substitute.For<IStringLocalizer<KnowledgeBaseResource>>();
            localizer[Arg.Any<string>()].Returns(call =>
            {
                var name = call.Arg<string>();
                return new LocalizedString(name, name);
            });
            localizer[Arg.Any<string>(), Arg.Any<object[]>()].Returns(call =>
            {
                var name = call.Arg<string>();
                var args = call.Arg<object[]>();
                return new LocalizedString(name, name + " " + string.Join(' ', args));
            });

            return new Fixture
            {
                ProjectId = Guid.NewGuid(),
                PurposeHooshvareId = Guid.NewGuid(),
                SeededHooshvareId = Guid.NewGuid(),
                Features = features,
                Workspace = workspace,
                Catalog = catalog,
                Resolver = new KbArticleEditorHooshvareResolver(features, workspace, catalog, localizer)
            };
        }
    }
}
