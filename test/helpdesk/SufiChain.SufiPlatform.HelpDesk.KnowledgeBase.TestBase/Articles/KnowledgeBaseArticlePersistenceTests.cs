using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Repositories;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;

public abstract class KnowledgeBaseArticlePersistenceTests<TModule> : AbpIntegratedTest<TModule>
    where TModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) =>
        options.UseAutofac();

    [Fact]
    public async Task Should_Publish_For_Owner_And_Hide_The_Article_From_Another_Tenant()
    {
        var tenantA = Guid.NewGuid();
        var tenantB = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var article = new KBArticle(Guid.NewGuid(), projectId, Guid.NewGuid(), tenantA);
        article.AddTranslation(Guid.NewGuid(), "en", "Title", "title", "Body");

        using (GetRequiredService<ICurrentTenant>().Change(tenantA))
        {
            await SeedProjectAsync(projectId, tenantA);
            await InUnitAsync(repository => repository.InsertAsync(article, autoSave: true));
            await InUnitAsync(async repository =>
            {
                var loaded = await repository.GetAsync(article.Id);
                loaded.Publish(DateTime.UtcNow);
                await repository.UpdateAsync(loaded, autoSave: true);
            });
            await InUnitAsync(async repository =>
            {
                var loaded = await repository.GetAsync(article.Id);
                loaded.Status.ShouldBe(ArticleStatus.Published);
                Should.Throw<ArticleAlreadyPublishedException>(() => loaded.Publish(DateTime.UtcNow));
            });
        }

        using (GetRequiredService<ICurrentTenant>().Change(tenantB))
        {
            await InUnitAsync(async repository =>
                (await repository.FindAsync(article.Id)).ShouldBeNull());
        }

        using (GetRequiredService<IDataFilter>().Disable<IMultiTenant>())
        {
            await InUnitAsync(async repository =>
                (await repository.FindAsync(article.Id)).ShouldNotBeNull());
        }
    }

    /// <summary>
    /// Creates the owning project row for providers that enforce the article's project foreign key.
    /// </summary>
    protected virtual Task SeedProjectAsync(Guid projectId, Guid tenantId) => Task.CompletedTask;

    private async Task InUnitAsync(Func<IKBArticleRepository, Task> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action(GetRequiredService<IKBArticleRepository>());
        await uow.CompleteAsync();
    }
}
