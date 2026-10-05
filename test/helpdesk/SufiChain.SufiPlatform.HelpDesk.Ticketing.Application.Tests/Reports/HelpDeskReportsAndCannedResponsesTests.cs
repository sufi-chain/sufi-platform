using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Shouldly;
using SufiChain.SufiPlatform.HelpDesk.Settings;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.CannedResponses;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Permissions;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Repositories;
using SufiChain.SufiPlatform.HelpDesk.Ticketing.Tickets;
using SufiChain.SufiPlatform.Settings;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Xunit;

namespace SufiChain.SufiPlatform.HelpDesk.Ticketing.Reports;

public class HelpDeskReportsAndCannedResponsesTests
{
    [Fact]
    public async Task Report_Summary_Should_Count_Every_Status_And_Sla_State()
    {
        var repository = Substitute.For<ITicketRepository>();
        repository.GetCountByStatusAsync(Arg.Any<TicketStatus>(), Arg.Any<CancellationToken>())
            .Returns(2L);
        repository.GetCountByQueueAsync(
                Arg.Any<Guid?>(),
                Arg.Any<TicketStatus?>(),
                Arg.Any<TicketPriority?>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<Guid?>(),
                Arg.Any<TicketSlaStatus?>(),
                Arg.Any<string?>(),
                Arg.Any<CancellationToken>())
            .Returns(5L);

        var summary = await new TicketReportAppService(repository).GetSummaryAsync();

        summary.Total.ShouldBe(2L * Enum.GetValues<TicketStatus>().Length);
        summary.ByStatus.Count.ShouldBe(Enum.GetValues<TicketStatus>().Length);
        summary.ByStatus.ShouldContain(row => row.Key == nameof(TicketStatus.New) && row.Count == 2);
        summary.BySlaStatus.Count.ShouldBe(Enum.GetValues<TicketSlaStatus>().Length);
        summary.BySlaStatus.ShouldAllBe(row => row.Count == 5);
    }

    [Fact]
    public void Catalog_Should_Round_Trip_Across_Setting_Chunks()
    {
        var json = new string('x', HelpDeskSettings.TicketingCannedResponsesChunkLength + 25);
        var chunks = CannedResponseCatalog.Split(json);

        chunks.Length.ShouldBe(HelpDeskSettings.TicketingCannedResponsesChunkCount);
        chunks[0].Length.ShouldBe(HelpDeskSettings.TicketingCannedResponsesChunkLength);
        chunks[1].ShouldBe(new string('x', 25));
        CannedResponseCatalog.Join(chunks).ShouldBe(json);
        string.Join(string.Empty, chunks).Length.ShouldBeLessThanOrEqualTo(
            HelpDeskSettings.TicketingCannedResponsesChunkCount * HelpDeskSettings.TicketingCannedResponsesChunkLength);
    }

    [Fact]
    public void Catalog_Should_Reject_Json_Past_The_Chunk_Capacity()
    {
        var capacity = HelpDeskSettings.TicketingCannedResponsesChunkCount
            * HelpDeskSettings.TicketingCannedResponsesChunkLength;
        var error = Should.Throw<CannedResponseCatalogTooLargeException>(
            () => CannedResponseCatalog.Split(new string('x', capacity + 1)));

        error.Capacity.ShouldBe(capacity);
    }

    [Fact]
    public async Task Canned_Responses_Should_Create_Update_And_Delete_In_Tenant_Settings()
    {
        var service = CreateCannedResponses();

        var created = await service.CreateAsync(new CreateCannedResponseDto
        {
            Title = "  Greeting  ",
            Body = " Hello ",
            Shortcut = "  "
        });
        created.Title.ShouldBe("Greeting");
        created.Body.ShouldBe("Hello");
        created.Shortcut.ShouldBeNull();

        var updated = await service.UpdateAsync(created.Id, new UpdateCannedResponseDto
        {
            Title = "Welcome",
            Body = "Welcome aboard",
            Shortcut = "hi"
        });
        updated.Shortcut.ShouldBe("hi");

        var listed = await service.GetListAsync();
        listed.Count.ShouldBe(1);
        listed[0].Title.ShouldBe("Welcome");

        await service.DeleteAsync(created.Id);
        (await service.GetListAsync()).ShouldBeEmpty();
    }

    [Fact]
    public async Task Canned_Responses_Should_Reject_Missing_And_Oversized_Catalogs()
    {
        var service = CreateCannedResponses();

        var missingTitle = await Should.ThrowAsync<BusinessException>(() =>
            service.CreateAsync(new CreateCannedResponseDto { Title = " ", Body = "Body" }));
        missingTitle.Code.ShouldBe(TicketingErrorCodes.CannedResponseTitleRequired);

        var missing = await Should.ThrowAsync<BusinessException>(() =>
            service.DeleteAsync(Guid.NewGuid()));
        missing.Code.ShouldBe(TicketingErrorCodes.CannedResponseNotFound);

        var store = service.Store;
        store[HelpDeskSettings.TicketingCannedResponsesChunk(0)] = "{";
        var invalid = await Should.ThrowAsync<BusinessException>(() => service.GetListAsync());
        invalid.Code.ShouldBe(TicketingErrorCodes.CannedResponseCatalogInvalid);

        store.Clear();
        BusinessException? tooLarge = null;
        for (var attempt = 0; attempt < HelpDeskSettings.TicketingCannedResponsesChunkCount + 2; attempt++)
        {
            try
            {
                await service.CreateAsync(new CreateCannedResponseDto
                {
                    Title = "Item " + attempt.ToString(),
                    Body = new string('a', CannedResponseAppService.MaxBodyLength)
                });
            }
            catch (BusinessException exception) when (exception.Code == TicketingErrorCodes.CannedResponseCatalogTooLarge)
            {
                tooLarge = exception;
                break;
            }
        }

        tooLarge.ShouldNotBeNull();
    }

