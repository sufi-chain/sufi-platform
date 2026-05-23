# SbChip

A compact element for displaying tags, categories, or actionable items. Chips can be static, clickable, or removable, with support for icons and various visual styles.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Variant | SbChipVariant | Filled | Visual style (Filled, Outlined) |
| Size | SbSize | Md | Chip size (Sm, Md, Lg) |
| Color | SbColor | Default | Color theme |
| Disabled | bool | false | Whether the chip is disabled |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClick | EventCallback<MouseEventArgs> | Fired when chip is clicked (makes chip interactive) |
| OnRemove | EventCallback | Fired when remove button is clicked (shows remove button) |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Label content of the chip |
| StartIcon | RenderFragment? | Icon displayed before the label |

### Template Usage

#### Basic

```razor
<SbChip>Label</SbChip>
```

#### With Start Icon

```razor
<SbChip>
    <StartIcon>
        <SbIcon Name="tag" Size="SbSize.Sm" />
    </StartIcon>
    <ChildContent>Category</ChildContent>
</SbChip>
```

## CSS Classes

- `sb-chip` - Base class
- `sb-chip--filled` - Filled variant
- `sb-chip--outlined` - Outlined variant
- `sb-chip--sm` - Small size
- `sb-chip--md` - Medium size
- `sb-chip--lg` - Large size
- `sb-chip--primary` - Primary color
- `sb-chip--secondary` - Secondary color
- `sb-chip--success` - Success color
- `sb-chip--warning` - Warning color
- `sb-chip--danger` - Danger color
- `sb-chip--clickable` - Applied when OnClick is provided
- `sb-chip--disabled` - Disabled state
- `sb-chip__icon` - Icon container
- `sb-chip__label` - Label container
- `sb-chip__remove` - Remove button

## Accessibility

- Renders as `<button>` when clickable, `<span>` otherwise
- Remove button has `aria-label="Remove"`
- Disabled state is reflected in the DOM

## Examples

### Basic Chips

```razor
<SbChip>Default</SbChip>
<SbChip Color="SbColor.Primary">Primary</SbChip>
<SbChip Color="SbColor.Success">Success</SbChip>
<SbChip Color="SbColor.Warning">Warning</SbChip>
<SbChip Color="SbColor.Danger">Danger</SbChip>
```

### Variants

```razor
<SbChip Variant="SbChipVariant.Filled" Color="SbColor.Primary">Filled</SbChip>
<SbChip Variant="SbChipVariant.Outlined" Color="SbColor.Primary">Outlined</SbChip>
```

### Sizes

```razor
<SbChip Size="SbSize.Sm">Small</SbChip>
<SbChip Size="SbSize.Md">Medium</SbChip>
<SbChip Size="SbSize.Lg">Large</SbChip>
```

### Clickable Chip

```razor
<SbChip OnClick="HandleClick" Color="SbColor.Primary">
    Click Me
</SbChip>

@code {
    private void HandleClick(MouseEventArgs args)
    {
        Console.WriteLine("Chip clicked!");
    }
}
```

### Removable Chip

```razor
<SbChip OnRemove="HandleRemove" Color="SbColor.Primary">
    Removable
</SbChip>

@code {
    private void HandleRemove()
    {
        Console.WriteLine("Chip removed!");
    }
}
```

### With Icon

```razor
<SbChip Color="SbColor.Primary">
    <StartIcon>
        <SbIcon Name="user" Size="SbSize.Sm" />
    </StartIcon>
    <ChildContent>John Doe</ChildContent>
</SbChip>
```

### Tag List Example

```razor
@foreach (var tag in tags)
{
    <SbChip Color="SbColor.Primary"
            Variant="SbChipVariant.Outlined"
            OnRemove="() => RemoveTag(tag)">
        @tag
    </SbChip>
}

@code {
    private List<string> tags = new() { "Blazor", "C#", ".NET" };
    
    private void RemoveTag(string tag)
    {
        tags.Remove(tag);
    }
}
```

### Disabled State

```razor
<SbChip Disabled="true">Disabled</SbChip>
<SbChip Disabled="true" OnClick="HandleClick">Can't Click</SbChip>
```
