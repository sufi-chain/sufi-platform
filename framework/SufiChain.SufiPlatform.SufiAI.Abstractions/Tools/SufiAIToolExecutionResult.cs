using System;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>
/// Result of an AI tool execution.
/// </summary>
public class SufiAIToolExecutionResult
{
    /// <summary>
    /// Whether the execution was successful.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Result data (serialized to JSON for model consumption).
    /// </summary>
    public object? Result { get; set; }

    /// <summary>
    /// Error message if execution failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Exception details if execution failed.
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// Execution duration in milliseconds.
    /// </summary>
    public long ExecutionTimeMs { get; set; }

    /// <summary>
    /// Timestamp (UTC) when execution completed.
    /// </summary>
    public DateTime ExecutedAt { get; set; }

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static SufiAIToolExecutionResult CreateSuccess(object? result, long executionTimeMs)
    {
        return new SufiAIToolExecutionResult
        {
            Success = true,
            Result = result,
            ExecutionTimeMs = executionTimeMs,
            ExecutedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Creates a failed result.
    /// </summary>
    public static SufiAIToolExecutionResult CreateFailure(string errorMessage, string? exceptionDetails = null)
    {
        return new SufiAIToolExecutionResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ExceptionDetails = exceptionDetails,
            ExecutedAt = DateTime.UtcNow
        };
    }
}
