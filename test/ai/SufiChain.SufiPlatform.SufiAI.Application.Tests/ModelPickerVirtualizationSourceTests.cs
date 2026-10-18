using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

// G-153 (#340): model catalog pickers load the cached list once and filter on the client,
// virtualized and debounced, with no server round-trip while the menu is open.
public class ModelPickerVirtualizationSourceTests
{
    [Theory]
    [InlineData("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/ModelConfigurationEditor.razor", "SelectableModels")]
    [InlineData("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Components/WorkspaceConnectionFields.razor", "ChatModels")]
    [InlineData("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Components/WorkspaceConnectionFields.razor", "DecisionChoices")]
    public void Model_Picker_Is_Virtualized_With_Debounced_Client_Filter(string relativePath, string items)
    {
        var source = File.ReadAllText(FindRepoFile(relativePath));
        var match = Regex.Match(source, "<SbSelect TItem=\"OpenAIModelDto\"(?<attrs>[^>]*?)Items=\"" + items + "\"(?<rest>[^>]*)>", RegexOptions.Singleline);

        match.Success.ShouldBeTrue();
        var attributes = match.Groups["attrs"].Value + match.Groups["rest"].Value;
        attributes.ShouldContain("Virtualize=\"true\"");
        attributes.ShouldContain("ClientFilterDebounceMs=");
        attributes.ShouldNotContain("SearchFunc=");
    }

    private static string FindRepoFile(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relative);
    }
}
