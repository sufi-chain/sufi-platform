namespace SufiChain.SufiPlatform.SufiAI.Blazor.Pages.AI;

public static class WorkspaceChatEmptyState
{
    public static bool ShowLoading(bool loading, int activeCount, bool hasWorkspace) =>
        !hasWorkspace && (loading || activeCount == 1);

    public static bool ShowChooseWorkspace(bool loading, int activeCount, bool hasWorkspace) =>
        !hasWorkspace && !loading && activeCount != 1;
}
