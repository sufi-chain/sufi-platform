using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Identity;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiAbp.Account;

public class VerificationCodeDispatcher : IVerificationCodeDispatcher, ITransientDependency
{
    protected IVerificationChannelResolver ChannelResolver { get; }

    protected IVerificationChannelSender[] ChannelSenders { get; }

    public VerificationCodeDispatcher(
        IVerificationChannelResolver channelResolver,
        IVerificationChannelSender[] channelSenders)
    {
        ChannelResolver = channelResolver;
        ChannelSenders = channelSenders;
    }

    public virtual async Task SendAsync(VerificationMessage message)
    {
        Check.NotNull(message, nameof(message));

        var channel = message.Channel ??
                      await ChannelResolver.ResolveAsync(message.Purpose, message.PreferredChannel);

        var sender = ChannelSenders.FirstOrDefault(x => x.Channel == channel);
        if (sender == null)
        {
            throw new BusinessException(IdentitySecurityErrorCodes.VerificationChannelUnavailable)
                .WithData("Channel", channel);
        }

        message.Channel = channel;
        await sender.SendAsync(message);
    }
}
