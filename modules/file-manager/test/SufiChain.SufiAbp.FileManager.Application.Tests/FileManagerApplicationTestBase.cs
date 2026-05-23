using Volo.Abp.Modularity;

namespace SufiChain.SufiAbp.FileManager;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class FileManagerApplicationTestBase<TStartupModule> : FileManagerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
