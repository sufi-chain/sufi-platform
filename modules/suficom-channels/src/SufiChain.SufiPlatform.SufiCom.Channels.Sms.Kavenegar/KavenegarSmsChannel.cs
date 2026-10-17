using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kavenegar;
using Kavenegar.Core.Exceptions;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;

/// <summary>
/// Kavenegar SMS provider implementation. OTPs use Verify Lookup with an approved template.
/// </summary>
public class KavenegarSmsChannel : ISmsOtpChannel, ITransientDependency
{
    private readonly ILogger<KavenegarSmsChannel> _logger;
    private readonly KavenegarVerifyLookupClient _lookupClient;
    private KavenegarApi? _api;
    private string? _apiKey;
    private string? _senderNumber;
    private Dictionary<string, string> _settings = new();

    public string Name => ChannelNames.SmsKavenegar;

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Sms;

    public string ProviderCode => "Kavenegar";
    
    public int Priority => 10; // High priority - preferred provider
    
    public KavenegarSmsChannel(ILogger<KavenegarSmsChannel> logger)
        : this(logger, new KavenegarVerifyLookupClient())
    {
    }

    public KavenegarSmsChannel(ILogger<KavenegarSmsChannel> logger, KavenegarVerifyLookupClient lookupClient)
    {
        _logger = logger;
        _lookupClient = lookupClient;
    }
    
    public void Configure(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrEmpty(apiKey))
        {
            throw new AbpException("Kavenegar ApiKey is required");
        }
        
        if (!settings.TryGetValue("SenderNumber", out var senderNumber) || string.IsNullOrEmpty(senderNumber))
        {
            throw new AbpException("Kavenegar SenderNumber is required");
        }
        
