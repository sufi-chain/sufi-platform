using Shouldly;
using SufiChain.SufiPlatform.SufiFinance.Accounting;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiFinance.Tests;

public class AccountingTests
{
    [Fact]
    public void Chart_of_accounts_supports_arbitrary_depth()
    {
        var chart = new ChartOfAccounts(Guid.NewGuid(), null, "Default", "USD");
        var assets = chart.AddAccount(Guid.NewGuid(), "1000", "Assets", AccountType.Asset, isPostingAllowed: false);
        var currentAssets = chart.AddAccount(Guid.NewGuid(), "1100", "Current Assets", AccountType.Asset, assets.Id, false);
        var cash = chart.AddAccount(Guid.NewGuid(), "1110", "Cash", AccountType.Asset, currentAssets.Id, false);
        var bank = chart.AddAccount(Guid.NewGuid(), "1110-01", "Bank Account A", AccountType.Asset, cash.Id);

        bank.ParentId.ShouldBe(cash.Id);
        chart.Accounts.Count.ShouldBe(4);
    }

    [Fact]
    public void Posted_journal_entry_must_balance_in_both_currencies()
    {
        var entry = new JournalEntry(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "JE-1",
            new DateTime(2026, 8, 31),
            "USD",
            "USD",
            new AccountingSourceReference("Payment", Guid.NewGuid().ToString()));

        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), 100m, 0m, 100m, 0m);
        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), 0m, 100m, 0m, 100m);

        entry.Post(new DateTime(2026, 8, 31));

        entry.Status.ShouldBe(JournalEntryStatus.Posted);
        Should.Throw<BusinessException>(() => entry.AddLine(
            Guid.NewGuid(), Guid.NewGuid(), 1m, 0m, 1m, 0m));
    }

    [Fact]
    public void Unbalanced_journal_entry_cannot_be_posted()
    {
        var entry = new JournalEntry(
            Guid.NewGuid(),
            null,
            Guid.NewGuid(),
            Guid.NewGuid(),
            "JE-2",
            new DateTime(2026, 8, 31),
            "USD",
            "USD",
            new AccountingSourceReference("Invoice", Guid.NewGuid().ToString()));

        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), 100m, 0m, 100m, 0m);
        entry.AddLine(Guid.NewGuid(), Guid.NewGuid(), 0m, 90m, 0m, 90m);

        Should.Throw<BusinessException>(() => entry.Post(new DateTime(2026, 8, 31)));
        entry.Status.ShouldBe(JournalEntryStatus.Draft);
    }

    [Fact]
    public void Fiscal_periods_cannot_overlap_and_locked_periods_cannot_reopen()
    {
        var year = new FiscalYear(
            Guid.NewGuid(),
            null,
            "FY2026",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 12, 31));

        var period = year.AddPeriod(
            Guid.NewGuid(),
            "2026-01",
            new DateTime(2026, 1, 1),
            new DateTime(2026, 1, 31));

        Should.Throw<BusinessException>(() => year.AddPeriod(
            Guid.NewGuid(),
            "Overlap",
            new DateTime(2026, 1, 15),
            new DateTime(2026, 2, 15)));

        year.ClosePeriod(period.Id);
        year.LockPeriod(period.Id);
        Should.Throw<BusinessException>(() => year.ReopenPeriod(period.Id));
    }

    [Fact]
    public void Dimensions_are_tenant_defined_and_values_are_unique()
    {
        var dimension = new Dimension(Guid.NewGuid(), null, "Department", "Department");
        dimension.AddValue(Guid.NewGuid(), "SALES", "Sales");

        Should.Throw<BusinessException>(() => dimension.AddValue(
            Guid.NewGuid(), "sales", "Duplicate"));
    }
}
