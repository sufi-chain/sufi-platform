using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace SufiChain.Chat.Realtime;

/// <summary>
/// Establishes the ABP ambient <see cref="ICurrentPrincipalAccessor"/> for chat hub invocations.
/// <para>
/// A plain SignalR <see cref="Hub"/> does not flow <c>Context.User</c> into ABP's current-principal
/// accessor, so <c>ICurrentUser</c>/<c>IPermissionChecker</c> would otherwise be anonymous inside hub
/// methods (causing every authorization check to fail). When the connection itself is anonymous but
/// carries a valid hub ticket (e.g. Blazor Server loopback connections that cannot send the auth
/// cookie), the ticket principal is used instead.
/// </para>
/// </summary>
public class ChatHubCurrentPrincipalFilter : IHubFilter
{
    public virtual async ValueTask<object?> InvokeMethodAsync(
        HubInvocationContext invocationContext,
        Func<HubInvocationContext, ValueTask<object?>> next)
    {
        var principal = ResolvePrincipal(invocationContext.Context, invocationContext.ServiceProvider);
        var accessor = invocationContext.ServiceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
        using (accessor.Change(principal))
        {
            return await next(invocationContext);
        }
    }

    public virtual async Task OnConnectedAsync(
        HubLifetimeContext context,
        Func<HubLifetimeContext, Task> next)
    {
        var principal = ResolvePrincipal(context.Context, context.ServiceProvider);
        var accessor = context.ServiceProvider.GetRequiredService<ICurrentPrincipalAccessor>();
        using (accessor.Change(principal))
        {
            await next(context);
        }
    }

    protected virtual ClaimsPrincipal ResolvePrincipal(HubCallerContext context, IServiceProvider serviceProvider)
    {
        var principal = context.User;
        if (principal?.Identity?.IsAuthenticated == true)
        {
            return principal;
        }

        var ticket = ReadTicket(context);
        if (!string.IsNullOrEmpty(ticket))
        {
            var ticketPrincipal = serviceProvider
                .GetRequiredService<IChatHubTicketProtector>()
                .Unprotect(ticket);

            if (ticketPrincipal?.Identity?.IsAuthenticated == true)
            {
                return ticketPrincipal;
            }
        }

        return principal ?? new ClaimsPrincipal(new ClaimsIdentity());
    }

    protected virtual string? ReadTicket(HubCallerContext context)
    {
        var httpContext = context.GetHttpContext();
        if (httpContext == null)
        {
            return null;
        }

        if (httpContext.Request.Query.TryGetValue("access_token", out var queryToken) &&
            !string.IsNullOrEmpty(queryToken))
        {
            return queryToken.ToString();
        }

        var authorization = httpContext.Request.Headers.Authorization.ToString();
        if (authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
        {
            return authorization["Bearer ".Length..].Trim();
        }

        return null;
    }
}
