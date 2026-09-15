using System;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Connections;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Application.Tests.Connections;

/// <summary>
/// Pure domain unit tests for <see cref="TelegramConnection"/> auth-state transitions.
/// These do not require the ABP test host.
/// </summary>
public class TelegramConnection_Tests
{
    private static TelegramConnection NewConnection(bool enabled = true)
    {
        return new TelegramConnection(
            Guid.NewGuid(),
            tenantId: null,
            phoneNumber: "+989120000001",
            displayName: "Support",
            roleLabel: "Support",
            isEnabled: enabled);
    }

    [Fact]
    public void Should_Start_Disabled_And_NotReady()
    {
        var connection = NewConnection();

        connection.AuthState.ShouldBe(TelegramAuthState.Disconnected);
        connection.CanSend().ShouldBeFalse();
        connection.ForeignSessionKey.ShouldBeNull();
    }

    [Fact]
    public void PrepareForAuth_Should_Move_To_WaitPhone_And_Store_SessionKey()
    {
        var connection = NewConnection();

        connection.PrepareForAuth("session-abc");

        connection.AuthState.ShouldBe(TelegramAuthState.WaitPhone);
        connection.ForeignSessionKey.ShouldBe("session-abc");
    }

    [Fact]
    public void Auth_Flow_Should_Transition_To_Ready()
    {
        var connection = NewConnection();
        connection.PrepareForAuth("session-abc");
        connection.MarkWaitCode();
        connection.MarkReady();

        connection.AuthState.ShouldBe(TelegramAuthState.Ready);
        connection.LastHealthAt.ShouldNotBeNull();
    }

    [Fact]
    public void CanSend_Should_Be_True_Only_When_Enabled_And_Ready_And_Not_FloodWaiting()
    {
        var connection = NewConnection(enabled: true);
        connection.PrepareForAuth("s");
        connection.MarkReady();

        connection.CanSend().ShouldBeTrue();

        connection.SetEnabled(false);
        connection.CanSend().ShouldBeFalse();

        connection.SetEnabled(true);
        connection.MarkError(errorCode: "FLOOD", floodWaitUntil: DateTime.UtcNow.AddMinutes(5));
        connection.CanSend().ShouldBeFalse();
    }

    [Fact]
    public void MarkReady_Should_Clear_Error_Flags()
    {
        var connection = NewConnection();
        connection.MarkError("PEER_FLOOD", floodWaitUntil: DateTime.UtcNow.AddMinutes(10), spamRestricted: true);

        connection.SpamRestricted.ShouldBeTrue();
        connection.FloodWaitUntil.ShouldNotBeNull();

        connection.MarkReady();

        connection.SpamRestricted.ShouldBeFalse();
        connection.FloodWaitUntil.ShouldBeNull();
        connection.LastErrorCode.ShouldBeNull();
    }

    [Fact]
    public void MarkDisconnected_Should_Clear_Session()
    {
        var connection = NewConnection();
        connection.PrepareForAuth("session-abc");
        connection.MarkReady();

        connection.MarkDisconnected();

        connection.AuthState.ShouldBe(TelegramAuthState.Disconnected);
        connection.ForeignSessionKey.ShouldBeNull();
    }

    [Fact]
    public void Relabel_Should_Update_DisplayName_And_Role()
    {
        var connection = NewConnection();

        connection.Relabel("Sales Team", "Sales");

        connection.DisplayName.ShouldBe("Sales Team");
        connection.RoleLabel.ShouldBe("Sales");
    }

    [Fact]
    public void Long_Values_Should_Be_Truncated()
    {
        var connection = new TelegramConnection(
            Guid.NewGuid(),
            null,
            new string('1', 100),
            new string('d', 200),
            new string('r', 100));

        connection.PhoneNumber.Length.ShouldBe(TelegramConnectionConsts.MaxPhoneNumberLength);
        connection.DisplayName.Length.ShouldBe(TelegramConnectionConsts.MaxDisplayNameLength);
        connection.RoleLabel.Length.ShouldBe(TelegramConnectionConsts.MaxRoleLabelLength);
    }

    [Fact]
    public void ClearSpamRestriction_Should_Unset_SpamRestricted()
    {
        var connection = NewConnection();
        connection.MarkError("PEER_FLOOD", spamRestricted: true);
        connection.SpamRestricted.ShouldBeTrue();

        connection.ClearSpamRestriction();

        connection.SpamRestricted.ShouldBeFalse();
    }

    [Fact]
    public void RecordSessionWipe_Should_Set_LastWipedAt()
    {
        var connection = NewConnection();
        connection.LastWipedAt.ShouldBeNull();

        var stamped = DateTime.UtcNow;
        connection.RecordSessionWipe(stamped);

        connection.LastWipedAt.ShouldNotBeNull();
        connection.LastWipedAt!.Value.Kind.ShouldBe(DateTimeKind.Utc);
    }

    [Fact]
    public void Disconnect_Flow_Should_Clear_Session_And_Allow_Wipe_Recording()
    {
        var connection = NewConnection();
        connection.PrepareForAuth("session-abc");
        connection.MarkReady();

        connection.MarkDisconnected();
        connection.RecordSessionWipe();

        connection.AuthState.ShouldBe(TelegramAuthState.Disconnected);
        connection.ForeignSessionKey.ShouldBeNull();
        connection.LastWipedAt.ShouldNotBeNull();
    }

    [Fact]
    public void RateLimit_Aggregate_Should_Record_And_Rollover()
    {
        var row = new TelegramConnectionRateLimit(Guid.NewGuid(), tenantId: null, Guid.NewGuid(), DateTime.UtcNow);

        row.TotalSent.ShouldBe(0);
        row.FirstContactSent.ShouldBe(0);

        row.RecordSend(isFirstContact: true, DateTime.UtcNow);
        row.RecordSend(isFirstContact: false, DateTime.UtcNow);

        row.TotalSent.ShouldBe(2);
        row.FirstContactSent.ShouldBe(1);
        row.LastSentAt.ShouldNotBeNull();

        var rolled = row.RollOverIfStale(DateTime.UtcNow.AddDays(-2), Guid.NewGuid());
        rolled.ShouldBeFalse(); // today is newer than 2-days-ago so no rollover

        // Reset to yesterday then roll over to today.
        var yesterday = DateTime.UtcNow.AddDays(-1);
        var staleRow = new TelegramConnectionRateLimit(Guid.NewGuid(), null, Guid.NewGuid(), yesterday);
        var rolledOver = staleRow.RollOverIfStale(DateTime.UtcNow, Guid.NewGuid());
        rolledOver.ShouldBeTrue();
        staleRow.TotalSent.ShouldBe(0);
        staleRow.FirstContactSent.ShouldBe(0);
    }
}
