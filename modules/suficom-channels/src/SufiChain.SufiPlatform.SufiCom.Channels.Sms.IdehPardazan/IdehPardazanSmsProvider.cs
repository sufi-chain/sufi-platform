using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using IPE.SmsIrClient;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan;

/// <summary>
/// IdehPardazan (SMS.ir) SMS provider implementation.
/// OTPs use the Verify route (service line, approved template); plain messages use bulk send.
/// Auto-discovered via ITransientDependency
/// </summary>
public class IdehPardazanSmsChannel : ISmsOtpChannel, ITransientDependency
{
    public const string DefaultCodeParameterName = "Code";

    private readonly ILogger<IdehPardazanSmsChannel> _logger;
    private readonly SmsIrRestClient _restClient;
    private SmsIr? _smsIrClient;
    private string? _apiKey;
    private string? _senderNumber;
    private Dictionary<string, string> _settings = new();

    public string Name => ChannelNames.SmsIdehPardazan;

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Sms;

    public string ProviderCode => "SmsIr"; // Keep compatible name
    
    public int Priority => 5; // Medium priority
    
    public IdehPardazanSmsChannel(ILogger<IdehPardazanSmsChannel> logger)
        : this(logger, new SmsIrRestClient())
    {
    }

    public IdehPardazanSmsChannel(ILogger<IdehPardazanSmsChannel> logger, SmsIrRestClient restClient)
    {
        _logger = logger;
        _restClient = restClient;
    }
    
    public void Configure(Dictionary<string, string> settings)
    {
        if (!settings.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrEmpty(apiKey))
        {
            throw new AbpException("SMS.ir ApiKey is required");
        }
        
        if (!settings.TryGetValue("SenderNumber", out var senderNumber) || string.IsNullOrEmpty(senderNumber))
        {
            throw new AbpException("SMS.ir SenderNumber is required");
        }
        
        _smsIrClient = new SmsIr(apiKey);
        _apiKey = apiKey;
        _senderNumber = senderNumber;
        _settings = new Dictionary<string, string>(settings);
    }

    public string? GetOtpConfigurationError()
    {
        var template = OtpProviderSettingKeys.ResolveTemplate(_settings, OtpPurposes.Verification);
        return template == null || !int.TryParse(template, out _)
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
        if (template == null || !int.TryParse(template, out var templateId))
        {
            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpProviderSettingKeys.TemplateRequiredError
            };
        }

        var parameterName = _settings.TryGetValue(OtpProviderSettingKeys.CodeParameterName, out var configuredName) &&
                            !string.IsNullOrWhiteSpace(configuredName)
            ? configuredName.Trim().Trim('#')
            : DefaultCodeParameterName;

