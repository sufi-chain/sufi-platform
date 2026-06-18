using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;

namespace SufiChain.SufiAbp.Identity;

public class IdentityDataSeeder : ITransientDependency, IIdentityDataSeeder
{
    protected IGuidGenerator GuidGenerator { get; }
    protected IIdentityRoleRepository RoleRepository { get; }
    protected IIdentityUserRepository UserRepository { get; }
    protected ILookupNormalizer LookupNormalizer { get; }
    protected IdentityUserManager UserManager { get; }
    protected IdentityRoleManager RoleManager { get; }
    protected ICurrentTenant CurrentTenant { get; }
    protected IOptions<IdentityOptions> IdentityOptions { get; }

    public IdentityDataSeeder(
        IGuidGenerator guidGenerator,
        IIdentityRoleRepository roleRepository,
        IIdentityUserRepository userRepository,
        ILookupNormalizer lookupNormalizer,
        IdentityUserManager userManager,
        IdentityRoleManager roleManager,
        ICurrentTenant currentTenant,
        IOptions<IdentityOptions> identityOptions)
    {
        GuidGenerator = guidGenerator;
        RoleRepository = roleRepository;
        UserRepository = userRepository;
        LookupNormalizer = lookupNormalizer;
        UserManager = userManager;
        RoleManager = roleManager;
        CurrentTenant = currentTenant;
        IdentityOptions = identityOptions;
    }

    [UnitOfWork]
    public virtual async Task<IdentityDataSeedResult> SeedAsync(
        string adminEmail,
        string adminPassword,
        Guid? tenantId = null,
        string? adminUserName = null)
    {
        Check.NotNullOrWhiteSpace(adminEmail, nameof(adminEmail));
        Check.NotNullOrWhiteSpace(adminPassword, nameof(adminPassword));

        using (CurrentTenant.Change(tenantId))
        {
            await IdentityOptions.SetAsync();

            var result = new IdentityDataSeedResult();
            //"admin" user
            if(adminUserName.IsNullOrWhiteSpace())
            {
                adminUserName = IdentityDataSeedConsts.AdminUserNameDefaultValue;
            }
            var adminUser = await UserRepository.FindByNormalizedUserNameAsync(
                LookupNormalizer.NormalizeName(adminUserName)
            );

            if (adminUser == null)
            {
                adminUser = new IdentityUser(
                    GuidGenerator.Create(),
                    adminUserName,
                    adminEmail,
                    tenantId
                )
                {
                    Name = adminUserName
                };

                (await UserManager.CreateAsync(adminUser, adminPassword, validatePassword: false)).CheckErrors();
                result.CreatedAdminUser = true;
            }

            //"admin" role
            var adminRoleName = IdentityDataSeedConsts.AdminRoleNameDefaultValue;
            var adminRole =
                await RoleRepository.FindByNormalizedNameAsync(LookupNormalizer.NormalizeName(adminRoleName));
            if (adminRole == null)
            {
                adminRole = new IdentityRole(
                    GuidGenerator.Create(),
                    adminRoleName,
                    tenantId
                )
                {
                    IsStatic = true,
                    IsPublic = true
                };

                (await RoleManager.CreateAsync(adminRole)).CheckErrors();
                result.CreatedAdminRole = true;
            }

            //"user" role
            var userRoleName = IdentityDataSeedConsts.UserRoleNameDefaultValue;
            var userRole =
                await RoleRepository.FindByNormalizedNameAsync(LookupNormalizer.NormalizeName(userRoleName));
            if (userRole == null)
            {
                userRole = new IdentityRole(
                    GuidGenerator.Create(),
                    userRoleName,
                    tenantId
                )
                {
                    IsDefault = true,
                    IsStatic = true,
                    IsPublic = true
                };

                (await RoleManager.CreateAsync(userRole)).CheckErrors();
                result.CreatedUserRole = true;
            }
            else if (!userRole.IsDefault)
            {
                userRole.IsDefault = true;
                (await RoleManager.UpdateAsync(userRole)).CheckErrors();
            }

            if (!await UserManager.IsInRoleAsync(adminUser, adminRoleName))
            {
                (await UserManager.AddToRoleAsync(adminUser, adminRoleName)).CheckErrors();
            }

            return result;
        }
    }
}
