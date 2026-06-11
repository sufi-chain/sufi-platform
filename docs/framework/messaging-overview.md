# SufiAbp Messaging System

## Overview

The SufiAbp Messaging system provides a unified, provider-based architecture for sending messages across multiple channels: **Email**, **SMS**, and **Voice Calls**. It replaces the legacy `SufiChain.SufiAbp.Emailing` package with a more comprehensive solution.

## Key Features

- **Multi-Channel Support**: Email, SMS, and Voice Call abstractions
- **Provider-Based Architecture**: Core abstractions with pluggable provider implementations
- **SMTP Built-In**: SMTP email sender included by default (no additional packages needed)
- **Background Job Support**: All message types support queued/background sending
- **Settings-Based Configuration**: Dynamic configuration via SufiAbp Settings system
- **Graceful Degradation**: Works without configuration (uses Null implementations)
- **Template Integration**: Full integration with SufiAbp TextTemplating system

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  SufiChain.SufiAbp.Messaging (Core Package)                 │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │ IEmailSender │  │ ISmsSender   │  │IVoiceCallSender│    │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
│         ↓                 ↓                   ↓              │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐      │
│  │SmtpEmailSender│ │NullSmsSender │  │NullVoiceCall │      │
│  │  (DEFAULT)   │  │              │  │   Sender     │      │
│  └──────────────┘  └──────────────┘  └──────────────┘      │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  Provider Modules (Separate Packages - Injected)            │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ SufiChain.SufiAbp.Messaging.Twilio                   │   │
│  │  - TwilioSmsSender (replaces NullSmsSender)         │   │
│  │  - TwilioVoiceCallSender (replaces NullVoiceCall)   │   │
│  │  - TwilioSettingDefinitionProvider (dynamic settings)│   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ SufiChain.SufiAbp.Messaging.SendGrid                 │   │
│  │  - SendGridEmailSender (replaces SmtpEmailSender)    │   │
│  │  - SendGridSettingDefinitionProvider                 │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Package Structure

### Core Package: `SufiChain.SufiAbp.Messaging`

**Included by default in all SufiAbp applications.**

**Dependencies:**
- `SufiChain.SufiAbp.BackgroundJobs.Abstractions`
- `SufiChain.SufiAbp.Localization`
- `SufiChain.SufiAbp.Settings`
- `SufiChain.SufiAbp.TextTemplating.Scriban`
- `SufiChain.SufiAbp.VirtualFileSystem`

**What's Included:**

1. **Email Support (SMTP by default)**
   - `IEmailSender` - Interface for sending emails
   - `SmtpEmailSender` - SMTP implementation (registered by default)
   - `EmailSenderBase` - Base class for custom email providers
   - `BackgroundEmailSendingJob` - Background job support

2. **SMS Support (Interface only)**
   - `ISmsSender` - Interface for sending SMS
   - `NullSmsSender` - Default no-op implementation
   - `SmsSenderBase` - Base class for SMS providers
   - `BackgroundSmsSendingJob` - Background job support

3. **Voice Call Support (Interface only)**
   - `IVoiceCallSender` - Interface for voice calls
   - `NullVoiceCallSender` - Default no-op implementation
   - `VoiceCallSenderBase` - Base class for voice providers
   - `BackgroundVoiceCallSendingJob` - Background job support

4. **Settings & Configuration**
   - `MessagingSettingNames` - Setting key constants
   - `MessagingSettingDefinitionProvider` - Core settings (SMTP, default addresses)

5. **Templates**
   - `StandardMessageTemplates` - Built-in message templates
   - `StandardMessageTemplateDefinitionProvider` - Template definitions

## Quick Start

### 1. Basic Email Sending (SMTP)

SMTP is included by default. Just configure settings:

**appsettings.json:**
```json
{
  "Settings": {
    "SufiAbp.Messaging.Email.DefaultFromAddress": "noreply@example.com",
    "SufiAbp.Messaging.Email.DefaultFromDisplayName": "My Application",
    "SufiAbp.Messaging.Email.Smtp.Host": "smtp.gmail.com",
    "SufiAbp.Messaging.Email.Smtp.Port": "587",
    "SufiAbp.Messaging.Email.Smtp.EnableSsl": "true",
    "SufiAbp.Messaging.Email.Smtp.UserName": "your-email@gmail.com",
    "SufiAbp.Messaging.Email.Smtp.Password": "your-app-password"
  }
}
```

**Usage in Application Service:**
```csharp
public class UserRegistrationService : SufiAbpApplicationService
{
    private readonly IEmailSender _emailSender;

    public UserRegistrationService(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task RegisterUserAsync(RegisterUserInput input)
    {
        // ... create user ...

        // Send welcome email
        await _emailSender.SendAsync(
            to: input.Email,
            subject: "Welcome to Our Platform!",
            body: "Thank you for registering...",
            isBodyHtml: true
        );
    }
}
```

### 2. Background Email Sending

```csharp
// Queue email for background processing
await _emailSender.SendAsync(
    to: "user@example.com",
    subject: "Your Report is Ready",
    body: reportHtml,
    isBodyHtml: true,
    additionalArgs: new AdditionalMessageSendingArgs
    {
        QueueMessage = true,  // Send via background job
        Priority = MessagePriority.High
    }
);
```

### 3. SMS Sending (Requires Provider Module)

