using System.Collections.Generic;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.Features;

public class FeatureSaveDiffTests
{
    private static readonly Dictionary<string, string?> Saved = new()
    {
        ["SufiAI.Enable"] = "true",
        ["SufiAI.MaxWorkspaces"] = "3",
        ["SufiAI.Model"] = null,
    };

    [Fact]
    public void Unchanged_features_are_not_sent()
    {
        FeatureSaveDiff.Changed(
                new Dictionary<string, string?>
                {
                    ["SufiAI.Enable"] = "true",
                    ["SufiAI.MaxWorkspaces"] = "3",
                    ["SufiAI.Model"] = null,
                },
                Saved)
            .ShouldBeEmpty();
    }

    [Fact]
    public void Only_changed_and_new_features_are_sent()
    {
        var changed = FeatureSaveDiff.Changed(
            new Dictionary<string, string?>
            {
                ["SufiAI.Enable"] = "false",
                ["SufiAI.MaxWorkspaces"] = "3",
                ["SufiAI.Model"] = "gpt",
                ["SufiAI.UsageAnalytics"] = "true",
            },
            Saved);

        changed.ConvertAll(item => item.Name + "=" + item.Value)
            .ShouldBe(new[] { "SufiAI.Enable=false", "SufiAI.Model=gpt", "SufiAI.UsageAnalytics=true" });
    }

    [Fact]
    public void Clearing_a_value_is_a_change()
    {
        var changed = FeatureSaveDiff.Changed(
            new[] { new KeyValuePair<string, string?>("SufiAI.MaxWorkspaces", null) },
            Saved);

        changed.Count.ShouldBe(1);
        changed[0].Value.ShouldBeNull();
    }
}
