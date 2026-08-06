using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using SufiChain.SufiPlatform.UI.Blazor.Circuit;
using SufiChain.SufiPlatform.UI.Messages;

namespace SufiChain.SufiPlatform.UI.Blazor.Messages;

/// <summary>
/// Implementation of IUiMessageService that communicates with SbMessageHost component.
/// </summary>
public class SufiBlazorMessageService : IUiMessageService
{
    private readonly ConcurrentDictionary<string, CircuitHandlers> _handlersByCircuitId = new();
    private readonly ConcurrentDictionary<string, List<MessageRequest>> _pendingMessagesByCircuit = new();
    private readonly ILogger<SufiBlazorMessageService>? _logger;
    private readonly IBlazorCircuitIdAccessor? _circuitIdAccessor;

    public SufiBlazorMessageService(
        ILogger<SufiBlazorMessageService>? logger = null,
        IBlazorCircuitIdAccessor? circuitIdAccessor = null)
    {
        _logger = logger;
        _circuitIdAccessor = circuitIdAccessor;
    }

    /// <summary>
    /// Registers the confirm dialog handler (called by SufiOverlayHost).
    /// </summary>
    public void RegisterConfirmHandler(Func<ConfirmRequest, Task<bool>> handler)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (string.IsNullOrEmpty(circuitId))
        {
            _logger?.LogWarning("Cannot register confirm handler: circuit ID is null");
            return;
        }
        
        var handlers = _handlersByCircuitId.GetOrAdd(circuitId, _ => new CircuitHandlers());
        handlers.ConfirmHandler = handler;
        _logger?.LogInformation("Registered confirm handler for circuit {CircuitId}", circuitId);
    }

    /// <summary>
    /// Registers the message dialog handler (called by SufiOverlayHost).
    /// </summary>
    public void RegisterMessageHandler(Func<MessageRequest, Task> handler)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (string.IsNullOrEmpty(circuitId))
        {
            _logger?.LogWarning("Cannot register message handler: circuit ID is null");
            return;
        }
        
        var handlers = _handlersByCircuitId.GetOrAdd(circuitId, _ => new CircuitHandlers());
        handlers.MessageHandler = handler;
        
        _logger?.LogInformation("Registered message handler for circuit {CircuitId}", circuitId);
        
        // Process any pending messages for this circuit
        if (_pendingMessagesByCircuit.TryRemove(circuitId, out var pendingMessages))
        {
            _logger?.LogInformation("Processing {Count} pending messages for circuit {CircuitId}", pendingMessages.Count, circuitId);
            foreach (var msg in pendingMessages)
            {
                _ = handler(msg);
            }
        }
    }

    /// <summary>
    /// Unregisters handlers for a circuit (called when SufiOverlayHost is disposed).
    /// </summary>
    public void UnregisterHandlers(string circuitId)
    {
        _handlersByCircuitId.TryRemove(circuitId, out _);
        _pendingMessagesByCircuit.TryRemove(circuitId, out _);
        _logger?.LogInformation("Unregistered handlers for circuit {CircuitId}", circuitId);
    }

    public async Task<bool> ConfirmAsync(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (string.IsNullOrEmpty(circuitId) || !_handlersByCircuitId.TryGetValue(circuitId, out var handlers) || handlers.ConfirmHandler == null)
        {
            // Fallback: no handler registered, return false
            _logger?.LogWarning("Confirm handler not available for circuit {CircuitId}", circuitId ?? "null");
            return false;
        }

        var opts = new UiMessageOptions();
        options?.Invoke(opts);

        var request = new ConfirmRequest
        {
            Message = message,
            Title = title,
            ConfirmButtonText = opts.ConfirmButtonText,
            CancelButtonText = opts.CancelButtonText,
            CloseOnBackdropClick = opts.CloseOnBackdropClick
        };

        return await handlers.ConfirmHandler(request);
    }

    public async Task InfoAsync(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        await ShowMessageAsync(UiMessageType.Info, message, title, options);
    }

    public async Task WarnAsync(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        await ShowMessageAsync(UiMessageType.Warning, message, title, options);
    }

    public async Task ErrorAsync(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        await ShowMessageAsync(UiMessageType.Error, message, title, options);
    }

    public async Task SuccessAsync(string message, string? title = null, Action<UiMessageOptions>? options = null)
    {
        await ShowMessageAsync(UiMessageType.Success, message, title, options);
    }

    private async Task ShowMessageAsync(UiMessageType type, string message, string? title, Action<UiMessageOptions>? options)
    {
        var opts = new UiMessageOptions();
        options?.Invoke(opts);
        
        var circuitId = _circuitIdAccessor?.CurrentCircuitId;
        if (string.IsNullOrEmpty(circuitId) || !_handlersByCircuitId.TryGetValue(circuitId, out var handlers) || handlers.MessageHandler == null)
        {
            // Queue the message until handler is registered (happens during layout initialization)
            _logger?.LogWarning("Message handler not available for circuit {CircuitId}. Queueing message: {Type} - {Message}", circuitId ?? "null", type, message);
            
            if (!string.IsNullOrEmpty(circuitId))
            {
                var messageRequest = new MessageRequest { Type = type, Message = message, Title = title, OkButtonText = opts.OkButtonText };
                _pendingMessagesByCircuit.AddOrUpdate(
                    circuitId,
                    _ => new List<MessageRequest> { messageRequest },
                    (_, list) => { list.Add(messageRequest); return list; }
                );
            }
            return;
        }

        var request = new MessageRequest
        {
            Type = type,
            Message = message,
            Title = title,
            OkButtonText = opts.OkButtonText
        };

        await handlers.MessageHandler(request);
    }

    public Task Success(LocalizedString localizedString)
    {
        return SuccessAsync(localizedString.Value);
    }
}

/// <summary>
/// Holds handlers for a specific circuit.
/// </summary>
internal class CircuitHandlers
{
    public Func<ConfirmRequest, Task<bool>>? ConfirmHandler { get; set; }
    public Func<MessageRequest, Task>? MessageHandler { get; set; }
}

/// <summary>
/// Request for a confirmation dialog.
/// </summary>
public class ConfirmRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? ConfirmButtonText { get; set; }
    public string? CancelButtonText { get; set; }
    public bool CloseOnBackdropClick { get; set; }
}

/// <summary>
/// Request for a message dialog.
/// </summary>
public class MessageRequest
{
    public UiMessageType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public string? OkButtonText { get; set; }
}
