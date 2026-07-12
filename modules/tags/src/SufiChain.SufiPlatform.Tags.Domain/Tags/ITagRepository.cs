using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Tags.Tags;

public interface ITagRepository : IRepository<Tag, Guid>
{
    Task<Tag?> FindByNameAsync(string scope, string normalizedName, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<List<Tag>> GetListByScopeAsync(string scope, Guid? tenantId = null, CancellationToken cancellationToken = default);
    Task<List<Tag>> SearchAsync(
        string? scope,
        string? filter,
        Guid? tenantId = null,
        int skipCount = 0,
        int maxResultCount = 20,
        CancellationToken cancellationToken = default);
}

