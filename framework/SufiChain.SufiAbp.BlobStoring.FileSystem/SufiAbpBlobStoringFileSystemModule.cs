using Volo.Abp.Modularity;
using Volo.Abp.BlobStoring.FileSystem;

namespace SufiChain.SufiAbp.BlobStoring.FileSystem;

[DependsOn(typeof(AbpBlobStoringFileSystemModule))]
public class SufiAbpBlobStoringFileSystemModule : AbpModule
{
}
