using SufiChain.SufiAbp.Identity;
using Volo.Abp.DependencyInjection;

namespace SufiChain.Chat.Participants;

public interface IChatParticipantDisplayNameResolver
{
    Task<string?> ResolveAsync(Guid userId);
}

public class ChatParticipantDisplayNameResolver : IChatParticipantDisplayNameResolver, ITransientDependency
{
    protected IIdentityUserRepository UserRepository { get; }

    public ChatParticipantDisplayNameResolver(IIdentityUserRepository userRepository)
    {
        UserRepository = userRepository;
    }

    public virtual async Task<string?> ResolveAsync(Guid userId)
    {
        var user = await UserRepository.FindAsync(userId);
        if (user == null)
        {
            return null;
        }

        var displayName = $"{user.Name} {user.Surname}".Trim();
        return displayName.IsNullOrWhiteSpace()
            ? user.UserName
            : displayName;
    }
}
