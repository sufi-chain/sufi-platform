using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account;

public interface IVerificationChannelSender
{
    VerificationDeliveryChannel Channel { get; }

    Task SendAsync(VerificationMessage message);
}
