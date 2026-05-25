# Creating Custom Messaging Providers

This guide explains how to create custom provider modules for the SufiAbp Messaging system (Email, SMS, and Voice Call providers).

## Provider Architecture

Providers are separate NuGet packages that:
1. Implement the core messaging interfaces (`IEmailSender`, `ISmsSender`, `IVoiceCallSender`)
2. Register themselves to replace the default/null implementations
3. Define provider-specific settings dynamically
4. Optionally provide UI for settings management

## Example 1: Creating a Twilio SMS Provider

### Step 1: Create the Provider Package

```bash
cd sufi-abp/framework
mkdir SufiChain.SufiAbp.Messaging.Twilio
cd SufiChain.SufiAbp.Messaging.Twilio
```

### Step 2: Create the Project File

**SufiChain.SufiAbp.Messaging.Twilio.csproj:**
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <RootNamespace>SufiChain.SufiAbp.Messaging.Twilio</RootNamespace>
    <GenerateEmbeddedFilesManifest>true</GenerateEmbeddedFilesManifest>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="Twilio" Version="7.0.0" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\SufiChain.SufiAbp.Messaging\SufiChain.SufiAbp.Messaging.csproj" />
  </ItemGroup>

  <ItemGroup>
    <EmbeddedResource Include="Localization\**\*.json" />
  </ItemGroup>
</Project>
```

### Step 3: Define Provider-Specific Settings

**TwilioMessagingSettingNames.cs:**
```csharp
namespace SufiChain.SufiAbp.Messaging.Twilio;

public static class TwilioMessagingSettingNames
{
    private const string Prefix = "SufiAbp.Messaging.Twilio";

    public const string AccountSid = Prefix + ".AccountSid";
    public const string AuthToken = Prefix + ".AuthToken";
    public const string DefaultFromNumber = Prefix + ".DefaultFromNumber";
    
    // Voice-specific settings
    public const string VoiceUrl = Prefix + ".VoiceUrl";
    public const string StatusCallbackUrl = Prefix + ".StatusCallbackUrl";
}
```

### Step 4: Create Settings Definition Provider

**TwilioMessagingSettingDefinitionProvider.cs:**
```csharp
using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.Messaging.Twilio.Localization;
using SufiChain.SufiAbp.Settings;

namespace SufiChain.SufiAbp.Messaging.Twilio;

