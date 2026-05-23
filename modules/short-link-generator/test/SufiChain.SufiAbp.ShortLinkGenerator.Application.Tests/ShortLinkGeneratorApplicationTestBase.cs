using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.ShortLinkGenerator;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class ShortLinkGeneratorApplicationTestBase<TStartupModule> : ShortLinkGeneratorTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