**Without Provider (No-op):**
```csharp
// This will log a warning but not fail
await _smsSender.SendAsync("+1234567890", "Your verification code is 123456");
```

**With Provider Module (e.g., Twilio):**

1. Install provider package:
   ```bash
   dotnet add package SufiChain.SufiAbp.Messaging.Twilio
   ```

2. Add module dependency:
   ```csharp
   [DependsOn(
       typeof(SufiAbpMessagingModule),
       typeof(SufiAbpMessagingTwilioModule)  // Replaces NullSmsSender
   )]
   public class MyApplicationModule : SufiAbpModule
   {
   }
   ```

3. Configure settings:
   ```json
   {
     "Settings": {
       "SufiAbp.Messaging.Sms.DefaultFromNumber": "+1234567890",
       "SufiAbp.Messaging.Sms.Twilio.AccountSid": "your-account-sid",
       "SufiAbp.Messaging.Sms.Twilio.AuthToken": "your-auth-token"
     }
   }
   ```

4. Use in code:
   ```csharp
   await _smsSender.SendAsync(
       phoneNumber: "+1234567890",
       message: "Your verification code is 123456"
   );
   ```

## Settings Reference

### Email Settings (SMTP)

| Setting Key | Description | Default |
|------------|-------------|---------|
| `SufiAbp.Messaging.Email.DefaultFromAddress` | Default sender email | - |
| `SufiAbp.Messaging.Email.DefaultFromDisplayName` | Default sender name | - |
| `SufiAbp.Messaging.Email.Smtp.Host` | SMTP server hostname | - |
| `SufiAbp.Messaging.Email.Smtp.Port` | SMTP server port | 25 |
| `SufiAbp.Messaging.Email.Smtp.EnableSsl` | Enable SSL/TLS | false |
| `SufiAbp.Messaging.Email.Smtp.UserName` | SMTP username | - |
| `SufiAbp.Messaging.Email.Smtp.Password` | SMTP password | - |
| `SufiAbp.Messaging.Email.Smtp.Domain` | SMTP domain (optional) | - |
| `SufiAbp.Messaging.Email.Smtp.UseDefaultCredentials` | Use Windows credentials | true |

### SMS Settings (Core)

| Setting Key | Description | Default |
|------------|-------------|---------|
| `SufiAbp.Messaging.Sms.DefaultFromNumber` | Default sender phone number | - |
| `SufiAbp.Messaging.Sms.ProviderName` | Active SMS provider name | - |

### Voice Call Settings (Core)

| Setting Key | Description | Default |
|------------|-------------|---------|
| `SufiAbp.Messaging.VoiceCall.DefaultFromNumber` | Default caller phone number | - |
| `SufiAbp.Messaging.VoiceCall.DefaultLanguage` | Default voice language | en-US |
| `SufiAbp.Messaging.VoiceCall.DefaultVoiceGender` | Default voice gender | Female |
| `SufiAbp.Messaging.VoiceCall.ProviderName` | Active voice provider name | - |

## Template Integration

The Messaging system integrates with SufiAbp TextTemplating for dynamic message content:

```csharp
public class WelcomeEmailSender : SufiAbpApplicationService
{
    private readonly IEmailSender _emailSender;
    private readonly ITemplateRenderer _templateRenderer;

    public WelcomeEmailSender(
        IEmailSender emailSender,
        ITemplateRenderer templateRenderer)
    {
        _emailSender = emailSender;
        _templateRenderer = templateRenderer;
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        var body = await _templateRenderer.RenderAsync(
            "WelcomeEmail",
            new { UserName = userName }
        );

        await _emailSender.SendAsync(
            to: email,
            subject: "Welcome!",
            body: body,
            isBodyHtml: true
        );
    }
}
```

## Migration from Legacy Emailing

If you're migrating from `SufiChain.SufiAbp.Emailing`:

1. **Remove old package reference:**
   ```xml
   <!-- Remove this -->
   <PackageReference Include="SufiChain.SufiAbp.Emailing" Version="*" />
   ```

2. **Add new package reference:**
   ```xml
   <!-- Add this (or it's already included via SufiAbpModule) -->
   <PackageReference Include="SufiChain.SufiAbp.Messaging" Version="*" />
   ```

3. **Update module dependencies:**
   ```csharp
   // Old
   [DependsOn(typeof(SufiAbpEmailingModule))]
   
   // New
   [DependsOn(typeof(SufiAbpMessagingModule))]
   ```

4. **Update code:**
   ```csharp
   // Old
   using SufiChain.SufiAbp.Emailing;
   
   // New
   using SufiChain.SufiAbp.Messaging;
   ```

5. **Update settings keys:**
   ```json
   // Old
   "SufiAbp.Emailing.DefaultFromAddress": "..."
   
   // New
   "SufiAbp.Messaging.Email.DefaultFromAddress": "..."
   ```

## Next Steps

- [Creating Custom Providers](./messagingvider-guide.md)
- [Dynamic Settings Configuration](./messaging-settings-guide.md)
- [TextTemplating Integration](./text-templating-overview.md)
- [Background Jobs Configuration](./messaging-background-jobs.md)

## See Also

- [SufiAbp Settings System](./settings.md)
- [SufiAbp Background Jobs](./background-jobs.md)
- [SufiAbp TextTemplating](./text-templating-overview.md)
