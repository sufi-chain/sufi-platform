using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.Identity;

public class IdentityRoleManager : RoleManager<IdentityRole>, IDomainService
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }
    
    protected override CancellationToken CancellationToken => CancellationTokenProvider.Token;

    public IdentityRoleManager(
        IdentityRoleStore store,
        IIdentityRoleRepository roleRepository,
        IEnumerable<IRoleValidator<IdentityRole>> roleValidators,
        ILookupNormalizer keyNormalizer,
        IdentityErrorDescriber errors,
        ILogger<IdentityRoleManager> logger,
        ICancellationTokenProvider cancellationTokenProvider)
        : base(
            store,
            roleValidators,
            keyNormalizer,
            errors,
            logger)
    {
        RoleRepository = roleRepository;
        CancellationTokenProvider = cancellationTokenProvider;
    }

    public virtual async Task<IdentityRole> GetByIdAsync(Guid id)
    {
        var role = await Store.FindByIdAsync(id.ToString(), CancellationToken);
        if (role == null)
        {
            throw new Volo.Abp.Domain.Entities.EntityNotFoundException(typeof(IdentityRole), id);
        }

        return role;
    }

    public virtual async Task<IdentityResult> SetRoleNameAsync(IdentityRole role, [NotNull] string name)
    {
        Check.NotNull(role, nameof(role));
        Check.NotNull(name, nameof(name));

        if (role.Name != name)
        {
            role.ChangeName(name);
            await UpdateNormalizedRoleNameAsync(role);
        }

        return IdentityResult.Success;
    }
}
