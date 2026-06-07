using SufiChain.Chat.Sessions;

namespace SufiChain.Chat.Blazor.Public;

public static class ChatSessionUiTitle
{
    public static string GetTitle(
        ChatSessionDto? session,
        Func<string, string> localize,
        Guid? currentUserId)
    {
        if (session == null)
        {
            return localize("Messenger:Conversations");
        }

        if (!string.IsNullOrWhiteSpace(session.Title))
        {
            return session.Title;
        }

        if (session.ConversationKind == ConversationKind.Direct && currentUserId.HasValue)
        {
            var otherParticipant = session.Participants
                .FirstOrDefault(participant =>
                    participant.LeftAt == null &&
                    participant.UserId.HasValue &&
                    participant.UserId != currentUserId);

            if (!string.IsNullOrWhiteSpace(otherParticipant?.DisplayName))
            {
                return otherParticipant.DisplayName;
            }
        }

        return localize($"Messenger:Kind:{session.ConversationKind}");
    }

    public static string GetTitle(
        ChatSessionListDto session,
        Func<string, string> localize)
    {
        if (!string.IsNullOrWhiteSpace(session.Title))
        {
            return session.Title;
        }

        return localize($"Messenger:Kind:{session.ConversationKind}");
    }
}
