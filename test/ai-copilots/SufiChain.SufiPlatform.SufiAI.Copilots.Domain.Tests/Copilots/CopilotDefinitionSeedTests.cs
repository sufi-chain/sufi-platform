using Shouldly;
using Volo.Abp;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Copilots.Copilots;

public class CopilotDefinitionSeedTests
{
    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Constructor_Should_Apply_Default_Enabled_Only_On_Insert(bool defaultEnabled)
    {
        var definition = CreateDefinition(defaultEnabled);

        definition.DefaultEnabled.ShouldBe(defaultEnabled);
        definition.IsEnabled.ShouldBe(defaultEnabled);
    }

    [Fact]
    public void Ensure_Custom_Key_Should_Assign_A_Stable_Key()
    {
        var id = Guid.NewGuid();
        var definition = new CopilotDefinition(
            id,
            tenantId: null,
            sourceModule: "Custom",
            displayName: "Custom Assistant",
            kind: CopilotKind.Assistant,
            purpose: "Assistant",
            workspaceId: Guid.NewGuid(),
            systemPrompt: "Test system prompt",
            persistChatSession: true);

        definition.EnsureCustomKey().ShouldBeTrue();
        definition.Key.ShouldBe($"custom.{id:N}");
        definition.Key!.Length.ShouldBeLessThanOrEqualTo(CopilotConsts.MaxKeyLength);
        definition.Key.ShouldStartWith("custom.");
        definition.Key.ShouldNotContain("-");
        definition.EnsureCustomKey().ShouldBeFalse();
        definition.Key.ShouldBe($"custom.{id:N}");
    }

    [Fact]
    public void Seed_Upgrade_Should_Preserve_Administrator_Disabled_State()
    {
        var definition = CreateDefinition(defaultEnabled: true);
        definition.TryApplySeed(CreateSeed(entityVersion: 1)).ShouldBeTrue();
        definition.Disable();

        var upgraded = CreateSeed(entityVersion: 2);
        upgraded.DisplayName = "Updated";
        upgraded.DefaultEnabled = true;

        definition.TryApplySeed(upgraded).ShouldBeTrue();

        definition.DisplayName.ShouldBe("Updated");
        definition.IsEnabled.ShouldBeFalse();
        definition.DefaultEnabled.ShouldBeTrue();
    }

    [Fact]
    public void Seed_Upgrade_Should_Enable_When_Default_Enabled_Flips_On()
    {
        var definition = CreateDefinition(defaultEnabled: false);
        definition.TryApplySeed(CreateSeed(entityVersion: 1, defaultEnabled: false)).ShouldBeTrue();

        definition.TryApplySeed(CreateSeed(entityVersion: 2, defaultEnabled: true)).ShouldBeTrue();

        definition.DefaultEnabled.ShouldBeTrue();
        definition.IsEnabled.ShouldBeTrue();
    }

    [Theory]
    [InlineData("other.key", "SufiAI.Copilots", "Assistant", CopilotKind.Assistant)]
    [InlineData("test.assistant", "OtherModule", "Assistant", CopilotKind.Assistant)]
    [InlineData("test.assistant", "SufiAI.Copilots", "OtherPurpose", CopilotKind.Assistant)]
    [InlineData("test.assistant", "SufiAI.Copilots", "Assistant", CopilotKind.Agent)]
    public void Seed_Identity_Drift_Should_Be_Rejected(
        string key,
        string sourceModule,
        string purpose,
        CopilotKind kind)
    {
        var definition = CreateDefinition(defaultEnabled: true);
        var seed = CreateSeed(entityVersion: 1);
        seed.Key = key;
        seed.SourceModule = sourceModule;
        seed.Purpose = purpose;
        seed.Kind = kind;

        var exception = Should.Throw<BusinessException>(() => definition.TryApplySeed(seed));

        exception.Code.ShouldBe(AICopilotsErrorCodes.SeedIdentityMismatch);
    }

    [Fact]
    public void Same_Version_Identity_Drift_Should_Not_Be_Silently_Ignored()
    {
        var definition = CreateDefinition(defaultEnabled: true);
        definition.TryApplySeed(CreateSeed(entityVersion: 2)).ShouldBeTrue();
        var drifted = CreateSeed(entityVersion: 2);
        drifted.SourceModule = "OtherModule";

        Should.Throw<BusinessException>(() => definition.TryApplySeed(drifted));
    }

    [Fact]
    public void Seed_Upgrade_Should_Replace_Required_Context_Contract()
    {
        var definition = CreateDefinition(defaultEnabled: true);
        var initial = CreateSeed(entityVersion: 1);
        initial.RequiredContextKeys = ["calendarId"];
        definition.TryApplySeed(initial).ShouldBeTrue();

        var upgraded = CreateSeed(entityVersion: 2);
        upgraded.RequiredContextKeys = ["calendarId", "timeZoneId", "calendarId"];
        definition.TryApplySeed(upgraded).ShouldBeTrue();

        definition.GetRequiredContextKeys().ShouldBe(["calendarId", "timeZoneId"]);
    }

    [Fact]
    public void Constructor_Should_Disable_User_Model_Selection()
    {
        var definition = CreateDefinition(defaultEnabled: true);

        definition.AllowUserModelSelection.ShouldBeFalse();
        definition.AllowedModelConfigurationIds.ShouldBeEmpty();
    }

    [Fact]
    public void Seed_Upgrade_Should_Apply_Model_Selection_Policy()
    {
        var definition = CreateDefinition(defaultEnabled: true);
        var allowedId = Guid.NewGuid();
        var seed = CreateSeed(entityVersion: 1);
        seed.AllowUserModelSelection = true;
        seed.AllowedModelConfigurationIds = [allowedId, Guid.Empty, allowedId];

        definition.TryApplySeed(seed).ShouldBeTrue();

        definition.AllowUserModelSelection.ShouldBeTrue();
        definition.AllowedModelConfigurationIds.ShouldBe([allowedId]);
    }

    [Fact]
    public void Set_Model_Selection_Policy_Should_Drop_Empty_Guids()
    {
        var definition = CreateDefinition(defaultEnabled: true);
        var allowedId = Guid.NewGuid();

        definition.SetModelSelectionPolicy(true, [Guid.Empty, allowedId, allowedId]);

        definition.AllowUserModelSelection.ShouldBeTrue();
        definition.AllowedModelConfigurationIds.ShouldBe([allowedId]);
    }

    private static CopilotDefinition CreateDefinition(bool defaultEnabled)
    {
        return new CopilotDefinition(
            Guid.NewGuid(),
            tenantId: null,
            sourceModule: "SufiAI.Copilots",
            displayName: "Test Assistant",
            kind: CopilotKind.Assistant,
            purpose: "Assistant",
            workspaceId: Guid.NewGuid(),
            systemPrompt: "Test system prompt",
            persistChatSession: true,
            key: "test.assistant",
            isStatic: true,
            defaultEnabled: defaultEnabled);
    }

    private static PlatformCopilotSeedDefinition CreateSeed(
        int entityVersion,
        bool defaultEnabled = true)
    {
        return new PlatformCopilotSeedDefinition
        {
            Key = "test.assistant",
            SourceModule = "SufiAI.Copilots",
            DisplayName = "Test Assistant",
            Kind = CopilotKind.Assistant,
            Purpose = "Assistant",
            PersistChatSession = true,
            DefaultEnabled = defaultEnabled,
            EntityVersion = entityVersion,
            SystemPrompt = "Test system prompt"
        };
    }
}
