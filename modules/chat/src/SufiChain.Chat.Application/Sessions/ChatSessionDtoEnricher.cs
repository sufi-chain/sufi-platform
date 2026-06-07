using SufiChain.Chat.Mapping;
using SufiChain.Chat.Participants;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;

namespace SufiChain.Chat.Sessions;

public interface IChatSessionDtoEnricher
{
    Task<ChatSessionDto> EnrichAsync(ChatSession session, List<ChatParticipant> participants);

    Task<ChatSessionListDto> EnrichListItemAsync(ChatSession session, List<ChatParticipant> participants);
}

public class ChatSessionDtoEnricher : IChatSessionDtoEnricher, ITransientDependency
{
    protected ChatApplicationMapper Mapper { get; }
    protected IChatParticipantDisplayNameResolver DisplayNameResolver { get; }
    protected ICurrentUser CurrentUser { get; }

    public ChatSessionDtoEnricher(
        ChatApplicationMapper mapper,
        IChatParticipantDisplayNameResolver displayNameResolver,
        ICurrentUser currentUser)
    {
        Mapper = mapper;
        DisplayNameResolver = displayNameResolver;
        CurrentUser = currentUser;
    }

    public virtual async Task<ChatSessionDto> EnrichAsync(ChatSession session, List<ChatParticipant> participants)
    {
        var dto = Mapper.ToDto(session, participants);
        await EnrichParticipantDtosAsync(dto.Participants);

        var displayTitle = await ResolveSessionDisplayTitleAsync(session, participants);
        if (!displayTitle.IsNullOrWhiteSpace())
        {
            dto.Title = displayTitle;
        }

        return dto;
    }

    public virtual async Task<ChatSessionListDto> EnrichListItemAsync(ChatSession session, List<ChatParticipant> participants)
    {
        var dto = Mapper.ToListDto(session, participants.Count);
        var displayTitle = await ResolveSessionDisplayTitleAsync(session, participants);
        if (!displayTitle.IsNullOrWhiteSpace())
        {
            dto.Title = displayTitle;
        }

        return dto;
    }

    protected virtual async Task<string?> ResolveSessionDisplayTitleAsync(
        ChatSession session,
        List<ChatParticipant> participants)
    {
        if (!session.Title.IsNullOrWhiteSpace())
        {
            return session.Title;
        }

        if (session.ConversationKind != ConversationKind.Direct || !CurrentUser.Id.HasValue)
        {
            return null;
        }

        var otherParticipant = participants.FirstOrDefault(participant =>
            participant.LeftAt == null &&
            participant.UserId.HasValue &&
            participant.UserId != CurrentUser.Id);

        if (otherParticipant == null)
        {
            return null;
        }

        if (!otherParticipant.DisplayName.IsNullOrWhiteSpace())
        {
            return otherParticipant.DisplayName;
        }

        return otherParticipant.UserId.HasValue
            ? await DisplayNameResolver.ResolveAsync(otherParticipant.UserId.Value)
            : null;
    }

    protected virtual async Task EnrichParticipantDtosAsync(List<ChatParticipantDto> participants)
    {
        foreach (var participant in participants)
        {
            if (!participant.DisplayName.IsNullOrWhiteSpace() || !participant.UserId.HasValue)
            {
                continue;
            }

            participant.DisplayName = await DisplayNameResolver.ResolveAsync(participant.UserId.Value);
        }
    }
}