        _api = new KavenegarApi(apiKey);
        _apiKey = apiKey;
        _senderNumber = senderNumber;
        _settings = new Dictionary<string, string>(settings);
    }

    public string? GetOtpConfigurationError()
    {
        return OtpProviderSettingKeys.ResolveTemplate(_settings, OtpPurposes.Verification) == null
            ? OtpProviderSettingKeys.TemplateRequiredError
            : null;
    }

    public async Task<SmsDeliveryResult> SendOtpAsync(OtpMessage message)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }

        var template = OtpProviderSettingKeys.ResolveTemplate(_settings, message.Purpose);
        if (template == null)
        {
            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpProviderSettingKeys.TemplateRequiredError
            };
        }

        try
        {
            var result = await _lookupClient.LookupAsync(
                _apiKey,
                IranianMobileNumber.ToLocal(message.Phone),
                message.Code,
                template,
                KavenegarVerifyLookupClient.TypeSms,
                OtpRequestTimeout.FromSettings(_settings, KavenegarVerifyLookupClient.DefaultTimeoutSeconds));

            if (result.Success)
            {
                _logger.LogInformation(
                    "Kavenegar Verify Lookup result: Status={Status}, MessageId={MessageId}",
                    result.ReturnStatus,
                    result.MessageId);
            }
            else
            {
                OtpFailureMessages.LogHttpFailure(
                    _logger,
                    ProviderCode,
                    result.HttpStatusCode,
                    result.ReturnStatus,
                    result.ResponseBody,
                    _apiKey,
                    message.Code);
            }

            return new SmsDeliveryResult
            {
                Success = result.Success,
                ExternalId = result.MessageId?.ToString(),
                StatusCode = result.ReturnStatus,
                IsTransientFailure = result.IsTransientFailure,
                ErrorMessage = result.Success
                    ? null
                    : OtpFailureMessages.Format(OtpFailureMessages.KavenegarRejected, result.ReturnStatus)
            };
        }
        catch (OperationCanceledException)
        {
            OtpFailureMessages.LogTimeout(_logger, ProviderCode);

            return new SmsDeliveryResult
            {
                Success = false,
                IsTransientFailure = true,
                ErrorMessage = OtpFailureMessages.TimedOut
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new SmsDeliveryResult
            {
                Success = false,
                IsTransientFailure = true,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
    }
    
    public async Task<SmsDeliveryResult> SendAsync(SmsMessage message)
    {
        if (_api == null || string.IsNullOrEmpty(_senderNumber))
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }
        
        try
        {
            _logger.LogInformation(
                "Sending SMS via Kavenegar to {Phone}",
                message.Phone);
            
            // Send SMS using Kavenegar API
            var result = await _api.Send(
                _senderNumber,
                message.Phone,
                message.Content);
            
            // Status codes: 1 = sent, 5 = delivered
            var statusCode = result.Status;
            var success = statusCode == 1 || statusCode == 5;
            
            _logger.LogInformation(
                "Kavenegar SMS send result: Status={Status}, MessageId={MessageId}",
                statusCode,
                result.Messageid);
            
            return new SmsDeliveryResult
            {
                Success = success,
                ExternalId = result.Messageid.ToString(),
                StatusCode = statusCode,
                AdditionalData = $"Status: {statusCode}"
            };
        }
        catch (ApiException ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.Format(OtpFailureMessages.KavenegarRejected, (int)ex.Code),
                StatusCode = (int)ex.Code
            };
        }
        catch (HttpException ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
    }
    
    public async Task<MessageDeliveryStatus> GetDeliveryStatusAsync(string externalId)
    {
        if (!long.TryParse(externalId, out var messageId))
        {
            throw new ArgumentException($"Invalid message ID format: {externalId}", nameof(externalId));
        }

        try
        {
            var results = await _api.Status(new List<string> { messageId.ToString() });
            var result = results.FirstOrDefault();
            
            if (result == null)
            {
                return new MessageDeliveryStatus
                {
                    ExternalId = externalId,
                    State = DeliveryState.Failed,
                    FailureReason = "No status result returned from Kavenegar",
                    ProviderCode = ProviderCode
                };
            }
            
            var statusCode = (int)result.Status;
            var state = statusCode switch
            {
                1 => DeliveryState.Sent,
                5 => DeliveryState.Delivered,
                10 => DeliveryState.Delivered,
                _ => DeliveryState.Failed
            };
            
            return new MessageDeliveryStatus
            {
                ExternalId = externalId,
                State = state,
                ProviderCode = ProviderCode
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);
            
            return new MessageDeliveryStatus
            {
                ExternalId = externalId,
                State = DeliveryState.Failed,
                FailureReason = "The provider request failed.",
                ProviderCode = ProviderCode
            };
        }
    }
    
    public async Task<ProviderHealthCheckResult> HealthCheckAsync()
    {
        var startTime = DateTime.UtcNow;
        
        try
        {
            if (_api == null)
            {
                return CreateUnhealthyResult(
                    startTime,
                    "Kavenegar provider is not configured");
            }

            // Simple health check: get account info
            var accountInfo = await _api.AccountInfo();
            
            var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var otpError = GetOtpConfigurationError();
            
            return new ProviderHealthCheckResult
            {
                IsHealthy = otpError == null,
                Message = otpError == null
                    ? $"Kavenegar API healthy. Remaining credit: {accountInfo.RemainCredit}"
                    : $"Kavenegar API reachable, but the OTP template is not configured. Remaining credit: {accountInfo.RemainCredit}",
                ResponseTimeMs = responseTime,
                CheckedAt = DateTime.UtcNow,
                AdditionalData = $"Expiry: {accountInfo.Expiredate}"
            };
        }
        catch (ApiException ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return CreateUnhealthyResult(
                startTime,
                OtpFailureMessages.Format(OtpFailureMessages.KavenegarRejected, (int)ex.Code));
        }
        catch (HttpException ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return CreateUnhealthyResult(
                startTime,
                OtpFailureMessages.RequestFailed);
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return CreateUnhealthyResult(
                startTime,
                OtpFailureMessages.RequestFailed);
        }
    }

    private static ProviderHealthCheckResult CreateUnhealthyResult(DateTime startTime, string message)
    {
        return new ProviderHealthCheckResult
        {
            IsHealthy = false,
            Message = message,
            ResponseTimeMs = (long)(DateTime.UtcNow - startTime).TotalMilliseconds,
            CheckedAt = DateTime.UtcNow
        };
    }
    
    public Task<List<string>> GetSupportedFeaturesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "SMS",
            "Otp",
            "Voice",
            "DeliveryReport",
            "Persian",
            "Unicode"
        });
    }
    
    public Task<SmsProviderMetadata> GetMetadataAsync()
    {
        return Task.FromResult(new SmsProviderMetadata
        {
            DisplayName = "Providers.Kavenegar.DisplayName",
            Description = "Providers.Kavenegar.Description",
            LogoUrl = "/images/providers/kavenegar.png",
            WebsiteUrl = "https://kavenegar.com"
        });
    }
    
    public Task<List<ProviderSettingField>> GetRequiredSettingsAsync()
    {
        var fields = new List<ProviderSettingField>
        {
            new ProviderSettingField
            {
                Key = "ApiKey",
                Label = "Providers.Kavenegar.ApiKey.Label",
                FieldType = "password",
                IsRequired = true,
                IsSensitive = true,
                Placeholder = "Providers.Kavenegar.ApiKey.Placeholder",
                HelpText = "Providers.Kavenegar.ApiKey.HelpText",
                ValidationPattern = @"^[a-zA-Z0-9\-_]+$"
            },
            new ProviderSettingField
            {
                Key = "SenderNumber",
                Label = "Providers.Kavenegar.SenderNumber.Label",
                FieldType = "text",
                IsRequired = true,
                Placeholder = "Providers.Kavenegar.SenderNumber.Placeholder",
                HelpText = "Providers.Kavenegar.SenderNumber.HelpText"
            },
            new ProviderSettingField
            {
                Key = "TimeoutSeconds",
                Label = "Providers.Kavenegar.TimeoutSeconds.Label",
                FieldType = "number",
                IsRequired = false,
                DefaultValue = "30",
                Placeholder = "Providers.Kavenegar.TimeoutSeconds.Placeholder",
                HelpText = "Providers.Kavenegar.TimeoutSeconds.HelpText"
            }
        };

        fields.AddRange(OtpProviderSettingKeys.CreateTemplateFields("Providers.Kavenegar", @"^[a-zA-Z0-9\-_]+$"));
        return Task.FromResult(fields);
    }
}
