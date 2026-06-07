using SufiChain.Chat.Participants;
using SufiChain.Chat.Permissions;
using SufiChain.Chat.Sessions;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace SufiChain.Chat.Realtime;

[ExposeServices(typeof(IChatRealtimeAccessChecker))]
public class ChatRealtimeAccessChecker : IChatRealtimeAccessChecker, ITransientDependency
{
    protected IChatSessionRepository SessionRepository { get; }
    protected IChatParticipantRepository ParticipantRepository { get; }
    protected IPermissionChecker PermissionChecker { get; }
    protected ICurrentUser CurrentUser { get; }

    public ChatRealtimeAccessChecker(
        IChatSessionRepository sessionRepository,
        IChatParticipantRepository participantRepository,
        IPermissionChecker permissionChecker,
        ICurrentUser currentUser)
    {
        SessionRepository = sessionRepository;
        ParticipantRepository = participantRepository;
        PermissionChecker = permissionChecker;
        CurrentUser = currentUser;
    }

    public virtual async Task<bool> CanJoinSessionAsync(Guid sessionId, string? anonymousVisitorId = null)
    {
        var session = await SessionRepository.FindAsync(sessionId);
        if (session == null)
        {
            return false;
        }

        if (CurrentUser.Id.HasValue && session.CreatorId == CurrentUser.Id)
        {
            return true;
        }

        if (await PermissionChecker.IsGrantedAsync(ChatPermissions.Inbox.Operator) ||
            await PermissionChecker.IsGrantedAsync(ChatPermissions.Inbox.Admin) ||
            await PermissionChecker.IsGrantedAsync(ChatPermissions.Sessions.Manage))
        {
            return true;
        }

        if (CurrentUser.Id.HasValue &&
            await ParticipantRepository.IsParticipantAsync(sessionId, CurrentUser.Id))
        {
            return true;
        }

        return !string.IsNullOrWhiteSpace(anonymousVisitorId) &&
               await ParticipantRepository.IsParticipantAsync(
                   sessionId,
                   anonymousVisitorId: anonymousVisitorId);
    }
}
