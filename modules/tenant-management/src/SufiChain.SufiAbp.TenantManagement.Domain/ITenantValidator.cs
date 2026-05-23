using System.Threading.Tasks;

namespace SufiChain.SufiAbp.TenantManagement;

public interface ITenantValidator
{
    Task ValidateAsync(Tenant tenant);
}
