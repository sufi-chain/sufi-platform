# SbSpacer

A utility component for adding vertical or horizontal spacing between elements.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Size | SbSpacerSize | Medium | Size preset for the spacer |
| Height | string? | null | Custom height (overrides Size) |
| Width | string? | null | Custom width (for horizontal spacing) |
| Grow | bool | false | Whether the spacer should grow to fill available space |
| Class | string? | null | Additional CSS classes |

## SbSpacerSize Enum

```csharp
public enum SbSpacerSize
{
    ExtraSmall,  // 0.25rem (4px)
    Small,       // 0.5rem (8px)
    Medium,      // 1rem (16px)
    Large,       // 1.5rem (24px)
    ExtraLarge   // 2rem (32px)
}
```

## CSS Classes

- `sb-spacer` - Base class

## Accessibility

- Uses `aria-hidden="true"` since it's a visual-only element

## Examples

### Basic Usage

```razor
<SbHeading Level="1">Title</SbHeading>
<SbSpacer />
<SbText>Some content with default spacing above.</SbText>
```

### Different Sizes

```razor
<div>Content 1</div>
<SbSpacer Size="SbSpacerSize.ExtraSmall" />
<div>Content 2 (4px gap)</div>

<div>Content 3</div>
<SbSpacer Size="SbSpacerSize.Small" />
<div>Content 4 (8px gap)</div>

<div>Content 5</div>
<SbSpacer Size="SbSpacerSize.Large" />
<div>Content 6 (24px gap)</div>

<div>Content 7</div>
<SbSpacer Size="SbSpacerSize.ExtraLarge" />
<div>Content 8 (32px gap)</div>
```

### Custom Height

```razor
<SbCard>First Card</SbCard>
<SbSpacer Height="3rem" />
<SbCard>Second Card with custom 3rem gap</SbCard>
```

### Horizontal Spacing

```razor
<SbStack Direction="SbStackDirection.Row">
    <SbButton>Button 1</SbButton>
    <SbSpacer Width="2rem" />
    <SbButton>Button 2</SbButton>
</SbStack>
```

### Growing Spacer (Push to End)

```razor
<SbStack Direction="SbStackDirection.Row" Style="width: 100%;">
    <SbText>Left Content</SbText>
    <SbSpacer Grow="true" />
    <SbButton>Right Button</SbButton>
</SbStack>
```

### In Navigation Menu

Use `SbSpacer Grow="true"` inside a vertical stack to push items to the bottom (e.g. in KomTheme's `KomSidebar` or `KomIconRail`):

```razor
<SbStack Direction="SbStackDirection.Column" Gap="0">
    <SbNavMenu>
        <SbNavItem Href="/" Icon="home">Dashboard</SbNavItem>
        <SbNavItem Href="/analytics" Icon="bar-chart">Analytics</SbNavItem>
    </SbNavMenu>
    <SbSpacer Grow="true" />
    <SbNavMenu>
        <SbNavItem Href="/settings" Icon="settings">Settings</SbNavItem>
        <SbNavItem Href="/help" Icon="help-circle">Help</SbNavItem>
    </SbNavMenu>
</SbStack>
```

### Section Dividers

```razor
<SbStack Gap="2">
    <SbHeading Level="2">Section 1</SbHeading>
    <SbText>Content for section 1...</SbText>
    
    <SbSpacer Size="SbSpacerSize.Large" />
    
    <SbHeading Level="2">Section 2</SbHeading>
    <SbText>Content for section 2...</SbText>
    
    <SbSpacer Size="SbSpacerSize.Large" />
    
    <SbHeading Level="2">Section 3</SbHeading>
    <SbText>Content for section 3...</SbText>
</SbStack>
```

### Footer Push Down

```razor
<SbStack Style="min-height: 100vh;">
    <header>Header</header>
    <main>Main Content</main>
    <SbSpacer Grow="true" />
    <footer>Footer pushed to bottom</footer>
</SbStack>
```
