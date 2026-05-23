# SbTimePicker

A time picker component with hour, minute, and optional second selection. Supports 12/24 hour formats.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TimeOnly? | null | The selected time (two-way bindable) |
| ValueChanged | EventCallback\<TimeOnly?\> | - | Callback when time changes |
| Use24Hour | bool | true | Whether to use 24-hour format |
| ShowSeconds | bool | false | Whether to show seconds selector |
| MinuteStep | int | 1 | Minute step interval |
| SecondStep | int | 1 | Second step interval |
| Placeholder | string? | null | Placeholder when no time selected; when null, uses localized default |
| Clearable | bool | true | Whether to show clear button |
| Disabled | bool | false | Whether the picker is disabled |
| Id | string? | null | Element ID |
| Label | string? | null | Label text displayed above the picker |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TimeOnly?\> | Fired when the time changes |

## CSS Classes

- `sb-timepicker` - Base class
- `sb-timepicker__label` - Label element
- `sb-timepicker__required` - Required asterisk
- `sb-timepicker__trigger` - Button that opens the picker
- `sb-timepicker__trigger--disabled` - Disabled trigger
- `sb-timepicker__value` - Selected value display
- `sb-timepicker__placeholder` - Placeholder text
- `sb-timepicker__clear` - Clear button
- `sb-timepicker__icon` - Clock icon
- `sb-timepicker__dropdown` - Dropdown panel
- `sb-timepicker__dropdown--flip-up` - Dropdown flips upward
- `sb-timepicker__selectors` - Selectors container
- `sb-timepicker__column` - Hour/minute/second column
- `sb-timepicker__column--period` - AM/PM column (when Use24Hour is false)
- `sb-timepicker__column-header` - Column header text
- `sb-timepicker__scroll` - Scrollable options area
- `sb-timepicker__option` - Individual time option
- `sb-timepicker__option--selected` - Selected option
- `sb-timepicker__separator` - Colon separator
- `sb-timepicker__footer` - Footer with action buttons
- `sb-timepicker__now-btn` - "Now" button
- `sb-timepicker__apply-btn` - Apply button (localized)

## Accessibility

- Clear button has aria-label
- Keyboard: Tab to navigate columns, Enter to select

## Examples

### Basic Usage

```razor
<SbTimePicker @bind-Value="selectedTime" Label="Appointment Time" />
```

### 12-Hour Format

```razor
<SbTimePicker @bind-Value="time" 
              Use24Hour="false"
              Label="Meeting Time" />
```

### With Seconds

```razor
<SbTimePicker @bind-Value="preciseTime" 
              ShowSeconds="true"
              Label="Exact Time" />
```

### 15-Minute Steps

```razor
<SbTimePicker @bind-Value="appointmentTime" 
              MinuteStep="15"
              Label="Book Appointment" />
```

### Required Field

```razor
<SbTimePicker @bind-Value="deadline" 
              Label="Deadline"
              Required="true"
              Clearable="false" />
```

### Disabled State

```razor
<SbTimePicker Value="@lockedTime" 
              Label="Fixed Time"
              Disabled="true" />
```
