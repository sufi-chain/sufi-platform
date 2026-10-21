using System;
using System.IO;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI.Application.Tests.Workspaces;

// #499: the tenant UI must not offer Clone or Convert-to-custom for an inherited workspace.
// The server rejects both with AI:InheritedWorkspaceCopyBlocked (InheritedWorkspaceCopyBlockTests);
// these tests prove the tenant screens never expose the two actions for an inherited row.
//
// This follows the established source-markup pattern for AI Blazor screens (see
// ModelConfigurationsListSourceTests, ModelConfigurationConnectionBlockSourceTests): no AI test
// project references SufiAI.Blazor, so the markup and code-behind are asserted directly.
public class InheritedWorkspaceUiGuardSourceTests
{
    private const string Blazor = "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor";
    private const string List = Blazor + "/Pages/AI/Workspaces";
    private const string Editor = Blazor + "/Pages/AI/WorkspaceEditor";
    private const string Localization = "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI";

    [Fact]
    public void Clone_Action_Is_Inside_The_Inherited_Guard()
    {
        var markup = Normalized(Read(List + ".razor"));

        // Exactly two row actions are hidden for inherited workspaces: Clone and Delete.
        CountOccurrences(markup, "@if (!cellContext.Item.IsInherited) {").ShouldBe(2);

        // The Clone button sits inside the guard, so an inherited row renders no clone icon.
        markup.ShouldContain(
            "@if (!cellContext.Item.IsInherited) { <AuthorizeView Policy=\"@AIPermissions.Workspaces.Create\"> <SbIconButton Size=\"SbSize.Sm\" AriaLabel=\"@L[\"Clone\"]\"");
    }

    [Fact]
    public void Convert_To_Custom_Is_Gone_From_The_List_And_The_Editor()
    {
        // The Convert-to-custom action was removed from both screens, not merely disabled.
        Read(List + ".razor").ShouldNotContain("ConvertToCustom");
        Read(Editor + ".razor").ShouldNotContain("ConvertToCustom");
        Read(List + ".razor.cs").ShouldNotContain("ConvertToCustomAsync");
        Read(Editor + ".razor.cs").ShouldNotContain("ConvertToCustomAsync");

        // Its loading key and the list handler are gone with it.
        Read(List + ".razor.cs").ShouldNotContain("ConvertWorkspace");
        Read(Editor + ".razor.cs").ShouldNotContain("ConvertWorkspace");
    }

    [Fact]
    public void Editor_Keeps_The_Read_Only_Notice_And_Blocks_Save()
    {
        var markup = Read(Editor + ".razor");

        // An inherited workspace is read-only and says so.
        markup.ShouldContain("@if (IsReadOnly)");
        markup.ShouldContain("L[\"InheritedWorkspaceReadOnly\"]");

        // Save stays disabled and the connection test stays hidden while read-only.
        markup.ShouldContain("Disabled=\"@IsReadOnly\"");
        markup.ShouldContain("@if (!IsReadOnly)");

        // Read-only is derived from inheritance, not from a permission.
        Read(Editor + ".razor.cs").ShouldContain("_workspace?.IsInherited == true");
    }

    [Theory]
    [InlineData("fa")]
    [InlineData("en")]
    [InlineData("ar")]
    [InlineData("es")]
    public void Copy_Blocked_Error_Is_Localized(string culture)
    {
        var json = Read($"{Localization}/{culture}.json");
        json.ShouldContain("\"AI:InheritedWorkspaceCopyBlocked\"");
    }

    [Fact]
    public void Fa_Copy_Keeps_The_Zwnj_Joins()
    {
        var fa = Read($"{Localization}/fa.json");

        // «ارث‌برده» and «نمی‌توان» carry ZWNJ (U+200C); a plain join would be a regression.
        fa.ShouldContain("ارث\u200Cبرده");
        fa.ShouldContain("نمی\u200Cتوان");
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var index = source.IndexOf(value, StringComparison.Ordinal);
        while (index >= 0)
        {
            count++;
            index = source.IndexOf(value, index + value.Length, StringComparison.Ordinal);
        }

        return count;
    }

    // Collapse runs of whitespace so an indentation change does not break the structural assertion.
    private static string Normalized(string source) =>
        Regex.Replace(source, "\\s+", " ");

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
