using System.Text.Json;
using Microsoft.Extensions.Localization;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Jobs;
using SufiChain.SufiPlatform.SufiForms.Localization;
using SufiChain.SufiPlatform.SufiForms.Records;
using SufiChain.SufiPlatform.SufiForms.Records.Dtos;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormRecordOrganizationUnitDisplayTests
{
    [Fact]
    public async Task A_page_resolves_distinct_units_in_one_batch_and_names_a_missing_unit()
    {
        var finance = Guid.NewGuid();
        var missing = Guid.NewGuid();
        var fixture = new Fixture(finance);
        var first = fixture.Record(new[] { finance, missing });
        var second = fixture.Record(finance);

        await fixture.Display.ApplyAsync(fixture.Definition, [first, second]);

        first.LookupDisplayValues["department"][0].DisplayName.ShouldBe("Finance");
        first.LookupDisplayValues["department"][1].DisplayName.ShouldBe("Not found");
        first.Values["department"].ShouldBeOfType<Guid[]>();
        second.LookupDisplayValues["department"].Single().DisplayName.ShouldBe("Finance");
        await fixture.Units.Received(1).GetListAsync(
            Arg.Is<IEnumerable<Guid>>(ids => ids.Count() == 2 && ids.Contains(finance) && ids.Contains(missing)),
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task More_than_one_batch_is_still_not_one_query_per_record()
    {
        var fixture = new Fixture(Guid.NewGuid());
        var ids = Enumerable.Range(0, FormRecordOrganizationUnitDisplay.BatchSize + 1)
            .Select(_ => Guid.NewGuid())
            .ToArray();
        await fixture.Display.ApplyAsync(fixture.Definition, [fixture.Record(ids)]);

        await fixture.Units.Received(2).GetListAsync(
            Arg.Any<IEnumerable<Guid>>(),
            false,
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task A_user_lookup_and_an_empty_value_do_not_query_identity()
    {
        var fixture = new Fixture(Guid.NewGuid(), FormFieldType.UserLookup);
        var user = fixture.Record(Guid.NewGuid());
        var empty = fixture.Record(null);
        await fixture.Display.ApplyAsync(fixture.Definition, [user, empty]);

        user.LookupDisplayValues.ShouldBeEmpty();
        empty.LookupDisplayValues.ShouldBeEmpty();
        await fixture.Units.DidNotReceiveWithAnyArgs()
            .GetListAsync(default!, false, default);
    }

    [Fact]
    public async Task Detail_and_export_use_the_same_names()
    {
        var finance = Guid.NewGuid();
        var missing = Guid.NewGuid();
        var fixture = new Fixture(finance);
        var detail = fixture.Record(JsonSerializer.SerializeToElement(finance.ToString()));
        await fixture.Display.ApplyAsync(fixture.Definition, [detail]);
        FormRecordLookupText.Format(detail, "department").ShouldBe("Finance");

        var record = new FormRecord(
            Guid.NewGuid(),
            null,
            fixture.Definition.Id,
            "pulse",
            1,
            JsonSerializer.Serialize(new Dictionary<string, object?>
            {
                ["department"] = new[] { finance, missing }
            }));
        var labels = await fixture.Display.ResolveJsonAsync(fixture.Definition, [record.ValuesJson]);
        var csv = FormRecordCsvHelper.BuildCsv(
            fixture.Definition,
            [record],
            organizationUnitNames: labels);

        csv.ShouldContain("Finance, Not found");
        csv.ShouldNotContain(finance.ToString());
        csv.ShouldNotContain(missing.ToString());
    }

    private sealed class Fixture
    {
        public TestDefinition Definition { get; }
        public IOrganizationUnitRepository Units { get; } = Substitute.For<IOrganizationUnitRepository>();
        public FormRecordOrganizationUnitDisplay Display { get; }

        public Fixture(Guid financeId, FormFieldType type = FormFieldType.OrganizationUnitsLookup)
        {
            Definition = new TestDefinition(Guid.NewGuid());
            Definition.AddField(new FormField(Guid.NewGuid(), "department", type, "Department"));
            var localizer = Substitute.For<IStringLocalizer<SufiFormsResource>>();
            localizer[FormRecordOrganizationUnitDisplay.NotFoundKey]
                .Returns(new LocalizedString(FormRecordOrganizationUnitDisplay.NotFoundKey, "Not found"));
            Units.GetListAsync(Arg.Any<IEnumerable<Guid>>(), false, Arg.Any<CancellationToken>())
                .Returns(call => call.Arg<IEnumerable<Guid>>()
                    .Where(id => id == financeId)
                    .Select(id => new OrganizationUnit(id, "Finance"))
                    .ToList());
            Display = new FormRecordOrganizationUnitDisplay(Units, localizer);
        }

        public FormRecordDto Record(object? value) => new()
        {
            Id = Guid.NewGuid(),
            FormDefinitionId = Definition.Id,
            FormKey = "pulse",
            Values = new Dictionary<string, object?> { ["department"] = value }
        };
    }

    private sealed class TestDefinition : FormDefinition
    {
        public TestDefinition(Guid id)
        {
            Id = id;
            SetKey("pulse");
            SetName("Pulse");
            SetDisplayName("Pulse");
        }
    }
}
