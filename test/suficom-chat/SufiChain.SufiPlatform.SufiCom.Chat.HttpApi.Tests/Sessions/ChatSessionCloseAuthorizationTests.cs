using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shouldly;
using SufiChain.SufiPlatform.SufiCom.Chat;
using SufiChain.SufiPlatform.SufiCom.Chat.Permissions;
using SufiChain.SufiPlatform.SufiCom.Chat.Sessions;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Modularity;
using Xunit;

namespace SufiChain.SufiPlatform.SufiCom.Chat.HttpApi;

[DependsOn(typeof(SufiComChatHttpApiTestModule))]
public class ChatCloseDeniedTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.Replace(ServiceDescriptor.Singleton<IMethodInvocationAuthorizationService, DenyCloseAuthorization>());
    }

    private sealed class DenyCloseAuthorization : IMethodInvocationAuthorizationService
    {
        public Task CheckAsync(MethodInvocationAuthorizationContext context)
        {
            var authorize = context.Method.GetCustomAttribute<AuthorizeAttribute>();
            if (authorize?.Policy == ChatPermissions.Sessions.Close)
            {
                throw new AbpAuthorizationException("Chat session close is denied.");
            }

            return Task.CompletedTask;
        }
    }
}

public class ChatSessionCloseAuthorizationTests : ChatApplicationTestBase<ChatCloseDeniedTestModule>
{
    [Fact]
    public async Task Close_Should_Be_Denied_Without_The_Close_Permission()
    {
        var sessions = GetRequiredService<IChatSessionAppService>();
        await Should.ThrowAsync<AbpAuthorizationException>(() => sessions.CloseAsync(Guid.NewGuid()));
    }
}
