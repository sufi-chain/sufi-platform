using System.IO;
using System.Reflection;
using System.Text;
using Volo.Abp;
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
        var error = LlmCallFailure.Sanitize(new LlmProviderCallException("upstream", statusCode: 400));

        Assert.Equal(AIErrorCodes.ProviderRequestFailed, error.Code);
        Assert.Equal(400, error.Data["StatusCode"]);
        var formatted = AiProviderFailureMessage.Format(
            "The AI provider could not complete the request (HTTP {StatusCode}). Please try again in a moment.",
            (int)error.Data["StatusCode"]!);
        Assert.Equal(
            "The AI provider could not complete the request (HTTP 400). Please try again in a moment.",
            formatted);
    }

    [Theory]
    [InlineData(401, "AI:ProviderAuthorizationFailed")]
    [InlineData(403, "AI:ProviderAuthorizationFailed")]
    [InlineData(429, "AI:ProviderRateLimited")]
    [InlineData(500, "AI:ProviderUnavailable")]
    [InlineData(502, "AI:ProviderUnavailable")]
    public void Provider_status_classes_map_to_their_own_errors(int status, string code)
    {
        var error = LlmCallFailure.Sanitize(new LlmProviderCallException(
            "Service request failed. Status: " + status + " key sk-or-v1-secretvalue",
            statusCode: status));

        Assert.Equal(code, error.Code);
        Assert.Equal(status, error.Data["StatusCode"]);
        Assert.DoesNotContain("sk-or", error.ToString(), StringComparison.Ordinal);
        var usage = LlmCallFailure.DescribeUsageFailure(error);
        Assert.Contains(status.ToString(), usage, StringComparison.Ordinal);
        Assert.DoesNotContain("sk-", usage, StringComparison.Ordinal);
    }

    [Fact]
    public void A_generic_provider_failure_is_remapped_from_its_status()
    {
        var error = LlmCallFailure.Sanitize(
            new BusinessException(AIErrorCodes.ProviderRequestFailed).WithData("StatusCode", 401));

        Assert.Equal(AIErrorCodes.ProviderAuthorizationFailed, error.Code);
        Assert.Equal(401, error.Data["StatusCode"]);
    }

    [Fact]
    public void Usage_failure_drops_a_provider_message_that_contains_a_key()
    {
        var error = new BusinessException(AIErrorCodes.ProviderAuthorizationFailed)
            .WithData("StatusCode", 401)
            .WithData("ProviderMessage", "rejected sk-live-secretvalue");

        var usage = LlmCallFailure.DescribeUsageFailure(error);

        Assert.Equal("AI:ProviderAuthorizationFailed (HTTP 401)", usage);
        Assert.DoesNotContain("sk-", usage, StringComparison.Ordinal);
    }

    [Fact]
    public void Logged_provider_bodies_do_not_keep_the_key()
    {
        var body = """{"error":{"message":"Unauthorized","api_key":"sk-or-v1-secretvalue"},"authorization":"Bearer sk-or-v1-secretvalue"}""";
        var redacted = LlmCallFailure.RedactSecrets(body);

        Assert.DoesNotContain("sk-or", redacted, StringComparison.Ordinal);
        Assert.DoesNotContain("secretvalue", redacted, StringComparison.Ordinal);
        Assert.Contains("Unauthorized", redacted, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("en", "AI provider authorization failed: check the workspace API key.")]
    [InlineData("fa", "احراز هویت ارائه‌دهنده هوش مصنوعی ناموفق بود: کلید API فضای کار را بررسی کنید.")]
    [InlineData("ar", "فشل تفويض مزود الذكاء الاصطناعي: تحقق من مفتاح واجهة برمجة التطبيقات لمساحة العمل.")]
    [InlineData("es", "Falló la autorización del proveedor de IA: compruebe la clave de API del espacio de trabajo.")]
    public void Provider_status_messages_are_localized(string culture, string authorization)
    {
        var json = ReadLocalization(culture);
        Assert.Contains("\"AI:ProviderAuthorizationFailed\": \"" + authorization + "\"", json, StringComparison.Ordinal);
        Assert.Contains("\"AI:ProviderRateLimited\":", json, StringComparison.Ordinal);
        Assert.Contains("\"AI:ProviderUnavailable\":", json, StringComparison.Ordinal);
        Assert.Contains("{StatusCode}", json, StringComparison.Ordinal);
    }

    private static string ReadLocalization(string culture)
    {
        var assembly = typeof(AIErrorCodes).Assembly;
        var name = Array.Find(
            assembly.GetManifestResourceNames(),
            resource => resource.EndsWith("Localization.AI." + culture + ".json", StringComparison.Ordinal));
        Assert.False(string.IsNullOrEmpty(name));
        using var stream = assembly.GetManifestResourceStream(name!);
        Assert.NotNull(stream);
        using var reader = new StreamReader(stream!, Encoding.UTF8);
        return reader.ReadToEnd();
    }

    [Fact]
    public void Usage_failure_keeps_the_provider_reason_and_status()
    {
        var error = new BusinessException(AIErrorCodes.ProviderRequestFailed)
            .WithData("StatusCode", 400)
            .WithData("ProviderMessage", "reasoning is not supported");

        Assert.Equal("HTTP 400: reasoning is not supported", LlmCallFailure.DescribeUsageFailure(error));
    }

    [Fact]
    public void Usage_failure_without_a_reason_keeps_the_code_and_status()
    {
        var error = new BusinessException(AIErrorCodes.ProviderRequestFailed).WithData("StatusCode", 502);

        Assert.Equal("AI:ProviderRequestFailed (HTTP 502)", LlmCallFailure.DescribeUsageFailure(error));
    }

    [Fact]
    public void A_normal_sentence_is_left_alone()
    {
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("The model declined the request."));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus(null));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("  "));
    }
}
