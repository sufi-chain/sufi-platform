using Riok.Mapperly.Abstractions;
using SufiChain.SufiAbp.Identity;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiAbp.Account;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
[MapExtraProperties]
public partial class IdentityUserToIdentityUserDtoMapper : MapperBase<IdentityUser, IdentityUserDto>
{
    public override partial IdentityUserDto Map(IdentityUser source);
    public override partial void Map(IdentityUser source, IdentityUserDto destination);
}
