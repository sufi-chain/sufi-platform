# SbBadge

A small status indicator that displays a count or dot to draw attention to an element. Commonly used for notification counts, status indicators, or labeling items.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | int? | null | Numeric value to display in the badge |
| Max | int | 99 | Maximum value before showing "max+" (e.g., "99+") |
| Dot | bool | false | When true, shows as a small dot without value |
| Color | SbColor | Primary | Badge color theme |
| AriaLabel | string? | null | Accessible label for screen readers |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Custom content to display inside the badge (takes precedence over Value) |

### Template Usage

```razor
<SbBadge>
    Custom Content
</SbBadge>
```

## CSS Classes

- `sb-badge` - Base class
- `sb-badge--primary` - Primary color
- `sb-badge--secondary` - Secondary color
- `sb-badge--success` - Success color
- `sb-badge--warning` - Warning color
- `sb-badge--danger` - Danger color
- `sb-badge--dot` - Dot variant (no text)

## Accessibility

- Uses `aria-label` for screen reader description
- Content is visible to assistive technologies

## Examples

### Basic Badge with Value

```razor
<SbBadge Value="5" />
<SbBadge Value="42" />
<SbBadge Value="150" Max="99" /> @* Shows "99+" *@
```

### Different Colors

```razor
<SbBadge Value="3" Color="SbColor.Primary" />
<SbBadge Value="3" Color="SbColor.Success" />
<SbBadge Value="3" Color="SbColor.Warning" />
<SbBadge Value="3" Color="SbColor.Danger" />
```

### Dot Badge

```razor
<SbBadge Dot="true" Color="SbColor.Success" />
<SbBadge Dot="true" Color="SbColor.Danger" />
```

### With Custom Content

```razor
<SbBadge Color="SbColor.Primary">
    NEW
</SbBadge>

<SbBadge Color="SbColor.Warning">
    BETA
</SbBadge>
```

### Badge on Element

```razor
<div style="position: relative; display: inline-block;">
    <SbIcon Name="bell" />
    <SbBadge Value="3" 
             Color="SbColor.Danger"
             Style="position: absolute; top: -8px; right: -8px;" />
</div>
```

### With Accessibility

```razor
<SbBadge Value="5" AriaLabel="5 unread notifications" />
```
