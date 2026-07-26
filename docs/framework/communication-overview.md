# Sufi Platform Communication System

## Overview

The Sufi Platform Communication system provides a unified, provider-based architecture for sending messages across multiple channels: **Email**, **SMS**, and **Voice Calls**. It replaces the legacy `SufiChain.SufiPlatform.Emailing` package with a more comprehensive solution.

## Key Features

- **Multi-Channel Support**: Email, SMS, and Voice Call abstractions
- **Provider-Based Architecture**: Core abstractions with pluggable provider implementations
- **SMTP Built-In**: SMTP email sender included by default (no additional packages needed)
- **Background Job Support**: All message types support queued/background sending
- **Settings-Based Configuration**: Dynamic configuration via Sufi Platform Settings system
- **Graceful Degradation**: Works without configuration (uses Null implementations)
- **Template Integration**: Full integration with Sufi Platform TextTemplating system

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  SufiChain.sufichain.communication (Core Package)                 │
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
│  │ SufiChain.sufichain.communication.Twilio                   │   │
│  │  - TwilioSmsSender (replaces NullSmsSender)         │   │
│  │  - TwilioVoiceCallSender (replaces NullVoiceCall)   │   │
│  │  - TwilioSettingDefinitionProvider (dynamic settings)│   │
│  └──────────────────────────────────────────────────────┘   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ SufiChain.sufichain.communication.SendGrid                 │   │
│  │  - SendGridEmailSender (replaces SmtpEmailSender)    │   │
│  │  - SendGridSettingDefinitionProvider                 │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Package Structure

### Core Package: `SufiChain.sufichain.communication`

**Included by default in all Sufi Platform applications.**

**Dependencies:**
- `SufiChain.SufiPlatform.BackgroundJobs.Abstractions`
- `SufiChain.SufiPlatform.Localization`
- `SufiChain.SufiPlatform.Settings`
- `SufiChain.SufiPlatform.TextTemplating.Scriban`
- `SufiChain.SufiPlatform.VirtualFileSystem`

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
    "Sufi.Communication.Email.DefaultFromAddress": "noreply@example.com",
    "Sufi.Communication.Email.DefaultFromDisplayName": "My Application",
    "Sufi.Communication.Email.Smtp.Host": "smtp.gmail.com",
    "Sufi.Communication.Email.Smtp.Port": "587",
    "Sufi.Communication.Email.Smtp.EnableSsl": "true",
    "Sufi.Communication.Email.Smtp.UserName": "your-email@gmail.com",
    "Sufi.Communication.Email.Smtp.Password": "your-app-password"
  }
}
```

**Usage in Application Service:**
```csharp
public class UserRegistrationService : SufiApplicationService
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
   dotnet add package SufiChain.sufichain.communication.Twilio
   ```

2. Add module dependency:
   ```csharp
   [DependsOn(
       typeof(SufiComModule),
       typeof(SufiComTwilioModule)  // Replaces NullSmsSender
   )]
   public class MyApplicationModule : SufiModule
   {
   }
   ```

3. Configure settings:
   ```json
   {
     "Settings": {
       "Sufi.Communication.Sms.DefaultFromNumber": "+1234567890",
       "Sufi.Communication.Sms.Twilio.AccountSid": "your-account-sid",
       "Sufi.Communication.Sms.Twilio.AuthToken": "your-auth-token"
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
| `Sufi.Communication.Email.DefaultFromAddress` | Default sender email | - |
| `Sufi.Communication.Email.DefaultFromDisplayName` | Default sender name | - |
| `Sufi.Communication.Email.Smtp.Host` | SMTP server hostname | - |
| `Sufi.Communication.Email.Smtp.Port` | SMTP server port | 25 |
| `Sufi.Communication.Email.Smtp.EnableSsl` | Enable SSL/TLS | false |
| `Sufi.Communication.Email.Smtp.UserName` | SMTP username | - |
| `Sufi.Communication.Email.Smtp.Password` | SMTP password | - |
| `Sufi.Communication.Email.Smtp.Domain` | SMTP domain (optional) | - |
| `Sufi.Communication.Email.Smtp.UseDefaultCredentials` | Use Windows credentials | true |

### SMS Settings (Core)

| Setting Key | Description | Default |
|------------|-------------|---------|
| `Sufi.Communication.Sms.DefaultFromNumber` | Default sender phone number | - |
| `Sufi.Communication.Sms.ProviderName` | Active SMS provider name | - |

### Voice Call Settings (Core)

| Setting Key | Description | Default |
|------------|-------------|---------|
| `Sufi.Communication.VoiceCall.DefaultFromNumber` | Default caller phone number | - |
| `Sufi.Communication.VoiceCall.DefaultLanguage` | Default voice language | en-US |
| `Sufi.Communication.VoiceCall.DefaultVoiceGender` | Default voice gender | Female |
| `Sufi.Communication.VoiceCall.ProviderName` | Active voice provider name | - |

## Template Integration

The Communication system integrates with Sufi Platform TextTemplating for dynamic message content:

```csharp
public class WelcomeEmailSender : SufiApplicationService
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

If you're migrating from `SufiChain.SufiPlatform.Emailing`:

1. **Remove old package reference:**
   ```xml
   <!-- Remove this -->
   <PackageReference Include="SufiChain.SufiPlatform.Emailing" Version="*" />
   ```

2. **Add new package reference:**
   ```xml
   <!-- Add this (or it's already included via SufiModule) -->
   <PackageReference Include="SufiChain.sufichain.communication" Version="*" />
   ```

3. **Update module dependencies:**
   ```csharp
   // Old
   [DependsOn(typeof(SufiComModule))]
   
   // New
   [DependsOn(typeof(SufiComModule))]
   ```

4. **Update code:**
   ```csharp
   // Old
   using SufiChain.SufiPlatform.Emailing;
   
   // New
   using SufiChain.sufichain.communication;
   ```

5. **Update settings keys:**
   ```json
   // Old
   "Sufi.Emailing.DefaultFromAddress": "..."
   
   // New
   "Sufi.Communication.Email.DefaultFromAddress": "..."
   ```

## Next Steps

- [Creating Custom Providers](./messagingvider-guide.md)
- [Dynamic Settings Configuration](./communication-settings-guide.md)
- [TextTemplating Integration](./text-templating-overview.md)
- [Background Jobs Configuration](./communication-background-jobs.md)

## See Also

- [Sufi Platform Settings System](./settings.md)
- [Sufi Platform Background Jobs](./background-jobs.md)
- [Sufi Platform TextTemplating](./text-templating-overview.md)
