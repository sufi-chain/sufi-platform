using System;
using System.IO;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

// G-154 (#340): the model configurations list shows endpoints LTR on one line, readable prices and token
// counts, keeps the picker-relevant columns visible, and pins the actions column.
public class ModelConfigurationsListSourceTests
{
    private const string Page = "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/ModelConfigurationsPage";

    [Fact]
    public void Endpoint_Cell_Is_Ltr_Isolated_One_Line_With_Tooltip_And_Chip()
    {
        var markup = Read(Page + ".razor");
        markup.ShouldContain("dir=\"ltr\"");
        markup.ShouldContain("unicode-bidi: isolate");
        markup.ShouldContain("text-overflow: ellipsis");
        markup.ShouldContain("title=\"@cellContext.Item.ApiEndpoint\"");
        markup.ShouldContain("L[\"RouteOwnConnection\"]");
    }

    [Fact]
    public void Prices_And_Max_Tokens_Are_Off_By_Default_And_Actions_Pinned()
    {
        var markup = Read(Page + ".razor");
        markup.ShouldContain("Title=\"@L[\"RouteInputPrice\"]\" MinWidth=\"160px\" Visible=\"false\"");
        markup.ShouldContain("Title=\"@L[\"RouteOutputPrice\"]\" MinWidth=\"160px\" Visible=\"false\"");
        markup.ShouldContain("Title=\"@L[\"MaxContextTokens\"]\" MinWidth=\"120px\" Visible=\"false\"");
        markup.ShouldContain("Freeze=\"SbColumnFreeze.Start\"");
        markup.ShouldContain("ShowColumnChooser=\"true\"");
        markup.ShouldContain("@FormatTokenCount(cellContext.Item.MaxContextTokens)");
    }

    [Fact]
    public void Fa_Copy_Uses_Full_Million_And_Workspace_Wording()
    {
        var fa = Read("modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI/fa.json");
        fa.ShouldContain("\"PriceSuffixMillionTokens\": \"/ ۱ میلیون توکن\"");
        fa.ShouldContain("\"InheritWorkspace\": \"از فضای کاری\"");
        fa.ShouldContain("\"RouteOwnConnection\": \"اتصال جداگانه\"");
        Read(Page + ".razor.cs").ShouldContain("',' => '\\u066C'");
    }

    private static string Read(string relative)
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null)
        {
            var candidate = Path.Combine(dir.FullName, relative);
            if (File.Exists(candidate))
            {
                return File.ReadAllText(candidate);
            }

            dir = dir.Parent;
        }

        throw new FileNotFoundException(relative);
    }
}
