using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using SufiChain.SufiPlatform.UI.Alerts;
using SufiChain.SufiPlatform.UI.BlockUi;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.UI.Messages;
using SufiChain.SufiPlatform.UI.MultiTenancy;
using SufiChain.SufiPlatform.UI.Notifications;
using SufiChain.SufiPlatform.UI.Progression;
using SufiChain.SufiPlatform.UI.Users;
using Volo.Abp.Authorization;

namespace SufiChain.SufiPlatform.UI.Blazor;

/// <summary>
/// Base class for Blazor components in the Sufi Platform.
/// Provides common services, loading state management, and exception handling.
/// </summary>
public abstract class SufiComponentBase : OwningComponentBase
{
    private readonly CancellationTokenSource _cts = new();
    private bool _isDisposed;
    private bool _isInteractive;

    /// <summary>
    /// Gets a cancellation token that is cancelled when the component is disposed.
    /// Use this token for async operations to properly cancel them on navigation.
    /// </summary>
    protected CancellationToken ComponentCancellationToken => _cts.Token;

    /// <summary>
    /// Gets whether the component has been disposed.
    /// </summary>
    protected bool IsDisposed => _isDisposed;

    /// <summary>
    /// Gets whether the component is in interactive mode (after first render).
    /// JavaScript interop is only available when this is true.
    /// Use this to defer JS-dependent operations until after prerendering.
    /// </summary>
    protected bool IsInteractive => _isInteractive;

    // ====== LAZY SERVICE REFERENCES ======
    private IStringLocalizerFactory? _stringLocalizerFactory;
    private IStringLocalizer? _localizer;
    private ILogger? _logger;
    private IAuthorizationService? _authorizationService;
    private ICurrentUserAccessor? _currentUser;
    private ICurrentTenant? _currentTenant;
    private IUiMessageService? _messageService;
    private IUiNotificationService? _notificationService;
    private IAlertManager? _alertManager;
    private IBlockUiService? _blockUiService;
    private IUiPageProgressService? _pageProgressService;
    private IUserExceptionInformer? _userExceptionInformer;

    // ====== LOCALIZATION ======

    /// <summary>
    /// The localization resource type. Set this in derived component constructors.
    /// </summary>
    protected Type? LocalizationResource { get; set; }

    /// <summary>
    /// Gets the string localizer factory.
    /// </summary>
    protected IStringLocalizerFactory StringLocalizerFactory =>
        LazyGetRequiredService(ref _stringLocalizerFactory);

    /// <summary>
    /// Gets the string localizer for the component's localization resource.
    /// Defaults to SufiFrameworkResource if no LocalizationResource is set.
    /// </summary>
    protected IStringLocalizer L
    {
        get
        {
            if (_localizer == null)
            {
                _localizer = LocalizationResource != null
                    ? StringLocalizerFactory.Create(LocalizationResource)
                    : StringLocalizerFactory.Create(typeof(SufiFrameworkResource));
            }
            return _localizer;
        }
    }

    // ====== LOGGING ======

    /// <summary>
    /// Gets a logger for this component type.
    /// </summary>
    protected ILogger Logger
    {
        get
        {
            if (_logger == null)
            {
                var loggerFactory = ScopedServices.GetService<ILoggerFactory>();
                _logger = loggerFactory?.CreateLogger(GetType()) ?? NullLogger.Instance;
            }
            return _logger;
        }
    }

    // ====== AUTHORIZATION ======

    /// <summary>
    /// Gets the authorization service.
    /// </summary>
    protected IAuthorizationService AuthorizationService =>
        LazyGetRequiredService(ref _authorizationService);

    /// <summary>
    /// Checks whether the current user is granted the permission (uses ABP's authorization integration).
    /// </summary>
    protected virtual Task<bool> IsGrantedAsync(string permissionName) =>
        AuthorizationService.IsGrantedAsync(permissionName);

