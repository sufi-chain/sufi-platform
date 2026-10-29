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

    [Fact]
    public void A_normal_sentence_is_left_alone()
    {
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("The model declined the request."));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus(null));
        Assert.False(AiUsageErrorText.IsUnfilledProviderStatus("  "));
    }
}
