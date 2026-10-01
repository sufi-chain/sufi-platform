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

using SufiChain.SufiPlatform.Identity.Integration;

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

        context.Services.Replace(ServiceDescriptor.Transient<IPlatformHooshvareResolver, TestPlatformHooshvareResolver>());

        context.Services.AddSingleton<TestHooshvareRuntimeAppService>();
        context.Services.Replace(ServiceDescriptor.Singleton<IHooshvareRuntimeAppService>(sp =>
            sp.GetRequiredService<TestHooshvareRuntimeAppService>()));

        context.Services.AddSingleton<TestSufiAIWorkspaceCatalog>();
        context.Services.Replace(ServiceDescriptor.Singleton<ISufiAIWorkspaceCatalog>(sp =>
            sp.GetRequiredService<TestSufiAIWorkspaceCatalog>()));



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
        // The chat host loads the Identity domain without its persistence; IdentityUserManager
        // needs these repositories to activate. Chat display names come from TestIdentityUserIntegrationService.
        context.Services.Replace(ServiceDescriptor.Singleton(_ => Substitute.For<IIdentityUserRepository>()));
        context.Services.Replace(ServiceDescriptor.Singleton(_ => Substitute.For<IOrganizationUnitRepository>()));
        context.Services.Replace(ServiceDescriptor.Singleton(_ => Substitute.For<IIdentityLinkUserRepository>()));

        context.Services.AddSingleton<TestIdentityUserIntegrationService>();
        context.Services.Replace(ServiceDescriptor.Singleton<IIdentityUserIntegrationService>(sp =>
            sp.GetRequiredService<TestIdentityUserIntegrationService>()));



        // Telegram user-channel connector activation: the Chat test host does not include the

        // SufiCom EF/MongoDB stack, so provide a no-op ITelegramConnectionRepository so the

        // connector registers cleanly. Outbound dispatch is covered by dedicated unit tests.

        context.Services.AddSingleton(_ => Substitute.For<ITelegramConnectionRepository>());
        context.Services.AddSingleton<ITelegramRateLimiter>(_ =>
        {
            var rateLimiter = Substitute.For<ITelegramRateLimiter>();
            rateLimiter
                .CheckAsync(Arg.Any<TelegramConnection>(), Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(SufiChain.SufiPlatform.SufiCom.Configuration.TelegramRateLimitDecision.Allow());
            return rateLimiter;
        });

        context.Services.AddSingleton<ITelegramForeignGateway>(_ =>

            new FakeTelegramForeignGateway(NullLogger<FakeTelegramForeignGateway>.Instance));



        context.Services.AddSingleton<ConfigurableAiService>();

        context.Services.Replace(ServiceDescriptor.Singleton<ISufiAIChatService>(sp =>

            sp.GetRequiredService<ConfigurableAiService>()));

    }

}

