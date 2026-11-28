using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Shouldly;
using SufiChain.SufiPlatform.Calendar.AI.Tools;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Data;
using SufiChain.SufiPlatform.Calendar.Hooshvare.Hooshvare;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.SufiAI.Data;
using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Guids;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Testing;
using Volo.Abp.Uow;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

public class EfCalendarAssistantSeedPersistenceTests : CalendarAssistantSeedPersistenceTests<HooshvareRagBindingSqliteTestModule>
{
}

public class MongoCalendarAssistantSeedPersistenceTests : CalendarAssistantSeedPersistenceTests<HooshvareRagBindingMongoTestModule>
{
}

public abstract class CalendarAssistantSeedPersistenceTests<TModule> : AbpIntegratedTest<TModule>
    where TModule : IAbpModule
{
    protected override void SetAbpApplicationCreationOptions(AbpApplicationCreationOptions options) =>
        options.UseAutofac();

    [Fact]
    public async Task SufiCalendar_Assistant_Seeds_Once_On_The_Hooshvare_Store()
    {
        var repository = GetRequiredService<IHooshvareDefinitionRepository>();
        var existing = new HooshvareDefinition(
            Guid.NewGuid(),
            tenantId: null,
            sourceModule: "Calendar",
            displayName: "previous",
            kind: HooshvareKind.Copilot,
            purpose: "Assistant",
            workspaceId: Guid.Empty,
            systemPrompt: "previous",
            persistChatSession: true,
            isPublic: true,
            key: CalendarAssistantHooshvareKeys.Key,
            isStatic: true);
        existing.SetEntityVersion(0);

        await InUnitAsync(async () =>
        {
            await repository.InsertAsync(existing, autoSave: true);
            await CreateContributor().SeedAsync(new DataSeedContext(null));
            await CreateContributor().SeedAsync(new DataSeedContext(null));
        });

        var rows = await InUnitAsync(() => repository.GetListAsync());
        var matches = rows.Where(row => row.Key == CalendarAssistantHooshvareKeys.Key).ToList();
        matches.Count.ShouldBe(1);
        matches[0].Id.ShouldBe(existing.Id);
        matches[0].SourceModule.ShouldBe("Calendar");
        matches[0].GetRequiredContextKeys().ShouldBe(["calendarId"]);
        matches[0].RuntimeOptions.UseMcpTools.ShouldBeTrue();
        matches[0].RuntimeOptions.AllowedMcpToolNames.ShouldContain(CalendarAIToolNames.ListCalendars);
        matches[0].RuntimeOptions.AllowedMcpToolNames.Count.ShouldBe(12);
    }

    private CalendarAssistantHooshvareDataSeedContributor CreateContributor()
    {
        var seeder = new PlatformHooshvareDefinitionSeeder(
            GetRequiredService<IHooshvareDefinitionRepository>(),
            GetRequiredService<ICurrentTenant>(),
            new NoWorkspaceSeeder(),
            SimpleGuidGenerator.Instance,
            new AcceptingSeedValidator(),
            NullLogger<PlatformHooshvareDefinitionSeeder>.Instance);
        return new CalendarAssistantHooshvareDataSeedContributor(
            seeder,
            new HooshvareLocalizedShortcutPromptsBuilder(Options.Create(new AbpLocalizationOptions())),
            new NoopLocalizationTextSeeder());
    }

    private async Task InUnitAsync(Func<Task> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        await action();
        await uow.CompleteAsync();
    }

    private async Task<T> InUnitAsync<T>(Func<Task<T>> action)
    {
        using var uow = GetRequiredService<IUnitOfWorkManager>().Begin(requiresNew: true, isTransactional: false);
        var result = await action();
        await uow.CompleteAsync();
        return result;
    }

    private sealed class NoWorkspaceSeeder : IDefaultAiWorkspaceSeeder
    {
        public Task<Guid?> EnsureDefaultWorkspaceAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(null);

        public Task<Guid?> EnsureDefaultWorkspaceAsync(Guid? tenantId, CancellationToken cancellationToken = default) =>
            Task.FromResult<Guid?>(null);
    }

    private sealed class AcceptingSeedValidator : IPlatformHooshvareSeedValidator
    {
        public Task ValidateAsync(PlatformHooshvareSeedDefinition definition, CancellationToken cancellationToken = default) =>
            Task.CompletedTask;
    }

    private sealed class NoopLocalizationTextSeeder : ILocalizationTextSeeder
    {
        public Task UpsertAsync(
            string resourceName,
            string key,
            IReadOnlyDictionary<string, string> cultureValues,
            Guid? tenantId = null,
            bool overwriteExisting = false,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task UpsertManyAsync(
            string resourceName,
            IReadOnlyDictionary<string, IReadOnlyDictionary<string, string>> keysByCulture,
            Guid? tenantId = null,
            bool overwriteExisting = false,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task ReplaceKeyValuesAsync(
            string resourceName,
            string key,
            IReadOnlyDictionary<string, string> cultureValues,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<string?> FindValueAsync(
            string resourceName,
            string cultureName,
            string key,
            Guid? tenantId = null,
            CancellationToken cancellationToken = default) => Task.FromResult<string?>(null);
    }
}
