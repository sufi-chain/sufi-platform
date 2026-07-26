# Dynamic Settings Configuration for Communication Providers

This guide explains how to create dynamic, provider-specific settings that automatically appear in the host application's settings UI when a provider module is installed.

## Overview

When you install a communication provider (e.g., Twilio, SendGrid), its settings should automatically become available in:
1. **appsettings.json** - For configuration
2. **Settings Management UI** - For runtime configuration (Blazor UI)
3. **ISettingProvider** - For programmatic access

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  Host Application                                            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ Settings Management UI (Blazor)                      │   │
│  │  - Automatically discovers all SettingDefinitions    │   │
│  │  - Renders UI based on setting metadata             │   │
│  │  - Groups settings by provider                       │   │
│  └──────────────────────────────────────────────────────┘   │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ISettingManager                                      │   │
│  │  - SetAsync(name, value)                             │   │
│  │  - GetAsync(name)                                    │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  Provider Module (e.g., Twilio)                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ TwilioMessagingSettingDefinitionProvider             │   │
│  │  - Defines provider-specific settings                │   │
│  │  - Metadata: display name, description, encryption   │   │
│  │  - Grouping: organizes settings in UI                │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Step-by-Step Guide

### Step 1: Define Setting Names

Create a static class with setting key constants:

**TwilioMessagingSettingNames.cs:**
```csharp
namespace SufiChain.SufiPlatform.Communication.Twilio;

public static class TwilioMessagingSettingNames
{
    private const string Prefix = "Sufi.Communication.Twilio";

    public const string AccountSid = Prefix + ".AccountSid";
    public const string AuthToken = Prefix + ".AuthToken";
    public const string DefaultFromNumber = Prefix + ".DefaultFromNumber";
    
    // SMS-specific
    public const string SmsStatusCallbackUrl = Prefix + ".Sms.StatusCallbackUrl";
    
    // Voice-specific
    public const string VoiceUrl = Prefix + ".Voice.Url";
    public const string VoiceStatusCallbackUrl = Prefix + ".Voice.StatusCallbackUrl";
    public const string VoiceMethod = Prefix + ".Voice.Method";
}
```

### Step 2: Create Setting Definition Provider

**TwilioMessagingSettingDefinitionProvider.cs:**
```csharp
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Communication.Twilio.Localization;
using SufiChain.SufiPlatform.Settings;

namespace SufiChain.SufiPlatform.Communication.Twilio;

public class TwilioMessagingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        // Group: Twilio Account
        context.Add(
            new SettingDefinition(
                TwilioMessagingSettingNames.AccountSid,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:AccountSid"),
                description: L("Settings:Twilio:AccountSidDescription"),
                isVisibleToClients: false,
                isEncrypted: true
            )
            .WithProperty("Group", "Twilio.Account")
            .WithProperty("Order", 1),
            
            new SettingDefinition(
                TwilioMessagingSettingNames.AuthToken,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:AuthToken"),
                description: L("Settings:Twilio:AuthTokenDescription"),
                isVisibleToClients: false,
                isEncrypted: true
            )
            .WithProperty("Group", "Twilio.Account")
            .WithProperty("Order", 2)
            .WithProperty("InputType", "password"),
            
            new SettingDefinition(
                TwilioMessagingSettingNames.DefaultFromNumber,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:DefaultFromNumber"),
                description: L("Settings:Twilio:DefaultFromNumberDescription"),
                isVisibleToClients: false
            )
            .WithProperty("Group", "Twilio.Account")
            .WithProperty("Order", 3)
            .WithProperty("InputType", "tel")
            .WithProperty("Placeholder", "+1234567890")
        );

        // Group: Twilio SMS
        context.Add(
            new SettingDefinition(
                TwilioMessagingSettingNames.SmsStatusCallbackUrl,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:SmsStatusCallbackUrl"),
                description: L("Settings:Twilio:SmsStatusCallbackUrlDescription"),
                isVisibleToClients: false
            )
            .WithProperty("Group", "Twilio.SMS")
            .WithProperty("Order", 1)
            .WithProperty("InputType", "url")
            .WithProperty("Placeholder", "https://yourapp.com/api/twilio/sms-status")
        );

        // Group: Twilio Voice
        context.Add(
            new SettingDefinition(
                TwilioMessagingSettingNames.VoiceUrl,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:VoiceUrl"),
                description: L("Settings:Twilio:VoiceUrlDescription"),
                isVisibleToClients: false
            )
            .WithProperty("Group", "Twilio.Voice")
            .WithProperty("Order", 1)
            .WithProperty("InputType", "url")
            .WithProperty("Placeholder", "https://yourapp.com/api/twilio/voice")
            .WithProperty("Required", true),
            
            new SettingDefinition(
                TwilioMessagingSettingNames.VoiceStatusCallbackUrl,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:VoiceStatusCallbackUrl"),
                description: L("Settings:Twilio:VoiceStatusCallbackUrlDescription"),
                isVisibleToClients: false
            )
            .WithProperty("Group", "Twilio.Voice")
            .WithProperty("Order", 2)
            .WithProperty("InputType", "url"),
            
            new SettingDefinition(
                TwilioMessagingSettingNames.VoiceMethod,
                defaultValue: "POST",
                displayName: L("Settings:Twilio:VoiceMethod"),
                description: L("Settings:Twilio:VoiceMethodDescription"),
                isVisibleToClients: false
            )
            .WithProperty("Group", "Twilio.Voice")
            .WithProperty("Order", 3)
            .WithProperty("InputType", "select")
            .WithProperty("Options", new[] { "GET", "POST" })
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TwilioMessagingResource>(name);
    }
}
```

