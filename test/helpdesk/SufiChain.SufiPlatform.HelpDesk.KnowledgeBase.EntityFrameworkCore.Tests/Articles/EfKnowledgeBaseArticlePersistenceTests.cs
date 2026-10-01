using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.EntityFrameworkCore;
using SufiChain.SufiPlatform.HelpDesk.Projects;
using Volo.Abp.Uow;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;

public class EfKnowledgeBaseArticlePersistenceTests : KnowledgeBaseArticlePersistenceTests<KnowledgeBaseSqliteTestModule>
{
    protected override async Task SeedProjectAsync(Guid projectId, Guid tenantId)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var dbContext = GetRequiredService<KnowledgeBaseDbContext>();
        dbContext.Set<HelpDeskProject>().Add(new HelpDeskProject(projectId, "Project", "project", tenantId));
        await dbContext.SaveChangesAsync();
        await uow.CompleteAsync();
    }
}
