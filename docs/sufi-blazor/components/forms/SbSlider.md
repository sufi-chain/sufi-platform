# SbSlider

A range slider component for selecting numeric values within a defined range.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TNumber? | null | The current value (two-way bindable) |
| ValueChanged | EventCallback\<TNumber?\> | - | Callback when value changes |
| Min | TNumber | - | Minimum value |
| Max | TNumber | - | Maximum value |
| Step | TNumber? | null | Step increment (when null, effective step is 1) |
| ShowValue | bool | true | Whether to display the current value |
| Disabled | bool | false | Whether the slider is disabled |
| Id | string? | null | Element ID for the slider thumb |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Supported Types

The component supports numeric struct types implementing `IComparable<T>`:
- `int`, `long`, `short`
- `float`, `double`, `decimal`

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TNumber?\> | Fired when the value changes |

## CSS Classes

- `sb-slider` - Base class
- `sb-slider__track-container` - Track container for mouse events
- `sb-slider__track` - Background track
- `sb-slider__fill` - Filled portion of track
- `sb-slider__thumb` - Draggable thumb
- `sb-slider__value` - Value display
- `sb-slider--disabled` - Disabled state

## Accessibility

- Uses `role="slider"` on thumb element
- `aria-valuenow`, `aria-valuemin`, `aria-valuemax` attributes
- Keyboard: Arrow keys to adjust value, Home/End for min/max

## Examples

### Basic Usage

```razor
<SbSlider TNumber="int" 
          @bind-Value="volume" 
          Min="0" 
          Max="100" />
```

### With Step

```razor
<SbSlider TNumber="int" 
          @bind-Value="brightness" 
          Min="0" 
          Max="100"
          Step="10" />
```

### Decimal Values

```razor
<SbSlider TNumber="decimal" 
          @bind-Value="rating" 
          Min="0m" 
          Max="5m"
          Step="0.5m" />
```

### Without Value Display

```razor
<SbSlider TNumber="int" 
          @bind-Value="value" 
          Min="0" 
          Max="10"
          ShowValue="false" />
```

### Custom Range

```razor
<SbSlider TNumber="int" 
          @bind-Value="year" 
          Min="2000" 
          Max="2030" />
```

### Disabled Slider

```razor
<SbSlider TNumber="int" 
          Value="75" 
          Min="0" 
          Max="100"
          Disabled="true" />
```

### With Label (using SbFormField)

```razor
<SbFormField Label="Volume">
    <SbSlider TNumber="int" 
              @bind-Value="volume" 
              Min="0" 
              Max="100" />
</SbFormField>
```
