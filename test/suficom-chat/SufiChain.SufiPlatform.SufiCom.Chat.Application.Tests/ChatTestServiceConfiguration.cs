using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.DependencyInjection.Extensions;

using Microsoft.Extensions.Logging.Abstractions;

using NSubstitute;

using SufiChain.SufiPlatform.SufiCom.Application.Connections;

using SufiChain.SufiPlatform.SufiCom.Connections;

using SufiChain.SufiPlatform.SufiCom.Chat.AiUsage;

using SufiChain.SufiPlatform.SufiCom.Chat.Contacts;

using SufiChain.SufiPlatform.SufiCom.Chat.Supports;

using SufiChain.SufiPlatform.SufiCom.Chat.Usage;

using SufiChain.SufiPlatform.SufiAI;

using SufiChain.SufiPlatform.SufiAI.Hooshvare.Hooshvare;

using SufiChain.SufiPlatform.Features;

using SufiChain.SufiPlatform.FileManager;

using SufiChain.SufiPlatform.Identity;

using Volo.Abp.DependencyInjection;

using Volo.Abp.Modularity;



namespace SufiChain.SufiPlatform.SufiCom.Chat;



public static class ChatTestServiceConfiguration

{

    public static void ConfigureTestServices(ServiceConfigurationContext context)

    {

        context.Services.AddSingleton<ConfigurableFeatureChecker>();

        context.Services.AddSingleton<IFeatureChecker>(sp => sp.GetRequiredService<ConfigurableFeatureChecker>());



        context.Services.AddSingleton<ConfigurableChatAiWorkspaceProvider>();

        context.Services.AddSingleton<IChatAiWorkspaceProvider>(sp =>

            sp.GetRequiredService<ConfigurableChatAiWorkspaceProvider>());



        context.Services.Replace(ServiceDescriptor.Transient<IChatAssistantWorkspaceResolver, DefaultChatAssistantWorkspaceResolver>());



        context.Services.AddSingleton<TestHooshvareCatalogAppService>();

        context.Services.Replace(ServiceDescriptor.Singleton<IHooshvareCatalogAppService>(sp =>

            sp.GetRequiredService<TestHooshvareCatalogAppService>()));



        context.Services.Replace(ServiceDescriptor.Singleton<ISufiAIAudioService>(_ =>

            Substitute.For<ISufiAIAudioService>()));



        context.Services.AddSingleton<TestChatUsageWalletResolver>();

        context.Services.Replace(ServiceDescriptor.Singleton<IChatUsageWalletResolver>(sp =>

            sp.GetRequiredService<TestChatUsageWalletResolver>()));



        context.Services.AddSingleton<TestChatContactProvider>();

        context.Services.AddSingleton<IChatContactProvider>(sp => sp.GetRequiredService<TestChatContactProvider>());



        context.Services.AddSingleton(_ => Substitute.For<IFileStorageIntegrationService>());

        var roleRepository = Substitute.For<IIdentityRoleRepository>();

        roleRepository.GetListAsync().Returns(new List<IdentityRole>());

        roleRepository

            .GetListAsync(Arg.Any<IEnumerable<Guid>>(), Arg.Any<CancellationToken>())

            .Returns(new List<IdentityRole>());

        context.Services.Replace(ServiceDescriptor.Singleton<IIdentityRoleRepository>(roleRepository));



        // Telegram user-channel connector activation: the Chat test host does not include the

        // SufiCom EF/MongoDB stack, so provide a no-op ITelegramConnectionRepository so the

        // connector registers cleanly. Outbound dispatch is covered by dedicated unit tests.

        context.Services.AddSingleton(_ => Substitute.For<ITelegramConnectionRepository>());

        context.Services.AddSingleton<ITelegramForeignGateway>(_ =>

            new FakeTelegramForeignGateway(NullLogger<FakeTelegramForeignGateway>.Instance));



        context.Services.AddSingleton<ConfigurableAiService>();

        context.Services.Replace(ServiceDescriptor.Singleton<ISufiAIChatService>(sp =>

            sp.GetRequiredService<ConfigurableAiService>()));

    }

}

