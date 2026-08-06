using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiPlatform.Editions;

public interface IEditionRepository : IRepository<Edition, Guid>
{
    Task<Edition?> FindByNameAsync(string name, CancellationToken cancellationToken = default);

    Task<Edition?> FindByCodeAsync(string code, CancellationToken cancellationToken = default);
}
