using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.Identity;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.Account;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class IdentityUserToIdentityUserDtoMapper : MapperBase<IdentityUser, IdentityUserDto>
{
    public override partial IdentityUserDto Map(IdentityUser source);
    public override partial void Map(IdentityUser source, IdentityUserDto destination);
}
