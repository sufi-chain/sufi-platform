namespace SufiChain.Chat.Blazor.Public;

public class ChatBlazorPublicOptions
{
    public string HubPath { get; set; } = Services.NavigationChatHubUrlResolver.DefaultHubPath;
}
