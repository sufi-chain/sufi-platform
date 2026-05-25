# SufiAbp TextTemplating System

## Overview

The SufiAbp TextTemplating system provides a powerful, extensible framework for rendering dynamic text content using templates. It's fully integrated with the Messaging system and supports multiple rendering engines (Scriban by default).

## Key Features

- **Multiple Rendering Engines**: Pluggable architecture (Scriban included)
- **Template Definitions**: Define templates with metadata and localization
- **Virtual File System Integration**: Store templates in embedded resources
- **Localization Support**: Multi-language template content
- **Dynamic Content Providers**: Load templates from database, files, or custom sources
- **Caching**: Built-in template caching for performance

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  SufiChain.SufiAbp.TextTemplating (Core)                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ITemplateRenderer                                    │   │
│  │  - RenderAsync(templateName, model)                  │   │
│  └──────────────────────────────────────────────────────┘   │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ITemplateRenderingEngine (abstraction)               │   │
│  └──────────────────────────────────────────────────────┘   │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ITemplateDefinitionManager                           │   │
│  │  - GetAsync(templateName)                            │   │
│  └──────────────────────────────────────────────────────┘   │
│         ↓                                                    │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ITemplateContentProvider                             │   │
│  │  - GetContentOrNullAsync(templateDefinition)         │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
                            ↓
┌─────────────────────────────────────────────────────────────┐
│  SufiChain.SufiAbp.TextTemplating.Scriban                   │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ ScribanTemplateRenderingEngine                       │   │
│  │  - Implements ITemplateRenderingEngine               │   │
│  │  - Uses Scriban library for rendering               │   │
│  │  - Supports localization via ScribanTemplateLocalizer│   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

## Package Structure

### Core Package: `SufiChain.SufiAbp.TextTemplating`

**Dependencies:**
- `SufiChain.SufiAbp.Localization`
- `SufiChain.SufiAbp.VirtualFileSystem`

**Key Interfaces:**

1. **ITemplateRenderer** - Main entry point for rendering templates
2. **ITemplateRenderingEngine** - Abstraction for rendering engines (Scriban, Razor, etc.)
3. **ITemplateDefinitionManager** - Manages template definitions
4. **ITemplateDefinitionProvider** - Defines templates in code
5. **ITemplateContentProvider** - Provides template content from various sources
6. **ILocalizedTemplateContentReader** - Reads localized template content

### Scriban Package: `SufiChain.SufiAbp.TextTemplating.Scriban`

**Dependencies:**
- `SufiChain.SufiAbp.TextTemplating`
- `Scriban` (NuGet package)

**What's Included:**
- `ScribanTemplateRenderingEngine` - Scriban implementation
- `ScribanTemplateLocalizer` - Localization support for Scriban templates

## Quick Start

### 1. Define a Template

**MyTemplateDefinitionProvider.cs:**
```csharp
using SufiChain.SufiAbp.TextTemplating;

namespace MyApp.Templates;

public class MyTemplateDefinitionProvider : TemplateDefinitionProvider
{
    public override void Define(ITemplateDefinitionContext context)
    {
        context.Add(
            new TemplateDefinition(
                name: "WelcomeEmail",
                localizationResource: typeof(MyAppResource),
                layout: "EmailLayout",
                isInlineLocalized: false
            )
            .WithVirtualFilePath(
                "/Templates/WelcomeEmail.tpl",
                isInlineLocalized: false
            )
        );

        context.Add(
            new TemplateDefinition(
                name: "PasswordResetEmail",
                localizationResource: typeof(MyAppResource)
            )
            .WithVirtualFilePath("/Templates/PasswordResetEmail.tpl", isInlineLocalized: false)
        );

        // Layout template
        context.Add(
            new TemplateDefinition(
                name: "EmailLayout",
                isLayout: true
            )
            .WithVirtualFilePath("/Templates/Layouts/EmailLayout.tpl", isInlineLocalized: false)
        );
    }
}
```

### 2. Create Template Files

**Templates/WelcomeEmail.tpl:**
```scriban
Hello {{ user_name }},

Welcome to {{ app_name }}! We're excited to have you on board.

Your account has been successfully created with the following details:
- Email: {{ user_email }}
- Registration Date: {{ registration_date | date.to_string "%Y-%m-%d" }}

To get started, please click the link below:
{{ activation_link }}

Best regards,
The {{ app_name }} Team
```