### Step 3: Add Localization Resources

**Localization/TwilioMessaging/en.json:**
```json
{
  "Culture": "en",
  "Texts": {
    "Settings:Twilio:AccountSid": "Account SID",
    "Settings:Twilio:AccountSidDescription": "Your Twilio Account SID from the Twilio Console (https://console.twilio.com)",
    "Settings:Twilio:AuthToken": "Auth Token",
    "Settings:Twilio:AuthTokenDescription": "Your Twilio Auth Token (keep this secret!)",
    "Settings:Twilio:DefaultFromNumber": "Default From Number",
    "Settings:Twilio:DefaultFromNumberDescription": "Default phone number for outgoing messages in E.164 format (e.g., +1234567890)",
    "Settings:Twilio:SmsStatusCallbackUrl": "SMS Status Callback URL",
    "Settings:Twilio:SmsStatusCallbackUrlDescription": "URL to receive SMS delivery status updates (optional)",
    "Settings:Twilio:VoiceUrl": "Voice URL",
    "Settings:Twilio:VoiceUrlDescription": "TwiML URL that returns voice call instructions (required for voice calls)",
    "Settings:Twilio:VoiceStatusCallbackUrl": "Voice Status Callback URL",
    "Settings:Twilio:VoiceStatusCallbackUrlDescription": "URL to receive call status updates (optional)",
    "Settings:Twilio:VoiceMethod": "Voice HTTP Method",
    "Settings:Twilio:VoiceMethodDescription": "HTTP method for voice URL requests (GET or POST)"
  }
}
```

### Step 4: Setting Metadata Properties

Use `.WithProperty()` to add metadata for UI rendering:

| Property | Description | Example Values |
|----------|-------------|----------------|
| `Group` | Groups related settings in UI | `"Twilio.Account"`, `"Twilio.SMS"` |
| `Order` | Display order within group | `1`, `2`, `3` |
| `InputType` | HTML input type | `"text"`, `"password"`, `"email"`, `"url"`, `"tel"`, `"number"`, `"select"`, `"checkbox"` |
| `Placeholder` | Input placeholder text | `"+1234567890"`, `"https://..."` |
| `Required` | Mark as required field | `true`, `false` |
| `Options` | Options for select/radio | `new[] { "Option1", "Option2" }` |
| `Min` | Minimum value (number) | `0`, `1` |
| `Max` | Maximum value (number) | `100`, `1000` |
| `Pattern` | Regex validation pattern | `"^\\+[1-9]\\d{1,14}$"` |

