using System.Threading.Tasks;

namespace SufiChain.SufiAbp.SettingManagement;

public interface IStaticSettingSaver
{
    Task SaveAsync();
}
