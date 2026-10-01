using Volo.Abp.Modularity;

namespace SufiChain.SufiPlatform.FileManager;

/* Inherit from this class for your application layer tests.
 * Derive application scenario tests from this class.
 */
public abstract class FileManagerApplicationTestBase<TStartupModule> : FileManagerTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
