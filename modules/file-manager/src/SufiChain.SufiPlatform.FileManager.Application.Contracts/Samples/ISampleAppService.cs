using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager.Samples;

[RemoteService(Name = FileManagerRemoteServiceConsts.RemoteServiceName)]
public interface ISampleAppService : IApplicationService
{
    Task<SampleDto> GetAsync();

    Task<SampleDto> GetAuthorizedAsync();
}
