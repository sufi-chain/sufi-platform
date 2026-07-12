using System.Collections;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SufiChain.SufiPlatform.UI.ExceptionHandling;
using SufiChain.SufiPlatform.UI.Localization;
using SufiChain.SufiPlatform.UI.Messages;
using Volo.Abp.Localization.ExceptionHandling;

namespace SufiChain.SufiPlatform.UI.Blazor.ExceptionHandling;

/// <summary>
/// Default implementation of IUserExceptionInformer that shows exceptions using the message service.
/// Includes special handling for ABP Framework exceptions.
/// </summary>
public class DefaultUserExceptionInformer : IUserExceptionInformer
{
    private readonly IUiMessageService _messageService;
    private readonly IStringLocalizer<SufiFrameworkResource> _localizer;
    private readonly IStringLocalizerFactory? _stringLocalizerFactory;
    private readonly AbpExceptionLocalizationOptions? _exceptionLocalizationOptions;
    private readonly ILogger<DefaultUserExceptionInformer> _logger;

    public DefaultUserExceptionInformer(
        IUiMessageService messageService,
        IStringLocalizer<SufiFrameworkResource> localizer,
        IStringLocalizerFactory? stringLocalizerFactory = null,
        IOptions<AbpExceptionLocalizationOptions>? exceptionLocalizationOptions = null,
        ILogger<DefaultUserExceptionInformer>? logger = null)
    {
        _messageService = messageService;
        _localizer = localizer;
        _stringLocalizerFactory = stringLocalizerFactory;
        _exceptionLocalizationOptions = exceptionLocalizationOptions?.Value;
        _logger = logger ?? NullLogger<DefaultUserExceptionInformer>.Instance;
    }

    public void Inform(UserExceptionInformerContext context)
    {
        // Fire and forget for sync version
        _ = InformAsync(context);
    }

    public async Task InformAsync(UserExceptionInformerContext context)
    {
        LogException(context);

        var errorInfo = GetErrorInfo(context);

        if (string.IsNullOrEmpty(errorInfo.Details))
        {
            await _messageService.ErrorAsync(errorInfo.Message, errorInfo.Title);
        }
        else
        {
            await _messageService.ErrorAsync(errorInfo.Details, errorInfo.Title ?? errorInfo.Message);
        }
    }

    protected virtual ErrorInfo GetErrorInfo(UserExceptionInformerContext context)
    {
        var exception = context.Exception;

        // Try to extract ABP error info first
        var abpErrorInfo = TryGetAbpErrorInfo(exception);
        if (abpErrorInfo != null)
        {
            return abpErrorInfo;
        }

        // Fall back to default handling
        var message = context.CustomMessage ?? GetUserFriendlyMessage(exception);
        var title = context.Title ?? _localizer["Error"];
        var details = context.ShowDetails ? exception.ToString() : null;

        return new ErrorInfo(message, title, details);
    }

    /// <summary>
    /// Attempts to extract error info from ABP exceptions using reflection.
    /// This allows handling ABP exceptions without requiring a direct dependency on ABP packages.
    /// </summary>
    protected virtual ErrorInfo? TryGetAbpErrorInfo(Exception exception)
    {
        var exceptionType = exception.GetType();
        var typeName = exceptionType.FullName ?? exceptionType.Name;

        // Handle Volo.Abp.Http.Client.AbpRemoteCallException
        if (typeName.Contains("AbpRemoteCallException") || 
            typeName.Contains("AbpHttpClientException"))
        {
            return ExtractAbpRemoteCallExceptionInfo(exception);
        }

        // Handle Volo.Abp.AbpValidationException
        if (typeName.Contains("AbpValidationException"))
        {
            return ExtractAbpValidationExceptionInfo(exception);
        }

        // Handle Volo.Abp.UserFriendlyException and IUserFriendlyException
        if (typeName.Contains("UserFriendlyException") || 
            exceptionType.GetInterfaces().Any(i => i.Name == "IUserFriendlyException"))
        {
            return new ErrorInfo(exception.Message, _localizer["Error"], null);
        }

        // Handle Volo.Abp.BusinessException and IBusinessException
        if (typeName.Contains("BusinessException") ||
            exceptionType.GetInterfaces().Any(i => i.Name == "IBusinessException"))
        {
            return ExtractAbpBusinessExceptionInfo(exception);
        }

        return null;
    }

