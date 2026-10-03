using System;
using System.Threading.Tasks;
using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Account.Otp;

public class OtpResendCooldownOptions
{
    public int CooldownSeconds { get; set; } = 120;

    public int BackoffAfterAttempts { get; set; } = 5;

    public int BackoffMultiplier { get; set; } = 2;

    public int MaxCooldownSeconds { get; set; } = 86400;

    public int CounterResetHours { get; set; } = 24;

    public bool IsEnabled => CooldownSeconds > 0;

    /// <summary>
    /// Seconds a phone number must wait after its <paramref name="sendCount"/>-th accepted request (1-based).
    /// The first <see cref="BackoffAfterAttempts"/> requests wait <see cref="CooldownSeconds"/>; each later request
    /// multiplies the wait by <see cref="BackoffMultiplier"/>, capped at <see cref="MaxCooldownSeconds"/>.
    /// </summary>
    public int GetCooldownSeconds(int sendCount)
    {
        if (!IsEnabled)
        {
            return 0;
        }

        var cap = MaxCooldownSeconds > 0 ? Math.Max(MaxCooldownSeconds, CooldownSeconds) : int.MaxValue;
        var backoffSteps = sendCount - Math.Max(BackoffAfterAttempts, 0);
        if (backoffSteps <= 0 || BackoffMultiplier <= 1)
        {
            return Math.Min(CooldownSeconds, cap);
        }

        var seconds = (double)CooldownSeconds * Math.Pow(BackoffMultiplier, backoffSteps);
        return seconds >= cap ? cap : (int)seconds;
    }

    public TimeSpan GetCounterResetWindow()
    {
        return TimeSpan.FromHours(CounterResetHours > 0 ? CounterResetHours : 24);
    }

    public static async Task<OtpResendCooldownOptions> CreateAsync(ISettingProvider settingProvider)
    {
        return new OtpResendCooldownOptions
        {
            CooldownSeconds = await settingProvider.GetAsync<int>(IdentitySettingNames.Otp.ResendCooldownSeconds),
            BackoffAfterAttempts = await settingProvider.GetAsync<int>(IdentitySettingNames.Otp.ResendBackoffAfterAttempts),
            BackoffMultiplier = await settingProvider.GetAsync<int>(IdentitySettingNames.Otp.ResendBackoffMultiplier),
            MaxCooldownSeconds = await settingProvider.GetAsync<int>(IdentitySettingNames.Otp.ResendMaxCooldownSeconds),
            CounterResetHours = await settingProvider.GetAsync<int>(IdentitySettingNames.Otp.ResendCounterResetHours)
        };
    }
}
