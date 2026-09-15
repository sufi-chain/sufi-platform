using System.Collections.Generic;

namespace SufiChain.SufiPlatform.SufiAI.OpenAI;

public class OpenAIChatCompletionResponse
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = "chat.completion";

    public long Created { get; set; }

    public string Model { get; set; } = string.Empty;

    public List<OpenAIChatCompletionChoice> Choices { get; set; } = new();

    public OpenAIUsageInfo Usage { get; set; } = new();
}

public class OpenAIChatCompletionChoice
{
    public int Index { get; set; }

    public OpenAIChatMessage Message { get; set; } = new();

    public string FinishReason { get; set; } = "stop";
}

public class OpenAIEmbeddingRequest
{
    public string WorkspaceName { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public object Input { get; set; } = string.Empty;
}

public class OpenAIEmbeddingResponse
{
    public string Object { get; set; } = "list";

    public List<OpenAIEmbeddingData> Data { get; set; } = new();

    public string Model { get; set; } = string.Empty;

    public OpenAIUsageInfo Usage { get; set; } = new();
}

public class OpenAIEmbeddingData
{
    public int Index { get; set; }

    public string Object { get; set; } = "embedding";

    public float[] Embedding { get; set; } = System.Array.Empty<float>();
}

public class OpenAIUsageInfo
{
    public int? PromptTokens { get; set; }

    public int? CompletionTokens { get; set; }

    public int? TotalTokens { get; set; }
}

public class OpenAIModelsResponse
{
    public string Object { get; set; } = "list";

    public List<OpenAIModelInfo> Data { get; set; } = new();
}

public class OpenAIModelInfo
{
    public string Id { get; set; } = string.Empty;

    public string Object { get; set; } = "model";

    public string OwnedBy { get; set; } = string.Empty;
}

public class OpenAIErrorResponse
{
    public OpenAIError Error { get; set; } = new();
}

public class OpenAIError
{
    public string Message { get; set; } = string.Empty;

    public string Type { get; set; } = "invalid_request_error";

    public string? Param { get; set; }

    public string? Code { get; set; }
}
