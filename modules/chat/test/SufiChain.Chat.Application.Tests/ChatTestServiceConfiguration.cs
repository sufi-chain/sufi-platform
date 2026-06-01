using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using SufiChain.Chat.AiUsage;
using SufiChain.Chat.Contacts;
using SufiChain.Chat.Supports;
using SufiChain.Chat.Usage;
using SufiChain.SufiAbp.Features;
using SufiChain.SufiAbp.FileManager.FileItems;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Modularity;

namespace SufiChain.Chat;

public static class ChatTestServiceConfiguration
{
    public static void ConfigureTestServices(ServiceConfigurationContext context)
    {
        context.Services.AddSingleton<ConfigurableFeatureChecker>();
        context.Services.AddSingleton<IFeatureChecker>(sp => sp.GetRequiredService<ConfigurableFeatureChecker>());

        context.Services.AddSingleton<ConfigurableChatAiWorkspaceProvider>();
        context.Services.AddSingleton<IChatAiWorkspaceProvider>(sp =>
            sp.GetRequiredService<ConfigurableChatAiWorkspaceProvider>());

        context.Services.AddSingleton<TestChatUsageWalletResolver>();
        context.Services.Replace(ServiceDescriptor.Singleton<IChatUsageWalletResolver>(sp =>
            sp.GetRequiredService<TestChatUsageWalletResolver>()));

        context.Services.AddSingleton<TestChatContactProvider>();
        context.Services.AddSingleton<IChatContactProvider>(sp => sp.GetRequiredService<TestChatContactProvider>());

        context.Services.AddSingleton(_ => Substitute.For<IFileItemAppService>());
    }
}
