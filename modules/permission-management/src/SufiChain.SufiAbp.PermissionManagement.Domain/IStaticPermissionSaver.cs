using System.Threading.Tasks;

namespace SufiChain.SufiAbp.PermissionManagement;

public interface IStaticPermissionSaver
{
    Task SaveAsync();
}