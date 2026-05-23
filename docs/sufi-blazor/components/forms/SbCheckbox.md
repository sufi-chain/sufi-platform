# SbCheckbox

A checkbox component with support for labels, indeterminate state, and color variants.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | bool | false | Checkbox checked state (two-way bindable) |
| ValueChanged | EventCallback\<bool\> | - | Callback when value changes |
| Label | string? | null | Checkbox label text |
| Indeterminate | bool | false | Whether the checkbox is in indeterminate state |
| Disabled | bool | false | Whether the checkbox is disabled |
| Color | SbColor | SbColor.Primary | Checkbox accent color |
| Id | string? | null | Element ID for the input |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes (CaptureUnmatchedValues) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<bool\> | Fired when the checkbox value changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Custom label content (takes precedence over Label parameter) |

### Template Usage Examples

#### Custom Label with ChildContent

```razor
<SbCheckbox @bind-Value="acceptTerms">
    I agree to the <SbLink Href="/terms">Terms of Service</SbLink> 
    and <SbLink Href="/privacy">Privacy Policy</SbLink>
</SbCheckbox>
```

## CSS Classes

- `sb-checkbox` - Base class
- `sb-checkbox--{color}` - Color variant (primary, secondary, success, danger, etc.)
- `sb-checkbox--checked` - Checked state
- `sb-checkbox--indeterminate` - Indeterminate state
- `sb-checkbox--disabled` - Disabled state
- `sb-checkbox__input` - Hidden native input
- `sb-checkbox__box` - Visual checkbox box
- `sb-checkbox__check` - Check icon
- `sb-checkbox__indeterminate` - Minus icon for indeterminate
- `sb-checkbox__label` - Label text

## Accessibility

- Uses native `<input type="checkbox">`
- `aria-checked` indicates current state (true/false/mixed)
- Label wraps input for click area expansion
- Keyboard accessible (Space to toggle)

## Examples

### Basic Usage

```razor
<SbCheckbox @bind-Value="isChecked" Label="Enable notifications" />
```

### Color Variants

```razor
<SbCheckbox @bind-Value="opt1" Label="Primary" Color="SbColor.Primary" />
<SbCheckbox @bind-Value="opt2" Label="Success" Color="SbColor.Success" />
<SbCheckbox @bind-Value="opt3" Label="Danger" Color="SbColor.Danger" />
```

### Indeterminate State

```razor
<SbCheckbox Value="@parentChecked" 
            Indeterminate="@isIndeterminate"
            ValueChanged="HandleParentChange"
            Label="Select all" />
```

### Disabled States

```razor
<SbCheckbox Value="true" Disabled="true" Label="Checked and disabled" />
<SbCheckbox Value="false" Disabled="true" Label="Unchecked and disabled" />
```

### With Rich Label Content

```razor
<SbCheckbox @bind-Value="rememberMe">
    <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
        <SbIcon Name="shield-check" Size="SbSize.Sm" />
        <span>Remember this device for 30 days</span>
    </SbStack>
</SbCheckbox>
```

### Form Validation

```razor
<EditForm Model="model">
    <SbCheckbox @bind-Value="model.AcceptTerms" 
                Label="I accept the terms and conditions"
                Id="accept-terms" />
    <SbFieldError For="() => model.AcceptTerms" />
</EditForm>
```

### Select All Pattern

```razor
@code {
    private bool selectAll;
    private bool isIndeterminate;
    private List<Item> items = new();
    
    private void OnSelectAllChanged(bool value)
    {
        selectAll = value;
        isIndeterminate = false;
        foreach (var item in items)
        {
            item.Selected = value;
        }
    }
    
    private void OnItemChanged()
    {
        var selectedCount = items.Count(i => i.Selected);
        selectAll = selectedCount == items.Count;
        isIndeterminate = selectedCount > 0 && selectedCount < items.Count;
    }
}

<SbCheckbox Value="@selectAll" 
            Indeterminate="@isIndeterminate"
            ValueChanged="OnSelectAllChanged"
            Label="Select all items" />

@foreach (var item in items)
{
    <SbCheckbox @bind-Value="item.Selected" 
                Label="@item.Name"
                ValueChanged="_ => OnItemChanged()" />
}
```
