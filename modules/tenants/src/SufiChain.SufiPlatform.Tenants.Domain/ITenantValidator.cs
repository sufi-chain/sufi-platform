using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantValidator
{
    Task ValidateAsync(Tenant tenant);
}
