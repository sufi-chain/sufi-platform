using Microsoft.EntityFrameworkCore;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace SufiChain.SufiAbp.LocalizationManagement.EntityFrameworkCore;

[ConnectionStringName(SufiAbpLocalizationManagementDbProperties.ConnectionStringName)]
public interface ISufiAbpLocalizationManagementDbContext : IEfCoreDbContext
{
    DbSet<LocalizationText> LocalizationTexts { get; }
    DbSet<LocalizationResource> LocalizationResources { get; }
}
