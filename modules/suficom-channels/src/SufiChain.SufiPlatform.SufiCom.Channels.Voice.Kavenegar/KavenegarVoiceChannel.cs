using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Kavenegar;
using Kavenegar.Core.Exceptions;
using Microsoft.Extensions.Logging;
using SufiChain.SufiPlatform.SufiCom.Channels;
using SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar;
using Volo.Abp;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.SufiCom.Channels.Voice.Kavenegar;

/// <summary>
/// Kavenegar Voice Call provider implementation (TTS - Text-to-Speech).
/// OTP calls use Verify Lookup with <c>type=call</c> and an approved template.
/// </summary>
public class KavenegarVoiceChannel : IVoiceOtpChannel, ITransientDependency
{
    private readonly ILogger<KavenegarVoiceChannel> _logger;
    private readonly KavenegarVerifyLookupClient _lookupClient;
    private KavenegarApi? _api;
    private string? _apiKey;
    private Dictionary<string, string> _settings = new();

    public string Name => ChannelNames.VoiceKavenegar;

    public ChannelOrigin ChannelOrigin => ChannelOrigin.Voice;

    public string ProviderCode => "Kavenegar";

    public int Priority => 10;

    public KavenegarVoiceChannel(ILogger<KavenegarVoiceChannel> logger)
        : this(logger, new KavenegarVerifyLookupClient())
    {
    }

    public KavenegarVoiceChannel(ILogger<KavenegarVoiceChannel> logger, KavenegarVerifyLookupClient lookupClient)
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

        _api = new KavenegarApi(apiKey);
        _apiKey = apiKey;
        _settings = new Dictionary<string, string>(settings);
    }

    public string? GetOtpConfigurationError()
    {
        return OtpProviderSettingKeys.ResolveTemplate(_settings, OtpPurposes.Verification) == null
            ? OtpProviderSettingKeys.TemplateRequiredError
            : null;
    }

    public async Task<VoiceCallDeliveryResult> SendOtpAsync(OtpMessage message)
    {
        if (string.IsNullOrEmpty(_apiKey))
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }

        var template = OtpProviderSettingKeys.ResolveTemplate(_settings, message.Purpose);
        if (template == null)
        {
            return new VoiceCallDeliveryResult
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
                KavenegarVerifyLookupClient.TypeCall,
                OtpRequestTimeout.FromSettings(_settings, KavenegarVerifyLookupClient.DefaultTimeoutSeconds));

            _logger.LogInformation(
                "Kavenegar Verify Lookup call result: Status={Status}, MessageId={MessageId}",
                result.ReturnStatus,
                result.MessageId);

            return new VoiceCallDeliveryResult
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

            return new VoiceCallDeliveryResult
            {
                Success = false,
                IsTransientFailure = true,
                ErrorMessage = OtpFailureMessages.TimedOut
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new VoiceCallDeliveryResult
            {
                Success = false,
                IsTransientFailure = true,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
    }

    public async Task<VoiceCallDeliveryResult> SendAsync(VoiceCallMessage message)
    {
        if (_api == null)
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }

        try
        {
            _logger.LogInformation(
                "Initiating voice call via Kavenegar TTS to {Phone}",
                message.Phone);

            var result = await _api.CallMakeTTS(
                message.Content,
                message.Phone);

            var statusCode = result.Status;
            var success = statusCode == 1 || statusCode == 5;

            _logger.LogInformation(
                "Kavenegar voice call result: Status={Status}, MessageId={MessageId}",
                statusCode,
                result.Messageid);

            return new VoiceCallDeliveryResult
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

            return new VoiceCallDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.Format(OtpFailureMessages.KavenegarRejected, (int)ex.Code),
                StatusCode = (int)ex.Code
            };
        }
        catch (HttpException ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new VoiceCallDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            return new VoiceCallDeliveryResult
            {
                Success = false,
                ErrorMessage = OtpFailureMessages.RequestFailed
            };
        }
    }

    public async Task<MessageDeliveryStatus> GetDeliveryStatusAsync(string externalId)
    {
        if (_api == null)
        {
            throw new AbpException("Provider not configured. Call Configure() first.");
        }

        try
        {
            if (!long.TryParse(externalId, out var messageId))
            {
                throw new ArgumentException($"Invalid call ID format: {externalId}");
            }

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
        if (_api == null)
        {
            return new ProviderHealthCheckResult
            {
                IsHealthy = false,
                Message = "Provider not configured",
                CheckedAt = DateTime.UtcNow
            };
        }

        var startTime = DateTime.UtcNow;

        try
        {
            var accountInfo = await _api.AccountInfo();
            var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;
            var otpError = GetOtpConfigurationError();

            return new ProviderHealthCheckResult
            {
                IsHealthy = otpError == null,
                Message = otpError == null
                    ? $"Kavenegar Voice API healthy. Remaining credit: {accountInfo.RemainCredit}"
                    : $"Kavenegar Voice API reachable, but the OTP template is not configured. Remaining credit: {accountInfo.RemainCredit}",
                ResponseTimeMs = responseTime,
                CheckedAt = DateTime.UtcNow,
                AdditionalData = $"Expiry: {accountInfo.Expiredate}"
            };
        }
        catch (Exception ex)
        {
            OtpFailureMessages.LogFailure(_logger, ProviderCode, ex);

            var responseTime = (long)(DateTime.UtcNow - startTime).TotalMilliseconds;

            return new ProviderHealthCheckResult
            {
                IsHealthy = false,
                Message = "Kavenegar health check failed.",
                ResponseTimeMs = responseTime,
                CheckedAt = DateTime.UtcNow
            };
        }
    }

    public Task<List<string>> GetSupportedFeaturesAsync()
    {
        return Task.FromResult(new List<string>
        {
            "Voice",
            "Otp",
            "TTS",
            "Persian",
            "Repeat",
            "DeliveryReport"
        });
    }

    public Task<SmsProviderMetadata> GetMetadataAsync()
    {
        return Task.FromResult(new SmsProviderMetadata
        {
            DisplayName = "Providers.Kavenegar.Voice.DisplayName",
            Description = "Providers.Kavenegar.Voice.Description",
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
            }
        };

        fields.AddRange(OtpProviderSettingKeys.CreateTemplateFields("Providers.Kavenegar.Voice", @"^[a-zA-Z0-9\-_]+$"));
        return Task.FromResult(fields);
    }
}
