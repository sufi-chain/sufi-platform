using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shouldly;
using SufiChain.SufiPlatform.Data;
using SufiChain.SufiPlatform.Editions;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiSaas.Data;
using SufiChain.SufiPlatform.SufiSaas.Plans;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.SufiSaas;

public class CalendarAssistantEditionFeatureTests
{
    [Fact]
    public void Every_edition_map_enables_SufiCalendar_Assistant()
    {
        var seed = new Probe(null!, null!, null!, null!, null!, null!, null!, null!, null!);

        seed.Demo()["SufiCalendar.Assistant"].ShouldBe("true");
        seed.Starter()["SufiCalendar.Assistant"].ShouldBe("true");
        seed.Growth()["SufiCalendar.Assistant"].ShouldBe("true");
        seed.Scale()["SufiCalendar.Assistant"].ShouldBe("true");
    }

    private sealed class Probe : SufiSaasPlanDataSeedContributor
    {
        public Probe(
            IEditionRepository editionRepository,
            EditionManager editionManager,
            IPlanRepository planRepository,
            IFeatureManager featureManager,
            IFeatureDefinitionManager featureDefinitionManager,
            IGuidGenerator guidGenerator,
            IConfiguration configuration,
            ILogger<SufiSaasPlanDataSeedContributor> logger,
            ILocalizationTextSeeder localizationTextSeeder)
            : base(
                editionRepository,
                editionManager,
                planRepository,
                featureManager,
                featureDefinitionManager,
                guidGenerator,
                configuration,
                logger,
                localizationTextSeeder)
        {
        }

        public Dictionary<string, string> Demo() => GetDemoFeatures();

        public Dictionary<string, string> Starter() => GetStarterFeatures();

        public Dictionary<string, string> Growth() => GetGrowthFeatures();

        public Dictionary<string, string> Scale() => GetScaleFeatures();
    }
}
