using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class ShortLinkGeneratorDomainTestBase<TStartupModule> : ShortLinkGeneratorTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
