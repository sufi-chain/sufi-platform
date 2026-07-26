using Volo.Abp;
using Volo.Abp.Application.Services;

namespace SufiChain.SufiPlatform.FileManager;

/// <summary>
/// Cross-module file storage contract (request/response). Prefer this over in-proc <c>IFileItemAppService</c>.
/// </summary>
[IntegrationService]
public interface IFileStorageIntegrationService : IApplicationService
{
    /// <summary>Uploads a file and returns a portable file reference.</summary>
    Task<FileReferenceDto> UploadAsync(FileUploadRequest input);

    /// <summary>Gets a file reference by id (metadata only).</summary>
    Task<FileReferenceDto> GetAsync(Guid id);

    /// <summary>Reads stored file bytes for authenticated callers (import / conversion).</summary>
    Task<FileContentBytesDto> GetContentAsync(Guid id);

    /// <summary>Issues a short-lived access token for media URL consumption.</summary>
    Task<string> GetAccessTokenAsync(Guid id);

    /// <summary>Deletes a stored file by id.</summary>
    Task DeleteAsync(Guid id);
}