    /// <summary>
    /// Checks whether the current user is granted any of the permissions.
    /// </summary>
    protected virtual Task<bool> IsGrantedAnyAsync(string permissionName) =>
        AuthorizationService.IsGrantedAnyAsync(permissionName);

    // ====== CURRENT USER & TENANT ======

    /// <summary>
    /// Gets the current user accessor.
    /// </summary>
    protected ICurrentUserAccessor CurrentUser =>
        LazyGetRequiredService(ref _currentUser);

    /// <summary>
    /// Gets the current tenant accessor.
    /// </summary>
    protected ICurrentTenant CurrentTenant =>
        LazyGetRequiredService(ref _currentTenant);

    // ====== UI SERVICES ======

    /// <summary>
    /// Gets the UI message service for showing dialogs and confirmations.
    /// </summary>
    protected IUiMessageService Message =>
        LazyGetRequiredService(ref _messageService);

    /// <summary>
    /// Gets the UI notification service for showing toasts.
    /// </summary>
    protected IUiNotificationService Notify =>
        LazyGetRequiredService(ref _notificationService);

    /// <summary>
    /// Gets the alert manager for page-level alerts.
    /// </summary>
    protected IAlertManager AlertManager =>
        LazyGetRequiredService(ref _alertManager);

    /// <summary>
    /// Gets the block UI service for blocking UI during long operations.
    /// </summary>
    protected IBlockUiService BlockUi =>
        LazyGetRequiredService(ref _blockUiService);

    /// <summary>
    /// Gets the page progress service for showing page-level progress indicators.
    /// </summary>
    protected IUiPageProgressService PageProgress =>
        LazyGetRequiredService(ref _pageProgressService);

    // ====== EXCEPTION HANDLING ======
    // Async errors (backend, validation, AbpRemoteCallException, etc.) flow through
    // ExecuteWithLoadingAsync → HandleErrorAsync → UserExceptionInformer (ABP-style message dialog).
    // Render/event exceptions that are NOT caught go to ErrorBoundary (blocking full-screen modal).
    // These two paths are complementary; UserExceptionInformer is not superseded by ErrorBoundary.

    /// <summary>
    /// Gets the user exception informer for displaying errors to users.
    /// </summary>
    protected IUserExceptionInformer UserExceptionInformer =>
        LazyGetRequiredService(ref _userExceptionInformer);

    /// <summary>
    /// Handles an exception by informing the user.
    /// Override this method to customize exception handling.
    /// During prerendering, errors are only logged (not shown to user via JS).
    /// </summary>
    protected virtual async Task HandleErrorAsync(Exception exception)
    {
        Logger.LogException(exception);

        // During prerendering, we can only log - JS interop is not available
        // The UserExceptionInformer is now prerender-safe, but we still want to
        // avoid unnecessary processing during static rendering
        if (_isInteractive)
        {
            await UserExceptionInformer.InformAsync(new UserExceptionInformerContext(exception));
        }
        else
        {
            // During prerendering, just log the error - it will be handled properly
            // when the component becomes interactive
            Logger.LogWarning(
                "Exception occurred during prerendering and cannot be shown to user: {Message}",
                exception.Message);
        }
    }

    // ====== LOADING STATE MANAGEMENT ======

    /// <summary>
    /// Simple loading flag for single operation scenarios.
    /// </summary>
    protected bool IsLoading { get; set; }

    /// <summary>
    /// Concurrent dictionary for tracking multiple loading operations.
    /// Use different keys for different operations (e.g., "save", "delete", "load").
    /// </summary>
    protected ConcurrentDictionary<string, bool> LoadingStates { get; } = new();

    /// <summary>
    /// Checks if a specific operation is currently loading.
    /// </summary>
    protected bool IsOperationLoading(string operationKey) =>
        LoadingStates.TryGetValue(operationKey, out var loading) && loading;

    /// <summary>
    /// Checks if any operation is currently loading.
    /// </summary>
    protected bool IsAnyOperationLoading => !LoadingStates.IsEmpty;

