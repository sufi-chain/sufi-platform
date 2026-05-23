# SbSelectOption

An option item for use within **SbSimpleSelect**. Must be used as a child of SbSimpleSelect; it registers itself and does not render visible markup on its own.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue? | null | The value of this option (must match SbSimpleSelect TValue) |
| ChildContent | RenderFragment? | null | Display content for the option (text or markup) |
| Disabled | bool | false | Whether this option is disabled and not selectable |

## CSS Classes

- `sb-select-option` - Base class
- `sb-select-option--selected` - Selected state
- `sb-select-option--disabled` - Disabled state

## Accessibility

- Uses `role="option"`
- `aria-selected` indicates selection state

## Examples

### Basic Usage

See [SbSimpleSelect](./SbSimpleSelect.md) for complete examples.

```razor
<SbSimpleSelect TValue="string" @bind-Value="color">
    <SbSelectOption TValue="string" Value="red">Red</SbSelectOption>
    <SbSelectOption TValue="string" Value="green">Green</SbSelectOption>
    <SbSelectOption TValue="string" Value="blue">Blue</SbSelectOption>
</SbSimpleSelect>
```

### With Rich Content (ChildContent)

```razor
<SbSimpleSelect TValue="string" @bind-Value="priority">
    <SbSelectOption TValue="string" Value="low">
        <span class="priority-dot low"></span> Low Priority
    </SbSelectOption>
    <SbSelectOption TValue="string" Value="medium">
        <span class="priority-dot medium"></span> Medium Priority
    </SbSelectOption>
    <SbSelectOption TValue="string" Value="high">
        <span class="priority-dot high"></span> High Priority
    </SbSelectOption>
</SbSimpleSelect>
```

### Disabled Option

```razor
<SbSelectOption TValue="string" Value="unavailable" Disabled="true">
    Temporarily Unavailable
</SbSelectOption>
```
