using System.Diagnostics;
using System.Security.Claims;
using Bunit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiBlazor.Components.Navigation;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using SufiChain.SufiPlatform.UI.Blazor.Components;
using Shouldly;
using Volo.Abp.Authorization;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SufiSettingsSectionTests : BunitContext
{
    private readonly RecordingAuthorizationService _authorization = new();

    public SufiSettingsSectionTests()
    {
        Services.AddSingleton<IAuthorizationService>(_authorization);
    }

    [Fact]
    public void Edit_grant_shows_an_editable_section_and_skips_the_read_only_policy()
    {
        _authorization.Set("Settings.Edit", true);
        _authorization.Set("Settings.View", true);

        var cut = RenderSection(policy: "Settings.Edit", readOnlyPolicy: "Settings.View");

        cut.WaitForAssertion(() =>
        {
            var section = cut.FindComponent<SbSettingsSection>().Instance;
            section.Pending.ShouldBeFalse();
            section.Visible.ShouldBeTrue();
            section.ReadOnly.ShouldBeFalse();
        });
        _authorization.Calls.ShouldBe(new[] { "Settings.Edit" });
    }

    [Fact]
    public void View_only_grant_shows_a_read_only_section()
    {
        _authorization.Set("Settings.Edit", false);
        _authorization.Set("Settings.View", true);

        var cut = RenderSection(policy: "Settings.Edit", readOnlyPolicy: "Settings.View");

        cut.WaitForAssertion(() =>
        {
            var section = cut.FindComponent<SbSettingsSection>().Instance;
            section.Pending.ShouldBeFalse();
            section.Visible.ShouldBeTrue();
            section.ReadOnly.ShouldBeTrue();
        });
        _authorization.Calls.ShouldBe(new[] { "Settings.Edit", "Settings.View" });
    }

    [Fact]
    public void Neither_grant_hides_the_section()
    {
        _authorization.Set("Settings.Edit", false);
        _authorization.Set("Settings.View", false);

        var cut = RenderSection(policy: "Settings.Edit", readOnlyPolicy: "Settings.View");

        cut.WaitForAssertion(() =>
        {
            var section = cut.FindComponent<SbSettingsSection>().Instance;
            section.Pending.ShouldBeFalse();
            section.Visible.ShouldBeFalse();
            section.ReadOnly.ShouldBeFalse();
        });
    }

    [Fact]
    public void No_policy_is_visible_without_a_permission_check()
    {
        var cut = RenderSection(policy: null, readOnlyPolicy: null);
        var section = cut.FindComponent<SbSettingsSection>().Instance;

        section.Pending.ShouldBeFalse();
        section.Visible.ShouldBeTrue();
        section.ReadOnly.ShouldBeFalse();
        _authorization.Calls.ShouldBeEmpty();
    }

    [Fact]
    public void Permission_check_stays_pending_until_it_resolves_and_is_cached()
    {
        var gate = _authorization.Gate("Settings.Edit");
        var cut = RenderSection(policy: "Settings.Edit", readOnlyPolicy: "Settings.View");
        var section = cut.FindComponent<SbSettingsSection>().Instance;

        section.Pending.ShouldBeTrue();
        section.Visible.ShouldBeFalse();

        gate.SetResult(true);
        cut.WaitForAssertion(() => cut.FindComponent<SbSettingsSection>().Instance.Pending.ShouldBeFalse());
        cut.FindComponent<SbSettingsSection>().Instance.Visible.ShouldBeTrue();
        var calls = _authorization.Calls.Count;

        cut.Render(parameters => parameters
            .Add(parameter => parameter.Id, "email")
            .Add(parameter => parameter.Label, "Email changed")
            .Add(parameter => parameter.Icon, "mail")
            .Add(parameter => parameter.Policy, "Settings.Edit")
            .Add(parameter => parameter.ReadOnlyPolicy, "Settings.View"));

        cut.FindComponent<SbSettingsSection>().Instance.Pending.ShouldBeFalse();
        _authorization.Calls.Count.ShouldBe(calls);
    }

    private IRenderedComponent<SufiSettingsSection> RenderSection(string? policy, string? readOnlyPolicy)
    {
        return Render<SufiSettingsSection>(parameters => parameters
            .Add(parameter => parameter.Id, "email")
            .Add(parameter => parameter.Label, "Email")
            .Add(parameter => parameter.Icon, "mail")
            .Add(parameter => parameter.Policy, policy)
            .Add(parameter => parameter.ReadOnlyPolicy, readOnlyPolicy));
    }

    private sealed class RecordingAuthorizationService : IAbpAuthorizationService
    {
        private readonly Dictionary<string, TaskCompletionSource<bool>> _gates = new(StringComparer.Ordinal);

        public ClaimsPrincipal CurrentPrincipal { get; } = new(new ClaimsIdentity());

        public IServiceProvider ServiceProvider { get; } = new ServiceCollection().BuildServiceProvider();

        public List<string> Calls { get; } = new();

        public void Set(string policy, bool granted)
        {
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            gate.SetResult(granted);
            _gates[policy] = gate;
        }

        public TaskCompletionSource<bool> Gate(string policy)
        {
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            _gates[policy] = gate;
            return gate;
        }

        public Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, IEnumerable<IAuthorizationRequirement> requirements) =>
            Task.FromResult(AuthorizationResult.Failed());

        public async Task<AuthorizationResult> AuthorizeAsync(ClaimsPrincipal user, object? resource, string policyName)
        {
            Calls.Add(policyName);
            if (!_gates.TryGetValue(policyName, out var gate))
            {
                return AuthorizationResult.Failed();
            }

            return await gate.Task ? AuthorizationResult.Success() : AuthorizationResult.Failed();
        }
    }
}

public class SettingComponentCreationContextTests
{
    [Fact]
    public void Normalize_logs_a_duplicate_order_and_keeps_sorting()
    {
        var listener = new RecordingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            var context = new SettingComponentCreationContext(new ServiceCollection().BuildServiceProvider());
            context.Groups.Add(Group("later", 20));
            context.Groups.Add(Group("first", 10));
            context.Groups.Add(Group("also-ten", 10));

            context.Normalize();

            context.Groups.Select(group => group.Id).ShouldBe(new[] { "first", "also-ten", "later" });
            listener.Messages.ShouldContain(message => message.Contains("10", StringComparison.Ordinal) && message.Contains("first", StringComparison.Ordinal) && message.Contains("also-ten", StringComparison.Ordinal));
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }
    }

    [Fact]
    public void Normalize_is_quiet_when_every_order_is_unique()
    {
        var listener = new RecordingTraceListener();
        Trace.Listeners.Add(listener);
        try
        {
            var context = new SettingComponentCreationContext(new ServiceCollection().BuildServiceProvider());
            context.Groups.Add(Group("email", 100));
            context.Groups.Add(Group("identity", 200));

            context.Normalize();

            listener.Messages.ShouldBeEmpty();
        }
        finally
        {
            Trace.Listeners.Remove(listener);
        }
    }

    private static SettingComponentGroup Group(string id, int order) => new()
    {
        Id = id,
        DisplayName = id,
        Icon = "mail",
        Order = order,
        ComponentType = typeof(SettingComponentCreationContextTests)
    };

    private sealed class RecordingTraceListener : TraceListener
    {
        public List<string> Messages { get; } = new();

        public override void Write(string? message)
        {
            if (!string.IsNullOrEmpty(message))
            {
                Messages.Add(message);
            }
        }

        public override void WriteLine(string? message) => Write(message);
    }
}
