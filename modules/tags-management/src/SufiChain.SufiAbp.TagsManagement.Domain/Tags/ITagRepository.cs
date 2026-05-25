using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.TagsManagement.Tags;

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<Tag?> FindByNameAsync(string scope, string normalizedName, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<List<Tag>> GetListByScopeAsync(string scope, Guid? tenantId = null, CancellationToken cancellationToken = default);
}

