using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class FileManagerDomainTestBase<TStartupModule> : FileManagerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