    /// <summary>
    /// Executes an async action with loading state management.
    /// Handles exceptions, cancellations, and StateHasChanged automatically.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="operationKey">A unique key to identify this operation.</param>
    /// <param name="loadingBehavior">The UI loading behavior (BlockUi blocks the page, PageProgress shows a progress bar, None for no UI feedback).</param>
    protected virtual async Task ExecuteWithLoadingAsync(
        Func<Task> action,
        string operationKey = "default",
        LoadingBehavior loadingBehavior = LoadingBehavior.None)
    {
        if (_isDisposed)
        {
            return;
        }

        try
        {
            LoadingStates[operationKey] = true;
            await StartLoadingUiAsync(loadingBehavior);
            await InvokeStateHasChangedSafeAsync();

            await action();
        }
        catch (OperationCanceledException)
        {
            // User navigated away or request was intentionally cancelled - expected behavior
        }
        catch (HttpRequestException ex) when (ex.InnerException is OperationCanceledException or TaskCanceledException)
        {
            // Remote HTTP call was cancelled - expected when user navigates away
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed during async operation - expected when user navigates away
        }
        catch (Exception ex)
        {
            if (!_isDisposed)
            {
                await HandleErrorAsync(ex);
            }
        }
        finally
        {
            await StopLoadingUiAsync(loadingBehavior);
            LoadingStates.TryRemove(operationKey, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }

    /// <summary>
    /// Executes an async function with loading state management and returns the result.
    /// Handles exceptions, cancellations, and StateHasChanged automatically.
    /// </summary>
    /// <typeparam name="T">The return type.</typeparam>
    /// <param name="action">The async function to execute.</param>
    /// <param name="operationKey">A unique key to identify this operation.</param>
    /// <param name="loadingBehavior">The UI loading behavior (BlockUi blocks the page, PageProgress shows a progress bar, None for no UI feedback).</param>
    /// <returns>The result of the action, or default if cancelled.</returns>
    protected virtual async Task<T?> ExecuteWithLoadingAsync<T>(
        Func<Task<T>> action,
        string operationKey = "default",
        LoadingBehavior loadingBehavior = LoadingBehavior.None)
    {
        if (_isDisposed)
        {
            return default;
        }

        try
        {
            LoadingStates[operationKey] = true;
            await StartLoadingUiAsync(loadingBehavior);
            await InvokeStateHasChangedSafeAsync();

            return await action();
        }
        catch (OperationCanceledException)
        {
            // User navigated away or request was intentionally cancelled - expected behavior
            return default;
        }
        catch (HttpRequestException ex) when (ex.InnerException is OperationCanceledException or TaskCanceledException)
        {
            // Remote HTTP call was cancelled - expected when user navigates away
            return default;
        }
        catch (ObjectDisposedException)
        {
            // Component was disposed during async operation - expected when user navigates away
            return default;
        }
        catch (Exception ex)
        {
            if (!_isDisposed)
            {
                await HandleErrorAsync(ex);
            }

            // Match the non-generic overload: errors are informed to the user, not
            // rethrown as unhandled render exceptions (especially during prerender).
            return default;
        }
        finally
        {
            await StopLoadingUiAsync(loadingBehavior);
            LoadingStates.TryRemove(operationKey, out _);
            await InvokeStateHasChangedSafeAsync();
        }
    }

    /// <summary>
    /// Starts the loading UI based on the specified behavior.
    /// </summary>
    private async Task StartLoadingUiAsync(LoadingBehavior behavior)
    {
        if (!_isInteractive) return;

        try
        {
            switch (behavior)
            {
                case LoadingBehavior.BlockUi:
                    await BlockUi.BlockAsync(busy: true);
                    break;
                case LoadingBehavior.PageProgress:
                    await PageProgress.ShowIndeterminateAsync();
                    break;
                case LoadingBehavior.None:
                    // No UI feedback
                    break;
            }
        }
        catch
        {
            // Ignore UI errors during loading state changes
        }
    }

    /// <summary>
    /// Stops the loading UI based on the specified behavior.
    /// </summary>
    private async Task StopLoadingUiAsync(LoadingBehavior behavior)
    {
        if (!_isInteractive) return;

        try
        {
            switch (behavior)
            {
                case LoadingBehavior.BlockUi:
                    await BlockUi.UnblockAsync();
                    break;
                case LoadingBehavior.PageProgress:
                    await PageProgress.HideAsync();
                    break;
                case LoadingBehavior.None:
                    // No UI feedback
                    break;
            }
        }
        catch
        {
            // Ignore UI errors during loading state changes
        }
    }

    // ====== LAZY SERVICE RESOLUTION ======

    /// <summary>
    /// Lazily gets a required service from the scoped service provider.
    /// </summary>
    protected T LazyGetRequiredService<T>(ref T? reference) where T : class
    {
        return reference ??= ScopedServices.GetRequiredService<T>();
    }

    /// <summary>
    /// Lazily gets an optional service from the scoped service provider.
    /// </summary>
    protected T? LazyGetService<T>(ref T? reference) where T : class
    {
        if (reference == null)
        {
            reference = ScopedServices.GetService<T>();
        }
        return reference;
    }

    // ====== LIFECYCLE ======

    /// <summary>
    /// Called after each render. Sets IsInteractive to true on first render.
    /// Override this to perform post-render operations that require JS interop.
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isInteractive = true;
        }

        await base.OnAfterRenderAsync(firstRender);
    }

