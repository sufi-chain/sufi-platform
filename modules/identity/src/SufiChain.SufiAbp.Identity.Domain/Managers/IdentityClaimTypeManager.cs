using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace SufiChain.SufiAbp.Identity;

public class IdentityClaimTypeManager : DomainService
{
    protected IIdentityClaimTypeRepository ClaimTypeRepository { get; }

    public IdentityClaimTypeManager(IIdentityClaimTypeRepository claimTypeRepository)
    {
        ClaimTypeRepository = claimTypeRepository;
    }

    public virtual async Task<IdentityClaimType> CreateAsync([NotNull] string name, bool required = false, bool isStatic = false, string? regex = null, string? regexDescription = null, string? description = null)
    {
        Check.NotNullOrWhiteSpace(name, nameof(name));

        await ValidateClaimTypeAsync(name);

        return new IdentityClaimType(
            GuidGenerator.Create(),
            name,
            required,
            isStatic,
            regex,
            regexDescription,
            description
        );
    }

    public virtual async Task UpdateAsync([NotNull] IdentityClaimType claimType)
    {
        Check.NotNull(claimType, nameof(claimType));

        await ValidateClaimTypeAsync(claimType.Name, claimType.Id);
    }

    protected virtual async Task ValidateClaimTypeAsync(string name, Guid? expectedId = null)
    {
        var existingClaimType = await ClaimTypeRepository.AnyAsync(name, expectedId);
        if (existingClaimType)
        {
            throw new BusinessException("Identity:DuplicateClaimType").WithData("Name", name);
        }
    }
}
