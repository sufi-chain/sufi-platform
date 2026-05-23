# Multi-Modal AI Implementation - Next Steps

## Current Status: Domain & Application Layers Complete ✅

The core architecture for multi-modal AI support has been implemented. All domain entities, services, repositories, DTOs, and application services are in place.

## Immediate Next Steps

### 1. Build & Test Compilation

```bash
cd /mnt/d/Projects/SCIS/alpha-2/src/modules/ai-management

# Restore packages
dotnet restore

# Build all projects
dotnet build

# If there are compilation errors, fix them before proceeding
```

**Expected Issues to Fix:**
- Missing using statements
- Namespace mismatches
- Missing package references (System.Net.Http, System.Text.Json)

### 2. Run Database Migration

```bash
# Option A: Using EF Core CLI
cd src/SufiChain.SufiAbp.AIManagement.EntityFrameworkCore
dotnet ef migrations add AddMultiModalAISupport --context AIManagementDbContext
dotnet ef database update --context AIManagementDbContext

# Option B: Using SQL script directly
# Execute: src/modules/ai-management/migrations/AddMultiModalAISupport.sql
```

### 3. Update Blazor UI - TestChat Page

**File:** `src/SufiChain.SufiAbp.AIManagement.Blazor/Pages/AIManagement/TestChat.razor`

**Changes Needed:**
1. Inject `IAIAppService` instead of `IChatAppService`
2. Add file upload controls for audio and images
3. Add capability selection (Chat, Audio, Vision)
4. Implement streaming chat responses
5. Add audio playback for TTS responses

**Example Structure:**
```razor
@page "/ai-management/test-chat"
@using SufiChain.SufiAbp.AIManagement.AI
@inject IAIAppService AIAppService

<SbCard>
    <SbCardHeader>
        <SbCardTitle>Multi-Modal AI Test</SbCardTitle>
    </SbCardHeader>
    <SbCardBody>
        <!-- Workspace selector -->
        <SbSelect @bind-Value="SelectedWorkspace">
            @foreach (var ws in Workspaces)
            {
                <option value="@ws.Name">@ws.Name</option>
            }
        </SbSelect>
        
        <!-- Capability selector -->
        <SbRadioGroup @bind-Value="SelectedCapability">
            <SbRadio Value="AICapabilityType.ChatCompletion">Chat</SbRadio>
            <SbRadio Value="AICapabilityType.AudioTranscription">Audio → Text</SbRadio>
            <SbRadio Value="AICapabilityType.TextToSpeech">Text → Audio</SbRadio>
            <SbRadio Value="AICapabilityType.VisionAnalysis">Vision</SbRadio>
        </SbRadioGroup>
        
        <!-- Input area (changes based on capability) -->
        @if (SelectedCapability == AICapabilityType.ChatCompletion)
        {
            <SbTextArea @bind-Value="Message" Rows="4" />
            <SbButton OnClick="SendChatMessage">Send</SbButton>
        }
        else if (SelectedCapability == AICapabilityType.AudioTranscription)
        {
            <InputFile OnChange="HandleAudioUpload" accept="audio/*" />
            <SbButton OnClick="TranscribeAudio">Transcribe</SbButton>
        }
        else if (SelectedCapability == AICapabilityType.VisionAnalysis)
        {
            <InputFile OnChange="HandleImageUpload" accept="image/*" />
            <SbTextArea @bind-Value="VisionPrompt" Placeholder="What do you want to know about this image?" />
            <SbButton OnClick="AnalyzeImage">Analyze</SbButton>
        }
        
        <!-- Response area -->
        <div class="response-area">
            @foreach (var msg in Messages)
            {
                <div class="message @msg.Role">
                    @msg.Content
                </div>
            }
        </div>
    </SbCardBody>
</SbCard>

@code {
    private string SelectedWorkspace = "";
    private AICapabilityType SelectedCapability = AICapabilityType.ChatCompletion;
    private string Message = "";
    private string VisionPrompt = "";
    private byte[] AudioData = Array.Empty<byte>();
    private byte[] ImageData = Array.Empty<byte>();
    private List<ChatMessageDto> Messages = new();
    
    private async Task SendChatMessage()
    {
        var input = new SendChatMessageInput
        {
            WorkspaceName = SelectedWorkspace,
            Message = Message,
            ConversationHistory = Messages
        };
        
        var response = await AIAppService.SendChatMessageAsync(input);
        
        Messages.Add(new ChatMessageDto { Role = "user", Content = Message });
        Messages.Add(new ChatMessageDto { Role = "assistant", Content = response.Message });
        
        Message = "";
    }
    
    private async Task HandleAudioUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        using var stream = file.OpenReadStream(maxAllowedSize: 25 * 1024 * 1024); // 25MB
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        AudioData = ms.ToArray();
    }
    
    private async Task TranscribeAudio()
    {
        var input = new TranscribeAudioInput
        {
            WorkspaceName = SelectedWorkspace,
            AudioData = AudioData,
            AudioFormat = "mp3"
        };
        
        var response = await AIAppService.TranscribeAudioAsync(input);
        
        Messages.Add(new ChatMessageDto { Role = "system", Content = $"Transcription: {response.Text}" });
    }
    
    private async Task HandleImageUpload(InputFileChangeEventArgs e)
    {
        var file = e.File;
        using var stream = file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        ImageData = ms.ToArray();
    }
    
    private async Task AnalyzeImage()
    {
        var input = new AnalyzeImageInput
        {
            WorkspaceName = SelectedWorkspace,
            ImageData = ImageData,
            ImageFormat = "png",
            Prompt = VisionPrompt
        };
        
        var response = await AIAppService.AnalyzeImageAsync(input);
        
        Messages.Add(new ChatMessageDto { Role = "system", Content = $"Vision: {response.Description}" });
    }
}
```

