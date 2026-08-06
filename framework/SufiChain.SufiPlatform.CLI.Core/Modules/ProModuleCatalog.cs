using SufiChain.SufiPlatform.CLI.Args;

namespace SufiChain.SufiPlatform.CLI.Modules;

internal static class ProModuleCatalog
{
    public static IEnumerable<ModuleDefinition> CreateAll()
    {
        yield return Feature(
            "ai-copilots",
            "AI Copilots",
            "AI copilot definitions, runtime configuration, and management UI",
            ["ai", "file-manager"],
            B(ModuleIntegrationPoint.DomainShared, "SufiChain.SufiPlatform.SufiAI.Copilots.Domain.Shared", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsDomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, "SufiChain.SufiPlatform.SufiAI.Copilots.Domain", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsDomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, "SufiChain.SufiPlatform.SufiAI.Copilots.Application.Contracts", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.SufiAI.Copilots.Application", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsApplicationModule"),
            B(ModuleIntegrationPoint.EntityFrameworkCore, "SufiChain.SufiPlatform.SufiAI.Copilots.EntityFrameworkCore", "SufiChain.SufiPlatform.SufiAI.Copilots.EntityFrameworkCore.SufiAICopilotsEntityFrameworkCoreModule", DatabaseProvider.EntityFrameworkCore),
            B(ModuleIntegrationPoint.MongoDB, "SufiChain.SufiPlatform.SufiAI.Copilots.MongoDB", "SufiChain.SufiPlatform.SufiAI.Copilots.MongoDB.SufiAICopilotsMongoDBModule", DatabaseProvider.MongoDB),
            B(ModuleIntegrationPoint.HttpApi, "SufiChain.SufiPlatform.SufiAI.Copilots.HttpApi", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsHttpApiModule"),
            B(ModuleIntegrationPoint.HttpApiClient, "SufiChain.SufiPlatform.SufiAI.Copilots.HttpApi.Client", "SufiChain.SufiPlatform.SufiAI.Copilots.SufiAICopilotsHttpApiClientModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.SufiAI.Copilots.Blazor.Server", "SufiChain.SufiPlatform.SufiAI.Copilots.Blazor.SufiAICopilotsBlazorServerModule"));

        yield return Feature(
            "branding",
            "Branding",
            "Tenant-aware application branding",
            [],
            B(ModuleIntegrationPoint.DomainShared, "SufiChain.SufiPlatform.Branding.Domain.Shared", "SufiChain.SufiPlatform.Branding.SufiBrandingDomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, "SufiChain.SufiPlatform.Branding.Domain", "SufiChain.SufiPlatform.Branding.SufiBrandingDomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, "SufiChain.SufiPlatform.Branding.Application.Contracts", "SufiChain.SufiPlatform.Branding.SufiBrandingApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.Branding.Application", "SufiChain.SufiPlatform.Branding.SufiBrandingApplicationModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.Branding.Blazor", "SufiChain.SufiPlatform.Branding.Blazor.SufiBrandingBlazorModule"),
            B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.Branding.Application", "SufiChain.SufiPlatform.Branding.SufiBrandingApplicationModule"));

        yield return Feature(
            "calendar-copilot",
            "Calendar Copilot",
            "AI assistance for calendar workflows",
            ["calendar", "ai-copilots"],
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.Calendar.Copilot.Application", "SufiChain.SufiPlatform.Calendar.Copilot.CalendarCopilotApplicationModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.Calendar.Copilot.Blazor", "SufiChain.SufiPlatform.Calendar.Copilot.Blazor.CalendarCopilotBlazorModule"));

        yield return Feature(
            "cms",
            "SufiCMS",
            "Content management and public content delivery",
            ["file-manager", "tags", "menus"],
            B(ModuleIntegrationPoint.DomainShared, "SufiChain.SufiPlatform.SufiCMS.Domain.Shared", "SufiChain.SufiPlatform.SufiCMS.SufiCMSDomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, "SufiChain.SufiPlatform.SufiCMS.Domain", "SufiChain.SufiPlatform.SufiCMS.SufiCMSDomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, "SufiChain.SufiPlatform.SufiCMS.Application.Contracts", "SufiChain.SufiPlatform.SufiCMS.SufiCMSApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.SufiCMS.Application", "SufiChain.SufiPlatform.SufiCMS.SufiCMSApplicationModule"),
            B(ModuleIntegrationPoint.EntityFrameworkCore, "SufiChain.SufiPlatform.SufiCMS.EntityFrameworkCore", "SufiChain.SufiPlatform.SufiCMS.EntityFrameworkCore.SufiCMSEntityFrameworkCoreModule", DatabaseProvider.EntityFrameworkCore),
            B(ModuleIntegrationPoint.MongoDB, "SufiChain.SufiPlatform.SufiCMS.MongoDB", "SufiChain.SufiPlatform.SufiCMS.MongoDB.SufiCMSMongoDbModule", DatabaseProvider.MongoDB),
            B(ModuleIntegrationPoint.HttpApi, "SufiChain.SufiPlatform.SufiCMS.HttpApi", "SufiChain.SufiPlatform.SufiCMS.SufiCMSHttpApiModule"),
            B(ModuleIntegrationPoint.HttpApiClient, "SufiChain.SufiPlatform.SufiCMS.HttpApi.Client", "SufiChain.SufiPlatform.SufiCMS.SufiCMSHttpApiClientModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.SufiCMS.Blazor.Server", "SufiChain.SufiPlatform.SufiCMS.Blazor.Server.SufiCMSBlazorServerModule"),
            B(ModuleIntegrationPoint.BlazorWebSite, "SufiChain.SufiPlatform.SufiCMS.Public.Blazor", "SufiChain.SufiPlatform.SufiCMS.Public.Blazor.SufiCMSPublicBlazorModule"),
            B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.SufiCMS.Application", "SufiChain.SufiPlatform.SufiCMS.SufiCMSApplicationModule"));

        yield return Feature(
            "crm",
            "SufiCRM Contacts",
            "Customer relationship and contacts management",
            ["tags"],
            B(ModuleIntegrationPoint.DomainShared, "SufiChain.SufiPlatform.SufiCRM.Contacts.Domain.Shared", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsDomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, "SufiChain.SufiPlatform.SufiCRM.Contacts.Domain", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsDomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, "SufiChain.SufiPlatform.SufiCRM.Contacts.Application.Contracts", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.SufiCRM.Contacts.Application", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsApplicationModule"),
            B(ModuleIntegrationPoint.EntityFrameworkCore, "SufiChain.SufiPlatform.SufiCRM.Contacts.EntityFrameworkCore", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsEntityFrameworkCoreModule", DatabaseProvider.EntityFrameworkCore),
            B(ModuleIntegrationPoint.MongoDB, "SufiChain.SufiPlatform.SufiCRM.Contacts.MongoDB", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsMongoDbModule", DatabaseProvider.MongoDB),
            B(ModuleIntegrationPoint.HttpApi, "SufiChain.SufiPlatform.SufiCRM.Contacts.HttpApi", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsHttpApiModule"),
            B(ModuleIntegrationPoint.HttpApiClient, "SufiChain.SufiPlatform.SufiCRM.Contacts.HttpApi.Client", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsHttpApiClientModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.SufiCRM.Contacts.Blazor", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsBlazorModule"),
            B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.SufiCRM.Contacts.Application", "SufiChain.SufiPlatform.SufiCRM.Contacts.SufiCRMContactsApplicationModule"));

        yield return Feature(
            "dashboard",
            "Dashboard",
            "Application dashboard widgets and navigation",
            [],
            B(ModuleIntegrationPoint.DomainShared, "SufiChain.SufiPlatform.Dashboard.Domain.Shared", "SufiChain.SufiPlatform.Dashboard.SufiDashboardDomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, "SufiChain.SufiPlatform.Dashboard.Domain", "SufiChain.SufiPlatform.Dashboard.SufiDashboardDomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, "SufiChain.SufiPlatform.Dashboard.Application.Contracts", "SufiChain.SufiPlatform.Dashboard.SufiDashboardApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, "SufiChain.SufiPlatform.Dashboard.Application", "SufiChain.SufiPlatform.Dashboard.SufiDashboardApplicationModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.Dashboard.Blazor", "SufiChain.SufiPlatform.Dashboard.Blazor.SufiDashboardBlazorModule"));

        yield return FullStack(
            "forms",
            "SufiForms",
            "Form building, publishing, and submissions",
            "SufiChain.SufiPlatform.SufiForms",
            "SufiChain.SufiPlatform.SufiForms",
            "SufiForms",
            ["file-manager"],
            blazorPackage: "SufiChain.SufiPlatform.SufiForms.Blazor.Server",
            blazorType: "SufiChain.SufiPlatform.SufiForms.Blazor.Server.SufiFormsBlazorServerModule",
            entityFrameworkCoreNamespace: "SufiChain.SufiPlatform.SufiForms.EntityFrameworkCore",
            mongoNamespace: "SufiChain.SufiPlatform.SufiForms.MongoDB",
            mongoTypeSuffix: "MongoDb");

        yield return FullStack(
            "saas",
            "SufiSaaS",
            "SaaS editions and tenant subscription management",
            "SufiChain.SufiPlatform.SufiSaas",
            "SufiChain.SufiPlatform.SufiSaas",
            "SufiSaas",
            ["tenants", "features", "editions"],
            blazorPackage: "SufiChain.SufiPlatform.SufiSaas.Blazor",
            blazorType: "SufiChain.SufiPlatform.SufiSaas.Blazor.SufiSaasBlazorModule",
            entityFrameworkCoreNamespace: "SufiChain.SufiPlatform.SufiSaas.EntityFrameworkCore",
            mongoNamespace: "SufiChain.SufiPlatform.SufiSaas.MongoDB",
            mongoTypeSuffix: "MongoDb");

        yield return Feature(
            "helpdesk",
            "HelpDesk Suite",
            "HelpDesk core, knowledge base, ticketing, and live chat",
            ["file-manager", "ai-copilots", "suficom"],
            FullStackBindings("SufiChain.SufiPlatform.HelpDesk", "SufiChain.SufiPlatform.HelpDesk", "HelpDesk", mongoTypeSuffix: "MongoDb")
                .Where(binding => binding.IntegrationPoint is not ModuleIntegrationPoint.HttpApi and not ModuleIntegrationPoint.HttpApiClient)
                .ToArray(),
            FullStackBindings("SufiChain.SufiPlatform.HelpDesk.KnowledgeBase", "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase", "HelpDeskKnowledgeBase", mongoTypeSuffix: "MongoDb"),
            FullStackBindings("SufiChain.SufiPlatform.HelpDesk.Ticketing", "SufiChain.SufiPlatform.HelpDesk.Ticketing", "HelpDeskTicketing", mongoTypeSuffix: "MongoDb"),
            FullStackBindings(
                "SufiChain.SufiPlatform.HelpDesk.LiveChat",
                "SufiChain.SufiPlatform.HelpDesk.LiveChat",
                "HelpDeskLiveChat",
                mongoTypeSuffix: "MongoDb",
                blazorNamespace: "SufiChain.SufiPlatform.HelpDesk.LiveChat.Blazor"),
            [
                B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Blazor.Server", "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.HelpDeskKnowledgeBaseBlazorServerModule"),
                B(ModuleIntegrationPoint.BlazorWebSite, "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.Blazor.Public", "SufiChain.SufiPlatform.HelpDesk.KnowledgeBase.HelpDeskKnowledgeBaseBlazorPublicModule"),
                B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.HelpDesk.Application", "SufiChain.SufiPlatform.HelpDesk.HelpDeskApplicationModule")
            ]);

        yield return Feature(
            "suficom",
            "SufiCom Suite",
            "Communications, inbox, chat, and channel providers",
            ["users"],
            FullStackBindings(
                "SufiChain.SufiPlatform.SufiCom",
                "SufiChain.SufiPlatform.SufiCom",
                "SufiCom",
                entityFrameworkCoreNamespace: "SufiChain.SufiPlatform.SufiCom.EntityFrameworkCore",
                mongoNamespace: "SufiChain.SufiPlatform.SufiCom.MongoDB",
                mongoTypeSuffix: "MongoDb"),
            FullStackBindings(
                "SufiChain.SufiPlatform.SufiCom.Chat",
                "SufiChain.SufiPlatform.SufiCom.Chat",
                "SufiComChat",
                mongoTypeSuffix: "MongoDb",
                blazorNamespace: "SufiChain.SufiPlatform.SufiCom.Chat.Blazor"),
            [
                B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.SufiCom.Blazor.Server", "SufiChain.SufiPlatform.SufiCom.SufiComBlazorServerModule"),
                B(ModuleIntegrationPoint.BlazorWebApp, "SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Server", "SufiChain.SufiPlatform.SufiCom.Chat.Blazor.Server.SufiComChatBlazorServerModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.AspNetCore", "SufiChain.SufiPlatform.SufiCom.SufiComAspNetCoreModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels", "SufiChain.SufiPlatform.SufiCom.Channels.SufiComChannelsModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Email", "SufiChain.SufiPlatform.SufiCom.Channels.Email.SufiComChannelsEmailModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Telegram", "SufiChain.SufiPlatform.SufiCom.Channels.Telegram.SufiComChannelsTelegramModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar", "SufiChain.SufiPlatform.SufiCom.Channels.Sms.Kavenegar.SufiComChannelsSmsKavenegarModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan", "SufiChain.SufiPlatform.SufiCom.Channels.Sms.IdehPardazan.SufiComChannelsSmsIdehPardazanModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Sms.FanapMobile", "SufiChain.SufiPlatform.SufiCom.Channels.Sms.FanapMobile.SufiComChannelsSmsFanapMobileModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiCom.Channels.Voice.Kavenegar", "SufiChain.SufiPlatform.SufiCom.Channels.Voice.Kavenegar.SufiComChannelsVoiceKavenegarModule"),
                B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.SufiCom.Application", "SufiChain.SufiPlatform.SufiCom.SufiComApplicationModule")
            ]);

        yield return Feature(
            "finance",
            "SufiFinance Suite",
            "Finance, payments, wallets, invoicing, and exchange rates",
            ["users"],
            FullStackBindings("SufiChain.SufiPlatform.SufiFinance", "SufiChain.SufiPlatform.SufiFinance", "SufiFinance"),
            FullStackBindings("SufiChain.SufiPlatform.SufiFinance.Payments", "SufiChain.SufiPlatform.SufiFinance.Payments", "SufiFinancePayments"),
            FullStackBindings("SufiChain.SufiPlatform.SufiFinance.Wallets", "SufiChain.SufiPlatform.SufiFinance.Wallets", "SufiFinanceWallets"),
            FullStackBindings("SufiChain.SufiPlatform.SufiFinance.Invoicing", "SufiChain.SufiPlatform.SufiFinance.Invoicing", "SufiFinanceInvoicing"),
            FullStackBindings("SufiChain.SufiPlatform.SufiFinance.ExchangeRates", "SufiChain.SufiPlatform.SufiFinance.ExchangeRates", "SufiFinanceExchangeRates"),
            [
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Virtual", "SufiChain.SufiPlatform.SufiFinance.Payments.Virtual.SufiFinanceVirtualProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Stripe", "SufiChain.SufiPlatform.SufiFinance.Payments.Stripe.SufiFinanceStripeProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.PayPal", "SufiChain.SufiPlatform.SufiFinance.Payments.PayPal.SufiFinancePayPalProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.AsanPardakht", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.AsanPardakht.SufiFinanceShaparakAsanPardakhtProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.IdPay", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.IdPay.SufiFinanceShaparakIdPayProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.IranKish", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.IranKish.SufiFinanceShaparakIranKishProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Mellat", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Mellat.SufiFinanceShaparakMellatProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Melli", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Melli.SufiFinanceShaparakMelliProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Parsian", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Parsian.SufiFinanceShaparakParsianProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Pasargad", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Pasargad.SufiFinanceShaparakPasargadProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Saman", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Saman.SufiFinanceShaparakSamanProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Sepehr", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Sepehr.SufiFinanceShaparakSepehrProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.ZarinPal", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.ZarinPal.SufiFinanceShaparakZarinPalProviderModule"),
                B(ModuleIntegrationPoint.BackendHost, "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Zibal", "SufiChain.SufiPlatform.SufiFinance.Payments.Shaparak.Zibal.SufiFinanceShaparakZibalProviderModule"),
                B(ModuleIntegrationPoint.DbMigrator, "SufiChain.SufiPlatform.SufiFinance.Application", "SufiChain.SufiPlatform.SufiFinance.SufiFinanceApplicationModule")
            ]);
    }

    private static ModuleDefinition FullStack(
        string key,
        string displayName,
        string description,
        string packagePrefix,
        string moduleNamespace,
        string typePrefix,
        string[] dependencies,
        string blazorPackage,
        string blazorType,
        string? entityFrameworkCoreNamespace = null,
        string? mongoNamespace = null,
        string mongoTypeSuffix = "MongoDB")
    {
        var bindings = FullStackBindings(
                packagePrefix,
                moduleNamespace,
                typePrefix,
                entityFrameworkCoreNamespace,
                mongoNamespace,
                mongoTypeSuffix)
            .Where(binding => binding.IntegrationPoint != ModuleIntegrationPoint.BlazorWebApp)
            .Append(B(ModuleIntegrationPoint.BlazorWebApp, blazorPackage, blazorType))
            .ToArray();
        return Feature(key, displayName, description, dependencies, bindings);
    }

    private static ModuleDefinition Feature(
        string key,
        string displayName,
        string description,
        string[] dependencies,
        params ModuleBinding[][] bindingGroups)
    {
        return Feature(key, displayName, description, dependencies, bindingGroups.SelectMany(group => group).ToArray());
    }

    private static ModuleDefinition Feature(
        string key,
        string displayName,
        string description,
        string[] dependencies,
        params ModuleBinding[] bindings)
    {
        return new ModuleDefinition
        {
            Key = key,
            DisplayName = displayName,
            NuGetPackagePrefix = bindings[0].PackageId,
            Category = ModuleCategory.Feature,
            IsDefault = true,
            Description = description,
            DependsOn = dependencies,
            ApplicableHosts = [],
            Bindings = bindings
        };
    }

    private static ModuleBinding[] FullStackBindings(
        string packagePrefix,
        string moduleNamespace,
        string typePrefix,
        string? entityFrameworkCoreNamespace = null,
        string? mongoNamespace = null,
        string mongoTypeSuffix = "MongoDB",
        string? blazorNamespace = null)
    {
        entityFrameworkCoreNamespace ??= moduleNamespace;
        mongoNamespace ??= moduleNamespace;
        blazorNamespace ??= moduleNamespace;

        return
        [
            B(ModuleIntegrationPoint.DomainShared, $"{packagePrefix}.Domain.Shared", $"{moduleNamespace}.{typePrefix}DomainSharedModule"),
            B(ModuleIntegrationPoint.Domain, $"{packagePrefix}.Domain", $"{moduleNamespace}.{typePrefix}DomainModule"),
            B(ModuleIntegrationPoint.ApplicationContracts, $"{packagePrefix}.Application.Contracts", $"{moduleNamespace}.{typePrefix}ApplicationContractsModule"),
            B(ModuleIntegrationPoint.Application, $"{packagePrefix}.Application", $"{moduleNamespace}.{typePrefix}ApplicationModule"),
            B(ModuleIntegrationPoint.EntityFrameworkCore, $"{packagePrefix}.EntityFrameworkCore", $"{entityFrameworkCoreNamespace}.{typePrefix}EntityFrameworkCoreModule", DatabaseProvider.EntityFrameworkCore),
            B(ModuleIntegrationPoint.MongoDB, $"{packagePrefix}.MongoDB", $"{mongoNamespace}.{typePrefix}{mongoTypeSuffix}Module", DatabaseProvider.MongoDB),
            B(ModuleIntegrationPoint.HttpApi, $"{packagePrefix}.HttpApi", $"{moduleNamespace}.{typePrefix}HttpApiModule"),
            B(ModuleIntegrationPoint.HttpApiClient, $"{packagePrefix}.HttpApi.Client", $"{moduleNamespace}.{typePrefix}HttpApiClientModule"),
            B(ModuleIntegrationPoint.BlazorWebApp, $"{packagePrefix}.Blazor", $"{blazorNamespace}.{typePrefix}BlazorModule")
        ];
    }

    private static ModuleBinding B(
        ModuleIntegrationPoint integrationPoint,
        string packageId,
        string moduleType,
        DatabaseProvider? databaseProvider = null)
    {
        return new ModuleBinding
        {
            IntegrationPoint = integrationPoint,
            PackageId = packageId,
            ModuleType = moduleType,
            DatabaseProvider = databaseProvider,
            VersionProperty = "SufiProVersion"
        };
    }
}
