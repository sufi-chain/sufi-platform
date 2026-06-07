namespace SufiChain.Chat;

public static class ChatErrorCodes
{
    public const string SessionNotFound = "Chat:SessionNotFound";
    public const string SessionClosed = "Chat:SessionClosed";
    public const string MessageNotFound = "Chat:MessageNotFound";
    public const string ParticipantNotFound = "Chat:ParticipantNotFound";
    public const string ParticipantRequired = "Chat:ParticipantRequired";
    public const string InvalidParticipant = "Chat:InvalidParticipant";
    public const string DirectSessionRequiresTwoParticipants = "Chat:DirectSessionRequiresTwoParticipants";
    public const string GroupParticipantLimitExceeded = "Chat:GroupParticipantLimitExceeded";
    public const string UsageLimitExceeded = "Chat:UsageLimitExceeded";
    public const string InvalidAttachment = "Chat:InvalidAttachment";
    public const string AttachmentsDisabled = "Chat:AttachmentsDisabled";
    public const string LocationSharingDisabled = "Chat:LocationSharingDisabled";
    public const string VoiceMessagesDisabled = "Chat:VoiceMessagesDisabled";
    public const string MaxFilesPerMessageExceeded = "Chat:MaxFilesPerMessageExceeded";
    public const string MessageContentRequired = "Chat:MessageContentRequired";
    public const string AiUnavailable = "Chat:AiUnavailable";
    public const string ConnectorNotRegistered = "Chat:ConnectorNotRegistered";
    public const string ConnectorExternalThreadIdRequired = "Chat:ConnectorExternalThreadIdRequired";
    public const string ConnectorExternalMessageIdRequired = "Chat:ConnectorExternalMessageIdRequired";
    public const string EmailConnectorDisabled = "Chat:EmailConnectorDisabled";
}