    /// <summary>
    /// Extracts error info from AbpRemoteCallException which contains RemoteServiceErrorInfo.
    /// </summary>
    private ErrorInfo ExtractAbpRemoteCallExceptionInfo(Exception exception)
    {
        try
        {
            // Try to get the Error property which is of type RemoteServiceErrorInfo
            var errorProperty = exception.GetType().GetProperty("Error");
            if (errorProperty != null)
            {
                var errorInfo = errorProperty.GetValue(exception);
                if (errorInfo != null)
                {
                    return ExtractRemoteServiceErrorInfo(errorInfo);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract ABP remote call exception info");
        }

        // Fallback to exception message
        return new ErrorInfo(exception.Message, _localizer["Error"], null);
    }

    /// <summary>
    /// Extracts error info from RemoteServiceErrorInfo object.
    /// </summary>
    private ErrorInfo ExtractRemoteServiceErrorInfo(object errorInfo)
    {
        var errorType = errorInfo.GetType();

        var code = GetPropertyValue<string>(errorInfo, "Code");

        // Get Message
        var message = NormalizeMessage(GetPropertyValue<string>(errorInfo, "Message"), code);

        // Get Details
        var details = GetPropertyValue<string>(errorInfo, "Details");

        var title = _localizer["Error"];

        // Get ValidationErrors
        var validationErrors = GetPropertyValue<IEnumerable>(errorInfo, "ValidationErrors");
        if (validationErrors != null)
        {
            var validationMessages = ExtractValidationMessages(validationErrors);
            if (!string.IsNullOrEmpty(validationMessages))
            {
                // If we have validation errors, show them as the details
                details = validationMessages;
            }
        }

        return new ErrorInfo(message, title, details);
    }

    /// <summary>
    /// Extracts error info from AbpValidationException.
    /// </summary>
    private ErrorInfo ExtractAbpValidationExceptionInfo(Exception exception)
    {
        try
        {
            // Try to get ValidationErrors property
            var validationErrorsProperty = exception.GetType().GetProperty("ValidationErrors");
            if (validationErrorsProperty != null)
            {
                var validationErrors = validationErrorsProperty.GetValue(exception) as IEnumerable;
                if (validationErrors != null)
                {
                    var messages = new StringBuilder();
                    foreach (var error in validationErrors)
                    {
                        // ValidationResult has ErrorMessage and MemberNames
                        var errorMessage = GetPropertyValue<string>(error, "ErrorMessage");
                        if (!string.IsNullOrEmpty(errorMessage))
                        {
                            messages.AppendLine($"• {errorMessage}");
                        }
                    }

                    if (messages.Length > 0)
                    {
                        return new ErrorInfo(
                            _localizer["PleaseCorrectErrors"],
                            _localizer["ValidationError"],
                            messages.ToString().TrimEnd());
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Failed to extract ABP validation exception info");
        }

        return new ErrorInfo(exception.Message, _localizer["ValidationError"], null);
    }

    /// <summary>
    /// Extracts error info from BusinessException.
    /// </summary>
    private ErrorInfo ExtractAbpBusinessExceptionInfo(Exception exception)
    {
        var details = GetPropertyValue<string>(exception, "Details");
        var code = GetPropertyValue<string>(exception, "Code");

        return new ErrorInfo(
            NormalizeMessage(exception.Message, code, exception.Data),
            _localizer["Error"],
            details);
    }

    /// <summary>
    /// Extracts validation messages from a collection of RemoteServiceValidationErrorInfo objects.
    /// </summary>
    private string ExtractValidationMessages(IEnumerable validationErrors)
    {
        var messages = new StringBuilder();

        foreach (var error in validationErrors)
        {
            if (error == null) continue;

            // RemoteServiceValidationErrorInfo has Message and Members properties
            var errorMessage = GetPropertyValue<string>(error, "Message");
            var members = GetPropertyValue<string[]>(error, "Members");

            if (!string.IsNullOrEmpty(errorMessage))
            {
                if (members != null && members.Length > 0)
                {
                    messages.AppendLine($"• {string.Join(", ", members)}: {errorMessage}");
                }
                else
                {
                    messages.AppendLine($"• {errorMessage}");
                }
            }
        }

        return messages.ToString().TrimEnd();
    }

    /// <summary>
    /// Gets a property value from an object using reflection.
    /// </summary>
    private static T? GetPropertyValue<T>(object obj, string propertyName)
    {
        try
        {
            var property = obj.GetType().GetProperty(propertyName, 
                BindingFlags.Public | BindingFlags.Instance);
            
            if (property != null)
            {
                var value = property.GetValue(obj);
                if (value is T typedValue)
                {
                    return typedValue;
                }
            }
        }
        catch
        {
            // Ignore reflection errors
        }

        return default;
    }

    protected virtual string GetUserFriendlyMessage(Exception exception)
    {
        // Handle common exception types with user-friendly messages
        return exception switch
        {
            HttpRequestException => _localizer["NetworkError"],
            TaskCanceledException => _localizer["OperationCancelled"],
            OperationCanceledException => _localizer["OperationCancelled"],
            UnauthorizedAccessException => _localizer["Unauthorized"],
            InvalidOperationException ex => ex.Message,
            ArgumentException ex => ex.Message,
            _ => exception.Message // Show the actual message instead of generic text
        };
    }

    private string NormalizeMessage(string? message, string? errorCode = null, IDictionary? data = null)
    {
        var localizedMessage = LocalizeErrorCode(errorCode, data);
        if (!string.IsNullOrWhiteSpace(localizedMessage))
        {
            return localizedMessage;
        }

        if (IsGenericExceptionMessage(message))
        {
            return _localizer["DefaultErrorMessage"];
        }

        return NormalizeKeyLikeMessage(message!);
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

    private static string NormalizeKeyLikeMessage(string message)
    {
        if (!LooksLikeLocalizationKey(message))
        {
            return message;
        }

        var key = message[(message.LastIndexOf(':') + 1)..];
        if (string.IsNullOrWhiteSpace(key))
        {
            return message;
        }

        var builder = new StringBuilder(key.Length + 8);
        for (var i = 0; i < key.Length; i++)
        {
            var current = key[i];
            if (i > 0 && char.IsUpper(current) && !char.IsWhiteSpace(key[i - 1]))
            {
                builder.Append(' ');
            }

            builder.Append(current);
        }

        return builder.ToString();
    }

    private static bool LooksLikeLocalizationKey(string message)
    {
        if (message.Any(char.IsWhiteSpace) || !message.Contains(':', StringComparison.Ordinal))
        {
            return false;
        }

        var key = message[(message.LastIndexOf(':') + 1)..];
        return key.Length > 0 &&
               key.Any(char.IsUpper) &&
               key.All(ch => char.IsLetterOrDigit(ch) || ch is '_' or '-' or '.');
    }

    protected virtual void LogException(UserExceptionInformerContext context)
    {
        _logger.LogError(context.Exception, "User exception occurred: {Message}", context.Exception.Message);
    }

    protected record ErrorInfo(string Message, string? Title, string? Details);
}
