using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

/// <summary>sufi-chain#551: the usage log must not show the unfilled HTTP {StatusCode} placeholder.</summary>
public class AiUsageErrorTextTests
{
    [Fact]
    public void Provider_failure_code_and_unfilled_placeholder_are_recognized()
    {
        Assert.True(AiUsageErrorText.IsUnfilledProviderStatus(AIErrorCodes.ProviderRequestFailed));
        Assert.True(AiUsageErrorText.IsUnfilledProviderStatus("The provider failed (HTTP {StatusCode})."));
    }

    [Theory]
    [InlineData("en", "The AI provider could not complete the request (HTTP {StatusCode}). Please try again in a moment.")]
    [InlineData("fa", "ارائه‌دهنده هوش مصنوعی نتوانست درخواست را تکمیل کند (HTTP {StatusCode}). لطفاً کمی بعد دوباره تلاش کنید.")]
    [InlineData("ar", "تعذّر على مزود الذكاء الاصطناعي إكمال الطلب (HTTP {StatusCode}). يرجى المحاولة مرة أخرى بعد لحظات.")]
    [InlineData("es", "El proveedor de IA no pudo completar la solicitud (HTTP {StatusCode}). Inténtelo de nuevo en un momento.")]
    public void Provider_failure_text_fills_the_status_code_in_every_language(string culture, string template)
    {
        var formatted = AiProviderFailureMessage.Format(template, 502);

        Assert.Contains("502", formatted, StringComparison.Ordinal);
        Assert.DoesNotContain("{StatusCode}", formatted, StringComparison.Ordinal);
        Assert.False(string.IsNullOrWhiteSpace(culture));
    }

    [Fact]
    public void Provider_failure_text_drops_the_placeholder_when_the_status_is_unknown()
    {
        var formatted = AiProviderFailureMessage.Format(
            "The AI provider could not complete the request (HTTP {StatusCode}). Please try again in a moment.",
            statusCode: null);

        Assert.DoesNotContain("{StatusCode}", formatted, StringComparison.Ordinal);
        Assert.Contains("could not complete the request", formatted, StringComparison.Ordinal);
    }

    [Fact]
    public void Sanitized_provider_error_keeps_the_status_for_the_modal()
    {
        var error = LlmCallFailure.Sanitize(new LlmProviderCallException("upstream", statusCode: 502));

        Assert.Equal(AIErrorCodes.ProviderRequestFailed, error.Code);
        Assert.Equal(502, error.Data["StatusCode"]);
        var formatted = AiProviderFailureMessage.Format(
            "The AI provider could not complete the request (HTTP {StatusCode}). Please try again in a moment.",
            (int)error.Data["StatusCode"]!);
        Assert.Equal(
            "The AI provider could not complete the request (HTTP 502). Please try again in a moment.",
            formatted);
    }

    [Fact]
    public void A_normal_sentence_is_left_alone()
    {
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("The model declined the request."));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus(null));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("  "));
    }
}
