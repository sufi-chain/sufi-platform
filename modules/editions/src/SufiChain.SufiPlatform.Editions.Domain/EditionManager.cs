using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiPlatform.Editions;

public class EditionManager : DomainService
{
    protected IEditionRepository EditionRepository { get; }

    public EditionManager(IEditionRepository editionRepository)
    {
        EditionRepository = editionRepository;
    }

    public virtual async Task<Edition> CreateAsync(string name, string displayName, string code, bool isActive = true)
    {
        await CheckNameUniqueAsync(name);
        await CheckCodeUniqueAsync(code);

        return new Edition(GuidGenerator.Create(), name, displayName, code, isActive);
    }

    public virtual async Task ChangeNameAsync(Edition edition, string name)
    {
        if (string.Equals(edition.Name, name, StringComparison.Ordinal))
        {
            return;
        }

        await CheckNameUniqueAsync(name, edition.Id);
        edition.SetName(name);
    }

    public virtual async Task ChangeCodeAsync(Edition edition, string code)
    {
        var normalized = Check.NotNullOrWhiteSpace(code, nameof(code), EditionConsts.MaxCodeLength).Trim().ToUpperInvariant();
        if (string.Equals(edition.Code, normalized, StringComparison.Ordinal))
        {
            return;
        }

        await CheckCodeUniqueAsync(normalized, edition.Id);
        edition.SetCode(normalized);
    }

    protected virtual async Task CheckNameUniqueAsync(string name, Guid? excludeId = null)
    {
        var existing = await EditionRepository.FindByNameAsync(name);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(EditionsErrorCodes.EditionNameAlreadyExists)
                .WithData("Name", name);
        }
    }

    protected virtual async Task CheckCodeUniqueAsync(string code, Guid? excludeId = null)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var existing = await EditionRepository.FindByCodeAsync(normalized);
        if (existing != null && existing.Id != excludeId)
        {
            throw new BusinessException(EditionsErrorCodes.EditionCodeAlreadyExists)
                .WithData("Code", normalized);
        }
    }
}