### 4. Create Model Configuration Management Page

**File:** `src/SufiChain.SufiAbp.AIManagement.Blazor/Pages/AIManagement/ModelConfigurations.razor`

**Features:**
- List all model configurations for a workspace
- Add new configuration (select capability, enter model ID, API endpoint, priority)
- Edit existing configuration
- Enable/disable configurations
- Delete configurations
- Reorder priorities (drag & drop or up/down buttons)

### 5. Create Usage Statistics Dashboard

**File:** `src/SufiChain.SufiAbp.AIManagement.Blazor/Pages/AIManagement/UsageStatistics.razor`

**Features:**
- Total cost this month
- Total tokens used
- Requests by capability (pie chart)
- Cost by model (bar chart)
- Cost over time (line chart)
- Recent usage logs (table)
- Export to CSV

### 6. Update Workspace Management

**Files to Update:**
- `WorkspaceCreateModal.razor` - Remove Model field, add "Configure Models Later" note
- `WorkspaceEditModal.razor` - Remove Model field, add "Manage Models" button → navigate to ModelConfigurations page
- `Workspaces.razor` - Add "Models" column showing configured capabilities

### 7. Testing Checklist

#### Unit Tests
- [ ] Test AIService.SendChatMessageAsync
- [ ] Test AIService.TranscribeAudioAsync
- [ ] Test AIService.AnalyzeImageAsync
- [ ] Test OpenAIProvider.SendChatMessageAsync
- [ ] Test AzureOpenAIProvider.SendChatMessageAsync
- [ ] Test AIModelConfigurationRepository.GetPrimaryConfigurationAsync
- [ ] Test AIUsageLogRepository.GetTotalCostAsync

#### Integration Tests
- [ ] Create workspace with multiple model configurations
- [ ] Send chat message using ChatCompletion configuration
- [ ] Transcribe audio using AudioTranscription configuration
- [ ] Analyze image using VisionAnalysis configuration
- [ ] Verify usage logs are created
- [ ] Verify cost calculation is correct
- [ ] Test priority-based fallback (disable primary, use secondary)

#### Manual Tests
- [ ] Create workspace in UI
- [ ] Add ChatCompletion configuration (GPT-4)
- [ ] Add AudioTranscription configuration (Whisper)
- [ ] Add VisionAnalysis configuration (GPT-4 Vision)
- [ ] Test chat in TestChat page
- [ ] Upload audio file and transcribe
- [ ] Upload image and analyze
- [ ] Check usage statistics dashboard
- [ ] Verify permissions work correctly

### 8. Documentation Updates

