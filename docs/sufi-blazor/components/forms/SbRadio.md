# SbRadio

A radio button component for single selection within a group. Must be used inside an SbRadioGroup.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue? | null | The value this radio represents |
| Label | string? | null | Radio button label text |
| Disabled | bool | false | Whether this radio is disabled |
| HelperText | string? | null | Optional helper text shown below the label |
| Id | string? | null | Element ID for the input |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes (CaptureUnmatchedValues) |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Custom label content (takes precedence over Label parameter) |

### Template Usage Examples

```razor
<SbRadioGroup TValue="string" @bind-Value="selected">
    <SbRadio TValue="string" Value="custom">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <SbIcon Name="star" />
            <span>Custom Label with Icon</span>
        </SbStack>
    </SbRadio>
</SbRadioGroup>
```

## CSS Classes

- `sb-radio` - Base class
- `sb-radio--checked` - Selected state
- `sb-radio--disabled` - Disabled state
- `sb-radio__input` - Native input
- `sb-radio__input-wrapper` - Wrapper around input
- `sb-radio__circle` - Outer circle
- `sb-radio__dot` - Inner dot (visible when selected)
- `sb-radio__text` - Wrapper for label and helper
- `sb-radio__label` - Label text
- `sb-radio__helper` - Helper text

## Accessibility

- Uses native `<input type="radio">`
- Shares `name` attribute with other radios in the group
- Keyboard navigation via arrow keys within group
- Label wraps input for expanded click area

## Examples

### Basic Usage

See [SbRadioGroup](./SbRadioGroup.md) for complete examples.

```razor
<SbRadioGroup TValue="string" @bind-Value="color">
    <SbRadio TValue="string" Value="red" Label="Red" />
    <SbRadio TValue="string" Value="green" Label="Green" />
    <SbRadio TValue="string" Value="blue" Label="Blue" />
</SbRadioGroup>
```

### With Rich Content

```razor
<SbRadioGroup TValue="PaymentMethod" @bind-Value="paymentMethod">
    <SbRadio TValue="PaymentMethod" Value="PaymentMethod.Card">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <SbIcon Name="credit-card" />
            <span>Credit Card</span>
        </SbStack>
    </SbRadio>
    <SbRadio TValue="PaymentMethod" Value="PaymentMethod.PayPal">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <img src="/icons/paypal.svg" alt="" class="payment-icon" />
            <span>PayPal</span>
        </SbStack>
    </SbRadio>
    <SbRadio TValue="PaymentMethod" Value="PaymentMethod.BankTransfer">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <SbIcon Name="building-bank" />
            <span>Bank Transfer</span>
        </SbStack>
    </SbRadio>
</SbRadioGroup>
```

### Disabled Option

```razor
<SbRadioGroup TValue="string" @bind-Value="subscription">
    <SbRadio TValue="string" Value="monthly" Label="Monthly - $10" />
    <SbRadio TValue="string" Value="yearly" Label="Yearly - $100" />
    <SbRadio TValue="string" Value="lifetime" Label="Lifetime - $500" Disabled="true" />
</SbRadioGroup>
```
