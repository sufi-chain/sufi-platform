using System.Threading.Tasks;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiAI.Data;

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
