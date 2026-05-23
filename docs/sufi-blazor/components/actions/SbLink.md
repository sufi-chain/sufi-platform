# SbLink

Styled anchor element for navigation with support for external links, underline styles, and colors.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Href | string? | null | The URL to navigate to |
| Target | string? | null | Link target (_blank, _self, etc.) |
| Rel | string? | null | Custom rel attribute |
| Disabled | bool | false | Whether the link is disabled |
| External | bool | false | Shows external link indicator icon |
| Color | SbColor | Primary | Link color (Default, Primary, Secondary, Success, Warning, Danger, Info, Muted) |
| Underline | SbLinkUnderline | Hover | Underline behavior (Always, Hover, None) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes (e.g. aria-*, data-*) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClick | EventCallback<MouseEventArgs> | Fired when the link is clicked |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | The link text or content |

### Template Usage Examples

#### Basic Link

```razor
<SbLink Href="/about">About Us</SbLink>
```

#### With Custom Content

```razor
<SbLink Href="/profile">
    <SbStack Direction="SbStackDirection.Row" Gap="1" Align="SbAlign.Center">
        <SbIcon Name="user" Size="SbSize.Sm" />
        <span>View Profile</span>
    </SbStack>
</SbLink>
```

## CSS Classes

- `sb-link` - Base class
- `sb-link--primary` - Primary color
- `sb-link--secondary` - Secondary color
- `sb-link--success` - Success color
- `sb-link--warning` - Warning color
- `sb-link--danger` - Danger color
- `sb-link--info` - Info color
- `sb-link--muted` - Muted color
- `sb-link--default` - Default color
- `sb-link--underline-always` - Always show underline
- `sb-link--underline-hover` - Underline on hover
- `sb-link--underline-none` - No underline
- `sb-link--disabled` - Disabled state
- `sb-link--external` - External link styling

## Accessibility

- Uses native `<a>` element
- `aria-disabled` set when disabled
- `tabindex="-1"` when disabled to remove from tab order
- `rel="noopener noreferrer"` automatically added for external links and `target="_blank"`
- External link indicator is hidden from screen readers

## Examples

### Basic Usage

```razor
<SbLink Href="/home">Go Home</SbLink>
```

### External Link

```razor
<SbLink Href="https://github.com" External="true" Target="_blank">
    GitHub
</SbLink>
```

### Colors

```razor
<SbStack Direction="SbStackDirection.Row" Gap="4">
    <SbLink Href="#" Color="SbColor.Primary">Primary</SbLink>
    <SbLink Href="#" Color="SbColor.Secondary">Secondary</SbLink>
    <SbLink Href="#" Color="SbColor.Success">Success</SbLink>
    <SbLink Href="#" Color="SbColor.Danger">Danger</SbLink>
    <SbLink Href="#" Color="SbColor.Muted">Muted</SbLink>
</SbStack>
```

### Underline Styles

```razor
<SbStack Direction="SbStackDirection.Row" Gap="4">
    <SbLink Href="#" Underline="SbLinkUnderline.Always">Always Underlined</SbLink>
    <SbLink Href="#" Underline="SbLinkUnderline.Hover">Underline on Hover</SbLink>
    <SbLink Href="#" Underline="SbLinkUnderline.None">No Underline</SbLink>
</SbStack>
```

### Disabled Link

```razor
<SbLink Href="/admin" Disabled="true">
    Admin (Restricted)
</SbLink>
```

### In Text

```razor
<SbText>
    For more information, please visit our 
    <SbLink Href="/help">help center</SbLink> or 
    <SbLink Href="/contact">contact us</SbLink>.
</SbText>
```

### With Click Handler

```razor
<SbLink Href="#" OnClick="HandleLinkClick">
    Click me to track
</SbLink>

@code {
    private void HandleLinkClick(MouseEventArgs args)
    {
        // Track click analytics
    }
}
```
