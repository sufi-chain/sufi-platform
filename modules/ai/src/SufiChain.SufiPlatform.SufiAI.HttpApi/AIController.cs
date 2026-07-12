using SufiChain.SufiPlatform.AspNetCore.Mvc.Controllers;

namespace SufiChain.SufiPlatform.SufiAI;

public static class AIRemoteServiceConsts
{
    public const string ModuleName = "SufiAI";
    public const string RemoteServiceName = "SufiAI";
}

public abstract class AIController : SufiControllerBase
{
}
