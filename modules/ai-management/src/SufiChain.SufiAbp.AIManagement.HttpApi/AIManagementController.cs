using SufiChain.SufiAbp.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiAbp.AIManagement;

public static class AIManagementRemoteServiceConsts
{
    public const string ModuleName = "ai-management";
    public const string RemoteServiceName = "AIManagement";
}

public abstract class AIManagementController : SufiAbpControllerBase
{
}
