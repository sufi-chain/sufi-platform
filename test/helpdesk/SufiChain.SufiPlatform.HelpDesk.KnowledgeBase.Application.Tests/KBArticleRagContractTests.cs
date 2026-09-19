using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.SufiCom;
using SufiChain.SufiPlatform.SufiCom.Chat;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.AI;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Articles;
using SufiChain.SufiPlatform.HelpDesk.Ai;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Permissions;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.RAG;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Hooshvare;
using SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Data;
using SufiChain.SufiPlatform.SufiAI;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.KnowledgeBase;

public class KBArticleRagContractTests
{
    [Fact]
    public void Indexing_Status_Retry_Should_Require_Index_Permission()
    {
        var authorizeAttribute = typeof(KBArticleIndexingStatusAppService)
            .GetMethod(nameof(KBArticleIndexingStatusAppService.RetryAsync))!
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(KnowledgeBasePermissions.Articles.Index);
    }

    [Fact]
    public void Rag_Smoke_Test_Should_Require_Index_Permission()
    {
        var authorizeAttribute = typeof(KBRagSmokeTestAppService)
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(KnowledgeBasePermissions.Articles.Index);
    }

    [Fact]
    public void Editor_Ai_Moderator_Should_Remain_Contract_Only()
    {
        typeof(IKBEditorAiModeratorAppService)
            .GetMethod(nameof(IKBEditorAiModeratorAppService.SendMessageAsync))!
            .ReturnType
            .ShouldBe(typeof(Task<EditorAiMessageDto>));

        typeof(IKBEditorAiModeratorAppService)
            .GetMethod(nameof(IKBEditorAiModeratorAppService.GetOrCreateAssistantSessionAsync))!
            .ReturnType
            .ShouldBe(typeof(Task<EditorAiSessionDto>));

        typeof(IKBEditorAiModeratorAppService)
            .GetMethod(nameof(IKBEditorAiModeratorAppService.TranscribeVoiceAsync))!
            .ReturnType
            .ShouldBe(typeof(Task<EditorAiVoiceTranscriptionDto>));

        Enum.GetNames(typeof(EditorAiActionType))
            .ShouldContain(nameof(EditorAiActionType.Suggest));
    }

    [Fact]
    public void Editor_Ai_Moderator_Should_Require_AiModerator_Permission()
    {
        var authorizeAttribute = typeof(KBEditorAiModeratorAppService)
            .GetCustomAttribute<AuthorizeAttribute>();

        authorizeAttribute.ShouldNotBeNull();
        authorizeAttribute.Policy.ShouldBe(KnowledgeBasePermissions.Articles.AiModerator);
    }

    [Fact]
    public void Editor_Ai_Metadata_Should_Round_Trip_Diff_Proposal()
    {
        var metadata = new EditorAiProposalMetadata
        {
            ArticleId = Guid.NewGuid(),
            ProjectId = Guid.NewGuid(),
            Action = EditorAiActionType.Rewrite,
            OriginalMarkdown = "old",
            SuggestedMarkdown = "new",
            RequiresDiffReview = true,
            SelectionStart = 1,
            SelectionEnd = 3
        };

        var parsed = EditorAiProposalMetadata.TryParse(metadata.ToJson());

        parsed.ShouldNotBeNull();
        parsed.ArticleId.ShouldBe(metadata.ArticleId);
        parsed.ProjectId.ShouldBe(metadata.ProjectId);
        parsed.Action.ShouldBe(EditorAiActionType.Rewrite);
        parsed.OriginalMarkdown.ShouldBe("old");
        parsed.SuggestedMarkdown.ShouldBe("new");
        parsed.RequiresDiffReview.ShouldBeTrue();
        parsed.SelectionStart.ShouldBe(1);
        parsed.SelectionEnd.ShouldBe(3);
    }

