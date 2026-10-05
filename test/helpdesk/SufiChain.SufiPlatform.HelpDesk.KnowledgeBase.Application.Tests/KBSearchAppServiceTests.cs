using System.Reflection;
using Microsoft.Extensions.Caching.Distributed;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Localization;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Search;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Timing;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBSearchAppServiceTests
{
    [Fact]
    public void Keyword_Search_Does_Not_Depend_On_Embeddings()
    {
        var parameters = typeof(KBSearchAppService).GetConstructors(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
            .SelectMany(constructor => constructor.GetParameters())
            .Select(parameter => parameter.ParameterType.Name);

        parameters.ShouldNotContain(name =>
            name.Contains("Rag", StringComparison.Ordinal) ||
            name.Contains("Embedding", StringComparison.Ordinal));
    }

    [Fact]
    public async Task Should_Find_Poshtibani_When_Query_And_Article_Use_Different_Yeh()
    {
        var projectId = Guid.NewGuid();
        var article = Published(projectId, "fa", "پشتيباني", "poshtibani", "راهنمای پشتيباني کاربران");
        var fixture = Fixture.Create(article);

        var result = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "fa", "پشتیبانی"));

        result.TotalCount.ShouldBe(1);
        result.Items.Single().Title.ShouldBe("پشتيباني");
        result.Items.Single().LanguageCode.ShouldBe("fa");
        article.IsIndexed.ShouldBeFalse();
        await fixture.Articles.Received(1).GetListWithTranslationsAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Should_Find_Article_When_Stored_Text_Has_Zwnj_Diacritics_Or_Arabic_Kaf()
    {
        var projectId = Guid.NewGuid();
        var article = Published(
            projectId,
            "fa",
            "پُشتیبانی",
            "support",
            "می\u200Cتوانید با پشتیبانی تماس بگیرید. كتاب راهنما.");
        var fixture = Fixture.Create(article);

        var byDiacritic = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "fa", "پشتیبانی"));
        var byHalfSpace = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "fa", "میتوانید"));
        var byKaf = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "fa", "کتاب"));

        byDiacritic.TotalCount.ShouldBe(1);
        byHalfSpace.TotalCount.ShouldBe(1);
        byKaf.TotalCount.ShouldBe(1);
        byKaf.Items.Single().Snippet.ShouldNotBeNull();
    }

    [Fact]
    public async Task Should_Keep_Other_Languages_Drafts_And_Private_Articles_Out_Of_Public_Search()
    {
        var projectId = Guid.NewGuid();
        var persian = Published(projectId, "fa", "پشتیبانی", "poshtibani", "متن پشتیبانی");
        var english = Published(projectId, "en", "Support", "support", "Contact support");
        var draft = new KBArticle(Guid.NewGuid(), projectId, Guid.NewGuid());
        draft.AddTranslation(Guid.NewGuid(), "fa", "پشتیبانی پیش‌نویس", "draft", "پشتیبانی");
        var privateArticle = Published(projectId, "fa", "پشتیبانی خصوصی", "private", "پشتیبانی");
        privateArticle.SetVisibility(ArticleVisibility.Private);
        var fixture = Fixture.Create(persian, english, draft, privateArticle);

        var persianHits = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "fa", "پشتیبانی"));
        var englishHits = await fixture.Service.SearchWithFacetsAsync(Query(projectId, "en", "پشتیبانی"));

        persianHits.Items.Select(item => item.ArticleId).ShouldBe(new[] { persian.Id });
        englishHits.TotalCount.ShouldBe(0);
    }

    [Theory]
    [InlineData("en.json", "No articles in this language.")]
    [InlineData("fa.json", "مقاله‌ای به این زبان وجود ندارد.")]
    [InlineData("ar.json", "لا توجد مقالات بهذه اللغة.")]
    [InlineData("es.json", "No hay artículos en este idioma.")]
    public void Empty_Language_Hint_Is_Localized(string fileName, string text)
    {
        var assembly = typeof(KnowledgeBaseResource).Assembly;
        var resourceName = assembly.GetManifestResourceNames()
            .Single(name => name.EndsWith("Localization.KnowledgeBase." + fileName, StringComparison.Ordinal));
        using var stream = assembly.GetManifestResourceStream(resourceName);
        using var reader = new StreamReader(stream!);
        var json = reader.ReadToEnd();

        json.ShouldContain("\"KnowledgeBase:NoArticlesInLanguage\": \"" + text + "\"");
    }

    private static KBSearchInput Query(Guid projectId, string languageCode, string query)
    {
        return new KBSearchInput
        {
            ProjectId = projectId,
            LanguageCode = languageCode,
            Query = query,
            PublicOnly = true,
            IncludeFacets = false,
            MaxResultCount = 20
        };
    }

    private static KBArticle Published(Guid projectId, string languageCode, string title, string slug, string content)
    {
        var article = new KBArticle(Guid.NewGuid(), projectId, Guid.NewGuid());
        article.AddTranslation(Guid.NewGuid(), languageCode, title, slug, content);
        article.Publish(DateTime.UtcNow);
        return article;
    }

    private sealed class Fixture
    {
        public IKBArticleRepository Articles { get; init; } = default!;
        public KBSearchAppService Service { get; init; } = default!;

        public static Fixture Create(params KBArticle[] articles)
        {
            var repository = Substitute.For<IKBArticleRepository>();
            repository.GetListWithTranslationsAsync(Arg.Any<CancellationToken>())
                .Returns(articles.ToList());

            var projects = Substitute.For<IHelpDeskProjectRepository>();
            var projectList = articles
                .GroupBy(article => article.ProjectId)
                .Select(group =>
                {
                    var project = new HelpDeskProject(group.Key, "Knowledge base", "kb-" + group.Key.ToString("N")[..8]);
                    project.SetIsKnowledgeBasePublic(true);
                    var languages = group
                        .SelectMany(article => article.Translations)
                        .Select(translation => translation.LanguageCode)
                        .Distinct(StringComparer.OrdinalIgnoreCase)
                        .Select(code => new HelpDeskProjectLanguageDto
                        {
                            LanguageCode = code,
                            DisplayName = code
                        })
                        .ToList();
                    if (languages.Count > 0)
                    {
                        project.SetLanguages(languages);
                        project.SetDefaultLanguage(languages[0].LanguageCode);
                    }

                    return project;
                })
                .ToList();
            projects.GetListAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>()).Returns(projectList);

            return new Fixture
            {
                Articles = repository,
                Service = new TestableKBSearchAppService(repository, projects, new EmptyDistributedCache())
            };
        }
    }

    private sealed class TestableKBSearchAppService : KBSearchAppService
    {
        public TestableKBSearchAppService(
            IKBArticleRepository articleRepository,
            IHelpDeskProjectRepository projectRepository,
            IDistributedCache distributedCache)
            : base(articleRepository, projectRepository, distributedCache)
        {
            var tenant = Substitute.For<ICurrentTenant>();
            tenant.Id.Returns((Guid?)null);

            var clock = Substitute.For<IClock>();
            clock.Now.Returns(new DateTime(2026, 10, 4, 12, 0, 0, DateTimeKind.Utc));

            var lazy = Substitute.For<IAbpLazyServiceProvider>();
            lazy.LazyGetRequiredService<ICurrentTenant>().Returns(tenant);
            lazy.LazyGetRequiredService<IClock>().Returns(clock);
            LazyServiceProvider = lazy;
        }
    }

    private sealed class EmptyDistributedCache : IDistributedCache
    {
        private readonly Dictionary<string, byte[]> _values = new(StringComparer.Ordinal);

        public byte[]? Get(string key) => _values.TryGetValue(key, out var value) ? value : null;

        public Task<byte[]?> GetAsync(string key, CancellationToken token = default) => Task.FromResult(Get(key));

        public void Refresh(string key)
        {
        }

        public Task RefreshAsync(string key, CancellationToken token = default) => Task.CompletedTask;

        public void Remove(string key) => _values.Remove(key);

        public Task RemoveAsync(string key, CancellationToken token = default)
        {
            Remove(key);
            return Task.CompletedTask;
        }

        public void Set(string key, byte[] value, DistributedCacheEntryOptions options) => _values[key] = value;

        public Task SetAsync(string key, byte[] value, DistributedCacheEntryOptions options, CancellationToken token = default)
        {
            Set(key, value, options);
            return Task.CompletedTask;
        }
    }
}
