using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.BlobStoring;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DynamicProxy;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;

namespace SufiChain.SufiPlatform.FileManager.Storage;

public class StructureBlobContainerProvider : IStructureBlobContainerProvider, ITransientDependency
{
    private readonly IBlobContainerFactory _blobContainerFactory;
    private readonly StructureBlobContainerConfigurationProvider _configurationProvider;
    private readonly IFileManagerStoragePolicyProvider _storagePolicyProvider;
    private readonly IEnumerable<IBlobProvider> _blobProviders;
    private readonly ICurrentTenant _currentTenant;
    private readonly ICancellationTokenProvider _cancellationTokenProvider;
    private readonly IBlobNormalizeNamingService _blobNormalizeNamingService;
    private readonly IServiceProvider _serviceProvider;

    public StructureBlobContainerProvider(
        IBlobContainerFactory blobContainerFactory,
        StructureBlobContainerConfigurationProvider configurationProvider,
        IFileManagerStoragePolicyProvider storagePolicyProvider,
        IEnumerable<IBlobProvider> blobProviders,
        ICurrentTenant currentTenant,
        ICancellationTokenProvider cancellationTokenProvider,
        IBlobNormalizeNamingService blobNormalizeNamingService,
        IServiceProvider serviceProvider)
    {
        _blobContainerFactory = blobContainerFactory;
        _configurationProvider = configurationProvider;
        _storagePolicyProvider = storagePolicyProvider;
        _blobProviders = blobProviders;
        _currentTenant = currentTenant;
        _cancellationTokenProvider = cancellationTokenProvider;
        _blobNormalizeNamingService = blobNormalizeNamingService;
        _serviceProvider = serviceProvider;
    }

    public Task<IBlobContainer> GetContainerAsync(string? structureKey, CancellationToken cancellationToken = default)
    {
        var containerName = string.IsNullOrEmpty(structureKey)
            ? FileStructureStorageConstants.DefaultContainerName
            : FileStructureStorageConstants.ContainerNamePrefix + structureKey;

        var container = _blobContainerFactory.Create(containerName);
        return Task.FromResult(container);
    }

    public Task<IBlobContainer> GetContainerAsync(
        string? structureKey,
        FileStructureStorageProvider? storageProvider,
        CancellationToken cancellationToken = default)
    {
        return storageProvider.HasValue
            ? Task.FromResult(CreateContainer(
                structureKey,
                storageProvider.Value,
                preferMatchingStructureConfiguration: true))
            : GetContainerAsync(structureKey, cancellationToken);
    }

    public async Task<StructureBlobContainerResult> GetWriteContainerAsync(
        string? structureKey,
        CancellationToken cancellationToken = default)
    {
        var policy = await _storagePolicyProvider.GetAsync(cancellationToken);
        return new StructureBlobContainerResult(
            CreateContainer(
                structureKey,
                policy.Provider,
                preferMatchingStructureConfiguration: false),
            policy.Provider);
    }

    protected virtual IBlobContainer CreateContainer(
        string? structureKey,
        FileStructureStorageProvider storageProvider,
        bool preferMatchingStructureConfiguration)
    {
        var containerName = GetContainerName(structureKey);
        var configuration = _configurationProvider.Get(
            containerName,
            storageProvider,
            preferMatchingStructureConfiguration);
        var provider = GetProvider(configuration, containerName);

        return new BlobContainer(
            containerName,
            configuration,
            provider,
            _currentTenant,
            _cancellationTokenProvider,
            _blobNormalizeNamingService,
            _serviceProvider);
    }

    protected virtual IBlobProvider GetProvider(
        BlobContainerConfiguration configuration,
        string containerName)
    {
        if (configuration.ProviderType == null)
        {
            throw new AbpException($"No BLOB Storage provider is configured for container {containerName}.");
        }

        var provider = _blobProviders.FirstOrDefault(candidate =>
            ProxyHelper.GetUnProxiedType(candidate).IsAssignableTo(configuration.ProviderType));
        if (provider == null)
        {
            throw new AbpException(
                $"Could not find the BLOB Storage provider with type {configuration.ProviderType.AssemblyQualifiedName} for container {containerName}.");
        }

        return provider;
    }

    private static string GetContainerName(string? structureKey) =>
        string.IsNullOrEmpty(structureKey)
            ? FileStructureStorageConstants.DefaultContainerName
            : FileStructureStorageConstants.ContainerNamePrefix + structureKey;
}