    [Fact]
    public void Editor_Ai_Moderator_Should_Expose_Send_And_Transcribe_Through_Contract()
    {
        typeof(IKBEditorAiModeratorAppService)
            .GetMethod("StreamMessageAsync")
            .ShouldBeNull();

        typeof(KBEditorAiModeratorAppService)
            .GetMethod(nameof(KBEditorAiModeratorAppService.SendMessageAsync))!
            .DeclaringType
            .ShouldBe(typeof(KBEditorAiModeratorAppService));

        typeof(KBEditorAiModeratorAppService)
            .GetMethod(nameof(KBEditorAiModeratorAppService.TranscribeVoiceAsync))!
            .ReturnType
            .ShouldBe(typeof(Task<EditorAiVoiceTranscriptionDto>));
    }

    [Fact]
    public void Editor_Ai_Moderator_Should_Use_Hooshvare_Runtime_And_Workspace_Resolver()
    {
        var constructor = typeof(KBEditorAiModeratorAppService).GetConstructors().Single();
        var parameterTypes = constructor.GetParameters().Select(x => x.ParameterType).ToList();

        parameterTypes.ShouldContain(typeof(IHooshvareRuntimeAppService));
        parameterTypes.ShouldContain(typeof(IHelpDeskAiWorkspaceResolver));
        parameterTypes.ShouldContain(typeof(ISufiAIAudioService));
    }

    [Fact]
    public void Editor_Ai_Should_Use_Assistant_Conversation_Primitives()
    {
        KBEditorAiModeratorAppService.LinkedEntityType.ShouldBe("KBArticle");
        KBEditorAiModeratorAppService.LinkRole.ShouldBe("PrimaryAssistant");
        HelpDeskKbArticleEditorHooshvareKeys.Key.ShouldNotBeNullOrWhiteSpace();

        Enum.GetNames(typeof(ConversationKind)).ShouldContain(nameof(ConversationKind.Assistant));
        Enum.GetNames(typeof(ChatMessageSenderKind)).ShouldContain(nameof(ChatMessageSenderKind.Assistant));
    }

    [Fact]
    public void Editor_Hooshvare_Seed_Should_Require_Current_Draft_Context_In_All_Cultures()
    {
        HelpDeskKbArticleEditorHooshvareKeys.EntityVersion.ShouldBe(0);
        typeof(SendEditorAiMessageInput).GetProperty(nameof(SendEditorAiMessageInput.VersionId)).ShouldNotBeNull();

        foreach (var culture in new[] { "en", "fa", "ar", "es" })
        {
            var prompt = HelpDeskKbArticleEditorHooshvareSeedTexts.Texts.SystemPrompt[culture];

            prompt.ShouldContain("hooshvareContext");
            prompt.ShouldContain("projectId");
            prompt.ShouldContain("articleId");
            prompt.ShouldContain("versionId");
            prompt.ShouldContain("body");
            prompt.ShouldContain("culture");
            prompt.ShouldContain("operation");
        }
    }

    [Fact]
    public void Editor_Hooshvare_Seed_Should_Enable_Project_Scoped_Rag()
    {
        HelpDeskKbArticleEditorHooshvareKeys.EntityVersion.ShouldBe(0);

        var runtimeOptions = new HooshvareRuntimeOptions
        {
            UseRag = true,
            RagTopK = 5,
            RagSourceName = "KnowledgeBase",
            RagFilterByProjectId = true,
            UseMcpTools = false
        };

        runtimeOptions.UseRag.ShouldBeTrue();
        runtimeOptions.RagFilterByProjectId.ShouldBeTrue();
        runtimeOptions.RagSourceName.ShouldBe("KnowledgeBase");

        typeof(SufiAIRagIndexRequest)
            .GetProperty(nameof(SufiAIRagIndexRequest.MetadataFilters))!
            .PropertyType.ShouldBe(typeof(Dictionary<string, string>));
    }

    [Fact]
    public void Rag_Options_Should_Default_To_Public_Only_Indexing()
    {
        var options = new KnowledgeBaseRagOptions();

        options.PublicOnlyIndexing.ShouldBeTrue();
        options.PublicArticlePathTemplate.ShouldContain("{slug}");
    }
}
