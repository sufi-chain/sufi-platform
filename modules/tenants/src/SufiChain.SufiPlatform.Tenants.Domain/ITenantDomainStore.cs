using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Tenants;

public interface ITenantDomainStore
{
    Task<string?> FindTenantNameByHostAsync(string host);
}