### Step 5: Register Provider in Module

**SufiComTwilioModule.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiPlatform.Localization;
using SufiChain.SufiPlatform.Communication.Twilio.Localization;
using SufiChain.SufiPlatform.Modularity;
using SufiChain.SufiPlatform.Settings;
using SufiChain.SufiPlatform.VirtualFileSystem;

namespace SufiChain.SufiPlatform.Communication.Twilio;

[DependsOn(
    typeof(SufiComModule),
    typeof(SufiSettingsBlazorModule)
)]
public class SufiComTwilioModule : SufiModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiComTwilioModule>();
        });

        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<TwilioMessagingResource>("en")
                .AddVirtualJson("/Localization/TwilioMessaging");
        });

        // Settings are automatically discovered via TwilioMessagingSettingDefinitionProvider
    }
}
```

## Settings UI Integration (Blazor)

### Automatic Discovery

When the host application includes the Twilio module, settings are automatically discovered:

```csharp
// Host application module
[DependsOn(
    typeof(SufiComModule),
    typeof(SufiComTwilioModule)  // Settings auto-discovered
)]
public class MyAppBlazorModule : SufiModule
{
}
```

### Settings Management Page

The host application's settings management page will automatically display Twilio settings grouped by category:

**Example Blazor Settings Page:**
```razor
@page "/settings/communication"
@using SufiChain.SufiPlatform.Settings
@inject ISettingManager SettingManager
@inject ISettingDefinitionManager SettingDefinitionManager

<h3>Communication Settings</h3>

@foreach (var group in GetSettingGroups())
{
    <Card>
        <CardHeader>@group.Key</CardHeader>
        <CardBody>
            @foreach (var setting in group.Value)
            {
                <FormGroup>
                    <Label>@setting.DisplayName</Label>
                    <Input Type="@GetInputType(setting)" 
                           @bind-Value="settingValues[setting.Name]"
                           Placeholder="@GetPlaceholder(setting)" />
                    <FormText>@setting.Description</FormText>
                </FormGroup>
            }
        </CardBody>
    </Card>
}

<Button Color="Color.Primary" Clicked="SaveSettingsAsync">Save</Button>

@code {
    private Dictionary<string, string> settingValues = new();

    protected override async Task OnInitializedAsync()
    {
        var definitions = await SettingDefinitionManager.GetAllAsync();
        
        foreach (var definition in definitions.Where(d => d.Name.StartsWith("Sufi.Communication")))
        {
            settingValues[definition.Name] = await SettingManager.GetOrNullAsync(definition.Name) ?? string.Empty;
        }
    }

    private Dictionary<string, List<SettingDefinition>> GetSettingGroups()
    {
        var definitions = SettingDefinitionManager.GetAll()
            .Where(d => d.Name.StartsWith("Sufi.Communication"));
        
        return definitions
            .GroupBy(d => d.Properties.GetOrDefault("Group")?.ToString() ?? "General")
            .OrderBy(g => g.Key)
            .ToDictionary(g => g.Key, g => g.OrderBy(s => s.Properties.GetOrDefault("Order")).ToList());
    }

    private string GetInputType(SettingDefinition setting)
    {
        return setting.Properties.GetOrDefault("InputType")?.ToString() ?? "text";
    }

    private string GetPlaceholder(SettingDefinition setting)
    {
        return setting.Properties.GetOrDefault("Placeholder")?.ToString() ?? string.Empty;
    }

    private async Task SaveSettingsAsync()
    {
        foreach (var kvp in settingValues)
        {
            await SettingManager.SetAsync(kvp.Key, kvp.Value);
        }
        
        // Show success message
    }
}
```

## Configuration in appsettings.json

Settings can also be configured via appsettings.json:

**appsettings.json:**
```json
{
  "Settings": {
    "Sufi.Communication.Twilio.AccountSid": "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx",
    "Sufi.Communication.Twilio.AuthToken": "your-auth-token-here",
    "Sufi.Communication.Twilio.DefaultFromNumber": "+1234567890",
    "Sufi.Communication.Twilio.Sms.StatusCallbackUrl": "https://myapp.com/api/twilio/sms-status",
    "Sufi.Communication.Twilio.Voice.Url": "https://myapp.com/api/twilio/voice",
    "Sufi.Communication.Twilio.Voice.StatusCallbackUrl": "https://myapp.com/api/twilio/voice-status",
    "Sufi.Communication.Twilio.Voice.Method": "POST"
  }
}
```

## Programmatic Access

Access settings in your code:

```csharp
public class TwilioSmsSender : SmsSenderBase
{
    protected ISettingProvider SettingProvider { get; }

