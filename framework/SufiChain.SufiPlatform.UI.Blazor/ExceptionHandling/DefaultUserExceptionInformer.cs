using System.Collections;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.UI.Messages;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Localization.ExceptionHandling;
using Volo.Abp.Validation;

namespace SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;

/// <summary>
/// Default implementation of IUserExceptionInformer that shows exceptions using the message service.
/// Builds user-facing text through ABP's <see cref="IExceptionToErrorInfoConverter"/> (or an equivalent
/// mapping) and never surfaces raw technical exception messages to end users.
/// </summary>
public class DefaultUserExceptionInformer : IUserExceptionInformer
{
    private readonly IUiMessageService _messageService;
    private readonly IStringLocalizer<SufiFrameworkResource> _localizer;
    private readonly IStringLocalizerFactory? _stringLocalizerFactory;
    private readonly AbpExceptionLocalizationOptions? _exceptionLocalizationOptions;
    private readonly IExceptionToErrorInfoConverter? _exceptionToErrorInfoConverter;
    private readonly ILogger<DefaultUserExceptionInformer> _logger;

    public DefaultUserExceptionInformer(
        IUiMessageService messageService,
        IStringLocalizer<SufiFrameworkResource> localizer,
        IStringLocalizerFactory? stringLocalizerFactory = null,
        IOptions<AbpExceptionLocalizationOptions>? exceptionLocalizationOptions = null,
        IExceptionToErrorInfoConverter? exceptionToErrorInfoConverter = null,
        ILogger<DefaultUserExceptionInformer>? logger = null)
    {
        _messageService = messageService;
        _localizer = localizer;
        _stringLocalizerFactory = stringLocalizerFactory;
        _exceptionLocalizationOptions = exceptionLocalizationOptions?.Value;
        _exceptionToErrorInfoConverter = exceptionToErrorInfoConverter;
        _logger = logger ?? NullLogger<DefaultUserExceptionInformer>.Instance;
    }

    public void Inform(UserExceptionInformerContext context)
    {
        _ = InformAsync(context);
    }

    public async Task InformAsync(UserExceptionInformerContext context)
    {
        LogException(context);

        var errorInfo = GetErrorInfo(context);

        try
        {
            if (string.IsNullOrEmpty(errorInfo.Details))
            {
                await _messageService.ErrorAsync(errorInfo.Message, errorInfo.Title);
            }
            else
            {
                await _messageService.ErrorAsync(errorInfo.Details, errorInfo.Title ?? errorInfo.Message);
            }
        }
        catch (Exception exception)
        {
            _logger.LogWarning(exception, "Could not display the user-facing error dialog.");
        }
    }

    public string GetLocalizedMessage(Exception exception)
    {
        return GetErrorInfo(new UserExceptionInformerContext(exception)).Message;
    }

    protected virtual ErrorInfo GetErrorInfo(UserExceptionInformerContext context)
    {
        var exception = Unwrap(context.Exception);

        if (!string.IsNullOrWhiteSpace(context.CustomMessage))
        {
            return new ErrorInfo(
                context.CustomMessage!,
                context.Title ?? _localizer["Error"],
                context.ShowDetails ? context.Exception.ToString() : null);
        }

        // Prefer ABP's converter when registered; post-process so authz / unmapped business /
        // generic exceptions never leak raw Message text.
        if (_exceptionToErrorInfoConverter != null)
        {
            var remote = _exceptionToErrorInfoConverter.Convert(
                exception,
                options =>
                {
                    options.SendExceptionsDetailsToClients = false;
                    options.SendStackTraceToClients = false;
                });
            var mapped = MapFromRemoteServiceError(exception, remote, context);
            if (mapped != null)
            {
                return mapped;
            }
        }

        return BuildEquivalentErrorInfo(exception, context);
    }

