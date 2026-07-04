using System.Collections.Generic;

namespace SufiChain.SufiAbp.Communications;

/// <summary>
/// Additional arguments for message sending operations
/// </summary>
public class AdditionalMessageSendingArgs
{
    /// <summary>
    /// Priority level for the message
    /// </summary>
    public MessagePriority Priority { get; set; } = MessagePriority.Normal;
    
    /// <summary>
    /// Whether to queue the message for background processing
    /// </summary>
    public bool QueueMessage { get; set; }
    
    /// <summary>
    /// Custom metadata for the message
    /// </summary>
    public Dictionary<string, object> Metadata { get; set; } = new();
    
    /// <summary>
    /// Template name if using templated messages
    /// </summary>
    public string? TemplateName { get; set; }
    
    /// <summary>
    /// Template model data
    /// </summary>
    public object? TemplateModel { get; set; }
}

public enum MessagePriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
