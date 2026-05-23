using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SufiChain.SufiAbp.FileManager.Configuration;
using SufiChain.SufiAbp.FileManager.Localization;
using SufiChain.SufiAbp.FileManager.Permissions;
using SufiChain.SufiAbp.FileManager.Storage;
using Volo.Abp;
using SufiChain.SufiAbp.Application.Dtos;
using SufiChain.SufiAbp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.ObjectExtending;

namespace SufiChain.SufiAbp.FileManager.FileStructures;

public class FileStructureAppService : 
    SufiAbpCrudAppService<FileStructure, FileStructureDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateFileStructureDto, CreateUpdateFileStructureDto>,
    IFileStructureAppService
{
    private readonly IFileStructureRepository _structureRepository;
    private readonly FileManagerOptions _options;
    private readonly IGuidGenerator _guidGenerator;
    private readonly IStructureStorageConfigEncryption _storageConfigEncryption;

    public FileStructureAppService(
        IFileStructureRepository structureRepository,
        IOptions<FileManagerOptions> options,
        IGuidGenerator guidGenerator,
        IStructureStorageConfigEncryption storageConfigEncryption)
        : base(structureRepository)
    {
        LocalizationResource = typeof(SufiAbpFileManagerResource);
        _structureRepository = structureRepository;
        _options = options.Value;
        _guidGenerator = guidGenerator;
        _storageConfigEncryption = storageConfigEncryption;
        
        //GetPolicyName = FileManagerPermissions.FileStructures.Default;
        //GetListPolicyName = FileManagerPermissions.FileStructures.Default;
        CreatePolicyName = FileManagerPermissions.FileStructures.Create;
        UpdatePolicyName = FileManagerPermissions.FileStructures.Update;
        DeletePolicyName = FileManagerPermissions.FileStructures.Delete;
    }

    public async Task<FileStructureDto> GetByKeyAsync(string key)
    {
        var query = await _structureRepository.GetQueryableAsync();
        var structure = await AsyncExecuter.FirstOrDefaultAsync(query.Where(x => x.Key == key));

        if (structure == null)
        {
            throw new UserFriendlyException($"File structure with key '{key}' not found");
        }

        var dto = ObjectMapper.Map<FileStructure, FileStructureDto>(structure);
        EnrichStorageConfig(dto, structure);
        EnrichWithDefaultInfo(dto);
        return dto;
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var query = await _structureRepository.GetQueryableAsync();
        return await AsyncExecuter.AnyAsync(query.Where(x => x.Key == key));
    }

    public override async Task<FileStructureDto> CreateAsync(CreateUpdateFileStructureDto input)
    {
        if (string.IsNullOrWhiteSpace(input.Key))
        {
            throw new UserFriendlyException(L["KeyRequired"]);
        }

        if (string.IsNullOrWhiteSpace(input.DisplayName))
        {
            throw new UserFriendlyException(L["DisplayNameRequired"]);
        }

        // Check if key already exists
        if (await ExistsAsync(input.Key))
        {
            throw new UserFriendlyException($"File structure with key '{input.Key}' already exists");
        }

        await CheckCreatePolicyAsync();
        var entity = await MapToEntityAsync(input);
        ApplyStorageConfigToEntity(entity, input.StorageConfig);
        await Repository.InsertAsync(entity);
        var result = ObjectMapper.Map<FileStructure, FileStructureDto>(entity);
        EnrichStorageConfig(result, entity);
        EnrichWithDefaultInfo(result);
        return result;
    }

    public override async Task<FileStructureDto> GetAsync(Guid id)
    {
        var entity = await _structureRepository.GetAsync(id);
        var dto = ObjectMapper.Map<FileStructure, FileStructureDto>(entity);
        EnrichStorageConfig(dto, entity);
        EnrichWithDefaultInfo(dto);
        return dto;
    }

    public override async Task<SufiChain.SufiAbp.Application.Dtos.PagedResultDto<FileStructureDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var baseResult = await base.GetListAsync(input);
        if (baseResult.Items.Count > 0)
        {
            var ids = baseResult.Items.Select(x => x.Id).ToList();
            var query = await _structureRepository.GetQueryableAsync();
            var entities = await AsyncExecuter.ToListAsync(query.Where(x => ids.Contains(x.Id)));
            foreach (var item in baseResult.Items)
            {
                var entity = entities.FirstOrDefault(e => e.Id == item.Id);
                if (entity != null)
                {
                    EnrichStorageConfig(item, entity);
                }
                EnrichWithDefaultInfo(item);
            }
        }
        
        return new SufiChain.SufiAbp.Application.Dtos.PagedResultDto<FileStructureDto>(baseResult.TotalCount, baseResult.Items);
    }

    public override async Task<FileStructureDto> UpdateAsync(Guid id, CreateUpdateFileStructureDto input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await _structureRepository.GetAsync(id);
        await MapToEntityAsync(input, entity);
        ApplyStorageConfigToEntity(entity, input.StorageConfig);
        await Repository.UpdateAsync(entity);
        var result = ObjectMapper.Map<FileStructure, FileStructureDto>(entity);
        EnrichStorageConfig(result, entity);
        EnrichWithDefaultInfo(result);
        return result;
    }

    public Task<FileStructureDefaultDto?> GetDefaultConfigAsync(string key)
    {
        var config = _options.Structures.FirstOrDefault(x => x.Key == key);
        if (config == null)
        {
            return Task.FromResult<FileStructureDefaultDto?>(null);
        }

        return Task.FromResult<FileStructureDefaultDto?>(MapConfigToDefaultDto(config));
    }

    public Task<List<FileStructureDefaultDto>> GetAllDefaultConfigsAsync()
    {
        var defaults = _options.Structures
            .Select(MapConfigToDefaultDto)
            .ToList();
        
        return Task.FromResult(defaults);
    }

    public async Task<FileStructureDto> ResetToDefaultAsync(Guid id)
    {
        var structure = await _structureRepository.GetAsync(id);
        var defaultConfig = _options.Structures.FirstOrDefault(x => x.Key == structure.Key);

        if (defaultConfig == null)
        {
            throw new UserFriendlyException($"No default configuration found for structure '{structure.Key}'. This structure was created manually and cannot be reset to default.");
        }

        // Reset all properties to default values
        structure.DisplayName = defaultConfig.DisplayName;
        structure.Description = defaultConfig.Description;
        structure.AllowedFileTypes = defaultConfig.AllowedFileTypes;
        structure.AllowedExtensions = defaultConfig.AllowedExtensions;
        structure.AllowedMimeTypes = defaultConfig.AllowedMimeTypes;
        structure.MaxFileSize = defaultConfig.MaxFileSize;
        structure.MinImageWidth = defaultConfig.MinImageWidth;
        structure.MinImageHeight = defaultConfig.MinImageHeight;
        structure.MaxImageWidth = defaultConfig.MaxImageWidth;
        structure.MaxImageHeight = defaultConfig.MaxImageHeight;
        structure.IsMultiple = defaultConfig.IsMultiple;
        structure.MaxCount = defaultConfig.MaxCount;
        structure.IsRequired = defaultConfig.IsRequired;
        structure.GenerateThumbnail = defaultConfig.GenerateThumbnail;
        structure.ThumbnailWidth = defaultConfig.ThumbnailWidth;
        structure.ThumbnailHeight = defaultConfig.ThumbnailHeight;
        structure.EnableWebPConversion = defaultConfig.EnableWebPConversion;
        structure.WebPQuality = defaultConfig.WebPQuality;
        structure.StorageProvider = defaultConfig.StorageProvider;
        structure.IsPublicAccess = defaultConfig.IsPublicAccess;
        structure.BaseUrl = defaultConfig.BaseUrl;
        structure.ResizeLargeImages = defaultConfig.ResizeLargeImages;

        await _structureRepository.UpdateAsync(structure);

        var dto = ObjectMapper.Map<FileStructure, FileStructureDto>(structure);
        EnrichWithDefaultInfo(dto);
        return dto;
    }

    public async Task<bool> IsModifiedFromDefaultAsync(Guid id)
    {
        var structure = await _structureRepository.GetAsync(id);
        var defaultConfig = _options.Structures.FirstOrDefault(x => x.Key == structure.Key);

        if (defaultConfig == null)
        {
            return false; // No default config means it's a custom structure
        }

        return IsModifiedFromDefault(structure, defaultConfig);
    }

    public async Task<int> SeedDefaultStructuresAsync()
    {
        // Add default structures to options if enabled
        if (_options.SeedDefaultStructures)
        {
            _options.AddDefaultStructures();
        }

        if (!_options.Structures.Any())
        {
            Logger.LogInformation("No file structures configured to seed.");
            return 0;
        }

        var seededCount = 0;

        foreach (var config in _options.Structures)
        {
            var existing = await _structureRepository.FindByKeyAsync(config.Key);
            
            if (existing == null)
            {
                var entity = new FileStructure(
                    _guidGenerator.Create(),
                    config.Key,
                    config.DisplayName,
                    config.AllowedFileTypes,
                    config.AllowedExtensions,
                    config.AllowedMimeTypes,
                    config.MaxFileSize)
                {
                    Description = config.Description,
                    MinImageWidth = config.MinImageWidth,
                    MinImageHeight = config.MinImageHeight,
                    MaxImageWidth = config.MaxImageWidth,
                    MaxImageHeight = config.MaxImageHeight,
                    IsMultiple = config.IsMultiple,
                    MaxCount = config.MaxCount,
                    IsRequired = config.IsRequired,
                    GenerateThumbnail = config.GenerateThumbnail,
                    ThumbnailWidth = config.ThumbnailWidth,
                    ThumbnailHeight = config.ThumbnailHeight,
                    EnableWebPConversion = config.EnableWebPConversion,
                    WebPQuality = config.WebPQuality,
                    StorageProvider = config.StorageProvider,
                    IsPublicAccess = config.IsPublicAccess,
                    BaseUrl = config.BaseUrl,
                    ResizeLargeImages = config.ResizeLargeImages
                };

                await _structureRepository.InsertAsync(entity);
                Logger.LogInformation("Seeded file structure '{StructureKey}' with ID {Id}.", config.Key, entity.Id);
                seededCount++;
            }
            else
            {
                Logger.LogDebug("File structure '{StructureKey}' already exists.", config.Key);
            }
        }

        return seededCount;
    }

    private void EnrichWithDefaultInfo(FileStructureDto dto)
    {
        var defaultConfig = _options.Structures.FirstOrDefault(x => x.Key == dto.Key);
        dto.HasDefaultConfig = defaultConfig != null;
        
        if (defaultConfig != null)
        {
            dto.IsModifiedFromDefault = IsModifiedFromDefaultDto(dto, defaultConfig);
        }
    }

    private static bool IsModifiedFromDefault(FileStructure structure, FileStructureConfig defaultConfig)
    {
        return structure.DisplayName != defaultConfig.DisplayName ||
               structure.Description != defaultConfig.Description ||
               structure.AllowedFileTypes != defaultConfig.AllowedFileTypes ||
               structure.AllowedExtensions != defaultConfig.AllowedExtensions ||
               structure.AllowedMimeTypes != defaultConfig.AllowedMimeTypes ||
               structure.MaxFileSize != defaultConfig.MaxFileSize ||
               structure.MinImageWidth != defaultConfig.MinImageWidth ||
               structure.MinImageHeight != defaultConfig.MinImageHeight ||
               structure.MaxImageWidth != defaultConfig.MaxImageWidth ||
               structure.MaxImageHeight != defaultConfig.MaxImageHeight ||
               structure.IsMultiple != defaultConfig.IsMultiple ||
               structure.MaxCount != defaultConfig.MaxCount ||
               structure.IsRequired != defaultConfig.IsRequired ||
               structure.GenerateThumbnail != defaultConfig.GenerateThumbnail ||
               structure.ThumbnailWidth != defaultConfig.ThumbnailWidth ||
               structure.ThumbnailHeight != defaultConfig.ThumbnailHeight ||
               structure.EnableWebPConversion != defaultConfig.EnableWebPConversion ||
               structure.WebPQuality != defaultConfig.WebPQuality ||
               structure.StorageProvider != defaultConfig.StorageProvider ||
               structure.IsPublicAccess != defaultConfig.IsPublicAccess ||
               structure.BaseUrl != defaultConfig.BaseUrl ||
               structure.ResizeLargeImages != defaultConfig.ResizeLargeImages;
    }

    private static bool IsModifiedFromDefaultDto(FileStructureDto dto, FileStructureConfig defaultConfig)
    {
        return dto.DisplayName != defaultConfig.DisplayName ||
               dto.Description != defaultConfig.Description ||
               dto.AllowedFileTypes != defaultConfig.AllowedFileTypes ||
               dto.AllowedExtensions != defaultConfig.AllowedExtensions ||
               dto.AllowedMimeTypes != defaultConfig.AllowedMimeTypes ||
               dto.MaxFileSize != defaultConfig.MaxFileSize ||
               dto.MinImageWidth != defaultConfig.MinImageWidth ||
               dto.MinImageHeight != defaultConfig.MinImageHeight ||
               dto.MaxImageWidth != defaultConfig.MaxImageWidth ||
               dto.MaxImageHeight != defaultConfig.MaxImageHeight ||
               dto.IsMultiple != defaultConfig.IsMultiple ||
               dto.MaxCount != defaultConfig.MaxCount ||
               dto.IsRequired != defaultConfig.IsRequired ||
               dto.GenerateThumbnail != defaultConfig.GenerateThumbnail ||
               dto.ThumbnailWidth != defaultConfig.ThumbnailWidth ||
               dto.ThumbnailHeight != defaultConfig.ThumbnailHeight ||
               dto.EnableWebPConversion != defaultConfig.EnableWebPConversion ||
               dto.WebPQuality != defaultConfig.WebPQuality ||
               dto.StorageProvider != defaultConfig.StorageProvider ||
               dto.IsPublicAccess != defaultConfig.IsPublicAccess ||
               dto.BaseUrl != defaultConfig.BaseUrl ||
               dto.ResizeLargeImages != defaultConfig.ResizeLargeImages;
    }

    private void ApplyStorageConfigToEntity(FileStructure entity, FileStructureStorageConfigDto? config)
    {
        if (config == null)
        {
            return;
        }

        entity.SetProperty(FileStructureStorageConstants.Provider, config.StorageProvider.ToString());

        switch (config.StorageProvider)
        {
            case FileStructureStorageProvider.Database:
                if (!string.IsNullOrWhiteSpace(config.DatabaseConnectionString))
                {
                    entity.SetProperty(FileStructureStorageConstants.DatabaseConnectionString,
                        _storageConfigEncryption.EncryptSensitiveValue(config.DatabaseConnectionString));
                }
                else if (!config.HasDatabaseConnectionString)
                {
                    entity.RemoveProperty(FileStructureStorageConstants.DatabaseConnectionString);
                }
                break;
            case FileStructureStorageProvider.FileSystem:
                entity.SetProperty(FileStructureStorageConstants.FileSystemBasePath, config.FileSystemBasePath ?? "");
                break;
            case FileStructureStorageProvider.MinIO:
                entity.SetProperty(FileStructureStorageConstants.MinioEndPoint, config.MinioEndPoint ?? "");
                if (!string.IsNullOrWhiteSpace(config.MinioAccessKey))
                {
                    entity.SetProperty(FileStructureStorageConstants.MinioAccessKey,
                        _storageConfigEncryption.EncryptSensitiveValue(config.MinioAccessKey));
                }
                else if (!config.HasMinioAccessKey)
                {
                    entity.RemoveProperty(FileStructureStorageConstants.MinioAccessKey);
                }
                if (!string.IsNullOrWhiteSpace(config.MinioSecretKey))
                {
                    entity.SetProperty(FileStructureStorageConstants.MinioSecretKey,
                        _storageConfigEncryption.EncryptSensitiveValue(config.MinioSecretKey));
                }
                else if (!config.HasMinioSecretKey)
                {
                    entity.RemoveProperty(FileStructureStorageConstants.MinioSecretKey);
                }
                entity.SetProperty(FileStructureStorageConstants.MinioBucketName, config.MinioBucketName ?? "");
                break;
            case FileStructureStorageProvider.S3Provider:
                entity.SetProperty(FileStructureStorageConstants.S3Endpoint, config.S3EndPoint ?? "");
                entity.SetProperty(FileStructureStorageConstants.S3Region, config.S3Region ?? "us-east-1");
                entity.SetProperty(FileStructureStorageConstants.S3ContainerName, config.S3ContainerName ?? "");
                if (!string.IsNullOrWhiteSpace(config.S3AccessKeyId))
                {
                    entity.SetProperty(FileStructureStorageConstants.S3AccessKeyId,
                        _storageConfigEncryption.EncryptSensitiveValue(config.S3AccessKeyId));
                }
                else if (!config.HasS3AccessKey)
                {
                    entity.RemoveProperty(FileStructureStorageConstants.S3AccessKeyId);
                }
                if (!string.IsNullOrWhiteSpace(config.S3SecretAccessKey))
                {
                    entity.SetProperty(FileStructureStorageConstants.S3SecretAccessKey,
                        _storageConfigEncryption.EncryptSensitiveValue(config.S3SecretAccessKey));
                }
                else if (!config.HasS3SecretKey)
                {
                    entity.RemoveProperty(FileStructureStorageConstants.S3SecretAccessKey);
                }
                break;
        }
    }

    private void EnrichStorageConfig(FileStructureDto dto, FileStructure entity)
    {
        var providerStr = entity.GetProperty<string?>(FileStructureStorageConstants.Provider);
        if (string.IsNullOrEmpty(providerStr) || !Enum.TryParse<FileStructureStorageProvider>(providerStr, out var provider))
        {
        dto.StorageConfig = new FileStructureStorageConfigDto
        {
            StorageProvider = FileStructureStorageProvider.Database,
            HasDatabaseConnectionString = false,
            HasMinioAccessKey = false,
            HasMinioSecretKey = false,
            HasS3AccessKey = false,
            HasS3SecretKey = false
        };
            return;
        }

        dto.StorageConfig = new FileStructureStorageConfigDto
        {
            StorageProvider = provider,
            HasDatabaseConnectionString = entity.HasProperty(FileStructureStorageConstants.DatabaseConnectionString) &&
                !string.IsNullOrEmpty(entity.GetProperty<string?>(FileStructureStorageConstants.DatabaseConnectionString)),
            FileSystemBasePath = provider == FileStructureStorageProvider.FileSystem
                ? entity.GetProperty<string?>(FileStructureStorageConstants.FileSystemBasePath)
                : null,
            MinioEndPoint = provider == FileStructureStorageProvider.MinIO
                ? entity.GetProperty<string?>(FileStructureStorageConstants.MinioEndPoint)
                : null,
            MinioBucketName = provider == FileStructureStorageProvider.MinIO
                ? entity.GetProperty<string?>(FileStructureStorageConstants.MinioBucketName)
                : null,
            HasMinioAccessKey = provider == FileStructureStorageProvider.MinIO &&
                entity.HasProperty(FileStructureStorageConstants.MinioAccessKey) &&
                !string.IsNullOrEmpty(entity.GetProperty<string?>(FileStructureStorageConstants.MinioAccessKey)),
            HasMinioSecretKey = provider == FileStructureStorageProvider.MinIO &&
                entity.HasProperty(FileStructureStorageConstants.MinioSecretKey) &&
                !string.IsNullOrEmpty(entity.GetProperty<string?>(FileStructureStorageConstants.MinioSecretKey)),
            S3EndPoint = provider == FileStructureStorageProvider.S3Provider
                ? entity.GetProperty<string?>(FileStructureStorageConstants.S3Endpoint)
                : null,
            S3Region = provider == FileStructureStorageProvider.S3Provider
                ? entity.GetProperty<string?>(FileStructureStorageConstants.S3Region)
                : null,
            S3ContainerName = provider == FileStructureStorageProvider.S3Provider
                ? entity.GetProperty<string?>(FileStructureStorageConstants.S3ContainerName)
                : null,
            HasS3AccessKey = provider == FileStructureStorageProvider.S3Provider &&
                entity.HasProperty(FileStructureStorageConstants.S3AccessKeyId) &&
                !string.IsNullOrEmpty(entity.GetProperty<string?>(FileStructureStorageConstants.S3AccessKeyId)),
            HasS3SecretKey = provider == FileStructureStorageProvider.S3Provider &&
                entity.HasProperty(FileStructureStorageConstants.S3SecretAccessKey) &&
                !string.IsNullOrEmpty(entity.GetProperty<string?>(FileStructureStorageConstants.S3SecretAccessKey))
        };
    }

    private static FileStructureDefaultDto MapConfigToDefaultDto(FileStructureConfig config)
    {
        return new FileStructureDefaultDto
        {
            Key = config.Key,
            DisplayName = config.DisplayName,
            Description = config.Description,
            AllowedFileTypes = config.AllowedFileTypes,
            AllowedExtensions = config.AllowedExtensions,
            AllowedMimeTypes = config.AllowedMimeTypes,
            MaxFileSize = config.MaxFileSize,
            MinImageWidth = config.MinImageWidth,
            MinImageHeight = config.MinImageHeight,
            MaxImageWidth = config.MaxImageWidth,
            MaxImageHeight = config.MaxImageHeight,
            IsMultiple = config.IsMultiple,
            MaxCount = config.MaxCount,
            IsRequired = config.IsRequired,
            GenerateThumbnail = config.GenerateThumbnail,
            ThumbnailWidth = config.ThumbnailWidth,
            ThumbnailHeight = config.ThumbnailHeight,
            EnableWebPConversion = config.EnableWebPConversion,
            WebPQuality = config.WebPQuality,
            StorageProvider = config.StorageProvider,
            IsPublicAccess = config.IsPublicAccess,
            BaseUrl = config.BaseUrl,
            ResizeLargeImages = config.ResizeLargeImages
        };
    }
}