    /// <summary>
    /// Maps a converter result (or builds an equivalent) for known exception kinds.
    /// </summary>
    protected virtual ErrorInfo? MapFromRemoteServiceError(
        Exception exception,
        RemoteServiceErrorInfo remote,
        UserExceptionInformerContext context)
    {
        if (IsAuthorizationException(exception))
        {
            return CreateAuthorizationError();
        }

        if (IsEntityNotFoundException(exception))
        {
            return CreateEntityNotFoundError();
        }

        if (IsValidationException(exception))
        {
            return CreateValidationError(exception, remote);
        }

        if (IsUserFriendlyException(exception))
        {
            var message = FirstNonEmpty(remote.Message, exception.Message);
            if (IsGenericExceptionMessage(message) || LooksLikeLocalizationKey(message))
            {
                return CreateGenericError(exception);
            }

            return new ErrorInfo(message!, _localizer["Error"], remote.Details);
        }

        if (IsBusinessException(exception))
        {
            return CreateBusinessError(exception);
        }

        if (IsConcurrencyException(exception))
        {
            return CreateConcurrencyError();
        }

        if (exception is HttpRequestException)
        {
            return new ErrorInfo(_localizer["NetworkError"], _localizer["Error"], null);
        }

        if (exception is TaskCanceledException or OperationCanceledException)
        {
            return new ErrorInfo(_localizer["OperationCancelled"], _localizer["Error"], null);
        }

        if (exception is UnauthorizedAccessException)
        {
            return CreateAuthorizationError();
        }

        // Converter fallback for remotes / unknowns: never pass through raw exception.Message.
        var converted = remote.Message;
        if (string.IsNullOrWhiteSpace(converted) ||
            IsGenericExceptionMessage(converted) ||
            string.Equals(converted, exception.Message, StringComparison.Ordinal) ||
            LooksLikeLocalizationKey(converted))
        {
            return CreateGenericError(exception);
        }

        return new ErrorInfo(
            converted,
            context.Title ?? _localizer["Error"],
            context.ShowDetails ? context.Exception.ToString() : remote.Details);
    }

    protected virtual ErrorInfo BuildEquivalentErrorInfo(
        Exception exception,
        UserExceptionInformerContext context)
    {
        if (IsAuthorizationException(exception))
        {
            return CreateAuthorizationError();
        }

        if (IsEntityNotFoundException(exception))
        {
            return CreateEntityNotFoundError();
        }

        if (IsValidationException(exception))
        {
            return CreateValidationError(exception, remote: null);
        }

        if (IsUserFriendlyException(exception))
        {
            if (IsGenericExceptionMessage(exception.Message) || LooksLikeLocalizationKey(exception.Message))
            {
                return CreateGenericError(exception);
            }

            var details = GetPropertyValue<string>(exception, "Details");
            return new ErrorInfo(exception.Message, _localizer["Error"], details);
        }

        if (IsBusinessException(exception))
        {
            return CreateBusinessError(exception);
        }

        if (IsConcurrencyException(exception))
        {
            return CreateConcurrencyError();
        }

        // Remote call: try to extract RemoteServiceErrorInfo without leaking raw messages.
        var typeName = exception.GetType().FullName ?? exception.GetType().Name;
        if (typeName.Contains("AbpRemoteCallException", StringComparison.Ordinal) ||
            typeName.Contains("AbpHttpClientException", StringComparison.Ordinal))
        {
            var remote = TryExtractRemoteServiceErrorInfo(exception);
            if (remote != null)
            {
                return MapFromRemoteServiceError(exception, remote, context)
                       ?? CreateGenericError(exception);
            }
        }

        if (exception is HttpRequestException)
        {
            return new ErrorInfo(_localizer["NetworkError"], _localizer["Error"], null);
        }

        if (exception is TaskCanceledException or OperationCanceledException)
        {
            return new ErrorInfo(_localizer["OperationCancelled"], _localizer["Error"], null);
        }

        if (exception is UnauthorizedAccessException)
        {
            return CreateAuthorizationError();
        }

        return CreateGenericError(exception);
    }

    private ErrorInfo CreateAuthorizationError()
    {
        var handling = _stringLocalizerFactory?.Create(typeof(SufiExceptionHandlingResource));
        var message = handling?["DefaultErrorMessage403Detail"];
        if (message is { ResourceNotFound: false } && !string.IsNullOrWhiteSpace(message.Value))
        {
            return new ErrorInfo(message.Value, _localizer["Unauthorized"], null);
        }

        return new ErrorInfo(_localizer["Unauthorized"], _localizer["Error"], null);
    }

    private ErrorInfo CreateEntityNotFoundError()
    {
        return new ErrorInfo(_localizer["EntityNotFound"], _localizer["Error"], null);
    }

    private ErrorInfo CreateValidationError(Exception exception, RemoteServiceErrorInfo? remote)
    {
        var details = remote?.Details;
        if (string.IsNullOrWhiteSpace(details) && remote?.ValidationErrors is { Length: > 0 })
        {
            details = FormatRemoteValidationErrors(remote.ValidationErrors);
        }

        if (string.IsNullOrWhiteSpace(details) && exception is IHasValidationErrors hasErrors)
        {
            details = FormatValidationResults(hasErrors);
        }

        if (string.IsNullOrWhiteSpace(details))
        {
            details = ExtractAbpValidationExceptionDetails(exception);
        }

        return new ErrorInfo(
            _localizer["PleaseCorrectErrors"],
            _localizer["ValidationError"],
            string.IsNullOrWhiteSpace(details) ? null : details);
    }

