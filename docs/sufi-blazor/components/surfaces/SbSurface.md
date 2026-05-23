# SbSurface

A low-level container component for creating elevated or outlined surfaces with customizable styling. Useful as a building block for custom components or when you need more control than SbCard provides.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Elevation | int | 1 | Shadow depth level (0-5) |
| Outlined | bool | false | Adds a border instead of relying solely on elevation |
| Background | string? | null | Custom background color (CSS value) |
| BorderRadius | string? | null | Custom border radius (CSS value) |
| Padding | string? | null | Custom padding (CSS value) |
| Class | string? | null | Additional CSS classes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Content to render inside the surface |

## CSS Classes

- `sb-surface` - Base class
- `sb-surface--elevation-0` through `sb-surface--elevation-5` - Shadow levels

## Examples

### Basic Surface

```razor
<SbSurface>
    <p>Content inside a surface</p>
</SbSurface>
```

### Elevation Levels

```razor
<SbSurface Elevation="0">Flat (no shadow)</SbSurface>
<SbSurface Elevation="1">Subtle shadow</SbSurface>
<SbSurface Elevation="2">Light shadow</SbSurface>
<SbSurface Elevation="3">Medium shadow</SbSurface>
<SbSurface Elevation="4">Strong shadow</SbSurface>
<SbSurface Elevation="5">Heavy shadow</SbSurface>
```

### Outlined Surface

```razor
<SbSurface Outlined="true" Elevation="0">
    <p>This surface has a border.</p>
</SbSurface>
```

### Custom Background

```razor
<SbSurface Background="#f5f5f5" Padding="24px">
    <p>Gray background surface</p>
</SbSurface>

<SbSurface Background="var(--sb-primary-light)" Padding="24px">
    <p>Using CSS variable for background</p>
</SbSurface>
```

### Custom Border Radius

```razor
<SbSurface BorderRadius="16px" Padding="24px">
    <p>Rounded corners</p>
</SbSurface>

<SbSurface BorderRadius="50%" Padding="48px">
    <p>Circle</p>
</SbSurface>
```

### Custom Padding

```razor
<SbSurface Padding="8px">Compact padding</SbSurface>
<SbSurface Padding="16px">Default-like padding</SbSurface>
<SbSurface Padding="32px">Spacious padding</SbSurface>
<SbSurface Padding="16px 32px">Horizontal emphasis</SbSurface>
```

### Combined Styling

```razor
<SbSurface 
    Elevation="2"
    Background="#ffffff"
    BorderRadius="12px"
    Padding="24px">
    <SbHeading Level="4">Custom Surface</SbHeading>
    <p>A fully customized surface component.</p>
</SbSurface>
```

### Building Custom Components

```razor
@* Custom tooltip-like element *@
<SbSurface 
    Elevation="3"
    Background="#333"
    BorderRadius="4px"
    Padding="8px 12px"
    Class="custom-tooltip">
    <SbText Color="SbColor.Light">Tooltip content</SbText>
</SbSurface>

@* Custom floating action panel *@
<SbSurface 
    Elevation="4"
    BorderRadius="24px"
    Padding="16px"
    Class="floating-panel">
    <SbStack Direction="SbDirection.Row" Gap="SbSpacing.Sm">
        <SbIconButton Icon="edit" />
        <SbIconButton Icon="share" />
        <SbIconButton Icon="delete" />
    </SbStack>
</SbSurface>
```

### Nested Surfaces

```razor
<SbSurface Elevation="1" Padding="24px">
    <SbHeading Level="4">Outer Surface</SbHeading>
    
    <SbSurface Elevation="2" Padding="16px" Background="#f9f9f9">
        <p>Nested inner surface with different elevation</p>
    </SbSurface>
</SbSurface>
```

### Dashboard Widget

```razor
<SbSurface 
    Elevation="1"
    BorderRadius="8px"
    Padding="20px"
    Class="dashboard-widget">
    <div class="widget-header">
        <SbText Variant="SbTextVariant.Caption">Monthly Sales</SbText>
        <SbIconButton Icon="more-vertical" Size="SbSize.Sm" />
    </div>
    <SbHeading Level="2">$12,450</SbHeading>
    <SbProgress Value="75" ShowLabel="true" />
</SbSurface>
```

### Comparison with SbCard

Use `SbSurface` when you need:
- More control over styling (background, radius, padding)
- A simpler container without header/footer structure
- Building blocks for custom components

Use `SbCard` when you need:
- Structured header/body/footer sections
- Clickable or linkable cards
- Standard card patterns
