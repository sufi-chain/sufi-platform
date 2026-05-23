using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.LocalizationManagement.Repositories;

public interface ILocalizationResourceRepository : IRepository<LocalizationResource, Guid>
{
    /// <summary>
    /// Finds a resource by name
    /// </summary>
    Task<LocalizationResource?> FindByNameAsync(
        string resourceName,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all enabled resources
    /// </summary>
    Task<List<LocalizationResource>> GetEnabledListAsync(
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all resource names
    /// </summary>
    Task<List<string>> GetResourceNamesAsync(
        bool enabledOnly = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list with optional filtering
    /// </summary>
    Task<List<LocalizationResource>> GetPagedListAsync(
        string? filter = null,
        bool? isEnabled = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count with optional filtering
    /// </summary>
    Task<long> GetCountAsync(
        string? filter = null,
        bool? isEnabled = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a resource name exists
    /// </summary>
    Task<bool> ExistsAsync(
        string resourceName,
        CancellationToken cancellationToken = default);
}