    [Fact]
    public void Routes_Should_Live_In_The_Ticketing_Blazor_Assembly_Both_Hosts_Already_Load()
    {
        const string reportsUrl = "/panel/admin/helpdesk/ticketing/reports";
        const string cannedResponsesUrl = "/panel/admin/helpdesk/ticketing/canned-responses";
        TicketingPermissions.GetAll().ShouldContain(TicketingPermissions.ViewReports);
        TicketingPermissions.GetAll().ShouldContain(TicketingPermissions.ManageCannedResponses);

        var reports = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/Pages/Admin/Reports.razor");
        var canned = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/Pages/Admin/CannedResponses.razor");
        var menuNames = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/Menus/TicketingMenus.cs");
        var menu = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/Menus/TicketingMenuContributor.cs");
        var blazorModule = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Blazor/HelpDeskTicketingBlazorModule.cs");
        var core = ReadRepoFile(
            "hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Blazor.WebApp/SufiPlatformModule.cs");
        var nonProduction = ReadRepoFile(
            "hosts/SufiChane.SufiPlatform/src/SufiChane.SufiPlatform.Blazor.WebApp/SufiPlatformNonProductionModule.cs");
        var permissions = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Ticketing.Application.Contracts/Permissions/TicketingPermissionDefinitionProvider.cs");
        var settings = ReadRepoFile(
            "pro-modules/helpdesk/src/SufiChain.SufiPlatform.HelpDesk.Domain/Settings/HelpDeskSettingDefinitionProvider.cs");

        menuNames.ShouldContain($"public const string ReportsUrl = \"{reportsUrl}\";");
        menuNames.ShouldContain($"public const string CannedResponsesUrl = \"{cannedResponsesUrl}\";");
        reports.ShouldContain($"@page \"{reportsUrl}\"");
        reports.ShouldContain("TicketingPermissions.Tickets.Default");
        canned.ShouldContain($"@page \"{cannedResponsesUrl}\"");
        canned.ShouldContain("TicketingPermissions.Tickets.Default");
        menu.ShouldContain("TicketingMenus.ReportsUrl");
        menu.ShouldContain("TicketingMenus.CannedResponsesUrl");
        menu.ShouldContain("TicketingPermissions.Tickets.Default");
        blazorModule.ShouldContain("options.AdditionalAssemblies.Add(typeof(HelpDeskTicketingBlazorModule).Assembly)");
        core.ShouldContain("typeof(HelpDeskTicketingBlazorModule)");
        nonProduction.ShouldContain("typeof(HelpDeskTicketingBlazorModule)");
        permissions.ShouldContain("TicketingPermissions.ViewReports");
        permissions.ShouldContain("TicketingPermissions.ManageCannedResponses");
        settings.ShouldContain("HelpDeskSettings.TicketingCannedResponsesChunk(index)");
        settings.ShouldContain("isInherited: false");
    }

    private static TestCannedResponseAppService CreateCannedResponses()
    {
        var store = new Dictionary<string, string?>(StringComparer.Ordinal);
        var settings = Substitute.For<ISettingManager>();
        settings.GetOrNullForCurrentTenantAsync(Arg.Any<string>(), Arg.Any<bool>())
            .Returns(call =>
            {
                store.TryGetValue(call.ArgAt<string>(0), out var value);
                return Task.FromResult(value);
            });
        settings.SetForCurrentTenantAsync(Arg.Any<string>(), Arg.Any<string?>())
            .Returns(call =>
            {
                store[call.ArgAt<string>(0)] = call.ArgAt<string?>(1);
                return Task.CompletedTask;
            });

        var services = new ServiceCollection();
        services.AddSingleton<IGuidGenerator>(SimpleGuidGenerator.Instance);
        return new TestCannedResponseAppService(settings, store)
        {
            LazyServiceProvider = new AbpLazyServiceProvider(services.BuildServiceProvider())
        };
    }

    private static string ReadRepoFile(string relativePath)
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory != null;
             directory = directory.Parent)
        {
            var candidate = Path.Combine(directory.FullName, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }
        }

        throw new FileNotFoundException($"Repository file was not found: {relativePath}");
    }

    private sealed class TestCannedResponseAppService : CannedResponseAppService
    {
        public TestCannedResponseAppService(ISettingManager settingManager, Dictionary<string, string?> store)
            : base(settingManager)
        {
            Store = store;
        }

        public Dictionary<string, string?> Store { get; }
    }
}