    private ErrorInfo CreateBusinessError(Exception exception)
    {
        var code = (exception as IHasErrorCode)?.Code
                   ?? GetPropertyValue<string>(exception, "Code");
        var localized = LocalizeErrorCode(code, exception.Data);
        if (!string.IsNullOrWhiteSpace(localized))
        {
            var details = (exception as IHasErrorDetails)?.Details
                          ?? GetPropertyValue<string>(exception, "Details");
            return new ErrorInfo(localized, _localizer["Error"], details);
        }

        if (!string.IsNullOrWhiteSpace(code))
        {
            _logger.LogWarning(
                "Unmapped business exception code shown as generic message. Code={Code}",
                code);
        }
        else
        {
            _logger.LogWarning(exception, "Business exception without a localizable code.");
        }

        return new ErrorInfo(_localizer["AnErrorOccurred"], _localizer["Error"], null);
    }

    private ErrorInfo CreateConcurrencyError()
    {
        var concurrencyMessage = _stringLocalizerFactory?
            .Create(typeof(SufiExceptionHandlingResource))["AbpDbConcurrencyErrorMessage"];
        if (concurrencyMessage is { ResourceNotFound: false })
        {
            return new ErrorInfo(concurrencyMessage.Value, _localizer["Error"], null);
        }

        return CreateGenericError(new InvalidOperationException("concurrency"));
    }

    private ErrorInfo CreateGenericError(Exception exception)
    {
        var correlationId = ResolveCorrelationId();
        _logger.LogError(
            exception,
            "Unhandled UI exception mapped to a generic user message. CorrelationId={CorrelationId}",
            correlationId);

        return new ErrorInfo(
            _localizer["DefaultErrorMessageWithReference", correlationId],
            _localizer["Error"],
            null);
    }

    private static string ResolveCorrelationId()
    {
        var activityId = Activity.Current?.Id;
        if (!string.IsNullOrWhiteSpace(activityId))
        {
            return activityId!;
        }

        return Guid.NewGuid().ToString("N")[..12];
    }

    private static Exception Unwrap(Exception exception)
    {
        if (exception is AggregateException { InnerException: { } inner } &&
            (IsAuthorizationException(inner) ||
             IsEntityNotFoundException(inner) ||
             IsValidationException(inner) ||
             IsBusinessException(inner) ||
             IsUserFriendlyException(inner)))
        {
            return inner;
        }

        return exception;
    }

    private static bool IsAuthorizationException(Exception exception)
    {
        return exception is AbpAuthorizationException ||
               exception.GetType().Name.Contains("AbpAuthorizationException", StringComparison.Ordinal);
    }

    private static bool IsEntityNotFoundException(Exception exception)
    {
        return exception is EntityNotFoundException ||
               string.Equals(exception.GetType().Name, "EntityNotFoundException", StringComparison.Ordinal);
    }

    private static bool IsValidationException(Exception exception)
    {
        return exception is IHasValidationErrors ||
               exception.GetType().Name.Contains("AbpValidationException", StringComparison.Ordinal);
    }

    private static bool IsUserFriendlyException(Exception exception)
    {
        return exception is IUserFriendlyException ||
               exception.GetType().Name.Contains("UserFriendlyException", StringComparison.Ordinal);
    }

    private static bool IsBusinessException(Exception exception)
    {
        return exception is IBusinessException ||
               exception.GetType().Name.Contains("BusinessException", StringComparison.Ordinal);
    }

    private static bool IsConcurrencyException(Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            var name = current.GetType().Name;
            if (name is "DbUpdateConcurrencyException" or "AbpDbConcurrencyException")
            {
                return true;
            }
        }

