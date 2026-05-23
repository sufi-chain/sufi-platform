# SbRadioGroup

A container component for grouping radio buttons, managing the selected value across multiple SbRadio children.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue? | null | The selected value (two-way bindable) |
| ValueChanged | EventCallback\<TValue?\> | - | Callback when selection changes |
| Name | string? | auto-generated | Name attribute for the radio inputs (when null, a GUID is used) |
| Label | string? | null | Visible label for the group (e.g. "Select Plan"); also used as aria-label when AriaLabel is not set |
| AriaLabel | string? | null | Accessible label for the group (when null, Label is used) |
| Disabled | bool | false | Whether all radios in the group are disabled |
| Orientation | SbOrientation | SbOrientation.Vertical | Layout direction (Vertical or Horizontal) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes (CaptureUnmatchedValues) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TValue?\> | Fired when the selected value changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | SbRadio components to include in the group |

### Template Usage Examples

```razor
<SbRadioGroup TValue="string" @bind-Value="selectedOption">
    <SbRadio TValue="string" Value="option1" Label="Option 1" />
    <SbRadio TValue="string" Value="option2" Label="Option 2" />
    <SbRadio TValue="string" Value="option3" Label="Option 3" />
</SbRadioGroup>
```

## CSS Classes

- `sb-radio-group` - Base class
- `sb-radio-group--vertical` - Vertical layout (default)
- `sb-radio-group--horizontal` - Horizontal layout
- `sb-radio-group__label` - Group label
- `sb-radio-group--disabled` - Disabled state

## Accessibility

- Radio buttons share the same `name` attribute for native grouping
- Keyboard navigation: Arrow keys to move between options
- Only one option selectable at a time

## Examples

### Basic Usage

```razor
<SbRadioGroup TValue="string" @bind-Value="selectedPlan">
    <SbRadio TValue="string" Value="basic" Label="Basic - Free" />
    <SbRadio TValue="string" Value="pro" Label="Pro - $9/mo" />
    <SbRadio TValue="string" Value="enterprise" Label="Enterprise - Contact us" />
</SbRadioGroup>
```

### Horizontal Layout

```razor
<SbRadioGroup TValue="int" @bind-Value="rating" Class="horizontal-group">
    @for (int i = 1; i <= 5; i++)
    {
        <SbRadio TValue="int" Value="@i" Label="@i.ToString()" />
    }
</SbRadioGroup>
```

### With Descriptions

```razor
<SbRadioGroup TValue="string" @bind-Value="shippingMethod">
    <SbRadio TValue="string" Value="standard">
        <div>
            <strong>Standard Shipping</strong>
            <SbText Size="SbTextSize.Sm" Color="SbColor.Muted">
                5-7 business days - Free
            </SbText>
        </div>
    </SbRadio>
    <SbRadio TValue="string" Value="express">
        <div>
            <strong>Express Shipping</strong>
            <SbText Size="SbTextSize.Sm" Color="SbColor.Muted">
                2-3 business days - $9.99
            </SbText>
        </div>
    </SbRadio>
    <SbRadio TValue="string" Value="overnight">
        <div>
            <strong>Overnight Shipping</strong>
            <SbText Size="SbTextSize.Sm" Color="SbColor.Muted">
                Next business day - $24.99
            </SbText>
        </div>
    </SbRadio>
</SbRadioGroup>
```

### Disabled Group

```razor
<SbRadioGroup TValue="string" @bind-Value="lockedOption" Disabled="true">
    <SbRadio TValue="string" Value="a" Label="Option A" />
    <SbRadio TValue="string" Value="b" Label="Option B" />
    <SbRadio TValue="string" Value="c" Label="Option C" />
</SbRadioGroup>
```

### Individual Disabled Options

```razor
<SbRadioGroup TValue="string" @bind-Value="selectedTier">
    <SbRadio TValue="string" Value="free" Label="Free Tier" />
    <SbRadio TValue="string" Value="basic" Label="Basic Tier" />
    <SbRadio TValue="string" Value="premium" Label="Premium Tier" Disabled="!isPremiumAvailable" />
</SbRadioGroup>
```

### Enum Binding

```razor
<SbRadioGroup TValue="Priority" @bind-Value="taskPriority">
    @foreach (var priority in Enum.GetValues<Priority>())
    {
        <SbRadio TValue="Priority" Value="@priority" Label="@priority.ToString()" />
    }
</SbRadioGroup>

@code {
    public enum Priority { Low, Medium, High, Critical }
    private Priority taskPriority = Priority.Medium;
}
```
