# SufiChain.SufiAbp.AI

Core AI framework for Sufi Platform, providing workspace-based AI configuration and Semantic Kernel integration.

## Features

- **Workspace Configuration**: Configure multiple AI workspaces with different providers and models
- **Typed Accessors**: Type-safe access to AI services using workspace identifiers
- **Semantic Kernel Integration**: Built on Microsoft Semantic Kernel for AI orchestration
- **Provider Agnostic**: Support for multiple AI providers (OpenAI, Ollama, etc.)

## Installation

```bash
dotnet add package SufiChain.SufiAbp.AI
```

## Usage

### 1. Define a Workspace Identifier

```csharp
[WorkspaceName("customer-support")]
public class CustomerSupportWorkspace { }
```

### 2. Configure Workspace

```csharp
Configure<SufiAIWorkspaceOptions>(options =>
{
    options.AddWorkspace("customer-support", ws =>
    {
        ws.Provider = "OpenAI";
        ws.Model = "gpt-4";
        ws.ApiKey = configuration["OpenAI:ApiKey"];
        ws.Temperature = 0.7f;
    });
});
```

### 3. Inject and Use

```csharp
public class MyService
{
    private readonly ISufiAbpChatClient<CustomerSupportWorkspace> _chatClient;
    
    public MyService(ISufiAbpChatClientAccessor<CustomerSupportWorkspace> accessor)
    {
        _chatClient = accessor.GetChatClient();
    }
    
    public async Task<string> GetResponseAsync(string userMessage)
    {
        var chatHistory = new ChatHistory();
        chatHistory.AddUserMessage(userMessage);
        
        var response = await _chatClient.ChatClient.GetChatMessageContentAsync(chatHistory);
        return response.Content;
    }
}
```

## Dependencies

- Volo.Abp.Core
- Microsoft.SemanticKernel

## Related Packages

- `SufiChain.SufiAbp.AI.Abstractions` - Core interfaces
- `SufiChain.SufiAbp.AI` - Full AI management module with UI

## License

Proprietary - Sufi Platform
