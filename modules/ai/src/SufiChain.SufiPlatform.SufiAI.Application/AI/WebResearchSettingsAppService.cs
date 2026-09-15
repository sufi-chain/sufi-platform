using System;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using SufiChain.SufiPlatform.Application.Services;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.SufiAI.Web;
using Volo.Abp;
using Volo.Abp.Auditing;

namespace SufiChain.SufiPlatform.SufiAI;

[Authorize("SufiAI.WebSearchSettings.Default")]
public class WebResearchSettingsAppService(
    WebResearchOptionsProvider optionsProvider, ISettingManager manager,
    IWebSearchService search) : SufiApplicationService, IWebResearchSettingsAppService
{
    public virtual async Task<WebResearchSettingsDto> GetAsync()
    {
        var options = await optionsProvider.GetAsync();
        return new WebResearchSettingsDto
        {
            Enabled = options.Enabled, Endpoint = options.Endpoint, HasStoredToken = !string.IsNullOrEmpty(options.Token),
            SafeSearch = options.SafeSearch, SearchTimeoutSeconds = options.SearchTimeoutSeconds,
            FetchTimeoutSeconds = options.FetchTimeoutSeconds, MaxSearchResults = options.MaxSearchResults,
            MaxPagesToFetch = options.MaxPagesToFetch, MaxPageBytes = options.MaxPageBytes,
            MaxExtractedCharactersPerPage = options.MaxExtractedCharactersPerPage,
            MaxTotalContextCharacters = options.MaxTotalContextCharacters
        };
    }

    [Authorize("SufiAI.WebSearchSettings.Update")]
    [DisableAuditing]
    public virtual async Task UpdateAsync(UpdateWebResearchSettingsInput input)
    {
        input.Endpoint = input.Endpoint?.Trim() ?? "";
        if ((input.Enabled || !string.IsNullOrWhiteSpace(input.Endpoint)) &&
            (!Uri.TryCreate(input.Endpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https" ||
                endpoint.UserInfo.Length > 0 || endpoint.Query.Length > 0 || endpoint.Fragment.Length > 0))
            throw new BusinessException("AI:WebResearchInvalidEndpoint");
        if (input.AuthorizationBearerToken?.IndexOfAny(['\r', '\n']) >= 0)
            throw new BusinessException("AI:WebResearchInvalidToken");
        var prior = await optionsProvider.GetAsync();
        // Never send an inherited/stored credential to a newly chosen endpoint accidentally.
        if (!StringComparer.Ordinal.Equals(prior.Endpoint.TrimEnd('/'), input.Endpoint.TrimEnd('/')) &&
            !string.IsNullOrEmpty(prior.Token) && string.IsNullOrWhiteSpace(input.AuthorizationBearerToken) && !input.ClearToken)
            throw new BusinessException("AI:WebResearchTokenRequiredForEndpointChange");
        await Set(WebResearchSettings.Enabled, input.Enabled.ToString());
        await Set(WebResearchSettings.Endpoint, input.Endpoint.Trim().TrimEnd('/'));
        if (input.ClearToken) await Set(WebResearchSettings.Token, "");
        else if (!string.IsNullOrWhiteSpace(input.AuthorizationBearerToken)) await Set(WebResearchSettings.Token, input.AuthorizationBearerToken.Trim());
        foreach (var (name, _) in WebResearchSettings.Limits)
            await Set(WebResearchSettings.Prefix + name,
                Convert.ToString(typeof(WebResearchSettingsDto).GetProperty(name)!.GetValue(input), CultureInfo.InvariantCulture)!);
    }

    [Authorize("SufiAI.WebSearchSettings.Test")]
    public virtual async Task<WebResearchConnectionDto> TestAsync()
    {
        try
        {
            var result = await search.SearchAsync(new WebSearchRequest { Query = "SearXNG", MaxResults = 1 }, await optionsProvider.GetAsync());
            return new() { Ready = result.Results.Count > 0, FailureCode = result.Results.Count == 0 ? "NoUsableSources" : null };
        }
        catch (WebResearchException ex) { return new() { FailureCode = ex.Code }; }
    }

    private Task Set(string name, string value) => manager.SetForTenantOrGlobalAsync(CurrentTenant.Id, name, value);
}
