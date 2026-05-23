using System.Collections.Concurrent;
using Microsoft.JSInterop;
using SufiChain.SufiAbp.UI.Blazor.Circuit;
using SufiChain.SufiAbp.UI.Notifications;

namespace SufiChain.SufiAbp.UI.Blazor.Notifications;

/// <summary>
/// Implementation of IUiNotificationService using SufiBlazor toast components.
/// In Blazor Server, notifications are isolated per circuit so one user's toasts
/// do not appear in another browser/session.
/// </summary>
public class SufiBlazorNotificationService : IUiNotificationService
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IBlazorCircuitIdAccessor? _circuitIdAccessor;
    private readonly List<NotificationItem> _notifications = new();
    private readonly ConcurrentDictionary<string, CircuitHandlers> _handlersByCircuitId = new();
    private readonly ConcurrentDictionary<Guid, string> _notificationCircuitIds = new();

    /// <summary>
    /// Event raised when a notification is added (fallback when circuit ID is not available, e.g. WebAssembly).
    /// </summary>
    public event Action<NotificationItem>? OnNotificationAdded;

    /// <summary>
    /// Event raised when a notification is removed.
    /// </summary>
    public event Action<NotificationItem>? OnNotificationRemoved;

    public SufiBlazorNotificationService(IJSRuntime jsRuntime, IBlazorCircuitIdAccessor? circuitIdAccessor = null)
    {
        _jsRuntime = jsRuntime;
        _circuitIdAccessor = circuitIdAccessor;
    }

    /// <summary>
    /// Registers handlers for a specific circuit. Only that circuit's SufiAbpOverlayHost will receive notifications.
    /// Call from SufiAbpOverlayHost.OnInitialized when circuit ID is available.
    /// </summary>
    public void RegisterCircuitHandler(string circuitId, Action<NotificationItem> onAdded, Action<NotificationItem> onRemoved)
    {
        _handlersByCircuitId[circuitId] = new CircuitHandlers(onAdded, onRemoved);
    }

    /// <summary>
    /// Unregisters the handler for a circuit. Call from SufiAbpOverlayHost.Dispose.
    /// </summary>
    public void UnregisterCircuitHandler(string circuitId)
    {
        _handlersByCircuitId.TryRemove(circuitId, out _);
    }

    private sealed record CircuitHandlers(Action<NotificationItem> OnAdded, Action<NotificationItem> OnRemoved);

    public Task InfoAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null)
    {
        return ShowNotificationAsync(UiNotificationType.Info, message, title, options);
    }

    public Task WarnAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null)
    {
        return ShowNotificationAsync(UiNotificationType.Warning, message, title, options);
    }

    public Task ErrorAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null)
    {
        return ShowNotificationAsync(UiNotificationType.Error, message, title, options);
    }

    public Task SuccessAsync(string message, string? title = null, Action<UiNotificationOptions>? options = null)
    {
        return ShowNotificationAsync(UiNotificationType.Success, message, title, options);
    }

    private Task ShowNotificationAsync(
        UiNotificationType type,
        string message,
        string? title,
        Action<UiNotificationOptions>? options)
    {
        var opts = new UiNotificationOptions();
        options?.Invoke(opts);

        var notification = new NotificationItem
        {
            Id = Guid.NewGuid(),
            Type = type,
            Message = message,
            Title = title,
            DurationMs = opts.DurationMs ?? GetDefaultDuration(type),
            Dismissible = opts.Dismissible,
            Position = opts.Position,
            CreatedAt = DateTime.UtcNow
        };

        _notifications.Add(notification);
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (!string.IsNullOrEmpty(circuitId))
        {
            _notificationCircuitIds[notification.Id] = circuitId;
        }
        InvokeHandlerForCurrentCircuit(notification);

        // Auto-remove after duration
        if (notification.DurationMs > 0)
        {
            _ = Task.Delay(notification.DurationMs).ContinueWith(_ =>
            {
                RemoveNotification(notification);
            });
        }

        return Task.CompletedTask;
    }

    private void InvokeHandlerForCurrentCircuit(NotificationItem notification)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (!string.IsNullOrEmpty(circuitId) && _handlersByCircuitId.TryGetValue(circuitId, out var handlers))
        {
            handlers.OnAdded(notification);
        }
        else
        {
            OnNotificationAdded?.Invoke(notification);
        }
    }

    public void RemoveNotification(NotificationItem notification)
    {
        if (_notifications.Remove(notification))
        {
            var hadCircuitId = _notificationCircuitIds.TryRemove(notification.Id, out var circuitId);
            InvokeRemovedHandlerForCircuit(notification, hadCircuitId ? circuitId : null);
        }
    }

    private void InvokeRemovedHandlerForCircuit(NotificationItem notification, string? circuitId)
    {
        if (_handlersByCircuitId.IsEmpty)
        {
            OnNotificationRemoved?.Invoke(notification);
            return;
        }
        if (!string.IsNullOrEmpty(circuitId) && _handlersByCircuitId.TryGetValue(circuitId, out var handlers))
        {
            handlers.OnRemoved(notification);
        }
        else
        {
            OnNotificationRemoved?.Invoke(notification);
        }
    }

    public IReadOnlyList<NotificationItem> GetNotifications() => _notifications.AsReadOnly();

    private static int GetDefaultDuration(UiNotificationType type)
    {
        return type switch
        {
            UiNotificationType.Error => 8000,
            UiNotificationType.Warning => 6000,
            _ => 4000
        };
    }
}

/// <summary>
/// Represents a notification item.
/// </summary>
public class NotificationItem
{
    public Guid Id { get; set; }
    public UiNotificationType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public int DurationMs { get; set; }
    public bool Dismissible { get; set; }
    public UiNotificationPosition Position { get; set; }
    public DateTime CreatedAt { get; set; }
}