        var mobile = IranianMobileNumber.ToNationalNumber(message.Phone);
        if (mobile == null)
        {
            return new SmsDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.InvalidPhone
            };
        }

        try
        {
            var result = await _restClient.VerifySendAsync(
                _apiKey,
                mobile,
                templateId,
                parameterName,
                message.Code,
                OtpRequestTimeout.FromSettings(_settings, SmsIrRestClient.DefaultTimeoutSeconds));

            if (result.Success)
            {
                _logger.LogInformation(
                    "SMS.ir Verify result: HttpStatus={HttpStatus}, Status={Status}, MessageId={MessageId}",
                    result.HttpStatusCode,
                    result.Status,
                    result.MessageId);
            }
            else
            {
                OtpFailureMessages.LogHttpFailure(
                    _logger,
                    ProviderCode,
                    result.HttpStatusCode,
                    result.Status,
                    result.ResponseBody,
                    _apiKey,
                    message.Code);
            }

            return new SmsDeliveryResult
            {
                Success = result.Success,
                ExternalId = result.MessageId?.ToString(),
                StatusCode = result.Status,
                IsTransientFailure = result.IsTransientFailure,
                ErrorMessage = result.Success
                    ? null
                    : OtpFailureMessages.Format(OtpFailureMessages.SmsIrRejected, result.HttpStatusCode, result.Status)
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
        if (_smsIrClient == null || string.IsNullOrEmpty(_senderNumber))
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }
        
        try
        {
            _logger.LogInformation(
                "Sending SMS via SMS.ir to {Phone}",
                message.Phone);
            
            var mobile = IranianMobileNumber.ToNationalNumber(message.Phone);
            if (mobile == null)
            {
                return new SmsDeliveryResult
                {
                    Success = false,
                    ErrorMessage = OtpFailureMessages.InvalidPhone
                };
            }

            var response = await _smsIrClient.BulkSendAsync(
                long.Parse(_senderNumber),
                message.Content,
                new string[] { mobile });
            
            var success = response.Status == 1 && response.Data != null;
            
            _logger.LogInformation(
                "SMS.ir send result: Status={Status}, Success={Success}",
                response.Status,
                success);
            
            return new SmsDeliveryResult
            {
                Success = success,
                ExternalId = response.Data?.MessageIds?.JoinAsString(","),
                StatusCode = response.Status,
                AdditionalData = response.Message
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
        try
        {
            // SMS.ir doesn't have a simple status check API in the current SDK
            // Return a basic status
            _logger.LogWarning("SMS.ir delivery status check not fully implemented");
            
            return await Task.FromResult(new MessageDeliveryStatus
            {
                ExternalId = externalId,
                State = DeliveryState.Sent, // Assume sent
                ProviderCode = ProviderCode
            });
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
            if (string.IsNullOrEmpty(_apiKey))
            {
                return new ProviderHealthCheckResult
                {
                    IsHealthy = false,
                    Message = "Provider not configured",
                    ResponseTimeMs = 0,
                    CheckedAt = DateTime.UtcNow
                };
            }

            var credit = await _restClient.GetCreditAsync(
                _apiKey,
                OtpRequestTimeout.FromSettings(_settings, SmsIrRestClient.DefaultTimeoutSeconds));
            var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (!credit.Success)
            {
                return new ProviderHealthCheckResult
                {
                    IsHealthy = false,
                    Message = OtpFailureMessages.Format(OtpFailureMessages.SmsIrRejected, credit.HttpStatusCode, credit.Status),
                    ResponseTimeMs = responseTime,
                    CheckedAt = DateTime.UtcNow
                };
            }

            var otpError = GetOtpConfigurationError();
            return new ProviderHealthCheckResult
            {
                IsHealthy = otpError == null,
                Message = otpError == null
                    ? $"SMS.ir API healthy. Remaining credit: {credit.Credit}"
                    : $"SMS.ir API reachable, but the OTP template id is not configured. Remaining credit: {credit.Credit}",
                ResponseTimeMs = responseTime,
                CheckedAt = DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);
            
            var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            
            return new ProviderHealthCheckResult
            {
                IsHealthy = false,
                Message = OtpFailureMessages.RequestFailed,
                ResponseTimeMs = responseTime,
                CheckedAt = DateTime.UtcNow
            };
        }
    }
    
    public Task<List<string>> GetSupportedFeaturesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "SMS",
            "Otp",
            "Bulk",
            "Persian",
            "Unicode"
        });
    }
    
    public Task<SmsProviderMetadata> GetMetadataAsync()
    {
        return Task.FromResult(new SmsProviderMetadata
        {
            DisplayName = "Providers.IdehPardazan.DisplayName",
            Description = "Providers.IdehPardazan.Description",
            LogoUrl = "/images/providers/smsir.png",
            WebsiteUrl = "https://sms.ir"
        });
    }
    
    public Task<List<ProviderSettingField>> GetRequiredSettingsAsync()
    {
        var fields = new List<ProviderSettingField>
        {
            new ProviderSettingField
            {
                Key = "ApiKey",
                Label = "Providers.IdehPardazan.ApiKey.Label",
                FieldType = "password",
                IsRequired = true,
                IsSensitive = true,
                Placeholder = "Providers.IdehPardazan.ApiKey.Placeholder",
                HelpText = "Providers.IdehPardazan.ApiKey.HelpText",
                ValidationPattern = @"^[a-zA-Z0-9\-_]+$"
            },
            new ProviderSettingField
            {
                Key = "SenderNumber",
                Label = "Providers.IdehPardazan.SenderNumber.Label",
                FieldType = "text",
                IsRequired = true,
                Placeholder = "Providers.IdehPardazan.SenderNumber.Placeholder",
                HelpText = "Providers.IdehPardazan.SenderNumber.HelpText",
                ValidationPattern = @"^\d+$"
            },
            new ProviderSettingField
            {
                Key = "TimeoutSeconds",
                Label = "Providers.IdehPardazan.TimeoutSeconds.Label",
                FieldType = "number",
                IsRequired = false,
                DefaultValue = "30",
                Placeholder = "Providers.IdehPardazan.TimeoutSeconds.Placeholder",
                HelpText = "Providers.IdehPardazan.TimeoutSeconds.HelpText"
            }
        };

        fields.AddRange(OtpProviderSettingKeys.CreateTemplateFields("Providers.IdehPardazan", @"^\d+$"));
        fields.Add(new ProviderSettingField
        {
            Key = OtpProviderSettingKeys.CodeParameterName,
            Label = "Providers.IdehPardazan.OtpCodeParameterName.Label",
            FieldType = "text",
            IsRequired = false,
            DefaultValue = DefaultCodeParameterName,
            Placeholder = "Providers.IdehPardazan.OtpCodeParameterName.Placeholder",
            HelpText = "Providers.IdehPardazan.OtpCodeParameterName.HelpText",
            ValidationPattern = @"^#?[a-zA-Z0-9_]+#?$"
        });

        return Task.FromResult(fields);
    }
}
