using SufiChain.SufiPlatform.Identity;
using SufiChain.SufiPlatform.Identity.Settings;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace SufiChain.SufiPlatform.Captcha;

/// <summary>
/// Generates and validates simple arithmetic captcha challenges stored in distributed cache.
/// </summary>
public class SimpleMathCaptchaProvider : ICaptchaProvider, ITransientDependency
{
    private const int DefaultExpirationMinutes = 5;

    protected IDistributedCache<SimpleMathCaptchaCacheItem> Cache { get; }

    protected ISettingProvider SettingProvider { get; }

    public string Name => CaptchaProviderNames.Simple;

    public SimpleMathCaptchaProvider(
        IDistributedCache<SimpleMathCaptchaCacheItem> cache,
        ISettingProvider settingProvider)
    {
        Cache = cache;
        SettingProvider = settingProvider;
    }

    public virtual async Task<CaptchaChallenge> GenerateChallengeAsync(CancellationToken cancellationToken = default)
    {
        var left = Random.Shared.Next(1, 20);
        var right = Random.Shared.Next(1, 20);
        var answer = left + right;
        var challengeId = Guid.NewGuid().ToString("N");
        var expirationMinutes = await GetExpirationMinutesAsync();

        await Cache.SetAsync(
            GetCacheKey(challengeId),
            new SimpleMathCaptchaCacheItem
            {
                AnswerHash = CaptchaAnswerHasher.Hash(answer.ToString())
            },
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(expirationMinutes)
            },
            token: cancellationToken);

        return new CaptchaChallenge
        {
            ChallengeId = challengeId,
            ProviderName = Name,
            Question = $"{left} + {right} = ?"
        };
    }

    public virtual async Task<CaptchaValidationResult> ValidateAsync(
        CaptchaValidationContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.ChallengeId) || string.IsNullOrWhiteSpace(context.Answer))
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        var cacheItem = await Cache.GetAsync(GetCacheKey(context.ChallengeId), token: cancellationToken);
        if (cacheItem == null)
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        var answerHash = CaptchaAnswerHasher.Hash(context.Answer.Trim());
        if (!string.Equals(cacheItem.AnswerHash, answerHash, StringComparison.OrdinalIgnoreCase))
        {
            return CaptchaValidationResult.Failure(IdentitySecurityErrorCodes.CaptchaValidationFailed);
        }

        await Cache.RemoveAsync(GetCacheKey(context.ChallengeId), token: cancellationToken);
        return CaptchaValidationResult.Success();
    }

    protected virtual async Task<int> GetExpirationMinutesAsync()
    {
        var value = await SettingProvider.GetOrNullAsync(IdentitySettingNames.Captcha.ChallengeExpirationMinutes);
        return int.TryParse(value, out var minutes) && minutes > 0
            ? minutes
            : DefaultExpirationMinutes;
    }

    protected static string GetCacheKey(string challengeId)
    {
        return $"Sufi:Captcha:SimpleMath:{challengeId}";
    }
}
