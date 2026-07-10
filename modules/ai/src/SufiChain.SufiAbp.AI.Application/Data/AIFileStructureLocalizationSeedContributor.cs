using System.Threading.Tasks;
using SufiChain.SufiAbp.LocalizationManagement;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.AI.Data;

/// <summary>
/// Seeds AI file structure business localization texts independently of structure entity seeding.
/// </summary>
public class AIFileStructureLocalizationSeedContributor : IDataSeedContributor, ITransientDependency
{
    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public AIFileStructureLocalizationSeedContributor(ILocalizationTextSeeder localizationTextSeeder)
    {
        LocalizationTextSeeder = localizationTextSeeder;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await LocalizationTextSeeder.UpsertStructureTextsAsync(
            AIFileStructureSeedTexts.ResourceName,
            AIFileStructureSeedTexts.StructureKey,
            AIFileStructureSeedTexts.DisplayName,
            AIFileStructureSeedTexts.Description,
            context.TenantId);
    }
}
