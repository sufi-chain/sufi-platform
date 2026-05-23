using System;

namespace SufiChain.SufiAbp.AIManagement;

public static class AIManagementTestData
{
    public static class Workspaces
    {
        public static Guid DefaultWorkspaceId { get; } = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");
        public const string DefaultWorkspaceName = "test-workspace";
        public const string DefaultDisplayName = "Test Workspace";
        public const string DefaultModelId = "gpt-4";
        public const string DefaultApiKey = "sk-test-key-123";
        public const string DefaultEndpoint = "https://api.openai.com/v1";

        public static Guid SecondaryWorkspaceId { get; } = Guid.Parse("4fa85f64-5717-4562-b3fc-2c963f66afa7");
        public const string SecondaryWorkspaceName = "secondary-openai-workspace";
        public const string SecondaryModelId = "gpt-4o-mini";
        public const string SecondaryEndpoint = "https://api.openai.com/v1";
    }

    public static class RAG
    {
        public const string SampleQuery = "What is artificial intelligence?";
        public const string SampleDocument1 = "Artificial intelligence (AI) is intelligence demonstrated by machines.";
        public const string SampleDocument2 = "Machine learning is a subset of artificial intelligence.";
        public const string SampleDocumentId1 = "doc-001";
        public const string SampleDocumentId2 = "doc-002";
    }
}