**Files to Create/Update:**
- [ ] `docs/MultiModalAI.md` - User guide for multi-modal features
- [ ] `docs/ModelConfiguration.md` - How to configure models per capability
- [ ] `docs/Providers.md` - Supported providers and their capabilities
- [ ] `docs/CostEstimation.md` - How cost calculation works
- [ ] `README.md` - Update with new features

### 9. Optional Enhancements (Future)

- [ ] Implement Ollama provider for local models
- [ ] Add streaming chat UI with real-time token display
- [ ] Add batch embeddings generation
- [ ] Add cost alerts (email when monthly cost exceeds threshold)
- [ ] Add rate limiting per workspace
- [ ] Add caching for embeddings
- [ ] Add image generation support (DALL-E)
- [ ] Add function calling integration with MCP tools

## Quick Start Commands

```bash
# 1. Navigate to module
cd /mnt/d/Projects/SCIS/alpha-2/src/modules/ai-management

# 2. Restore and build
dotnet restore
dotnet build

# 3. Run migrations
cd src/SufiChain.SufiAbp.AIManagement.EntityFrameworkCore
dotnet ef database update

# 4. Run the host application
cd ../../host/SufiChain.SufiAbp.AIManagement.Blazor.Host
dotnet run

# 5. Open browser
# Navigate to: https://localhost:44300/ai-management/test-chat
```

## Troubleshooting

### Compilation Errors

**Error: "The type or namespace name 'IAIService' could not be found"**
- Solution: Add `using SufiChain.SufiAbp.AIManagement.AI;` to the file

**Error: "HttpClient does not contain a definition for 'PostAsync'"**
- Solution: Add package reference `<PackageReference Include="System.Net.Http" Version="4.3.4" />`

**Error: "JsonSerializer does not exist"**
- Solution: Add `using System.Text.Json;`

### Runtime Errors

**Error: "No provider registered for AIProviderType.OpenAI"**
- Solution: Ensure OpenAIProvider and AzureOpenAIProvider are registered in DI container
- Check: AIManagementDomainModule or AIManagementApplicationModule

**Error: "Workspace not found"**
- Solution: Create a workspace first in the Workspaces page
- Ensure workspace name matches exactly (case-sensitive)

**Error: "Capability not configured"**
- Solution: Add a model configuration for the capability you're trying to use
- Go to Model Configurations page and add the required capability

### Database Errors

**Error: "Invalid object name 'AIManagementAIModelConfigurations'"**
- Solution: Run the migration: `dotnet ef database update`

**Error: "Cannot insert duplicate key"**
- Solution: Check if configuration already exists for that workspace + capability + priority combination

## Success Criteria

✅ All projects compile without errors  
✅ Database migration runs successfully  
✅ Can create workspace with multiple model configurations  
✅ Can send chat message and receive response  
✅ Can transcribe audio file  
✅ Can analyze image  
✅ Usage logs are created automatically  
✅ Cost calculation is accurate  
✅ Permissions work correctly  
✅ UI is responsive and user-friendly  

## Contact & Support

If you encounter issues:
1. Check the implementation summary: `MULTIMODAL_AI_IMPLEMENTATION_SUMMARY.md`
2. Review the architecture diagram in the summary
3. Check existing MCP implementation for patterns: `MCP_IMPLEMENTATION_COMPLETE.md`
4. Review SufiAbp conventions in AGENTS.md files

## Timeline Estimate

- **Build & Fix Compilation**: 1-2 hours
- **Database Migration**: 30 minutes
- **Update TestChat UI**: 2-3 hours
- **Create Model Configuration Page**: 2-3 hours
- **Create Usage Statistics Page**: 2-3 hours
- **Testing**: 2-3 hours
- **Documentation**: 1-2 hours

**Total**: 11-16 hours of development work

## Priority Order

1. **Critical**: Build & fix compilation errors
2. **Critical**: Run database migration
3. **High**: Update TestChat page for basic chat functionality
4. **High**: Create Model Configuration management page
5. **Medium**: Add audio transcription to TestChat
6. **Medium**: Add vision analysis to TestChat
7. **Medium**: Create Usage Statistics dashboard
8. **Low**: Add streaming chat
9. **Low**: Add TTS support
10. **Low**: Write comprehensive tests
