# SbIconButton

Icon-only button for toolbar actions, close buttons, and other compact interactions. Use **AriaLabel** (or **Title**) for accessibility when there is no visible text.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Icon | RenderFragment? | null | Icon content when set explicitly (e.g. `Icon="@(...)"`) |
| ChildContent | RenderFragment? | null | Icon content when placed between tags. At least one of Icon or ChildContent should be set. |
| AriaLabel | string? | null | Accessible label for screen readers (recommended for icon-only buttons) |
| Variant | SbButtonVariant | Ghost | Button style variant (Solid, Outline, Ghost, Link) |
| Size | SbSize | Md | Button size (Xs, Sm, Md, Lg, Xl) |
| Color | SbColor | Default | Color intent (Default, Primary, Secondary, Success, Warning, Danger, Info, Muted) |
| Disabled | bool | false | Whether the button is disabled |
| Loading | bool | false | Whether the button shows a loading spinner |
| Type | string | "button" | HTML button type (button, submit, reset) |
| Title | string? | null | Tooltip text |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes (e.g. aria-*, data-*) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClick | EventCallback<MouseEventArgs> | Fired when the button is clicked |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment? | Icon when set explicitly (e.g. `Icon="@(...)"`) |
| ChildContent | RenderFragment? | Icon when placed between tags (default slot). Use either Icon or ChildContent. |

### Template Usage Examples

#### Using ChildContent (content between tags)

```razor
<SbIconButton AriaLabel="Close">
    <SbIcon Name="x" />
</SbIconButton>
```

#### Using Icon slot

```razor
<SbIconButton AriaLabel="Settings" Title="Open Settings">
    <Icon><SbIcon Name="settings" /></Icon>
</SbIconButton>
```

## CSS Classes

- `sb-icon-button` - Base class
- `sb-icon-button--solid` - Solid variant
- `sb-icon-button--outline` - Outline variant
- `sb-icon-button--ghost` - Ghost variant
- `sb-icon-button--link` - Link variant
- `sb-icon-button--xs` - Extra small size
- `sb-icon-button--sm` - Small size
- `sb-icon-button--md` - Medium size
- `sb-icon-button--lg` - Large size
- `sb-icon-button--xl` - Extra large size
- `sb-icon-button--primary` - Primary color
- `sb-icon-button--secondary` - Secondary color
- `sb-icon-button--success` - Success color
- `sb-icon-button--warning` - Warning color
- `sb-icon-button--danger` - Danger color
- `sb-icon-button--info` - Info color
- `sb-icon-button--muted` - Muted color
- `sb-icon-button--disabled` - Disabled state
- `sb-icon-button--loading` - Loading state

## Accessibility

- Uses native `<button>` element
- **AriaLabel** (or **Title**) is recommended for icon-only buttons so screen readers have a label
- `aria-busy` set when loading
- `title` attribute provides tooltip on hover
- Keyboard accessible: Tab to focus, Enter/Space to activate

## Examples

### Basic Usage

```razor
<SbIconButton AriaLabel="Close dialog">
    <SbIcon Name="x" />
</SbIconButton>
```

### Colors

```razor
<SbStack Direction="SbStackDirection.Row" Gap="2">
    <SbIconButton AriaLabel="Edit" Color="SbColor.Primary">
        <SbIcon Name="edit" />
    </SbIconButton>
    <SbIconButton AriaLabel="Delete" Color="SbColor.Danger">
        <SbIcon Name="trash" />
    </SbIconButton>
    <SbIconButton AriaLabel="Favorite" Color="SbColor.Warning">
        <SbIcon Name="star" />
    </SbIconButton>
</SbStack>
```

### Sizes

```razor
<SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
    <SbIconButton AriaLabel="Small" Size="SbSize.Sm">
        <SbIcon Name="heart" Size="SbSize.Sm" />
    </SbIconButton>
    <SbIconButton AriaLabel="Medium" Size="SbSize.Md">
        <SbIcon Name="heart" />
    </SbIconButton>
    <SbIconButton AriaLabel="Large" Size="SbSize.Lg">
        <SbIcon Name="heart" Size="SbSize.Lg" />
    </SbIconButton>
</SbStack>
```

### Variants

```razor
<SbStack Direction="SbStackDirection.Row" Gap="2">
    <SbIconButton AriaLabel="Ghost" Variant="SbButtonVariant.Ghost">
        <SbIcon Name="settings" />
    </SbIconButton>
    <SbIconButton AriaLabel="Outline" Variant="SbButtonVariant.Outline">
        <SbIcon Name="settings" />
    </SbIconButton>
    <SbIconButton AriaLabel="Solid" Variant="SbButtonVariant.Solid">
        <SbIcon Name="settings" />
    </SbIconButton>
    <SbIconButton AriaLabel="Link" Variant="SbButtonVariant.Link">
        <SbIcon Name="settings" />
    </SbIconButton>
</SbStack>
```

### States (Disabled, Loading)

```razor
<SbStack Direction="SbStackDirection.Row" Gap="2">
    <SbIconButton AriaLabel="Delete"><SbIcon Name="trash" /></SbIconButton>
    <SbIconButton AriaLabel="Delete (disabled)" Disabled="true"><SbIcon Name="trash" /></SbIconButton>
    <SbIconButton AriaLabel="Loading" Loading="true"><SbIcon Name="trash" /></SbIconButton>
</SbStack>
```

### With Tooltip

```razor
<SbIconButton AriaLabel="Refresh data" Title="Refresh">
    <SbIcon Name="refresh" />
</SbIconButton>
```

### In a Toolbar

```razor
<SbStack Direction="SbStackDirection.Row" Gap="1">
    <SbIconButton AriaLabel="Bold" Title="Bold (Ctrl+B)">
        <SbIcon Name="bold" Size="SbSize.Sm" />
    </SbIconButton>
    <SbIconButton AriaLabel="Italic" Title="Italic (Ctrl+I)">
        <SbIcon Name="italic" Size="SbSize.Sm" />
    </SbIconButton>
    <SbIconButton AriaLabel="Underline" Title="Underline (Ctrl+U)">
        <SbIcon Name="underline" Size="SbSize.Sm" />
    </SbIconButton>
</SbStack>
```
