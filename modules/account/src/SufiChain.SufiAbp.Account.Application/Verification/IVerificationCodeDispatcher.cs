using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Account;

public interface IVerificationCodeDispatcher
{
    Task SendAsync(VerificationMessage message);
}