    // ====== SAFE UI UPDATES ======

    /// <summary>
    /// Safely notifies the renderer that state changed.
    /// Never awaits a new dispatcher turn from inside an existing one — doing so during
    /// <c>OnInitializedAsync</c> / loading helpers can deadlock the Blazor Server circuit
    /// (UI frozen until a full page reload).
    /// </summary>
    protected Task InvokeStateHasChangedSafeAsync()
    {
        if (_isDisposed)
        {
            return Task.CompletedTask;
        }

        try
        {
            var invokeTask = InvokeAsync(() =>
            {
                if (!_isDisposed)
                {
                    StateHasChanged();
                }
            });

            // Inline completion (already on the renderer sync context) is safe to observe.
            if (invokeTask.IsCompleted)
            {
                return invokeTask.IsFaulted
                    ? Task.FromException(invokeTask.Exception!.InnerException ?? invokeTask.Exception)
                    : Task.CompletedTask;
            }

            // Queued on another turn — do not block this turn waiting for it.
            _ = invokeTask.ContinueWith(
                static t => _ = t.Exception,
                CancellationToken.None,
                TaskContinuationOptions.OnlyOnFaulted | TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default);

            return Task.CompletedTask;
        }
        catch (ObjectDisposedException)
        {
            return Task.CompletedTask;
        }
    }

    // ====== DISPOSAL ======

    /// <summary>
    /// Disposes the component's resources and cancels any pending operations.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;

        if (disposing)
        {
            try
            {
                _cts.Cancel();
                _cts.Dispose();
            }
            catch
            {
                // Ignore disposal errors
            }
        }

        base.Dispose(disposing);
    }
}

/// <summary>
/// Extension methods for logging.
/// </summary>
internal static class LoggerExtensions
{
    public static void LogException(this ILogger logger, Exception exception)
    {
        logger.LogError(exception, "An error occurred: {Message}", exception.Message);
    }
}

/// <summary>
/// Defines the UI loading behavior during async operations.
/// </summary>
public enum LoadingBehavior
{
    /// <summary>
    /// Blocks the entire page with an overlay and busy indicator.
    /// This is the default behavior for operations that should prevent user interaction.
    /// </summary>
    BlockUi,

    /// <summary>
    /// Shows an indeterminate progress bar at the top of the page.
    /// Use for background operations where the user can still interact with the page.
    /// </summary>
    PageProgress,

    /// <summary>
    /// No global UI feedback. Only local loading state is tracked.
    /// Use when the component handles its own loading UI (e.g., inline spinners).
    /// </summary>
    None
}
