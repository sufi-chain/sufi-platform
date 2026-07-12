using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Localization;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Calendar.Data;

public class CalendarMcpToolLocalizationSeedContributor : IDataSeedContributor, ITransientDependency
{
    private const string ResourceName = "Calendar";

    protected ILocalizationTextSeeder LocalizationTextSeeder { get; }

    public CalendarMcpToolLocalizationSeedContributor(ILocalizationTextSeeder localizationTextSeeder)
    {
        LocalizationTextSeeder = localizationTextSeeder;
    }

    public virtual async Task SeedAsync(DataSeedContext context)
    {
        foreach (var toolName in CalendarMcpToolSeedTexts.ToolNames)
        {
            var texts = CalendarMcpToolSeedTexts.Get(toolName);

            await LocalizationTextSeeder.UpsertAsync(
                context,
                ResourceName,
                BusinessLocalizationKeys.McpToolDisplayName(toolName),
                texts.DisplayNames);

            await LocalizationTextSeeder.UpsertAsync(
                context,
                ResourceName,
                BusinessLocalizationKeys.McpToolDescription(toolName),
                texts.Descriptions);
        }
    }
}