    public TwilioSmsSender(
        IBackgroundJobManager backgroundJobManager,
        ISettingProvider settingProvider)
        : base(backgroundJobManager)
    {
        SettingProvider = settingProvider;
    }

    protected override async Task SendSmsAsync(string phoneNumber, string message, string from = null)
    {
        // Get settings
        var accountSid = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AccountSid);
        var authToken = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AuthToken);
        var defaultFromNumber = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.DefaultFromNumber);

        // Validate
        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
        {
            Logger.LogWarning("Twilio credentials not configured. SMS will not be sent.");
            return;
        }

        // Use settings
        var fromNumber = from ?? defaultFromNumber;
        
        // ... send SMS ...
    }
}
```

## Multi-Tenancy Support

Settings can be tenant-specific:

```csharp
context.Add(
    new SettingDefinition(
        TwilioMessagingSettingNames.AccountSid,
        defaultValue: string.Empty,
        displayName: L("Settings:Twilio:AccountSid"),
        description: L("Settings:Twilio:AccountSidDescription"),
        isVisibleToClients: false,
        isEncrypted: true,
        scopes: SettingScopes.Tenant  // Tenant-specific setting
    )
);
```

**Access tenant-specific settings:**
```csharp
// Get current tenant's setting
var accountSid = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AccountSid);

// Get specific tenant's setting
var accountSid = await SettingProvider.GetOrNullForTenantAsync(
    TwilioMessagingSettingNames.AccountSid,
    tenantId
);

// Set tenant-specific setting
await SettingManager.SetForTenantAsync(
    tenantId,
    TwilioMessagingSettingNames.AccountSid,
    "ACxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxxx"
);
```

## Encryption Support

Sensitive settings (API keys, tokens) should be encrypted:

```csharp
new SettingDefinition(
    TwilioMessagingSettingNames.AuthToken,
    defaultValue: string.Empty,
    displayName: L("Settings:Twilio:AuthToken"),
    description: L("Settings:Twilio:AuthTokenDescription"),
    isVisibleToClients: false,
    isEncrypted: true  // Encrypted in database
)
```

## Best Practices

1. **Use Descriptive Names**: Setting keys should be self-documenting
2. **Group Related Settings**: Use the `Group` property for organization
3. **Provide Descriptions**: Help users understand what each setting does
4. **Encrypt Secrets**: Always encrypt API keys, tokens, and passwords
5. **Set Defaults**: Provide sensible default values when possible
6. **Validate Input**: Use `Pattern`, `Min`, `Max` properties for validation
7. **Support Multi-Tenancy**: Consider tenant-specific settings when appropriate
8. **Document Settings**: Include links to provider documentation in descriptions

## Example: Complete Provider with Settings

See the [Creating Custom Providers](./messagingvider-guide.md) guide for complete examples of providers with dynamic settings.

## Next Steps

- [Creating Custom Providers](./messagingvider-guide.md)
- [Settings Management UI](./settings-management-ui.md)
- [Multi-Tenancy Configuration](./multi-tenancy-settings.md)

## See Also

- [Sufi Platform Settings System](./settings.md)
- [Sufi Platform Multi-Tenancy](./multi-tenancy.md)
- [Sufi Platform Localization](./localization.md)
