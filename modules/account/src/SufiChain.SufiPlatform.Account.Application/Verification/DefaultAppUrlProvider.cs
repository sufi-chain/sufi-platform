using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Volo.Abp.DependencyInjection;

namespace SufiChain.SufiPlatform.Account;

public class DefaultAppUrlProvider : IAppUrlProvider, ITransientDependency
{
    protected SufiAccountUrlOptions Options { get; }

    public DefaultAppUrlProvider(IOptions<SufiAccountUrlOptions> options)
    {
        Options = options.Value;
    }

    public virtual Task<string> GetEmailConfirmationUrlAsync(
        string appName,
        Guid userId,
        string confirmationToken,
        string? returnUrl = null,
        string? returnUrlHash = null)
    {
        return Task.FromResult(BuildUrl(
            appName,
            "account/confirm-email",
            userId,
            confirmationToken,
            returnUrl,
            returnUrlHash));
    }

    public virtual Task<string> GetPasswordResetUrlAsync(
        string appName,
        Guid userId,
        string resetToken,
        string? returnUrl = null,
        string? returnUrlHash = null)
    {
        return Task.FromResult(BuildUrl(
            appName,
            "account/reset-password",
            userId,
            resetToken,
            returnUrl,
            returnUrlHash));
    }

    protected virtual string BuildUrl(
        string appName,
        string path,
        Guid userId,
        string token,
        string? returnUrl,
        string? returnUrlHash)
    {
        var rootUrl = Options.AppRootUrls.TryGetValue(appName, out var configuredRoot)
            ? configuredRoot
            : Options.DefaultRootUrl;

        rootUrl = rootUrl.TrimEnd('/');

        var url = $"{rootUrl}/{path}?userId={userId}&token={Uri.EscapeDataString(token)}";

        if (!string.IsNullOrWhiteSpace(returnUrl))
        {
            url += $"&returnUrl={Uri.EscapeDataString(returnUrl)}";
        }

        if (!string.IsNullOrWhiteSpace(returnUrlHash))
        {
            url += $"&returnUrlHash={Uri.EscapeDataString(returnUrlHash)}";
        }

        return url;
    }
}
