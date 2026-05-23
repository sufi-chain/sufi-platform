using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using SufiChain.SufiAbp.LocalizationManagement.Entities;
using Volo.Abp.Domain.Repositories;

namespace SufiChain.SufiAbp.LocalizationManagement.Repositories;

public interface ILocalizationTextRepository : IRepository<LocalizationText, Guid>
{
    /// <summary>
    /// Finds a specific localization text by resource, culture, and key
    /// </summary>
    Task<LocalizationText?> FindAsync(
        string resourceName,
        string cultureName,
        string key,
        bool includeDetails = true,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all texts for a resource and culture
    /// </summary>
    Task<List<LocalizationText>> GetListAsync(
        string resourceName,
        string cultureName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all texts for a resource (all cultures)
    /// </summary>
    Task<List<LocalizationText>> GetListByResourceAsync(
        string resourceName,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets paginated list with optional filtering
    /// </summary>
    Task<List<LocalizationText>> GetPagedListAsync(
        string? resourceName = null,
        string? cultureName = null,
        string? keyFilter = null,
        string? valueFilter = null,
        int skipCount = 0,
        int maxResultCount = int.MaxValue,
        string? sorting = null,
        bool includeDetails = false,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets count with optional filtering
    /// </summary>
    Task<long> GetCountAsync(
        string? resourceName = null,
        string? cultureName = null,
        string? keyFilter = null,
        string? valueFilter = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct resource names
    /// </summary>
    Task<List<string>> GetResourceNamesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets distinct culture names for a resource
    /// </summary>
    Task<List<string>> GetCultureNamesAsync(
        string? resourceName = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all texts for a resource
    /// </summary>
    Task DeleteByResourceAsync(
        string resourceName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes all texts for a resource and culture
    /// </summary>
    Task DeleteByResourceAndCultureAsync(
        string resourceName,
        string cultureName,
        CancellationToken cancellationToken = default);
}
