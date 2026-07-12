using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Settings;

public interface IStaticSettingSaver
{
    Task SaveAsync();
}
