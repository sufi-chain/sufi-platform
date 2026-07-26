using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Users;

namespace SufiChain.SufiPlatform.Account;

/// <summary>
/// User-facing linked-account API. For trusted server-side linking (e.g. SaaS tenant provision),
/// inject <see cref="IdentityLinkUserManager"/> and call <c>LinkAsync</c> in-process instead of this service.
/// </summary>
[Authorize]
public class IdentityLinkUserAppService : SufiApplicationService, IIdentityLinkUserAppService
{
    protected IdentityLinkUserManager IdentityLinkUserManager { get; }
    protected IdentityUserManager UserManager { get; }
    protected ITenantStore TenantStore { get; }

    public IdentityLinkUserAppService(
        IdentityLinkUserManager identityLinkUserManager,
        IdentityUserManager userManager,
        ITenantStore tenantStore)
    {
        IdentityLinkUserManager = identityLinkUserManager;
        UserManager = userManager;
        TenantStore = tenantStore;
    }

    public virtual async Task<ListResultDto<LinkUserDto>> GetAllListAsync()
    {
        var currentUserInfo = CreateCurrentUserInfo();
        var directLinks = await IdentityLinkUserManager.GetListAsync(currentUserInfo);
        var allLinks = await IdentityLinkUserManager.GetListAsync(currentUserInfo, includeIndirect: true);

        var directTargets = new HashSet<(Guid UserId, Guid? TenantId)>();
        foreach (var link in directLinks)
        {
            directTargets.Add(GetOtherSide(link, currentUserInfo));
        }

        var targets = new Dictionary<(Guid UserId, Guid? TenantId), bool>();
        foreach (var link in allLinks)
        {
            var key = GetOtherSide(link, currentUserInfo);
            targets[key] = directTargets.Contains(key);
        }

        var dtos = new List<LinkUserDto>();
        foreach (var ((userId, tenantId), directlyLinked) in targets)
        {
            string? userName = null;
            using (CurrentTenant.Change(tenantId))
            {
                var user = await UserManager.FindByIdAsync(userId.ToString());
                userName = user?.UserName;
            }

            string? tenantName = null;
            if (tenantId.HasValue)
            {
                var tenant = await TenantStore.FindAsync(tenantId.Value);
                tenantName = tenant?.Name;
            }

            dtos.Add(new LinkUserDto
            {
                TargetUserId = userId,
                TargetUserName = userName,
                TargetTenantId = tenantId,
                TargetTenantName = tenantName,
                DirectlyLinked = directlyLinked
            });
        }

        return new ListResultDto<LinkUserDto>(dtos);
    }

    protected virtual (Guid UserId, Guid? TenantId) GetOtherSide(
        IdentityLinkUser link,
        IdentityLinkUserInfo currentUserInfo)
    {
        var isSource =
            link.SourceUserId == currentUserInfo.UserId &&
            link.SourceTenantId == currentUserInfo.TenantId;

        return isSource
            ? (link.TargetUserId, link.TargetTenantId)
            : (link.SourceUserId, link.SourceTenantId);
    }

    public virtual async Task LinkAsync(LinkUserInput input)
    {
        var sourceUserInfo = CreateCurrentUserInfo();
        var targetUserInfo = new IdentityLinkUserInfo(input.UserId, input.TenantId);

        if (!await IdentityLinkUserManager.VerifyLinkTokenAsync(
                targetUserInfo,
                input.Token,
                LinkUserTokenProviderConsts.LinkUserTokenPurpose))
        {
            throw new BusinessException(AccountErrorCodes.InvalidLinkUserToken);
        }

        await IdentityLinkUserManager.LinkAsync(sourceUserInfo, targetUserInfo);
    }

    public virtual async Task UnlinkAsync(UnLinkUserInput input)
    {
        await IdentityLinkUserManager.UnlinkAsync(
            CreateCurrentUserInfo(),
            new IdentityLinkUserInfo(input.UserId, input.TenantId));
    }

    public virtual Task<bool> IsLinkedAsync(IsLinkedInput input)
    {
        return IdentityLinkUserManager.IsLinkedAsync(
            CreateCurrentUserInfo(),
            new IdentityLinkUserInfo(input.UserId, input.TenantId));
    }

    public virtual Task<string> GenerateLinkTokenAsync()
    {
        return IdentityLinkUserManager.GenerateLinkTokenAsync(
            CreateCurrentUserInfo(),
            LinkUserTokenProviderConsts.LinkUserTokenPurpose);
    }

    [AllowAnonymous]
    public virtual Task<bool> VerifyLinkTokenAsync(VerifyLinkTokenInput input)
    {
        return IdentityLinkUserManager.VerifyLinkTokenAsync(
            new IdentityLinkUserInfo(input.UserId, input.TenantId),
            input.Token,
            LinkUserTokenProviderConsts.LinkUserTokenPurpose);
    }

    public virtual Task<string> GenerateLinkLoginTokenAsync()
    {
        return IdentityLinkUserManager.GenerateLinkTokenAsync(
            CreateCurrentUserInfo(),
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose);
    }

    [AllowAnonymous]
    public virtual Task<bool> VerifyLinkLoginTokenAsync(VerifyLinkLoginTokenInput input)
    {
        return IdentityLinkUserManager.VerifyLinkTokenAsync(
            new IdentityLinkUserInfo(input.UserId, input.TenantId),
            input.Token,
            LinkUserTokenProviderConsts.LinkUserLoginTokenPurpose);
    }

    protected virtual IdentityLinkUserInfo CreateCurrentUserInfo()
    {
        return new IdentityLinkUserInfo(CurrentUser.GetId(), CurrentTenant.Id);
    }
}