        return false;
    }

    private RemoteServiceErrorInfo? TryExtractRemoteServiceErrorInfo(Exception exception)
    {
        try
        {
            var errorProperty = exception.GetType().GetProperty("Error");
            var errorInfo = errorProperty?.GetValue(exception);
            if (errorInfo is RemoteServiceErrorInfo typed)
            {
                return typed;
            }

            if (errorInfo == null)
            {
                return null;
            }

            return new RemoteServiceErrorInfo(
                GetPropertyValue<string>(errorInfo, "Message"),
                GetPropertyValue<string>(errorInfo, "Details"),
                GetPropertyValue<string>(errorInfo, "Code"));
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract ABP remote call exception info");
            return null;
        }
    }

    private static string? FormatRemoteValidationErrors(RemoteServiceValidationErrorInfo[] errors)
    {
        var messages = new StringBuilder();
        foreach (var error in errors)
        {
            if (string.IsNullOrWhiteSpace(error.Message))
            {
                continue;
            }

            if (error.Members is { Length: > 0 })
            {
                messages.AppendLine($"• {string.Join(", ", error.Members)}: {error.Message}");
            }
            else
            {
                messages.AppendLine($"• {error.Message}");
            }
        }

        var text = messages.ToString().TrimEnd();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private static string? FormatValidationResults(IHasValidationErrors validationException)
    {
        var messages = new StringBuilder();
        foreach (var error in validationException.ValidationErrors)
        {
            if (string.IsNullOrWhiteSpace(error.ErrorMessage))
            {
                continue;
            }

            messages.AppendLine($"• {error.ErrorMessage}");
        }

        var text = messages.ToString().TrimEnd();
        return string.IsNullOrWhiteSpace(text) ? null : text;
    }

    private string? ExtractAbpValidationExceptionDetails(Exception exception)
    {
        try
        {
            var validationErrorsProperty = exception.GetType().GetProperty("ValidationErrors");
            if (validationErrorsProperty?.GetValue(exception) is not IEnumerable validationErrors)
            {
                return null;
            }

            var messages = new StringBuilder();
            foreach (var error in validationErrors)
            {
                var errorMessage = GetPropertyValue<string>(error, "ErrorMessage")
                                   ?? GetPropertyValue<string>(error, "Message");
                if (!string.IsNullOrEmpty(errorMessage))
                {
                    messages.AppendLine($"• {errorMessage}");
                }
            }

            var text = messages.ToString().TrimEnd();
            return string.IsNullOrWhiteSpace(text) ? null : text;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract ABP validation exception info");
            return null;
        }
    }

    private static T? GetPropertyValue<T>(object? obj, string propertyName)
    {
        if (obj == null)
        {
            return default;
        }

        try
        {
            var property = obj.GetType().GetProperty(
                propertyName,
                BindingFlags.Public | BindingFlags.Instance);

            if (property?.GetValue(obj) is T typedValue)
            {
                return typedValue;
            }
        }
        catch
        {
            // Ignore reflection errors
        }

        return default;
    }

    private static bool IsGenericExceptionMessage(string? message)
    {
        return string.IsNullOrWhiteSpace(message) ||
               (message.StartsWith("Exception of type '", StringComparison.Ordinal) &&
                message.EndsWith("' was thrown.", StringComparison.Ordinal));
    }

    private string? LocalizeErrorCode(string? errorCode, IDictionary? data)
    {
        if (string.IsNullOrWhiteSpace(errorCode) ||
            !errorCode.Contains(':', StringComparison.Ordinal) ||
            _stringLocalizerFactory == null ||
            _exceptionLocalizationOptions == null)
        {
            return null;
        }

        var codeNamespace = errorCode.Split(':')[0];
        if (!_exceptionLocalizationOptions.ErrorCodeNamespaceMappings.TryGetValue(codeNamespace, out var resourceType))
        {
            return null;
        }

        var localizedString = _stringLocalizerFactory.Create(resourceType)[errorCode];
        if (localizedString.ResourceNotFound)
        {
            return null;
        }

        var value = localizedString.Value;
        if (data == null || data.Count == 0)
        {
            return value;
        }

        foreach (var key in data.Keys)
        {
            value = value.Replace("{" + key + "}", data[key]?.ToString(), StringComparison.Ordinal);
        }

        return value;
    }

    /// <summary>
    /// Detects Namespace:PascalCaseKey-shaped strings. Never convert these into English words.
    /// </summary>
    private static bool LooksLikeLocalizationKey(string? message)
    {
        if (string.IsNullOrWhiteSpace(message) ||
            message.Any(char.IsWhiteSpace) ||
            !message.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        var key = message[(message.LastIndexOf(':') + 1)..];
        return key.Length > 0 &&
               key.Any(char.IsUpper) &&
               key.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.');
    }

    private static string? FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return null;
    }

    protected virtual void LogException(UserExceptionInformerContext context)
    {
        SufiExceptionLog.LogError(_logger, context.Exception, "User exception occurred");
    }

    protected record ErrorInfo(string Message, string? Title, string? Details);
}
