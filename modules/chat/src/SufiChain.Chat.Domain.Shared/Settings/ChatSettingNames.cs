namespace SufiChain.Chat.Settings;

public static class ChatSettingNames
{
    public static class General
    {
        public const string MaxConcurrentOpenSessions = "Chat.Usage.MaxConcurrentOpenSessions";
        public const string MaxMessagesPerTenantPerDay = "Chat.Usage.MaxMessagesPerTenantPerDay";
    }

    public static class Usage
    {
        public static class PublicAnonymous
        {
            public const string MaxSessionsPerUserPerDay = "Chat.Usage.PublicAnonymous.MaxSessionsPerUserPerDay";
            public const string MaxMessagesPerSession = "Chat.Usage.PublicAnonymous.MaxMessagesPerSession";
            public const string MaxAttachmentsPerSession = "Chat.Usage.PublicAnonymous.MaxAttachmentsPerSession";
            public const string MaxAttachmentBytesPerSession = "Chat.Usage.PublicAnonymous.MaxAttachmentBytesPerSession";
            public const string EnableIpGuard = "Chat.Usage.PublicAnonymous.EnableIpGuard";
            public const string MaxSessionsPerIpPerDay = "Chat.Usage.PublicAnonymous.MaxSessionsPerIpPerDay";
            public const string MaxMessagesPerIpPerDay = "Chat.Usage.PublicAnonymous.MaxMessagesPerIpPerDay";
            public const string MaxAiSessionsPerIpPerHour = "Chat.Usage.PublicAnonymous.MaxAiSessionsPerIpPerHour";
            public const string MaxMessagesBeforeSignupRequired = "Chat.Usage.PublicAnonymous.MaxMessagesBeforeSignupRequired";
            public const string MaxAiQuestionsBeforeSignupRequired = "Chat.Usage.PublicAnonymous.MaxAiQuestionsBeforeSignupRequired";
            public const string LimitExceededAction = "Chat.Usage.PublicAnonymous.LimitExceededAction";
        }

        public static class PublicAuthenticated
        {
            public const string MaxSessionsPerUserPerDay = "Chat.Usage.PublicAuthenticated.MaxSessionsPerUserPerDay";
            public const string MaxMessagesPerSession = "Chat.Usage.PublicAuthenticated.MaxMessagesPerSession";
            public const string MaxAttachmentsPerSession = "Chat.Usage.PublicAuthenticated.MaxAttachmentsPerSession";
            public const string MaxAttachmentBytesPerSession = "Chat.Usage.PublicAuthenticated.MaxAttachmentBytesPerSession";
            public const string LimitExceededAction = "Chat.Usage.PublicAuthenticated.LimitExceededAction";
        }

        public static class Internal
        {
            public const string MaxSessionsPerUserPerDay = "Chat.Usage.Internal.MaxSessionsPerUserPerDay";
            public const string MaxMessagesPerSession = "Chat.Usage.Internal.MaxMessagesPerSession";
            public const string MaxAttachmentsPerSession = "Chat.Usage.Internal.MaxAttachmentsPerSession";
            public const string MaxAttachmentBytesPerSession = "Chat.Usage.Internal.MaxAttachmentBytesPerSession";
            public const string LimitExceededAction = "Chat.Usage.Internal.LimitExceededAction";
        }

        public static class GlobalFloor
        {
            public const string MaxMessagesPerSession = "Chat.Usage.GlobalFloor.MaxMessagesPerSession";
            public const string MaxAttachmentBytesPerSession = "Chat.Usage.GlobalFloor.MaxAttachmentBytesPerSession";
        }
    }

    public static class Ai
    {
        public const string Enabled = "Chat.Ai.Enabled";
        public const string UsageGuard = "Chat.Ai.UsageGuard";
        public const string RequireOperatorForAnonymousHandoff = "Chat.Ai.RequireOperatorForAnonymousHandoff";
        public const string DefaultWorkspaceName = "Chat.Ai.DefaultWorkspaceName";
        public const string MaxRepliesPerSession = "Chat.Ai.MaxRepliesPerSession";
        public const string MaxTokensPerSession = "Chat.Ai.MaxTokensPerSession";
        public const string MaxTokensPerTenantPerDay = "Chat.Ai.MaxTokensPerTenantPerDay";
        public const string MaxAnonymousAiSessionsPerHour = "Chat.Ai.MaxAnonymousAiSessionsPerHour";
        public const string MaxSuggestionsPerOperatorPerDay = "Chat.Ai.MaxSuggestionsPerOperatorPerDay";
        public const string MaxSummariesPerOperatorPerDay = "Chat.Ai.MaxSummariesPerOperatorPerDay";
        public const string MaxCopilotMessagesPerArticlePerDay = "Chat.Ai.MaxCopilotMessagesPerArticlePerDay";
        public const string MaxRagChunksPerRequest = "Chat.Ai.MaxRagChunksPerRequest";
        public const string DoNotTrain = "Chat.Ai.DoNotTrain";
        public const string FallbackMessageKey = "Chat.Ai.FallbackMessageKey";
    }

    public static class Retention
    {
        public const string MessageRetentionDays = "Chat.Retention.MessageRetentionDays";
        public const string ClosedSessionRetentionDays = "Chat.Retention.ClosedSessionRetentionDays";
        public const string UsageRecordRetentionDays = "Chat.Retention.UsageRecordRetentionDays";
    }

    public static class Realtime
    {
        public const string Enabled = "Chat.Realtime.Enabled";
        public const string TypingIndicatorTtlSeconds = "Chat.Realtime.TypingIndicatorTtlSeconds";
        public const string PresenceTtlSeconds = "Chat.Realtime.PresenceTtlSeconds";
    }

    public static class EmailConnector
    {
        public const string Enabled = "Chat.Connector.Email.Enabled";
        public const string DefaultFromAddress = "Chat.Connector.Email.DefaultFromAddress";
        public const string ReplyToAddress = "Chat.Connector.Email.ReplyToAddress";
        public const string InboundProtocol = "Chat.Connector.Email.InboundProtocol";
        public const string InboundHost = "Chat.Connector.Email.InboundHost";
        public const string InboundPort = "Chat.Connector.Email.InboundPort";
        public const string InboundUseSsl = "Chat.Connector.Email.InboundUseSsl";
        public const string InboundUserName = "Chat.Connector.Email.InboundUserName";
        public const string InboundPassword = "Chat.Connector.Email.InboundPassword";
        public const string SmtpHost = "Chat.Connector.Email.SmtpHost";
        public const string SmtpPort = "Chat.Connector.Email.SmtpPort";
        public const string SmtpUseSsl = "Chat.Connector.Email.SmtpUseSsl";
        public const string SmtpUserName = "Chat.Connector.Email.SmtpUserName";
        public const string SmtpPassword = "Chat.Connector.Email.SmtpPassword";
    }
}
