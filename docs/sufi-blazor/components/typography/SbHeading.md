# SbHeading

Semantic heading component that renders h1-h6 elements with consistent typography styles. Supports visual size override for flexible design while maintaining proper document structure.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Level | int | 2 | Semantic heading level (1-6), determines the HTML tag |
| Size | int? | null | Visual size override (1-6). If not set, matches Level |
| Color | SbColor | Default | Text color |
| Align | SbTextAlign | Start | Text alignment (Start, Center, End) |
| Truncate | bool | false | Truncates text with ellipsis on overflow |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Heading text content |

## CSS Classes

- `sb-heading` - Base class
- `sb-heading--h1` through `sb-heading--h6` - Size variants
- `sb-heading--primary` - Primary color
- `sb-heading--secondary` - Secondary color
- `sb-heading--success` - Success color
- `sb-heading--warning` - Warning color
- `sb-heading--danger` - Danger color
- `sb-heading--align-center` - Center aligned
- `sb-heading--align-end` - End aligned
- `sb-heading--truncate` - Text truncation

## Rendered Element

Renders the appropriate heading tag based on `Level`:
- Level 1 → `<h1>`
- Level 2 → `<h2>`
- Level 3 → `<h3>`
- Level 4 → `<h4>`
- Level 5 → `<h5>`
- Level 6 → `<h6>`

## Examples

### Basic Headings

```razor
<SbHeading Level="1">Heading 1</SbHeading>
<SbHeading Level="2">Heading 2</SbHeading>
<SbHeading Level="3">Heading 3</SbHeading>
<SbHeading Level="4">Heading 4</SbHeading>
<SbHeading Level="5">Heading 5</SbHeading>
<SbHeading Level="6">Heading 6</SbHeading>
```

### Visual Size Override

Use `Size` to visually style a heading differently from its semantic level. This is useful for maintaining proper document structure while achieving desired visual hierarchy.

```razor
@* Semantically h2, but visually styled as h4 *@
<SbHeading Level="2" Size="4">Section Title</SbHeading>

@* Semantically h3, but visually styled as h1 *@
<SbHeading Level="3" Size="1">Important Subsection</SbHeading>
```

### Colors

```razor
<SbHeading Level="3" Color="SbColor.Primary">Primary Heading</SbHeading>
<SbHeading Level="3" Color="SbColor.Secondary">Secondary Heading</SbHeading>
<SbHeading Level="3" Color="SbColor.Success">Success Heading</SbHeading>
<SbHeading Level="3" Color="SbColor.Warning">Warning Heading</SbHeading>
<SbHeading Level="3" Color="SbColor.Danger">Danger Heading</SbHeading>
```

### Text Alignment

```razor
<SbHeading Level="3" Align="SbTextAlign.Start">Left Aligned</SbHeading>
<SbHeading Level="3" Align="SbTextAlign.Center">Center Aligned</SbHeading>
<SbHeading Level="3" Align="SbTextAlign.End">Right Aligned</SbHeading>
```

### Text Truncation

```razor
<div style="width: 200px;">
    <SbHeading Level="4" Truncate="true">
        This is a very long heading that will be truncated
    </SbHeading>
</div>
```

### Page Header

```razor
<header>
    <SbHeading Level="1">Dashboard</SbHeading>
    <SbText Variant="SbTextVariant.BodySmall" Color="SbColor.Secondary">
        Welcome back, John
    </SbText>
</header>
```

### Card Header

```razor
<SbCard>
    <Header>
        <SbHeading Level="3" Size="4">Card Title</SbHeading>
    </Header>
    <ChildContent>
        <p>Card content goes here.</p>
    </ChildContent>
</SbCard>
```

### Section with Subheading

```razor
<section>
    <SbHeading Level="2">Main Section</SbHeading>
    <SbText Color="SbColor.Secondary">
        Introduction paragraph for this section.
    </SbText>
    
    <SbHeading Level="3">Subsection A</SbHeading>
    <p>Content for subsection A...</p>
    
    <SbHeading Level="3">Subsection B</SbHeading>
    <p>Content for subsection B...</p>
</section>
```

### Hero Section

```razor
<div class="hero" style="text-align: center;">
    <SbHeading Level="1" Align="SbTextAlign.Center">
        Build Better Apps
    </SbHeading>
    <SbText Variant="SbTextVariant.Body" Align="SbTextAlign.Center">
        A comprehensive component library for Blazor applications.
    </SbText>
    <SbButton Size="SbSize.Lg">Get Started</SbButton>
</div>
```

### Accessibility Best Practices

```razor
@* Maintain proper heading hierarchy for accessibility *@
<article>
    <SbHeading Level="1">Article Title</SbHeading>
    
    <section>
        <SbHeading Level="2">First Section</SbHeading>
        <p>Content...</p>
        
        <SbHeading Level="3">Subsection</SbHeading>
        <p>More content...</p>
    </section>
    
    <section>
        <SbHeading Level="2">Second Section</SbHeading>
        <p>Content...</p>
    </section>
</article>
```
