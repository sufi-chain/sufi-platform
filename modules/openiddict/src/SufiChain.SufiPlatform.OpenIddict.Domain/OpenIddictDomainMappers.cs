using Riok.Mapperly.Abstractions;
using SufiChain.SufiPlatform.OpenIddict.Applications;
using Volo.Abp.Mapperly;

namespace SufiChain.SufiPlatform.OpenIddict;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.Target)]
public partial class OpenIddictApplicationToOpenIddictApplicationEtoMapper
    : MapperBase<OpenIddictApplication, OpenIddictApplicationEto>
{
    public override partial OpenIddictApplicationEto Map(OpenIddictApplication source);

    public override partial void Map(OpenIddictApplication source, OpenIddictApplicationEto destination);
}
