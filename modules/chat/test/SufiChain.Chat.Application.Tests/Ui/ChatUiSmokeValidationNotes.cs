using Xunit;

namespace SufiChain.Chat.Ui;

/// <summary>
/// Smoke validation checklist for key Chat routes. Execute manually against the dev host in Phase 16.
/// </summary>
public class ChatUiSmokeValidationNotes
{
    [Fact(Skip = "Manual UI smoke: /chat/inbox")]
    public void Manual_Validate_User_Inbox_Route() { }

    [Fact(Skip = "Manual UI smoke: /admin/chat/inbox")]
    public void Manual_Validate_Operator_Inbox_Route() { }

    [Fact(Skip = "Manual UI smoke: /admin/chat/sessions")]
    public void Manual_Validate_Admin_Sessions_Route() { }

    [Fact(Skip = "Manual UI smoke: /admin/chat/sessions/{id}")]
    public void Manual_Validate_Session_Detail_Route() { }

    [Fact(Skip = "Manual UI smoke: /admin/chat/usage (includes AI usage section)")]
    public void Manual_Validate_Usage_Route() { }

    [Fact(Skip = "Manual UI smoke: /admin/chat/settings")]
    public void Manual_Validate_Settings_Route() { }

    [Fact(Skip = "Manual UI smoke: Start AI Chat enabled/disabled states on /chat/inbox")]
    public void Manual_Validate_Start_Ai_Chat_Availability() { }
}
