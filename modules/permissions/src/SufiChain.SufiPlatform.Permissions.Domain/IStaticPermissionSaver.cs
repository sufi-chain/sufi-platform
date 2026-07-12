using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Permissions;

public interface IStaticPermissionSaver
{
    Task SaveAsync();
}