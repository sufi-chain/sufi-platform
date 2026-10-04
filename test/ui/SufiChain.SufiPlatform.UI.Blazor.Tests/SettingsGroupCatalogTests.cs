using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Settings.Blazor.Settings;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class SettingsGroupCatalogTests
{
    [Fact]
    public async Task A_denied_contributor_adds_no_section()
    {
        var groups = await SettingsGroupCatalog.LoadAsync(
            [new FakeContributor(allow: false, throwOnCheck: false, throwAfterAdd: false)],
            new ServiceCollection().BuildServiceProvider());

        groups.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_throwing_permission_check_adds_no_section()
    {
        var groups = await SettingsGroupCatalog.LoadAsync(
            [new FakeContributor(allow: true, throwOnCheck: true, throwAfterAdd: false)],
            new ServiceCollection().BuildServiceProvider());

        groups.ShouldBeEmpty();
    }

    [Fact]
    public async Task A_contributor_that_throws_after_adding_a_group_is_removed()
    {
        var groups = await SettingsGroupCatalog.LoadAsync(
            [
                new FakeContributor(allow: true, throwOnCheck: false, throwAfterAdd: true, id: "email", order: 100),
                new FakeContributor(allow: true, throwOnCheck: false, throwAfterAdd: false, id: "identity", order: 200)
            ],
            new ServiceCollection().BuildServiceProvider());

        groups.Select(group => group.Id).ShouldBe(["identity"]);
    }

    [Fact]
    public async Task Granted_groups_keep_their_icons_and_sort_by_order()
    {
        var groups = await SettingsGroupCatalog.LoadAsync(
            [
                new FakeContributor(allow: true, throwOnCheck: false, throwAfterAdd: false, id: "identity", order: 200, icon: "user-cog"),
                new FakeContributor(allow: true, throwOnCheck: false, throwAfterAdd: false, id: "email", order: 100, icon: "mail")
            ],
            new ServiceCollection().BuildServiceProvider());

        groups.Select(group => group.Id).ShouldBe(["email", "identity"]);
        groups.Select(group => group.Icon).ShouldBe(["mail", "user-cog"]);
    }

    private sealed class FakeContributor : ISettingComponentContributor
    {
        private readonly bool _allow;
        private readonly bool _throwOnCheck;
        private readonly bool _throwAfterAdd;
        private readonly string _id;
        private readonly int _order;
        private readonly string _icon;

        public FakeContributor(bool allow, bool throwOnCheck, bool throwAfterAdd, string id = "email", int order = 100, string icon = "mail")
        {
            _allow = allow;
            _throwOnCheck = throwOnCheck;
            _throwAfterAdd = throwAfterAdd;
            _id = id;
            _order = order;
            _icon = icon;
        }

        public Task ConfigureAsync(SettingComponentCreationContext context)
        {
            context.Groups.Add(new SettingComponentGroup
            {
                Id = _id,
                DisplayName = _id,
                Icon = _icon,
                Order = _order,
                ComponentType = typeof(FakeContributor)
            });

            if (_throwAfterAdd)
            {
                throw new InvalidOperationException("configure failed");
            }

            return Task.CompletedTask;
        }

        public Task<bool> CheckPermissionsAsync(SettingComponentCreationContext context)
        {
            if (_throwOnCheck)
            {
                throw new InvalidOperationException("permission check failed");
            }

            return Task.FromResult(_allow);
        }
    }
}

public class SettingGroupEditorTests
{
    [Fact]
    public void Changing_a_value_is_dirty_until_discard_restores_it()
    {
        var model = new SampleSettings { Name = "saved" };
        var editor = new SettingGroupEditor<SampleSettings>(() => model, value => model = value ?? new SampleSettings());
        var changes = 0;
        editor.Changed += () => changes++;

        editor.Capture();
        editor.HasUnsavedChanges.ShouldBeFalse();

        model.Name = "edited";
        editor.Observe();

        editor.HasUnsavedChanges.ShouldBeTrue();
        changes.ShouldBe(1);

        editor.Restore();

        model.Name.ShouldBe("saved");
        editor.HasUnsavedChanges.ShouldBeFalse();
        changes.ShouldBe(2);
    }

    private sealed class SampleSettings
    {
        public string? Name { get; set; }
    }
}