public class TwilioMessagingSettingDefinitionProvider : SettingDefinitionProvider
{
    public override void Define(ISettingDefinitionContext context)
    {
        context.Add(
            new SettingDefinition(
                TwilioMessagingSettingNames.AccountSid,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:AccountSid"),
                description: L("Settings:Twilio:AccountSidDescription"),
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                TwilioMessagingSettingNames.AuthToken,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:AuthToken"),
                description: L("Settings:Twilio:AuthTokenDescription"),
                isVisibleToClients: false,
                isEncrypted: true
            ),
            new SettingDefinition(
                TwilioMessagingSettingNames.DefaultFromNumber,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:DefaultFromNumber"),
                description: L("Settings:Twilio:DefaultFromNumberDescription"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                TwilioMessagingSettingNames.VoiceUrl,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:VoiceUrl"),
                description: L("Settings:Twilio:VoiceUrlDescription"),
                isVisibleToClients: false
            ),
            new SettingDefinition(
                TwilioMessagingSettingNames.StatusCallbackUrl,
                defaultValue: string.Empty,
                displayName: L("Settings:Twilio:StatusCallbackUrl"),
                description: L("Settings:Twilio:StatusCallbackUrlDescription"),
                isVisibleToClients: false
            )
        );
    }

    private static LocalizableString L(string name)
    {
        return LocalizableString.Create<TwilioMessagingResource>(name);
    }
}
```

### Step 5: Implement SMS Sender

**TwilioSmsSender.cs:**
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SufiChain.SufiAbp.Messaging.Twilio;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISmsSender))]
public class TwilioSmsSender : SmsSenderBase, ITransientDependency
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
        var accountSid = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AccountSid);
        var authToken = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AuthToken);

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
        {
            Logger.LogWarning("Twilio credentials not configured. SMS will not be sent.");
            return;
        }

        var fromNumber = from ?? await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.DefaultFromNumber);
        if (string.IsNullOrEmpty(fromNumber))
        {
            throw new InvalidOperationException("Twilio 'from' number is not configured.");
        }

        TwilioClient.Init(accountSid, authToken);

        var messageResource = await MessageResource.CreateAsync(
            to: new PhoneNumber(phoneNumber),
            from: new PhoneNumber(fromNumber),
            body: message
        );

        Logger.LogInformation($"SMS sent via Twilio. SID: {messageResource.Sid}, Status: {messageResource.Status}");
    }
}
```

### Step 6: Implement Voice Call Sender

**TwilioVoiceCallSender.cs:**
```csharp
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.Settings;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SufiChain.SufiAbp.Messaging.Twilio;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IVoiceCallSender))]
public class TwilioVoiceCallSender : VoiceCallSenderBase, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public TwilioVoiceCallSender(
        IBackgroundJobManager backgroundJobManager,
        ISettingProvider settingProvider)
        : base(backgroundJobManager)
    {
        SettingProvider = settingProvider;
    }

    protected override async Task SendVoiceCallAsync(
        string phoneNumber,
        string message,
        string from = null,
        VoiceCallOptions voiceOptions = null)
    {
        var accountSid = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AccountSid);
        var authToken = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.AuthToken);

        if (string.IsNullOrEmpty(accountSid) || string.IsNullOrEmpty(authToken))
        {
            Logger.LogWarning("Twilio credentials not configured. Voice call will not be made.");
            return;
        }

        var fromNumber = from ?? await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.DefaultFromNumber);
        var voiceUrl = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.VoiceUrl);
        var statusCallbackUrl = await SettingProvider.GetOrNullAsync(TwilioMessagingSettingNames.StatusCallbackUrl);

        if (string.IsNullOrEmpty(fromNumber))
        {
            throw new InvalidOperationException("Twilio 'from' number is not configured.");
        }

        if (string.IsNullOrEmpty(voiceUrl))
        {
            throw new InvalidOperationException("Twilio voice URL is not configured.");
        }

        TwilioClient.Init(accountSid, authToken);

        var call = await CallResource.CreateAsync(
            to: new PhoneNumber(phoneNumber),
            from: new PhoneNumber(fromNumber),
            url: new Uri(voiceUrl),
            statusCallback: string.IsNullOrEmpty(statusCallbackUrl) ? null : new Uri(statusCallbackUrl)
        );

        Logger.LogInformation($"Voice call initiated via Twilio. SID: {call.Sid}, Status: {call.Status}");
    }
}
```

### Step 7: Create the Module Class

**SufiAbpMessagingTwilioModule.cs:**
```csharp
using Microsoft.Extensions.DependencyInjection;
using SufiChain.SufiAbp.Localization;
using SufiChain.SufiAbp.Messaging.Twilio.Localization;
using SufiChain.SufiAbp.Modularity;
using SufiChain.SufiAbp.Settings;
using SufiChain.SufiAbp.VirtualFileSystem;

namespace SufiChain.SufiAbp.Messaging.Twilio;

[DependsOn(
    typeof(SufiAbpMessagingModule)
)]
public class SufiAbpMessagingTwilioModule : SufiAbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<SufiAbpMessagingTwilioModule>();
        });

        Configure<SufiAbpLocalizationOptions>(options =>
        {
            options.Resources
                .Add<TwilioMessagingResource>("en")
                .AddVirtualJson("/Localization/TwilioMessaging");
        });
    }
}
```

### Step 8: Add Localization Resources

**Localization/TwilioMessaging/en.json:**
```json
{
  "Culture": "en",
  "Texts": {
    "Settings:Twilio:AccountSid": "Twilio Account SID",
    "Settings:Twilio:AccountSidDescription": "Your Twilio Account SID from the Twilio Console",
    "Settings:Twilio:AuthToken": "Twilio Auth Token",
    "Settings:Twilio:AuthTokenDescription": "Your Twilio Auth Token from the Twilio Console",
    "Settings:Twilio:DefaultFromNumber": "Default From Number",
    "Settings:Twilio:DefaultFromNumberDescription": "Default phone number for outgoing messages (E.164 format)",
    "Settings:Twilio:VoiceUrl": "Voice URL",
    "Settings:Twilio:VoiceUrlDescription": "TwiML URL for voice call instructions",
    "Settings:Twilio:StatusCallbackUrl": "Status Callback URL",
    "Settings:Twilio:StatusCallbackUrlDescription": "URL to receive call status updates"
  }
}
```

**Localization/TwilioMessagingResource.cs:**
```csharp
using SufiChain.SufiAbp.Localization;

namespace SufiChain.SufiAbp.Messaging.Twilio.Localization;

[LocalizationResourceName("TwilioMessaging")]
public class TwilioMessagingResource
{
}
```

## Example 2: Creating a SendGrid Email Provider

### SendGridEmailSender.cs

```csharp
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;
using SufiChain.SufiAbp.BackgroundJobs;
using SufiChain.SufiAbp.DependencyInjection;
using SufiChain.SufiAbp.Settings;

namespace SufiChain.SufiAbp.Messaging.SendGrid;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(IEmailSender))]
public class SendGridEmailSender : EmailSenderBase, ITransientDependency
{
    protected ISettingProvider SettingProvider { get; }

    public SendGridEmailSender(
        IEmailSenderConfiguration configuration,
        IBackgroundJobManager backgroundJobManager,
        ISettingProvider settingProvider)
        : base(configuration, backgroundJobManager)
    {
        SettingProvider = settingProvider;
    }

    protected override async Task SendEmailAsync(System.Net.Mail.MailMessage mail)
    {
        var apiKey = await SettingProvider.GetOrNullAsync(SendGridMessagingSettingNames.ApiKey);
        if (string.IsNullOrEmpty(apiKey))
        {
            Logger.LogWarning("SendGrid API key not configured. Email will not be sent.");
            return;
        }

        var client = new SendGridClient(apiKey);
        var from = new EmailAddress(mail.From.Address, mail.From.DisplayName);
        var subject = mail.Subject;
        var to = new EmailAddress(mail.To[0].Address, mail.To[0].DisplayName);
        var plainTextContent = mail.Body;
        var htmlContent = mail.IsBodyHtml ? mail.Body : null;

        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

        // Add attachments
        foreach (var attachment in mail.Attachments)
        {
            using var stream = attachment.ContentStream;
            var bytes = new byte[stream.Length];
            await stream.ReadAsync(bytes, 0, (int)stream.Length);
            var base64 = Convert.ToBase64String(bytes);
            msg.AddAttachment(attachment.Name, base64);
        }

        var response = await client.SendEmailAsync(msg);

        if (response.IsSuccessStatusCode)
        {
            Logger.LogInformation($"Email sent successfully via SendGrid to {mail.To}");
        }
        else
        {
            var body = await response.Body.ReadAsStringAsync();
            Logger.LogError($"SendGrid email failed: {response.StatusCode} - {body}");
            throw new Exception($"SendGrid email failed: {response.StatusCode}");
        }
    }
}
```

## Using Providers in Host Applications

### Step 1: Install Provider Package

```bash
dotnet add package SufiChain.SufiAbp.Messaging.Twilio
```

### Step 2: Add Module Dependency

```csharp
[DependsOn(
    typeof(SufiAbpMessagingModule),
    typeof(SufiAbpMessagingTwilioModule)  // Replaces NullSmsSender and NullVoiceCallSender
)]
public class MyApplicationModule : SufiAbpModule
{
}
```

### Step 3: Configure Settings

**appsettings.json:**
```json
{
  "Settings": {
    "SufiAbp.Messaging.Twilio.AccountSid": "your-account-sid",
    "SufiAbp.Messaging.Twilio.AuthToken": "your-auth-token",
    "SufiAbp.Messaging.Twilio.DefaultFromNumber": "+1234567890",
    "SufiAbp.Messaging.Twilio.VoiceUrl": "https://yourapp.com/api/twilio/voice",
    "SufiAbp.Messaging.Twilio.StatusCallbackUrl": "https://yourapp.com/api/twilio/status"
  }
}
```

### Step 4: Use in Application Code

```csharp
public class NotificationService : SufiAbpApplicationService
{
    private readonly ISmsSender _smsSender;
    private readonly IVoiceCallSender _voiceCallSender;

    public NotificationService(
        ISmsSender smsSender,
        IVoiceCallSender voiceCallSender)
    {
        _smsSender = smsSender;
        _voiceCallSender = voiceCallSender;
    }

    public async Task SendVerificationCodeAsync(string phoneNumber, string code)
    {
        // Now uses TwilioSmsSender instead of NullSmsSender
        await _smsSender.SendAsync(
            phoneNumber: phoneNumber,
            message: $"Your verification code is: {code}"
        );
    }

    public async Task SendEmergencyCallAsync(string phoneNumber, string message)
    {
        // Now uses TwilioVoiceCallSender instead of NullVoiceCallSender
        await _voiceCallSender.SendAsync(
            phoneNumber: phoneNumber,
            message: message,
            voiceOptions: new VoiceCallOptions
            {
                Language = "en-US",
                VoiceGender = VoiceGender.Female
            }
        );
    }
}
```

## Key Provider Patterns

### 1. Service Replacement

Use `[Dependency(ReplaceServices = true)]` to replace default implementations:

```csharp
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ISmsSender))]
public class TwilioSmsSender : SmsSenderBase, ITransientDependency
{
}
```

### 2. Settings-Based Configuration

Always use `ISettingProvider` for configuration (never hardcode):

```csharp
var apiKey = await SettingProvider.GetOrNullAsync(ProviderSettingNames.ApiKey);
if (string.IsNullOrEmpty(apiKey))
{
    Logger.LogWarning("Provider not configured. Message will not be sent.");
    return;
}
```

### 3. Graceful Degradation

Log warnings instead of throwing exceptions when not configured:

```csharp
if (string.IsNullOrEmpty(apiKey))
{
    Logger.LogWarning("Provider not configured.");
    return; // Don't throw, just skip
}
```

### 4. Background Job Support

Inherit from base classes to get automatic background job support:

```csharp
public class TwilioSmsSender : SmsSenderBase // Inherits background job logic
{
    protected override async Task SendSmsAsync(string phoneNumber, string message, string from = null)
    {
        // Your implementation
    }
}
```

## Next Steps

- [Dynamic Settings UI Configuration](./messaging-settings-ui-guide.md)
- [Testing Providers](./messaging-provider-testing.md)
- [Provider Best Practices](./messaging-provider-best-practices.md)
