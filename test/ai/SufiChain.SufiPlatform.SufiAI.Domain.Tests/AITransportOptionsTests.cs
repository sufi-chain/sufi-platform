using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

public class AITransportOptionsTests
{
    [Fact]
    public void Default_is_no_time_limit_for_model_requests()
    {
        // 9.4 (sufi-chain#340): the default must not cut off a slow model. Only a caller token ends a turn.
        Assert.Equal(Timeout.InfiniteTimeSpan, new AITransportOptions().GetRequestTimeout());
    }

    [Fact]
    public void Host_override_still_sets_an_explicit_deadline()
    {
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["SufiAI:Transport:RequestTimeoutSeconds"] = "1200"
        }).Build();
        var services = new ServiceCollection();
        services.Configure<AITransportOptions>(configuration.GetSection(AITransportOptions.SectionName));
        using var provider = services.BuildServiceProvider();
        Assert.Equal(TimeSpan.FromMinutes(20),
            provider.GetRequiredService<IOptions<AITransportOptions>>().Value.GetRequestTimeout());
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(3601)]
    public void Invalid_timeout_does_not_silently_disable_the_request_deadline(int seconds)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new AITransportOptions { RequestTimeoutSeconds = seconds }.GetRequestTimeout());
    }
}
