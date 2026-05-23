# SbSlugEditor

A specialized input for editing URL slugs with auto-generation, validation, and preview capabilities.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | string? | null | The slug value (two-way bindable) |
| ValueChanged | EventCallback\<string?\> | - | Callback when value changes |
| BaseUrl | string? | null | Base URL to display before the slug |
| SourceText | string? | null | Source text to generate slug from (e.g., title) |
| ShowGenerateButton | bool | true | Whether to show generate button |
| ShowPreview | bool | true | Whether to show URL preview |
| AutoGenerate | bool | false | Whether to auto-generate when source changes |
| MinLength | int | 1 | Minimum slug length |
| MaxLength | int | 100 | Maximum slug length |
| ReservedSlugs | string[] | empty | Slugs that cannot be used |
| CheckAvailability | Func\<string, Task\<bool\>\>? | null | Async function to check if slug is available |
| Placeholder | string | "enter-slug-here" | Placeholder text |
| Disabled | bool | false | Whether the editor is disabled |
| ReadOnly | bool | false | Whether the editor is read-only |
| Error | bool | false | Whether there's a validation error |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<string?\> | Fired when the slug value changes |

## CSS Classes

- `sb-slug-editor` - Base class
- `sb-slug-editor__input-row` - Input row container
- `sb-slug-editor__base-url` - Base URL display
- `sb-slug-editor__input-wrapper` - Input wrapper
- `sb-slug-editor__input` - Text input
- `sb-slug-editor__preview` - Preview section
- `sb-slug-editor__preview-label` - Preview label
- `sb-slug-editor__preview-url` - Full URL preview
- `sb-slug-editor__error` - Error message
- `sb-slug-editor__generate` - Generate button
- `sb-slug-editor--error` - Error state

## Slug Generation

The component automatically:
- Converts to lowercase
- Replaces spaces with hyphens
- Removes special characters
- Removes consecutive hyphens
- Trims hyphens from ends

## Examples

### Basic Usage

```razor
<SbSlugEditor @bind-Value="slug" />
```

### With Base URL

```razor
<SbSlugEditor @bind-Value="pageSlug" 
              BaseUrl="https://example.com/pages/" />
```

### Generate from Title

```razor
<SbTextField @bind-Value="articleTitle" Label="Title" />
<SbSlugEditor @bind-Value="articleSlug" 
              SourceText="@articleTitle"
              BaseUrl="https://blog.example.com/" />
```

### Auto-Generate Mode

```razor
<SbTextField @bind-Value="productName" Label="Product Name" />
<SbSlugEditor @bind-Value="productSlug" 
              SourceText="@productName"
              AutoGenerate="true"
              BaseUrl="/products/" />
```

### With Availability Check

```razor
<SbSlugEditor @bind-Value="username" 
              BaseUrl="@("https://example.com/u/")"
              CheckAvailability="CheckUsernameAvailable"
              Placeholder="your-username" />

@code {
    private async Task<bool> CheckUsernameAvailable(string slug)
    {
        return await UserService.IsUsernameAvailableAsync(slug);
    }
}
```

### With Reserved Slugs

```razor
<SbSlugEditor @bind-Value="pageSlug" 
              BaseUrl="/pages/"
              ReservedSlugs="@(new[] { "admin", "api", "login", "register" })" />
```

### With Length Constraints

```razor
<SbSlugEditor @bind-Value="shortCode" 
              MinLength="3"
              MaxLength="20"
              Placeholder="short-code" />
```
