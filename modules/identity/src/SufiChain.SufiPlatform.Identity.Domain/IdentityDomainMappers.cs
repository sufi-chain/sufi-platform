using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.Users;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.Identity;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class IdentityUserToUserEtoMapper : MapperBase<IdentityUser, UserEto>
{
    public override partial UserEto Map(IdentityUser source);

    public override partial void Map(IdentityUser source, UserEto destination);
}
