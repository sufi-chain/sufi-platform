using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.FileManager.Configuration;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Timing;

namespace SufiChain.SufiPlatform.FileManager.FileItems;

/// <summary>Creates, resolves and revokes Quick Share links (sufi-chain#436).</summary>
public class FileShareLinkService : ITransientDependency
{
    private readonly IFileItemRepository _fileItemRepository;
    private readonly FileItemBlobAccessService _blobAccessService;
    private readonly IDataFilter _dataFilter;
    private readonly ICurrentTenant _currentTenant;
    private readonly IClock _clock;
    private readonly FileManagerOptions _options;
    private readonly AbpStringEncryptionOptions _encryptionOptions;

    public FileShareLinkService(
        IFileItemRepository fileItemRepository,
        FileItemBlobAccessService blobAccessService,
        IDataFilter dataFilter,
        ICurrentTenant currentTenant,
        IClock clock,
        IOptions<FileManagerOptions> options,
        IOptions<AbpStringEncryptionOptions> encryptionOptions)
    {
        _fileItemRepository = fileItemRepository;
        _blobAccessService = blobAccessService;
        _dataFilter = dataFilter;
        _currentTenant = currentTenant;
        _clock = clock;
        _options = options.Value;
        _encryptionOptions = encryptionOptions.Value;
    }

    public virtual string CreateUrl(FileItem fileItem, int durationMinutes)
    {
        var minutes = Math.Clamp(durationMinutes, 1, 10080);
        var token = FileShareTokens.Create(GetSecret(), new FileShareTokens.Payload(
            fileItem.Id,
            fileItem.TenantId,
            DateTimeOffset.UtcNow.AddMinutes(minutes),
            GetVersion(fileItem)));
        var baseUrl = (_options.BaseUrl ?? "/").TrimEnd('/');
        return $"{baseUrl}/s/{token}";
    }

    public virtual async Task RevokeAsync(Guid fileId)
    {
        var fileItem = await _fileItemRepository.GetAsync(fileId);
        fileItem.SetProperty(FileShareTokens.VersionPropertyName, GetVersion(fileItem) + 1);
        await _fileItemRepository.UpdateAsync(fileItem, autoSave: true);
    }

    public virtual async Task<FileShareContentResultDto> ResolveAsync(string? token)
    {
        var status = FileShareTokens.Validate(GetSecret(), token, DateTimeOffset.UtcNow, out var payload);
        if (status != FileShareLinkStatus.Valid)
        {
            return new FileShareContentResultDto { Status = status };
        }

        FileItem? fileItem;
        using (_dataFilter.Disable<IMultiTenant>())
        {
            fileItem = await _fileItemRepository.FindAsync(payload.FileId);
        }

        if (fileItem == null)
        {
            return new FileShareContentResultDto { Status = FileShareLinkStatus.Invalid };
        }

        status = FileShareTokens.CheckAgainstFile(payload, fileItem.TenantId, GetVersion(fileItem));
        if (status != FileShareLinkStatus.Valid)
        {
            return new FileShareContentResultDto { Status = status };
        }

        using (_currentTenant.Change(fileItem.TenantId))
        {
            var content = await _blobAccessService.GetContentForIntegrationAsync(fileItem.Id);
            return new FileShareContentResultDto
            {
                Status = content.Content == null ? FileShareLinkStatus.Invalid : FileShareLinkStatus.Valid,
                Content = content.Content
            };
        }
    }

    protected virtual int GetVersion(FileItem fileItem) =>
        fileItem.GetProperty<int?>(FileShareTokens.VersionPropertyName) ?? 0;

    protected virtual string GetSecret()
    {
        var secret = !string.IsNullOrWhiteSpace(_options.FileAccessTokenSecret)
            ? _options.FileAccessTokenSecret
            : _encryptionOptions.DefaultPassPhrase;
        if (string.IsNullOrWhiteSpace(secret))
        {
            throw new UserFriendlyException("Quick Share is not configured.");
        }

        return secret.Trim();
    }
}