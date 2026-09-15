using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Wallets;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Domain.Tests;

public class WalletTests
{
    [Fact]
    public void Reserve_Partial_Settle_Should_Debit_Actual_And_Release_Remainder()
    {
        var wallet = CreateWallet();
        wallet.Credit(Guid.NewGuid(), 100, "Test", "credit", "credit-1", Now, null, Guid.NewGuid());
        var reservation = wallet.Reserve(Guid.NewGuid(), 80, "Usage", "operation", "reserve-1", Now, Now.AddMinutes(10), Guid.NewGuid());

        var entry = wallet.SettleReservation(reservation.Id, Guid.NewGuid(), 50, "settle-1", Now, null,
            Guid.NewGuid(), Guid.NewGuid());

        wallet.Balance.ShouldBe(50);
        wallet.ReservedAmount.ShouldBe(0);
        wallet.AvailableBalance.ShouldBe(50);
        reservation.Status.ShouldBe(FundReservationStatus.Settled);
        reservation.SettledAmount.ShouldBe(50);
        entry.EntryType.ShouldBe(WalletEntryType.ReservationSettlement);
    }

    [Fact]
    public void Duplicate_Credit_Should_Return_Original_Entry()
    {
        var wallet = CreateWallet();
        var original = wallet.Credit(Guid.NewGuid(), 25, "Payment", "p1", "same-key", Now, null, Guid.NewGuid());

        var duplicate = wallet.Credit(Guid.NewGuid(), 25, "Payment", "p1", "same-key", Now, null, Guid.NewGuid());

        duplicate.Id.ShouldBe(original.Id);
        wallet.Balance.ShouldBe(25);
        wallet.Entries.Count.ShouldBe(1);
    }

    [Fact]
    public void Conflicting_Idempotency_ReUse_Should_Fail()
    {
        var wallet = CreateWallet();
        wallet.Credit(Guid.NewGuid(), 25, "Payment", "p1", "same-key", Now, null, Guid.NewGuid());

        var exception = Should.Throw<BusinessException>(() =>
            wallet.Credit(Guid.NewGuid(), 30, "Payment", "p1", "same-key", Now, null, Guid.NewGuid()));

        exception.Code.ShouldBe(SufiFinanceWalletsErrorCodes.IdempotencyConflict);
    }

    [Fact]
    public void Release_And_Expiry_Should_Be_Idempotent()
    {
        var wallet = CreateWallet();
        wallet.Credit(Guid.NewGuid(), 100, "Test", null, "credit", Now, null, Guid.NewGuid());
        var reservation = wallet.Reserve(Guid.NewGuid(), 40, "Test", null, "reserve", Now, Now.AddMinutes(-1), Guid.NewGuid());

        wallet.ReleaseReservation(reservation.Id, Now, Guid.NewGuid(), "expired", expired: true);
        wallet.ReleaseReservation(reservation.Id, Now, Guid.NewGuid(), "expired", expired: true);

        wallet.ReservedAmount.ShouldBe(0);
        reservation.Status.ShouldBe(FundReservationStatus.Expired);
    }

    [Fact]
    public void Reversal_Should_Append_Compensating_Entry()
    {
        var wallet = CreateWallet();
        var credit = wallet.Credit(Guid.NewGuid(), 100, "Manual", null, "credit", Now, null, Guid.NewGuid());

        var reversal = wallet.Reverse(credit.Id, Guid.NewGuid(), "reverse", "correction", Now, null, Guid.NewGuid());

        wallet.Balance.ShouldBe(0);
        wallet.Entries.Count.ShouldBe(2);
        reversal.ReversalOfEntryId.ShouldBe(credit.Id);
        wallet.Entries.Sum(entry => entry.EntryType is WalletEntryType.Credit or WalletEntryType.ReversalCredit
            ? entry.Amount : -entry.Amount).ShouldBe(wallet.Balance);
    }

    private static readonly DateTime Now = new(2026, 7, 15, 12, 0, 0, DateTimeKind.Utc);

    private static Wallet CreateWallet() =>
        new(Guid.NewGuid(), null, "User", Guid.NewGuid().ToString(), "USD", Guid.NewGuid(), Now);
}
