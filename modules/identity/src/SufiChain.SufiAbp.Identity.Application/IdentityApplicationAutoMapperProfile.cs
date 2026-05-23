using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.Identity.Dtos;
using SufiChain.SufiAbp.Identity.OrganizationUnits.Dtos;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.Identity;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class SecurityLogToSecurityLogDtoMapper : MapperBase<IdentitySecurityLog, SecurityLogDto>
{
    public override partial SecurityLogDto Map(IdentitySecurityLog source);
    public override partial void Map(IdentitySecurityLog source, SecurityLogDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class SecurityLogToSecurityLogListItemDtoMapper : MapperBase<IdentitySecurityLog, SecurityLogListItemDto>
{
    public override partial SecurityLogListItemDto Map(IdentitySecurityLog source);
    public override partial void Map(IdentitySecurityLog source, SecurityLogListItemDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class OrganizationUnitToOrganizationUnitDtoMapper : MapperBase<OrganizationUnit, OrganizationUnitDto>
{
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.Children))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.MemberCount))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.RoleCount))]
    public override partial OrganizationUnitDto Map(OrganizationUnit source);

    [MapperIgnoreTarget(nameof(OrganizationUnitDto.Children))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.MemberCount))]
    [MapperIgnoreTarget(nameof(OrganizationUnitDto.RoleCount))]
    public override partial void Map(OrganizationUnit source, OrganizationUnitDto destination);

    public override void AfterMap(OrganizationUnit source, OrganizationUnitDto destination)
    {
        destination.RoleCount = source.Roles?.Count ?? 0;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityUserToOrganizationUnitMemberDtoMapper : MapperBase<IdentityUser, OrganizationUnitMemberDto>
{
    [MapperIgnoreTarget(nameof(OrganizationUnitMemberDto.UserId))]
    public override partial OrganizationUnitMemberDto Map(IdentityUser source);

    [MapperIgnoreTarget(nameof(OrganizationUnitMemberDto.UserId))]
    public override partial void Map(IdentityUser source, OrganizationUnitMemberDto destination);

    public override void AfterMap(IdentityUser source, OrganizationUnitMemberDto destination)
    {
        destination.UserId = source.Id;
    }
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityRoleToOrganizationUnitRoleDtoMapper : MapperBase<IdentityRole, OrganizationUnitRoleDto>
{
    [MapperIgnoreTarget(nameof(OrganizationUnitRoleDto.RoleId))]
    [MapperIgnoreTarget(nameof(OrganizationUnitRoleDto.RoleName))]
    public override partial OrganizationUnitRoleDto Map(IdentityRole source);

    [MapperIgnoreTarget(nameof(OrganizationUnitRoleDto.RoleId))]
    [MapperIgnoreTarget(nameof(OrganizationUnitRoleDto.RoleName))]
    public override partial void Map(IdentityRole source, OrganizationUnitRoleDto destination);

    public override void AfterMap(IdentityRole source, OrganizationUnitRoleDto destination)
    {
        destination.RoleId = source.Id;
        destination.RoleName = source.Name;
    }
}
