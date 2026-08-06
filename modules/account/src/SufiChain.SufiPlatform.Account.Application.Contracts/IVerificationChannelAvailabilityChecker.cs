using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiPlatform.Account;

public interface IVerificationChannelAvailabilityChecker
{
    Task<IReadOnlyList<VerificationDeliveryChannel>> GetAvailableChannelsAsync();
}
