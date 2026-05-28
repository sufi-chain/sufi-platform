using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Account;

public interface IVerificationChannelResolver
{
    Task<VerificationDeliveryChannel> ResolveAsync(
        VerificationPurpose purpose,
        VerificationDeliveryChannel? preferredChannel = null);
}
