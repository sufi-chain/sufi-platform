using Microsoft.EntityFrameworkCore;
using SufiChain.SufiPlatform.Localization.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiPlatform.Localization.EntityFrameworkCore;

[ConnectionStringName(SufiLocalizationDbProperties.ConnectionStringName)]
public interface ISufiLocalizationDbContext : IEfCoreDbContext
{
    DbSet<LocalizationText> LocalizationTexts { get; }
    DbSet<LocalizationResource> LocalizationResources { get; }
}
