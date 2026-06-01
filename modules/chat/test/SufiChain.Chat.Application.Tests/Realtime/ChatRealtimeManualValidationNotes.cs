using Xunit;

namespace SufiChain.Chat.Realtime;

/// <summary>
/// Manual validation notes for SignalR group delivery. Automated hub tests require a hosted SignalR pipeline.
/// </summary>
public class ChatRealtimeManualValidationNotes
{
    [Fact(Skip = "Manual: join ChatHub session group and verify message broadcast to participants.")]
    public void Manual_Validate_SignalR_Group_Delivery()
    {
        /*
         * 1. Run the dev host with Chat.Blazor.Server registered.
         * 2. Open two browser sessions on the same chat session.
         * 3. Join the session group via ChatHub JoinSessionGroupAsync.
         * 4. Send a message from session A and confirm session B receives ChatMessageSent realtime event.
         * 5. Leave the group and confirm subsequent messages are not delivered to the disconnected client.
         */
    }
}
