using System;
using System.IO;
using Shouldly;
using Xunit;

namespace SufiChain.SufiPlatform.SufiAI;

// G-160 (#340): the model configuration editor's connection block says which workspace endpoint and key a
// route uses, never tells the admin to leave the key empty, blocks an empty custom connection, and labels
// the test button by mode with inline failures.
public class ModelConfigurationConnectionBlockSourceTests
{
    private const string Editor = "modules/ai/src/SufiChain.SufiPlatform.SufiAI.Blazor/Pages/AI/ModelConfigurationEditor";

    [Fact]
    public void Workspace_Mode_Shows_Endpoint_And_Key_State()
    {
        var markup = Read(Editor + ".razor");
        var code = Read(Editor + ".razor.cs");
        markup.ShouldContain("_workspace?.ApiBaseUrl");
        markup.ShouldContain("L[\"WorkspaceEndpointLine\"");
        markup.ShouldContain("L[\"WorkspaceEndpointDefault\"]");
        markup.ShouldContain("_workspace?.HasApiKey == true");
        markup.ShouldContain("L[\"WorkspaceKeyStored\"]");
        markup.ShouldContain("L[\"WorkspaceKeyMissing\"]");
        markup.ShouldContain("L[\"OpenWorkspaceSettings\"]");
        code.ShouldContain("/panel/admin/ai/workspaces/{WorkspaceId}/edit");
    }

    [Fact]
    public void Custom_Mode_Key_Copy_And_Empty_Connection_Guard()
    {
        var markup = Read(Editor + ".razor");
        var code = Read(Editor + ".razor.cs");
        code.ShouldNotContain("LeaveEmptyToKeepCurrent");
        code.ShouldContain("L[\"RouteKeyStoredHelp\"]");
        code.ShouldContain("_customConnectionError = !CustomConnectionHasEndpointOrKey;");
        code.ShouldContain("|| !string.IsNullOrWhiteSpace(_model.ApiKey)");
        markup.ShouldContain("L[\"CustomConnectionEmpty\"]");
    }

    [Fact]
    public void Test_Button_Is_Labelled_By_Mode_With_Inline_Failure()
    {
        var markup = Read(Editor + ".razor");
        var code = Read(Editor + ".razor.cs");
        markup.ShouldContain("_useWorkspaceConnection ? L[\"TestWorkspaceConnection\"] : L[\"TestThisConnection\"]");
        markup.ShouldNotContain("@L[\"TestConnection\"]");
        markup.ShouldContain("@_testError");
        code.ShouldContain("_testError = ConnectionTestErrorText(ex);");
        code.ShouldContain("Notify.SuccessAsync(L[\"ConnectionTestSuccessful\"])");
    }

    [Theory]
    [InlineData("fa")]
    [InlineData("en")]
    [InlineData("ar")]
    [InlineData("es")]
    public void Api_Key_Copy_No_Longer_Says_Leave_Empty(string culture)
    {
        var json = Read($"modules/ai/src/SufiChain.SufiPlatform.SufiAI.Domain.Shared/Localization/AI/{culture}.json");
        json.ShouldContain("\"RouteKeyStoredHelp\"");
        json.ShouldContain("\"TestThisConnection\"");
        json.ShouldNotContain("برای استفاده از کلید API فضای کاری خالی بگذارید");
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
