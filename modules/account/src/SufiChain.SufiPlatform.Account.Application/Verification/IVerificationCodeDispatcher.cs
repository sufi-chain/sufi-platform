using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account;

public interface IVerificationCodeDispatcher
{
    Task SendAsync(VerificationMessage message);
}
