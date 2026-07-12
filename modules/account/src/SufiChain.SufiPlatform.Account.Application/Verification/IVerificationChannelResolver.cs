using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account;

public interface IVerificationChannelResolver
{
    Task<VerificationDeliveryChannel> ResolveAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel? preferredChannel = null);
}
