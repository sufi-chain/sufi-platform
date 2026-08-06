using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Editions.EntityFrameworkCore;

[ConnectionStringName(EditionsDbProperties.ConnectionStringName)]
public interface IEditionsDbContext : IEfCoreDbContext
{
    DbSet<Edition> Editions { get; }
}
