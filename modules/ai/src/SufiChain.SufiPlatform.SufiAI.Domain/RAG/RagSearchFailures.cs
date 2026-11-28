using System.Text.RegularExpressions;
using SufiChain.SufiPlatform.SufiAI.Workspaces;
using Volo.Abp;

namespace SufiChain.SufiPlatform.SufiAI.RAG;

public static class RagSearchFailures
{
    private static readonly Regex DimensionMismatch = new(
        @"Expected\s+(\d+),\s+found\s+(\d+)",
        RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static bool IsCancellation(Exception exception) =>
        exception is OperationCanceledException;

    public static BusinessException ToBusinessException(
        Exception exception,
        string phase,
        string workspaceName,
        string? sourceName)
    {
        var detail = Trim(exception.Message);
        var mismatch = DimensionMismatch.Match(exception.Message ?? string.Empty);
        var retired = exception as EmbeddingModelRetiredException;
        string code;
        if (retired != null)
        {
            code = AIErrorCodes.EmbeddingModelRetired;
            detail = retired.Message;
        }
        else if (mismatch.Success || phase == "VectorSearch")
        {
            code = AIErrorCodes.VectorSearchFailed;
            if (mismatch.Success)
            {
                detail = "Embedding dimension mismatch, re-index required.";
            }
        }
        else
        {
            code = phase == "GenerateEmbeddings"
                ? AIErrorCodes.EmbeddingGenerationFailed
                : AIErrorCodes.VectorSearchFailed;
        }

        var business = new BusinessException(code, detail, innerException: exception)
            .WithData("WorkspaceName", workspaceName)
            .WithData("SourceName", sourceName ?? string.Empty)
            .WithData("Phase", phase)
            .WithData("Error", detail);
        if (retired != null)
        {
            business
                .WithData("Model", retired.Model)
                .WithData("Capability", retired.Capability);
        }

        if (mismatch.Success)
        {
            business
                .WithData("Expected", mismatch.Groups[1].Value)
                .WithData("Found", mismatch.Groups[2].Value);
        }

        return business;
    }

    public static bool RequiresReindex(string? previousModel, int previousDimensions, string? nextModel, int nextDimensions) =>
        !string.Equals(previousModel?.Trim(), nextModel?.Trim(), StringComparison.OrdinalIgnoreCase)
        || previousDimensions != nextDimensions;

    private static string Trim(string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return "Search failed.";
        }

        var text = message.Replace("\r", " ", StringComparison.Ordinal).Replace("\n", " ", StringComparison.Ordinal).Trim();
        return text.Length <= 240 ? text : text[..240];
    }
}
