using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

/* Inherit from this class for your domain layer tests.
 * Derive domain scenario tests from this class.
 */
public abstract class FileManagerDomainTestBase<TStartupModule> : FileManagerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
