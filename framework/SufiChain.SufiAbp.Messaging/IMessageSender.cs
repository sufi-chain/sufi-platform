namespace SufiChain.SufiAbp.Messaging;

/// <summary>
/// Base interface for all message senders (Email, SMS, Voice)
/// </summary>
public interface IMessageSender
{
    /// <summary>
    /// Gets the message type this sender handles
    /// </summary>
    MessageType MessageType { get; }
}

/// <summary>
/// Message types supported by the messaging system
/// </summary>
public enum MessageType
{
    Email = 1,
    Sms = 2,
    VoiceCall = 3
}
