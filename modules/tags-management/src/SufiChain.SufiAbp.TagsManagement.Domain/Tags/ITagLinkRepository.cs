using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public interface ITagLinkRepository : IRepository<TagLink, Guid>
{
    Task<bool> ExistsAsync(Guid tagId, string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<List<TagLink>> GetListByEntityAsync(string entityType, Guid entityId, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<List<TagLink>> GetListByTagAsync(Guid tagId, Guid? tenantId = null, CancellationToken cancellationToken = default);
}

