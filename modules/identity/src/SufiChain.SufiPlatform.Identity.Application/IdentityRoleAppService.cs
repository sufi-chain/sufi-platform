using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Application.Services;

namespace SufiChain.SufiPlatform.Identity;

public class IdentityRoleAppService : SufiApplicationService, IIdentityRoleAppService
{
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IdentityRoleManager RoleManager { get; }

    public IdentityRoleAppService(
        IIdentityRoleRepository roleRepository,
        IdentityRoleManager roleManager)
    {
        RoleRepository = roleRepository;
        RoleManager = roleManager;
    }

    public virtual async Task<ListResultDto<IdentityRoleDto>> GetAllListAsync()
    {
        var roles = await RoleRepository.GetListAsync();
        return new ListResultDto<IdentityRoleDto>(roles.Select(MapToDto).ToList());
    }

    public virtual async Task<PagedResultDto<IdentityRoleDto>> GetListAsync(GetIdentityRolesInput input)
    {
        var count = await RoleRepository.GetCountAsync(input.Filter);
        var roles = await RoleRepository.GetListAsync(
            input.Sorting,
            input.MaxResultCount,
            input.SkipCount,
            input.Filter);

        return new PagedResultDto<IdentityRoleDto>(count, roles.Select(MapToDto).ToList());
    }

    public virtual async Task<IdentityRoleDto> GetAsync(Guid id)
    {
        return MapToDto(await RoleRepository.GetAsync(id));
    }

    public virtual async Task<IdentityRoleDto> CreateAsync(IdentityRoleCreateDto input)
    {
        var role = new IdentityRole(GuidGenerator.Create(), input.Name, CurrentTenant.Id)
        {
            IsDefault = input.IsDefault,
            IsPublic = input.IsPublic
        };

        var result = await RoleManager.CreateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return MapToDto(role);
    }

    public virtual async Task<IdentityRoleDto> UpdateAsync(Guid id, IdentityRoleUpdateDto input)
    {
        var role = await RoleManager.GetByIdAsync(id);
        role.ConcurrencyStamp = input.ConcurrencyStamp;
        await RoleManager.SetRoleNameAsync(role, input.Name);
        role.IsDefault = input.IsDefault;
        role.IsPublic = input.IsPublic;

        var result = await RoleManager.UpdateAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        return MapToDto(role);
    }

    public virtual async Task DeleteAsync(Guid id)
    {
        var role = await RoleManager.GetByIdAsync(id);
        var result = await RoleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }
    }

    protected virtual IdentityRoleDto MapToDto(IdentityRole role)
    {
        return new IdentityRoleDto
        {
            Id = role.Id,
            Name = role.Name,
            IsDefault = role.IsDefault,
            IsStatic = role.IsStatic,
            IsPublic = role.IsPublic,
            ConcurrencyStamp = role.ConcurrencyStamp,
            CreationTime = role.CreationTime
        };
    }
}
