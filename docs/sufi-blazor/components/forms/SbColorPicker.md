# SbColorPicker

A color picker component with preset colors, custom color input, and optional opacity control.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | string? | null | The selected color in hex format (two-way bindable) |
| ValueChanged | EventCallback\<string?\> | - | Callback when color changes |
| Presets | string[] | (default palette) | Preset color options |
| ShowPresets | bool | true | Whether to show preset colors |
| ShowOpacity | bool | false | Whether to show opacity slider |
| Placeholder | string? | null | Placeholder when no color selected; when null, uses localized default |
| Clearable | bool | true | Whether to show clear button |
| Disabled | bool | false | Whether the picker is disabled |
| Id | string? | null | Element ID |
| Label | string? | null | Label text displayed above the picker |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<string?\> | Fired when the color changes |

## CSS Classes

- `sb-colorpicker` - Base class
- `sb-colorpicker__label` - Label element
- `sb-colorpicker__required` - Required asterisk
- `sb-colorpicker__trigger` - Button that opens the picker
- `sb-colorpicker__trigger--disabled` - Disabled trigger
- `sb-colorpicker__preview` - Color preview swatch
- `sb-colorpicker__value` - Hex value display
- `sb-colorpicker__clear` - Clear button
- `sb-colorpicker__dropdown` - Dropdown panel
- `sb-colorpicker__dropdown--flip-up` - Dropdown flips upward
- `sb-colorpicker__presets` - Preset colors grid
- `sb-colorpicker__preset` - Individual preset button
- `sb-colorpicker__preset--selected` - Selected preset
- `sb-colorpicker__custom` - Custom color section
- `sb-colorpicker__label` - Section label
- `sb-colorpicker__native-wrapper` - Native color input wrapper
- `sb-colorpicker__native` - Native color input
- `sb-colorpicker__hex` - Hex text input
- `sb-colorpicker__opacity` - Opacity slider section
- `sb-colorpicker__opacity-slider` - Opacity range input
- `sb-colorpicker__footer` - Footer with action buttons

## Accessibility

- Clear button has aria-label
- Preset buttons have title attributes with hex values

## Examples

### Basic Usage

```razor
<SbColorPicker @bind-Value="selectedColor" Label="Primary Color" />
```

### With Custom Presets

```razor
<SbColorPicker @bind-Value="brandColor" 
               Label="Brand Color"
               Presets="@brandColors" />

@code {
    private string[] brandColors = new[] 
    {
        "#1a73e8", "#4285f4", "#34a853", "#fbbc04", "#ea4335"
    };
}
```

### Without Presets

```razor
<SbColorPicker @bind-Value="customColor" 
               Label="Custom Color"
               ShowPresets="false" />
```

### With Opacity

```razor
<SbColorPicker @bind-Value="overlayColor" 
               Label="Overlay Color"
               ShowOpacity="true" />
```

### Required Field

```razor
<SbColorPicker @bind-Value="themeColor" 
               Label="Theme Color"
               Required="true"
               Clearable="false" />
```

### Disabled State

```razor
<SbColorPicker Value="#3b82f6" 
               Label="Locked Color"
               Disabled="true" />
```