**Templates/Layouts/EmailLayout.tpl:**
```scriban
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8">
    <title>{{ title }}</title>
    <style>
        body { font-family: Arial, sans-serif; line-height: 1.6; }
        .container { max-width: 600px; margin: 0 auto; padding: 20px; }
        .footer { margin-top: 30px; font-size: 12px; color: #666; }
    </style>
</head>
<body>
    <div class="container">
        {{ content }}
        <div class="footer">
            <p>This is an automated message. Please do not reply.</p>
            <p>&copy; {{ date.now.year }} {{ app_name }}. All rights reserved.</p>
        </div>
    </div>
</body>
</html>
```

### 3. Embed Templates in Project

**MyApp.csproj:**
```xml
<ItemGroup>
  <EmbeddedResource Include="Templates\**\*.tpl" />
</ItemGroup>
```

### 4. Configure Virtual File System

**MyAppModule.cs:**
```csharp
[DependsOn(
    typeof(SufiAbpTextTemplatingScribanModule)
)]
public class MyAppModule : SufiAbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<SufiAbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<MyAppModule>();
        });
    }
}
```

### 5. Render Templates

**UserRegistrationService.cs:**
```csharp
using SufiChain.SufiAbp.TextTemplating;

public class UserRegistrationService : SufiAbpApplicationService
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailSender _emailSender;

    public UserRegistrationService(
        ITemplateRenderer templateRenderer,
        IEmailSender emailSender)
    {
        _templateRenderer = templateRenderer;
        _emailSender = emailSender;
    }

    public async Task RegisterUserAsync(RegisterUserInput input)
    {
        // ... create user ...

        // Render welcome email from template
        var emailBody = await _templateRenderer.RenderAsync(
            "WelcomeEmail",
            new
            {
                user_name = input.Name,
                user_email = input.Email,
                app_name = "My Application",
                registration_date = DateTime.Now,
                activation_link = $"https://myapp.com/activate?token={activationToken}"
            }
        );

        // Send email
        await _emailSender.SendAsync(
            to: input.Email,
            subject: "Welcome to My Application",
            body: emailBody,
            isBodyHtml: true
        );
    }
}
```

## Advanced Features

### 1. Localized Templates

**Define localized template:**
```csharp
context.Add(
    new TemplateDefinition(
        name: "WelcomeEmail",
        localizationResource: typeof(MyAppResource)
    )
    .WithVirtualFilePath("/Templates/WelcomeEmail/{{culture}}.tpl", isInlineLocalized: false)
);
```

**Create culture-specific files:**
```
Templates/
  WelcomeEmail/
    en.tpl
    ar.tpl
    fr.tpl
```

**Templates/WelcomeEmail/en.tpl:**
```scriban
Hello {{ user_name }},
Welcome to {{ app_name }}!
```

**Templates/WelcomeEmail/ar.tpl:**
```scriban
مرحبا {{ user_name }}،
مرحبا بك في {{ app_name }}!
```

**Render with culture:**
```csharp
using (CultureHelper.Use("ar"))
{
    var emailBody = await _templateRenderer.RenderAsync(
        "WelcomeEmail",
        new { user_name = "أحمد", app_name = "تطبيقي" }
    );
}
```

### 2. Inline Localization

**Template with inline localization:**
```scriban
{{ L "WelcomeMessage" }}

{{ L "Greeting" user_name }}

{{ L "RegistrationDate" registration_date }}
```

**Localization resource:**
```json
{
  "Culture": "en",
  "Texts": {
    "WelcomeMessage": "Welcome to our platform!",
    "Greeting": "Hello {0}!",
    "RegistrationDate": "Registered on: {0:yyyy-MM-dd}"
  }
}
```

### 3. Template Layouts

**Define layout:**
```csharp
context.Add(
    new TemplateDefinition(
        name: "EmailLayout",
        isLayout: true
    )
    .WithVirtualFilePath("/Templates/Layouts/EmailLayout.tpl", isInlineLocalized: false)
);

context.Add(
    new TemplateDefinition(
        name: "WelcomeEmail",
        layout: "EmailLayout"  // Use layout
    )
    .WithVirtualFilePath("/Templates/WelcomeEmail.tpl", isInlineLocalized: false)
);
```

