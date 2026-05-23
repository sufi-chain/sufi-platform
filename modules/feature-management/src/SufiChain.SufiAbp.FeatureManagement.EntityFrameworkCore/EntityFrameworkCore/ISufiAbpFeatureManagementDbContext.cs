using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiAbp.FeatureManagement;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiAbp.FeatureManagement.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiAbpFeatureManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpFeatureManagementDbContext : IEfCoreDbContext
{
    DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; }
    DbSet<FeatureDefinitionRecord> Features { get; }
    DbSet<FeatureValue> FeatureValues { get; }
}
