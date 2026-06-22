using System;

namespace SufiChain.SufiAbp.AI.MCP.Abstractions;

/// <summary>
/// Result of an MCP tool execution.
/// </summary>
public class MCPToolExecutionResult
{
    /// <summary>
    /// Whether the execution was successful.
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// Result data (serialized to JSON for AI consumption).
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
    /// Timestamp when execution started.
    /// </summary>
    public DateTime ExecutedAt { get; set; }
    
    public static MCPToolExecutionResult CreateSuccess(object? result, long executionTimeMs)
    {
        return new MCPToolExecutionResult
        {
            Success = true,
            Result = result,
            ExecutionTimeMs = executionTimeMs,
            ExecutedAt = DateTime.UtcNow
        };
    }
    
    public static MCPToolExecutionResult CreateFailure(string errorMessage, string? exceptionDetails = null)
    {
        return new MCPToolExecutionResult
        {
            Success = false,
            ErrorMessage = errorMessage,
            ExceptionDetails = exceptionDetails,
            ExecutedAt = DateTime.UtcNow
        };
    }
}
