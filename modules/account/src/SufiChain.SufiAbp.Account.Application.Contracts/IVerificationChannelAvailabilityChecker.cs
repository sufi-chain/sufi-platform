using System.Collections.Generic;
using System.Threading.Tasks;

namespace SufiChain.SufiAbp.Account;

public interface IVerificationChannelAvailabilityChecker
{
    Task<IReadOnlyList<VerificationDeliveryChannel>> GetAvailableChannelsAsync();
}
