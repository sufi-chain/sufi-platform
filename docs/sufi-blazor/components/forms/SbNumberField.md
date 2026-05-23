# SbNumberField

A numeric input component with increment/decrement spin buttons, supporting min/max constraints and step increments.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TNumber | - | The current numeric value (two-way bindable) |
| ValueChanged | EventCallback\<TNumber\> | - | Callback when value changes |
| Min | TNumber? | null | Minimum allowed value |
| Max | TNumber? | null | Maximum allowed value |
| Step | TNumber? | null | Step increment (defaults to 1) |
| ShowSpinButtons | bool | true | Whether to show increment/decrement buttons |
| Placeholder | string? | null | Placeholder text |
| Disabled | bool | false | Whether the field is disabled |
| ReadOnly | bool | false | Whether the field is read-only |
| Id | string? | null | Element ID |
| Label | string? | null | Label text displayed above the input |
| Required | bool | false | Whether the field is required |
| HelperText | string? | null | Helper or hint text shown below the input |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes for the input |
| IncreaseAriaLabel | string | "Increase" | Aria label for increase button |
| DecreaseAriaLabel | string | "Decrease" | Aria label for decrease button |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TNumber\> | Fired when the value changes |

## Supported Types

The component supports any numeric struct type implementing `IComparable<T>`:
- `int`, `long`, `short`, `byte`
- `float`, `double`, `decimal`

## CSS Classes

- `sb-number-field` - Base class
- `sb-number-field__label` - Label element (stacked above input)
- `sb-number-field__required` - Required asterisk
- `sb-number-field__input-group` - Wrapper for input and spin buttons
- `sb-number-field__input` - The input element
- `sb-number-field__spinners` - Spin buttons container
- `sb-number-field__spinner` - Individual spinner button
- `sb-number-field__helper` - Helper text container
- `sb-number-field--disabled` - Disabled state

## Accessibility

- Uses native `<input type="number">`
- Spin buttons have appropriate aria-labels
- Min/max/step reflected in HTML attributes
- Keyboard: Arrow up/down to increment/decrement

## Examples

### Basic Usage

```razor
<SbNumberField TNumber="int" @bind-Value="quantity" Label="Quantity" />
```

### With Constraints

```razor
<SbNumberField TNumber="int" 
               @bind-Value="age" 
               Label="Age"
               Min="0"
               Max="150"
               HelperText="Enter age between 0 and 150" />
```

### Decimal with Step

```razor
<SbNumberField TNumber="decimal" 
               @bind-Value="price" 
               Label="Price"
               Min="0"
               Step="0.01m" />
```

### Without Spin Buttons

```razor
<SbNumberField TNumber="int" 
               @bind-Value="year" 
               Label="Year"
               ShowSpinButtons="false"
               Min="1900"
               Max="2100" />
```

### Percentage

```razor
<SbNumberField TNumber="int" 
               @bind-Value="percentage" 
               Label="Discount %"
               Min="0"
               Max="100"
               Step="5" />
```

### Disabled State

```razor
<SbNumberField TNumber="int" 
               Value="42" 
               Label="Fixed Value"
               Disabled="true" />
```

### Float with Precision

```razor
<SbNumberField TNumber="float" 
               @bind-Value="temperature" 
               Label="Temperature (°C)"
               Min="-273.15f"
               Step="0.1f" />
```
