namespace SufiChain.SufiPlatform.SufiAI.MCP.Entities;

/// <summary>
/// sufi-chain#384 slice 6b: who an external MCP server's credentials belong to.
/// <para>
/// <see cref="TenantLevel"/> is the historical behaviour (and the default for existing rows): one client
/// per server per tenant, with whatever credential the server itself carries.
/// </para>
/// <para>
/// <see cref="UserScoped"/> means the server acts for the signed-in user: every call carries a
/// short-lived token minted for that user, and no client is shared between users. STDIO cannot carry a
/// token, so STDIO + UserScoped is rejected.
/// </para>
/// </summary>
public enum MCPBindingMode
{
    /// <summary>One binding per tenant. Existing rows keep this.</summary>
    TenantLevel = 0,

    /// <summary>One binding per signed-in user, carried by a short-lived token on every call.</summary>
    UserScoped = 1
}
