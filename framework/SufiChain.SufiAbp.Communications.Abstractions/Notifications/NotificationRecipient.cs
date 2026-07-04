namespace SufiChain.SufiAbp.Communications.Notifications;

/// <summary>
/// A single notification target. Only the fields relevant to the enabled channels
/// need to be populated; the publisher drops channels that lack a resolvable address.
/// </summary>
[Serializable]
public class NotificationRecipient
{
    /// <summary>
    /// Target user id (used for the InApp channel).
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// Target email address (used for the Email channel).
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Target phone number, E.164 (used for Sms and Voice channels).
    /// </summary>
    public string? PhoneNumber { get; set; }

    public string? DisplayName { get; set; }
}
