using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.SufiAbp.UI.Browser;

namespace SufiChain.SufiAbp.UI.Blazor.Http;

/// <summary>
/// HTTP message handler that adds common headers for Blazor client requests:
/// - Accept-Language header from localStorage
/// - Anti-forgery (XSRF) token for non-GET requests
/// </summary>
public class SufiAbpBlazorClientHttpMessageHandler : DelegatingHandler
{
    private const string AntiForgeryCookieName = "XSRF-TOKEN";
    private const string AntiForgeryHeaderName = "RequestVerificationToken";
    private const string SelectedLanguageKey = "SufiAbp.SelectedLanguage";

    private readonly IJSRuntime _jsRuntime;
    private readonly ICookieService _cookieService;
    private readonly NavigationManager _navigationManager;

    public SufiAbpBlazorClientHttpMessageHandler(
        IJSRuntime jsRuntime,
        ICookieService cookieService,
        NavigationManager navigationManager)
    {
        _jsRuntime = jsRuntime;
        _cookieService = cookieService;
        _navigationManager = navigationManager;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        await SetLanguageAsync(request, cancellationToken);
        await SetAntiForgeryTokenAsync(request);

        return await base.SendAsync(request, cancellationToken);
    }

    /// <summary>
    /// Sets the Accept-Language header from localStorage if a language preference is stored.
    /// </summary>
    private async Task SetLanguageAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var selectedLanguage = await _jsRuntime.InvokeAsync<string?>(
                "localStorage.getItem",
                cancellationToken,
                SelectedLanguageKey);

            if (!string.IsNullOrWhiteSpace(selectedLanguage))
            {
                request.Headers.AcceptLanguage.Clear();
                request.Headers.AcceptLanguage.Add(new StringWithQualityHeaderValue(selectedLanguage));
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
        catch (TaskCanceledException)
        {
            // Request cancelled, ignore
        }
    }

    /// <summary>
    /// Sets the anti-forgery token header for mutating requests (POST, PUT, DELETE, PATCH).
    /// Only adds the header for requests to the same host/port as the current page.
    /// </summary>
    private async Task SetAntiForgeryTokenAsync(HttpRequestMessage request)
    {
        // Skip for safe methods (GET, HEAD, OPTIONS, TRACE)
        if (request.Method == HttpMethod.Get || 
            request.Method == HttpMethod.Head ||
            request.Method == HttpMethod.Trace || 
            request.Method == HttpMethod.Options)
        {
            return;
        }

        // Only add token for same-origin requests
        var selfUri = new Uri(_navigationManager.Uri);
        if (request.RequestUri is null ||
            request.RequestUri.Host != selfUri.Host || 
            request.RequestUri.Port != selfUri.Port)
        {
            return;
        }

        try
        {
            var token = await _cookieService.GetAsync(AntiForgeryCookieName);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.TryAddWithoutValidation(AntiForgeryHeaderName, token);
            }
        }
        catch (JSDisconnectedException)
        {
            // Circuit disconnected, ignore
        }
    }
}
