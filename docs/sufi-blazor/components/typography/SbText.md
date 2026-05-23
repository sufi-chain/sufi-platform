# SbText

A versatile typography component for rendering text with various styles and semantic elements. Supports multiple variants, colors, and alignment options.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Variant | SbTextVariant | Body | Typography style variant |
| Tag | string? | null | Override the semantic HTML tag |
| Color | SbColor | Default | Text color |
| Align | SbTextAlign | Start | Text alignment (Start, Center, End) |
| Truncate | bool | false | Truncates text with ellipsis on overflow |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Text Variants

| Variant | Default Tag | Description |
|---------|-------------|-------------|
| Body | `<p>` | Standard body text |
| BodySmall | `<p>` | Smaller body text |
| Caption | `<span>` | Small caption text |
| Overline | `<span>` | Small uppercase text |
| Code | `<code>` | Monospace code text |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Text content |

## CSS Classes

- `sb-text` - Base class
- `sb-text--body` - Body variant
- `sb-text--bodysmall` - Small body variant
- `sb-text--caption` - Caption variant
- `sb-text--overline` - Overline variant
- `sb-text--code` - Code variant
- `sb-text--primary` - Primary color
- `sb-text--secondary` - Secondary color
- `sb-text--success` - Success color
- `sb-text--warning` - Warning color
- `sb-text--danger` - Danger color
- `sb-text--align-center` - Center aligned
- `sb-text--align-end` - End aligned
- `sb-text--truncate` - Text truncation

## Examples

### Basic Variants

```razor
<SbText Variant="SbTextVariant.Body">
    This is regular body text for paragraphs and general content.
</SbText>

<SbText Variant="SbTextVariant.BodySmall">
    This is smaller body text for secondary information.
</SbText>

<SbText Variant="SbTextVariant.Caption">
    Caption text for labels and small annotations
</SbText>

<SbText Variant="SbTextVariant.Overline">
    OVERLINE TEXT
</SbText>

<SbText Variant="SbTextVariant.Code">
    const value = "code";
</SbText>
```

### Colors

```razor
<SbText Color="SbColor.Primary">Primary colored text</SbText>
<SbText Color="SbColor.Secondary">Secondary colored text</SbText>
<SbText Color="SbColor.Success">Success colored text</SbText>
<SbText Color="SbColor.Warning">Warning colored text</SbText>
<SbText Color="SbColor.Danger">Danger colored text</SbText>
```

### Text Alignment

```razor
<SbText Align="SbTextAlign.Start">Left aligned text</SbText>
<SbText Align="SbTextAlign.Center">Center aligned text</SbText>
<SbText Align="SbTextAlign.End">Right aligned text</SbText>
```

### Custom HTML Tag

```razor
@* Render as a <div> instead of default tag *@
<SbText Tag="div" Variant="SbTextVariant.Body">
    Content in a div element
</SbText>

@* Render as a <label> *@
<SbText Tag="label" Variant="SbTextVariant.Caption">
    Form field label
</SbText>

@* Render as a <small> *@
<SbText Tag="small" Variant="SbTextVariant.BodySmall">
    Fine print text
</SbText>
```

### Text Truncation

```razor
<div style="width: 200px;">
    <SbText Truncate="true">
        This is a very long text that will be truncated with an ellipsis when it exceeds the container width.
    </SbText>
</div>
```

### Article Content

```razor
<article>
    <SbHeading Level="1">Article Title</SbHeading>
    
    <SbText Variant="SbTextVariant.Overline" Color="SbColor.Primary">
        CATEGORY
    </SbText>
    
    <SbText Variant="SbTextVariant.Caption" Color="SbColor.Secondary">
        Published on January 15, 2024 · 5 min read
    </SbText>
    
    <SbText Variant="SbTextVariant.Body">
        Main article content goes here. This is the primary body text
        that makes up the bulk of the article.
    </SbText>
    
    <SbText Variant="SbTextVariant.BodySmall" Color="SbColor.Secondary">
        Additional notes or less important information.
    </SbText>
</article>
```

### Form Field with Labels

```razor
<div class="form-field">
    <SbText Tag="label" Variant="SbTextVariant.Caption">
        Email Address
    </SbText>
    <SbTextField @bind-Value="email" />
    <SbText Variant="SbTextVariant.BodySmall" Color="SbColor.Secondary">
        We'll never share your email with anyone else.
    </SbText>
</div>
```

### Code Snippet

```razor
<SbText Variant="SbTextVariant.Body">
    To install the package, run:
</SbText>
<SbText Variant="SbTextVariant.Code">
    dotnet add package SufiChain.SufiBlazor
</SbText>
```

### Card with Typography

```razor
<SbCard>
    <Header>
        <SbText Variant="SbTextVariant.Overline" Color="SbColor.Primary">
            FEATURED
        </SbText>
        <SbHeading Level="3">Product Name</SbHeading>
    </Header>
    <ChildContent>
        <SbText Variant="SbTextVariant.Body">
            Product description with all the important details about this item.
        </SbText>
        <SbText Variant="SbTextVariant.Caption" Color="SbColor.Secondary">
            SKU: PRD-12345
        </SbText>
    </ChildContent>
</SbCard>
```

### Status Message

```razor
<SbText Color="SbColor.Success">
    <SbIcon Name="check-circle" Size="SbSize.Sm" /> Operation completed successfully!
</SbText>

<SbText Color="SbColor.Danger">
    <SbIcon Name="alert-circle" Size="SbSize.Sm" /> An error occurred. Please try again.
</SbText>
```
