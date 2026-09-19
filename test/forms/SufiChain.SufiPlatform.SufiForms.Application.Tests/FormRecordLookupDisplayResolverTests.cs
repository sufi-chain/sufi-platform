using System.Text.Json;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.Identity.Integration;
using SufiChain.SufiPlatform.SufiForms.Enums;
using SufiChain.SufiPlatform.SufiForms.Forms;
using SufiChain.SufiPlatform.SufiForms.Forms.Dtos;
using SufiChain.SufiPlatform.SufiForms.Mcp;
using SufiChain.SufiPlatform.SufiForms.Records;
using SufiChain.SufiPlatform.SufiForms.Records.Dtos;
using Volo.Abp.Authorization;
using Xunit;

namespace SufiChain.SufiPlatform.SufiForms;

public class FormRecordLookupDisplayResolverTests
{
    [Fact]
    public async Task Query_returns_OU_name_and_preserves_the_original_GUID_and_total_count()
    {
        var fixture = new Fixture();
        var record = fixture.Record(JsonSerializer.SerializeToElement(fixture.UnitId));
        var records = Substitute.For<IFormRecordAppService>();
        records.QueryAsync("pulse", Arg.Any<FormQueryInput>())
            .Returns(new PagedResultDto<FormRecordDto>(7, new[] { record }));
        var tools = new FormsMcpAppService(fixture.Definitions, Substitute.For<IFormDefinitionAppService>(),
            records, fixture.Resolver);

        var result = await tools.QueryRecordsAsync("pulse", new FormQueryInput());

        result.TotalCount.ShouldBe(7);
        result.Items[0].LookupDisplayValues["department"].Single().DisplayName.ShouldBe("منابع انسانی");
        ((JsonElement)record.Values["department"]!).GetGuid().ShouldBe(fixture.UnitId);
    }

    [Fact]
    public async Task Multiple_OUs_and_repeated_references_are_resolved_in_one_distinct_batch()
    {
        var fixture = new Fixture(FormFieldType.OrganizationUnitsLookup);
        var missingId = Guid.NewGuid();
        var first = fixture.Record(new[] { fixture.UnitId, missingId });
        var second = fixture.Record(JsonSerializer.SerializeToElement(new[] { fixture.UnitId }));
        await fixture.Resolver.ResolveAsync(new[] { first, second });

        first.LookupDisplayValues["department"][0].DisplayName.ShouldBe("منابع انسانی");
        first.LookupDisplayValues["department"][1].Id.ShouldBe(missingId);
        first.LookupDisplayValues["department"][1].DisplayName.ShouldBeNull();
        second.LookupDisplayValues["department"].Single().DisplayName.ShouldBe("منابع انسانی");
        await fixture.Units.Received(1).GetDisplayNamesAsync(Arg.Is<OrganizationUnitDisplayNamesInput>(input =>
            input.Ids.Count == 2 && input.Ids.Contains(fixture.UnitId) && input.Ids.Contains(missingId)));
    }

    [Fact]
    public async Task Read_permission_denial_leaves_names_unresolved_without_hiding_the_record()
    {
        var fixture = new Fixture();
        fixture.Units.GetDisplayNamesAsync(Arg.Any<OrganizationUnitDisplayNamesInput>())
            .Returns(_ => Task.FromException<List<OrganizationUnitDisplayNameDto>>(new AbpAuthorizationException()));
        var record = fixture.Record(fixture.UnitId.ToString());
        await fixture.Resolver.ResolveAsync(new[] { record });

        record.Values["department"].ShouldBe(fixture.UnitId.ToString());
        record.LookupDisplayValues["department"].Single().Id.ShouldBe(fixture.UnitId);
        record.LookupDisplayValues["department"].Single().DisplayName.ShouldBeNull();
    }

    [Fact]
    public async Task A_GUID_in_an_unrelated_field_is_not_treated_as_an_OU()
    {
        var fixture = new Fixture(FormFieldType.UserLookup);
        var record = fixture.Record(fixture.UnitId);
        await fixture.Resolver.ResolveAsync(new[] { record });
        record.LookupDisplayValues.ShouldBeEmpty();
        await fixture.Units.DidNotReceiveWithAnyArgs().GetDisplayNamesAsync(default!);
    }

    [Fact]
    public async Task Empty_or_malformed_values_do_not_trigger_identity_queries()
    {
        var fixture = new Fixture();
        var records = new[] { fixture.Record(null), fixture.Record("not-a-guid"), fixture.Record(Guid.Empty) };
        await fixture.Resolver.ResolveAsync(records);
        records.ShouldAllBe(record => record.LookupDisplayValues["department"].Count == 0);
        await fixture.Units.DidNotReceiveWithAnyArgs().GetDisplayNamesAsync(default!);
    }

    [Fact]
    public async Task Single_record_create_and_update_tools_also_return_labels()
    {
        var fixture = new Fixture();
        var record = fixture.Record(fixture.UnitId);
        var records = Substitute.For<IFormRecordAppService>();
        records.GetAsync(record.Id).Returns(record);
        records.CreateAsync("pulse", Arg.Any<CreateUpdateFormRecordDto>()).Returns(record);
        records.UpdateAsync(record.Id, Arg.Any<CreateUpdateFormRecordDto>()).Returns(record);
        var tools = new FormsMcpAppService(fixture.Definitions, Substitute.For<IFormDefinitionAppService>(),
            records, fixture.Resolver);

        (await tools.GetRecordAsync(record.Id)).LookupDisplayValues["department"].Single().DisplayName.ShouldBe("منابع انسانی");
        record.LookupDisplayValues.Clear();
        (await tools.CreateRecordAsync("pulse", new())).LookupDisplayValues["department"].Single().DisplayName.ShouldBe("منابع انسانی");
        record.LookupDisplayValues.Clear();
        (await tools.UpdateRecordAsync(record.Id, new())).LookupDisplayValues["department"].Single().DisplayName.ShouldBe("منابع انسانی");
    }

    private sealed class Fixture
    {
        public Guid UnitId { get; } = Guid.NewGuid();
        public TestDefinition Definition { get; } = new(Guid.NewGuid());
        public IFormDefinitionRepository Definitions { get; } = Substitute.For<IFormDefinitionRepository>();
        public IOrganizationUnitIntegrationService Units { get; } = Substitute.For<IOrganizationUnitIntegrationService>();
        public FormRecordLookupDisplayResolver Resolver { get; }

        public Fixture(FormFieldType type = FormFieldType.OrganizationUnitLookup)
        {
            Definition.AddField(new FormField(Guid.NewGuid(), "department", type, "واحد سازمانی"));
            Definitions.GetAsync(Definition.Id, true, default).Returns(Definition);
            Units.GetDisplayNamesAsync(Arg.Any<OrganizationUnitDisplayNamesInput>())
                .Returns(new List<OrganizationUnitDisplayNameDto>
                {
                    new() { Id = UnitId, DisplayName = "منابع انسانی" }
                });
            Resolver = new FormRecordLookupDisplayResolver(Definitions, Units);
        }

        public FormRecordDto Record(object? value) => new()
        {
            Id = Guid.NewGuid(), FormDefinitionId = Definition.Id, FormKey = "pulse",
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
            SetDisplayName("Pulse survey");
        }
    }
}
