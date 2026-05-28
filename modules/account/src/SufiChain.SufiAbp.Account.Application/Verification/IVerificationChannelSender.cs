using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Account;

public interface IVerificationChannelSender
{
    VerificationDeliveryChannel Channel { get; }

    Task SendAsync(VerificationMessage message);
}
