using System.Text.Json;
using Microsoft.JSInterop;
using Shouldly;
using SufiChain.SufiPlatform.UI.Blazor.Theming;
using SufiChain.SufiPlatform.UI.Browser;
using SufiChain.SufiPlatform.UI.Theming;
using Xunit;

namespace SufiChain.SufiPlatform.UI.Blazor.Tests;

public class ThemeSwitchServiceTests
{
    [Fact]
    public void SeedResolved_reports_dark_before_js()
    {
        var js = new RecordingJsRuntime();
        var storage = new InMemoryLocalStorage();
        var service = new ThemeSwitchService(storage, js);

        service.SeedResolved(true);

        service.IsDarkMode.ShouldBeTrue();
        service.IsPreferenceResolved.ShouldBeFalse();
    }

    [Fact]
    public async Task GetStoredThemeAsync_skips_apply_when_seeded_value_matches()
    {
        var js = new RecordingJsRuntime
        {
            ThemeState = new SufiThemeJsState
            {
                Stored = "Dark",
                SystemDark = false,
                IsLight = false
            }
        };
        var storage = new InMemoryLocalStorage();
        var service = new ThemeSwitchService(storage, js);
        service.SeedResolved(true);

        await service.GetStoredThemeAsync();

        js.ApplyCallCount.ShouldBe(0);
        service.IsDarkMode.ShouldBeTrue();
        service.IsPreferenceResolved.ShouldBeTrue();
    }

    [Fact]
    public async Task GetStoredThemeAsync_keeps_seed_when_js_is_unavailable()
    {
        var js = new RecordingJsRuntime { ThrowOnInvoke = true };
        var storage = new InMemoryLocalStorage();
        var service = new ThemeSwitchService(storage, js);
        service.SeedResolved(true);

        await service.GetStoredThemeAsync();

        service.IsDarkMode.ShouldBeTrue();
        service.IsPreferenceResolved.ShouldBeFalse();
        js.ApplyCallCount.ShouldBe(0);
    }

    private sealed class InMemoryLocalStorage : ILocalStorageService
    {
        private readonly Dictionary<string, string> _items = new(StringComparer.Ordinal);

        public ValueTask<string?> GetItemAsync(string key) =>
            ValueTask.FromResult(_items.TryGetValue(key, out var value) ? value : null);

        public ValueTask SetItemAsync(string key, string value)
        {
            _items[key] = value;
            return ValueTask.CompletedTask;
        }

        public ValueTask RemoveItemAsync(string key)
        {
            _items.Remove(key);
            return ValueTask.CompletedTask;
        }

        public ValueTask ClearAsync()
        {
            _items.Clear();
            return ValueTask.CompletedTask;
        }

        public ValueTask<int> GetLengthAsync() => ValueTask.FromResult(_items.Count);

        public ValueTask<string?> GetKeyAsync(int index)
        {
            var key = _items.Keys.ElementAtOrDefault(index);
            return ValueTask.FromResult<string?>(key);
        }
    }

    private sealed class RecordingJsRuntime : IJSRuntime
    {
        public int ApplyCallCount { get; private set; }
        public SufiThemeJsState? ThemeState { get; set; }
        public bool ThrowOnInvoke { get; set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (ThrowOnInvoke)
            {
                throw new InvalidOperationException("JS unavailable");
            }

            if (identifier == "sufiTheme.getState")
            {
                var json = JsonSerializer.Serialize(
                    ThemeState,
                    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
                var value = JsonSerializer.Deserialize<TValue>(json);
                return ValueTask.FromResult(value!);
            }

            throw new InvalidOperationException($"Unexpected invoke: {identifier}");
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync<TValue>(identifier, args);

        public ValueTask InvokeAsync(string identifier, object?[]? args)
        {
            if (ThrowOnInvoke)
            {
                throw new InvalidOperationException("JS unavailable");
            }

            if (identifier == "sufiTheme.apply")
            {
                ApplyCallCount++;
                return ValueTask.CompletedTask;
            }

            throw new InvalidOperationException($"Unexpected invoke: {identifier}");
        }

        public ValueTask InvokeAsync(string identifier, CancellationToken cancellationToken, object?[]? args) =>
            InvokeAsync(identifier, args);
    }
}
