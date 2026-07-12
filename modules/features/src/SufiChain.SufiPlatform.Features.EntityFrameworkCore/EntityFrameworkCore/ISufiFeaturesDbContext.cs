using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;
using SufiChain.SufiPlatform.Features;
using Volo.Abp.MultiTenancy;

namespace SufiChain.SufiPlatform.Features.EntityFrameworkCore;

[IgnoreMultiTenancy]
[ConnectionStringName(SufiFeaturesDbProperties.ConnectionStringName)]
public interface ISufiFeaturesDbContext : IEfCoreDbContext
{
    DbSet<FeatureGroupDefinitionRecord> FeatureGroups { get; }
    DbSet<FeatureDefinitionRecord> Features { get; }
    DbSet<FeatureValue> FeatureValues { get; }
}