**Layout template (EmailLayout.tpl):**
```scriban
<!DOCTYPE html>
<html>
<head>
    <title>{{ title }}</title>
</head>
<body>
    <div class="header">
        <img src="{{ logo_url }}" alt="Logo">
    </div>
    
    {{ content }}  <!-- Child template content injected here -->
    
    <div class="footer">
        <p>&copy; {{ date.now.year }} {{ company_name }}</p>
    </div>
</body>
</html>
```

**Child template (WelcomeEmail.tpl):**
```scriban
<h1>Welcome {{ user_name }}!</h1>
<p>Thank you for joining us.</p>
```

### 4. Custom Template Content Providers

Load templates from database or external sources:

**DatabaseTemplateContentProvider.cs:**
```csharp
using SufiChain.SufiAbp.TextTemplating;
using SufiChain.SufiAbp.DependencyInjection;

public class DatabaseTemplateContentProvider : ITemplateContentProvider, ITransientDependency
{
    private readonly ITemplateRepository _templateRepository;

    public DatabaseTemplateContentProvider(ITemplateRepository templateRepository)
    {
        _templateRepository = templateRepository;
    }

    public async Task<string> GetContentOrNullAsync(TemplateDefinition templateDefinition, string cultureName = null)
    {
        var template = await _templateRepository.FindByNameAsync(templateDefinition.Name, cultureName);
        return template?.Content;
    }
}
```

**Register provider:**
```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<SufiAbpTextTemplatingOptions>(options =>
    {
        options.ContentProviders.Add<DatabaseTemplateContentProvider>();
    });
}
```

## Scriban Syntax Reference

### Variables
```scriban
{{ variable_name }}
{{ object.property }}
{{ array[0] }}
```

### Conditionals
```scriban
{{ if user_is_premium }}
  Premium content here
{{ else }}
  Standard content here
{{ end }}
```

### Loops
```scriban
{{ for item in items }}
  - {{ item.name }}: {{ item.price }}
{{ end }}
```

### Filters
```scriban
{{ user_name | string.upcase }}
{{ price | math.format "0.00" }}
{{ date | date.to_string "%Y-%m-%d" }}
```

### Functions
```scriban
{{ string.capitalize user_name }}
{{ math.round price 2 }}
{{ date.now }}
```

### Localization (SufiAbp Extension)
```scriban
{{ L "LocalizationKey" }}
{{ L "LocalizationKeyWithParam" param1 param2 }}
```

## Integration with Messaging

The TextTemplating system is fully integrated with the Messaging system:

```csharp
public class NotificationService : SufiAbpApplicationService
{
    private readonly ITemplateRenderer _templateRenderer;
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;

    public async Task SendOrderConfirmationAsync(Order order)
    {
        // Render email template
        var emailBody = await _templateRenderer.RenderAsync(
            "OrderConfirmationEmail",
            new { order_number = order.Number, total = order.Total }
        );

        await _emailSender.SendAsync(
            to: order.CustomerEmail,
            subject: "Order Confirmation",
            body: emailBody,
            isBodyHtml: true
        );

        // Render SMS template
        var smsMessage = await _templateRenderer.RenderAsync(
            "OrderConfirmationSms",
            new { order_number = order.Number }
        );

        await _smsSender.SendAsync(
            phoneNumber: order.CustomerPhone,
            message: smsMessage
        );
    }
}
```

## Best Practices

1. **Use Layouts**: Define common layouts for consistent styling
2. **Localize Templates**: Support multiple languages from the start
3. **Embed Templates**: Use embedded resources for deployment simplicity
4. **Cache Templates**: Templates are cached automatically by the framework
5. **Test Templates**: Write unit tests for template rendering
6. **Version Templates**: Use version control for template changes
7. **Validate Models**: Ensure template models have all required properties

## Performance Considerations

- Templates are cached after first render
- Use `isInlineLocalized: false` for better performance when not using inline localization
- Consider database-backed templates only for user-editable content
- Scriban is highly optimized and suitable for production use

## Next Steps

- [Messaging System Overview](./messaging-overview.md)
- [Creating Custom Rendering Engines](./text-templating-custom-engines.md)
- [Template Testing Guide](./text-templating-testing.md)

## See Also

- [Scriban Documentation](https://github.com/scriban/scriban)
- [SufiAbp Localization](./localization.md)
- [SufiAbp Virtual File System](./virtual-file-system.md)
