using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using SufiChain.SufiAbp.Uow;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Threading;

namespace SufiChain.SufiAbp.Identity;

public class IdentityRoleStore :
    IRoleStore<IdentityRole>,
    IRoleClaimStore<IdentityRole>
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IGuidGenerator GuidGenerator { get; }
    protected ICancellationTokenProvider CancellationTokenProvider { get; }
    protected IUnitOfWorkManager UnitOfWorkManager { get; }

    public virtual bool AutoSaveChanges { get; set; } = true;

    public IdentityRoleStore(
        IIdentityRoleRepository roleRepository,
        IGuidGenerator guidGenerator,
        ICancellationTokenProvider cancellationTokenProvider,
        IUnitOfWorkManager unitOfWorkManager)
    {
        RoleRepository = roleRepository;
        GuidGenerator = guidGenerator;
        CancellationTokenProvider = cancellationTokenProvider;
        UnitOfWorkManager = unitOfWorkManager;
    }

    protected virtual CancellationToken GetCancellationToken(CancellationToken cancellationToken)
    {
        return CancellationTokenProvider.FallbackToProvider(cancellationToken);
    }

    public void Dispose()
    {
    }

    protected virtual async Task<TResult> ExecuteInUnitOfWorkAsync<TResult>(Func<Task<TResult>> action, CancellationToken cancellationToken = default)
    {
        if (UnitOfWorkManager.HasActiveUnitOfWork)
        {
            return await action();
        }

        using var unitOfWork = UnitOfWorkManager.Begin(requiresNew: true);
        var result = await action();
        await unitOfWork.CompleteAsync(GetCancellationToken(cancellationToken));
        return result;
    }

    protected virtual async Task ExecuteInUnitOfWorkAsync(Func<Task> action, CancellationToken cancellationToken = default)
    {
        if (UnitOfWorkManager.HasActiveUnitOfWork)
        {
            await action();
            return;
        }

        using var unitOfWork = UnitOfWorkManager.Begin(requiresNew: true);
        await action();
        await unitOfWork.CompleteAsync(GetCancellationToken(cancellationToken));
    }

    public virtual async Task<IdentityResult> CreateAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        await RoleRepository.InsertAsync(role, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityResult> UpdateAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        await RoleRepository.UpdateAsync(role, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual async Task<IdentityResult> DeleteAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        await RoleRepository.DeleteAsync(role, AutoSaveChanges, GetCancellationToken(cancellationToken));
        return IdentityResult.Success;
    }

    public virtual Task<string> GetRoleIdAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(role.Id.ToString());
    }

    public virtual Task<string?> GetRoleNameAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(role.Name);
    }

    public virtual Task SetRoleNameAsync(IdentityRole role, string? roleName, CancellationToken cancellationToken = default)
    {
        role.Name = roleName!;
        return Task.CompletedTask;
    }

    public virtual Task<string?> GetNormalizedRoleNameAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        return Task.FromResult<string?>(role.NormalizedName);
    }

    public virtual Task SetNormalizedRoleNameAsync(IdentityRole role, string? normalizedName, CancellationToken cancellationToken = default)
    {
        role.NormalizedName = normalizedName!;
        return Task.CompletedTask;
    }

    public virtual async Task<IdentityRole?> FindByIdAsync(string roleId, CancellationToken cancellationToken = default)
    {
        return await RoleRepository.FindAsync(Guid.Parse(roleId), cancellationToken: GetCancellationToken(cancellationToken));
    }

    public virtual Task<IdentityRole?> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken = default)
    {
        return ExecuteInUnitOfWorkAsync(
            () => RoleRepository.FindByNormalizedNameAsync(normalizedRoleName, cancellationToken: GetCancellationToken(cancellationToken)),
            cancellationToken);
    }

    public virtual Task<IList<Claim>> GetClaimsAsync(IdentityRole role, CancellationToken cancellationToken = default)
    {
        return ExecuteInUnitOfWorkAsync<IList<Claim>>(async () =>
        {
            await RoleRepository.EnsureCollectionLoadedAsync(role, r => r.Claims, GetCancellationToken(cancellationToken));
            return role.Claims.Select(c => c.ToClaim()).ToList();
        }, cancellationToken);
    }

    public virtual async Task AddClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await RoleRepository.EnsureCollectionLoadedAsync(role, r => r.Claims, GetCancellationToken(cancellationToken));
        role.AddClaim(GuidGenerator, claim);
    }

    public virtual async Task RemoveClaimAsync(IdentityRole role, Claim claim, CancellationToken cancellationToken = default)
    {
        await RoleRepository.EnsureCollectionLoadedAsync(role, r => r.Claims, GetCancellationToken(cancellationToken));
        role.RemoveClaim(claim);
    }
}
