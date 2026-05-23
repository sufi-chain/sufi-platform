# SbButton

Primary action element with multiple variants, sizes, and states. Can render as a button or anchor element.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Variant | SbButtonVariant | Solid | Button style variant (Solid, Outline, Ghost, Link) |
| Size | SbSize | Md | Button size (Xs, Sm, Md, Lg, Xl) |
| Color | SbColor | Primary | Color intent (Default, Primary, Secondary, Success, Warning, Danger, Info, Muted) |
| Disabled | bool | false | Whether the button is disabled |
| Loading | bool | false | Whether the button shows a loading spinner |
| FullWidth | bool | false | Whether the button takes full container width |
| Type | string | "button" | HTML button type (button, submit, reset) |
| Href | string? | null | If set, renders as anchor element instead of button |
| Target | string? | null | Target for anchor links (_blank, _self, etc.) |
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
| ChildContent | RenderFragment | The button's text or content |
| StartIcon | RenderFragment | Icon rendered before the content |
| EndIcon | RenderFragment | Icon rendered after the content |

### Template Usage Examples

#### Basic Content

```razor
<SbButton>Click Me</SbButton>
```

#### With Start Icon

```razor
<SbButton>
    <StartIcon><SbIcon Name="plus" Size="SbSize.Sm" /></StartIcon>
    <ChildContent>Add Item</ChildContent>
</SbButton>
```

#### With End Icon

```razor
<SbButton Variant="SbButtonVariant.Outline">
    <ChildContent>Download</ChildContent>
    <EndIcon><SbIcon Name="download" Size="SbSize.Sm" /></EndIcon>
</SbButton>
```

#### With Both Icons

```razor
<SbButton>
    <StartIcon><SbIcon Name="save" Size="SbSize.Sm" /></StartIcon>
    <ChildContent>Save Changes</ChildContent>
    <EndIcon><SbIcon Name="check" Size="SbSize.Sm" /></EndIcon>
</SbButton>
```

## CSS Classes

- `sb-button` - Base class
- `sb-button--solid` - Solid variant
- `sb-button--outline` - Outline variant
- `sb-button--ghost` - Ghost variant
- `sb-button--link` - Link variant
- `sb-button--xs` - Extra small size
- `sb-button--sm` - Small size
- `sb-button--md` - Medium size
- `sb-button--lg` - Large size
- `sb-button--xl` - Extra large size
- `sb-button--primary` - Primary color
- `sb-button--secondary` - Secondary color
- `sb-button--success` - Success color
- `sb-button--warning` - Warning color
- `sb-button--danger` - Danger color
- `sb-button--info` - Info color
- `sb-button--muted` - Muted color
- `sb-button--disabled` - Disabled state
- `sb-button--loading` - Loading state
- `sb-button--full-width` - Full width

## Accessibility

- Uses native `<button>` or `<a>` elements
- Keyboard navigation: Tab to focus, Enter/Space to activate
- `aria-disabled` set when disabled (for anchor variant)
- `aria-busy` set when loading
- `rel="noopener noreferrer"` automatically added for external links

## Examples

### Variants

```razor
<SbStack Direction="SbStackDirection.Row" Gap="3">
    <SbButton Variant="SbButtonVariant.Solid">Solid</SbButton>
    <SbButton Variant="SbButtonVariant.Outline">Outline</SbButton>
    <SbButton Variant="SbButtonVariant.Ghost">Ghost</SbButton>
    <SbButton Variant="SbButtonVariant.Link">Link</SbButton>
</SbStack>
```

### Colors

```razor
<SbStack Direction="SbStackDirection.Row" Gap="3">
    <SbButton Color="SbColor.Primary">Primary</SbButton>
    <SbButton Color="SbColor.Secondary">Secondary</SbButton>
    <SbButton Color="SbColor.Success">Success</SbButton>
    <SbButton Color="SbColor.Warning">Warning</SbButton>
    <SbButton Color="SbColor.Danger">Danger</SbButton>
</SbStack>
```

### Sizes

```razor
<SbStack Direction="SbStackDirection.Row" Gap="3" Align="SbAlign.Center">
    <SbButton Size="SbSize.Sm">Small</SbButton>
    <SbButton Size="SbSize.Md">Medium</SbButton>
    <SbButton Size="SbSize.Lg">Large</SbButton>
</SbStack>
```

### States

```razor
<SbStack Direction="SbStackDirection.Row" Gap="3">
    <SbButton>Normal</SbButton>
    <SbButton Disabled="true">Disabled</SbButton>
    <SbButton Loading="true">Loading</SbButton>
</SbStack>
```

### As Link

```razor
<SbButton Href="https://github.com" Target="_blank">
    Go to GitHub
</SbButton>
```

### Form Submit

```razor
<SbButton Type="submit" Color="SbColor.Success">
    Submit Form
</SbButton>
```

### Click Handler (OnClick)

```razor
<SbButton OnClick="HandleClick">Click me</SbButton>

@code {
    private void HandleClick(MouseEventArgs e) { ... }
}
```
