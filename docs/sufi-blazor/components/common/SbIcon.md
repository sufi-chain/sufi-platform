# SbIcon

Renders SVG icons from the Sufi Icons library with support for sizing, coloring, and accessibility features. Can also render custom SVG content.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Name | string? | null | Icon name from the Sufi Icons library (e.g., "home" or "si-home") |
| Size | SbSize | Md | Icon size (Sm, Md, Lg, Xl) |
| Color | SbColor? | null | Icon color. When null, inherits from parent via currentColor |
| AriaLabel | string? | null | Accessible label. When set, icon becomes semantic (role="img") |
| Mirror | bool | false | Mirrors icon in RTL layouts (for directional icons) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| CustomContent | RenderFragment? | Custom SVG or content (takes precedence over Name) |

### Template Usage

```razor
<SbIcon>
    <CustomContent>
        <svg viewBox="0 0 24 24">
            <!-- Custom SVG paths -->
        </svg>
    </CustomContent>
</SbIcon>
```

## CSS Classes

- `sb-icon` - Base class
- `sb-icon--sm` - Small size
- `sb-icon--md` - Medium size
- `sb-icon--lg` - Large size
- `sb-icon--xl` - Extra large size
- `sb-icon--primary` - Primary color
- `sb-icon--secondary` - Secondary color
- `sb-icon--success` - Success color
- `sb-icon--warning` - Warning color
- `sb-icon--danger` - Danger color
- `sb-icon--mirror` - Mirrored for RTL

## Accessibility

- **Decorative icons** (no `AriaLabel`): Uses `aria-hidden="true"` to hide from screen readers
- **Semantic icons** (with `AriaLabel`): Uses `role="img"` and `aria-label` for screen readers

## Examples

### Basic Icons

```razor
<SbIcon Name="home" />
<SbIcon Name="user" />
<SbIcon Name="settings" />
<SbIcon Name="search" />
<SbIcon Name="menu" />
```

### Sizes

```razor
<SbIcon Name="star" Size="SbSize.Sm" />
<SbIcon Name="star" Size="SbSize.Md" />
<SbIcon Name="star" Size="SbSize.Lg" />
<SbIcon Name="star" Size="SbSize.Xl" />
```

### Colors

```razor
<SbIcon Name="circle" Color="SbColor.Primary" />
<SbIcon Name="circle" Color="SbColor.Secondary" />
<SbIcon Name="circle" Color="SbColor.Success" />
<SbIcon Name="circle" Color="SbColor.Warning" />
<SbIcon Name="circle" Color="SbColor.Danger" />
```

### Inheriting Color

```razor
@* Icon inherits the parent's text color *@
<span style="color: purple;">
    <SbIcon Name="heart" /> Favorite
</span>

<SbButton Color="SbColor.Primary">
    <SbIcon Name="save" /> Save
</SbButton>
```

### Accessible Icons

```razor
@* Decorative icon (hidden from screen readers) *@
<SbButton>
    <SbIcon Name="save" /> Save Document
</SbButton>

@* Semantic icon (announced by screen readers) *@
<SbIcon Name="warning" AriaLabel="Warning" />

@* Icon-only button with accessible label *@
<SbIconButton Icon="close" AriaLabel="Close dialog" />
```

### RTL Mirror

```razor
@* Arrow icons that should flip in RTL layouts *@
<SbIcon Name="arrow-right" Mirror="true" />
<SbIcon Name="chevron-left" Mirror="true" />
```

### Custom SVG Content

```razor
<SbIcon Size="SbSize.Lg">
    <CustomContent>
        <svg viewBox="0 0 24 24" fill="currentColor">
            <path d="M12 2L2 7v10l10 5 10-5V7L12 2z"/>
        </svg>
    </CustomContent>
</SbIcon>
```

### In Buttons

```razor
<SbButton>
    <SbIcon Name="plus" Size="SbSize.Sm" /> Add Item
</SbButton>

<SbButton Variant="SbButtonVariant.Outlined">
    <SbIcon Name="download" Size="SbSize.Sm" /> Download
</SbButton>

<SbIconButton Icon="edit" />
```

### In Navigation

```razor
<SbNavMenu>
    <SbNavItem Href="/dashboard">
        <IconTemplate>
            <SbIcon Name="home" />
        </IconTemplate>
        Dashboard
    </SbNavItem>
    <SbNavItem Href="/users">
        <IconTemplate>
            <SbIcon Name="users" />
        </IconTemplate>
        Users
    </SbNavItem>
</SbNavMenu>
```

### Status Indicators

```razor
<span>
    <SbIcon Name="check-circle" Color="SbColor.Success" /> Active
</span>

<span>
    <SbIcon Name="clock" Color="SbColor.Warning" /> Pending
</span>

<span>
    <SbIcon Name="x-circle" Color="SbColor.Danger" /> Failed
</span>
```

### Loading State

```razor
<SbIcon Name="loader" Class="spin" />

<style>
    .spin {
        animation: spin 1s linear infinite;
    }
    @@keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
</style>
```

### Icon List

```razor
<SbStack Direction="SbDirection.Row" Gap="SbSpacing.Sm">
    <SbIcon Name="facebook" Size="SbSize.Lg" />
    <SbIcon Name="twitter" Size="SbSize.Lg" />
    <SbIcon Name="linkedin" Size="SbSize.Lg" />
    <SbIcon Name="github" Size="SbSize.Lg" />
</SbStack>
```
