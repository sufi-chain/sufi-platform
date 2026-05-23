# SbDatePicker

A date picker component with calendar dropdown, supporting multiple calendar systems (Gregorian, Persian, Hijri), date constraints, and keyboard navigation.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | DateOnly? | null | The selected date (two-way bindable) |
| ValueChanged | EventCallback\<DateOnly?\> | - | Callback when date changes |
| CalendarSystem | SbCalendarSystem? | null | Calendar system; when null, uses current UI culture (e.g. Farsi → Persian/Jalali) |
| Min | DateOnly? | null | Minimum selectable date |
| Max | DateOnly? | null | Maximum selectable date |
| DisableWeekends | bool | false | When true, disables weekend days (e.g. Sat/Sun for Gregorian, Friday for Persian) |
| IsDateDisabled | Func\<DateOnly, bool\>? | null | Function to disable specific dates (use for custom rules) |
| Format | string? | null | Date format string |
| Placeholder | string? | null | Placeholder when no date selected; when null, uses localized default |
| Clearable | bool | true | Whether to show clear button |
| Disabled | bool | false | Whether the picker is disabled |
| Label | string? | null | Label text displayed above the picker |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<DateOnly?\> | Fired when the selected date changes |

## CSS Classes

- `sb-datepicker` - Base class
- `sb-datepicker__label` - Label element
- `sb-datepicker__trigger` - Button that opens the calendar
- `sb-datepicker__trigger--disabled` - Disabled trigger state
- `sb-datepicker__value` - Selected value display
- `sb-datepicker__placeholder` - Placeholder text
- `sb-datepicker__clear` - Clear button
- `sb-datepicker__icon` - Calendar icon
- `sb-datepicker__dropdown` - Calendar dropdown
- `sb-datepicker__dropdown--flip-up` - Dropdown flips upward
- `sb-datepicker__header` - Month/year navigation header
- `sb-datepicker__nav-btn` - Navigation buttons
- `sb-datepicker__title` - Month/year title
- `sb-datepicker__day-names` - Day names row
- `sb-datepicker__day-name` - Individual day name
- `sb-datepicker__days` - Days grid
- `sb-datepicker__day` - Individual day button
- `sb-datepicker__day--other-month` - Day from adjacent month
- `sb-datepicker__day--today` - Today's date
- `sb-datepicker__day--selected` - Selected date
- `sb-datepicker__day--disabled` - Disabled date
- `sb-datepicker__footer` - Footer with today button

## Accessibility

- Keyboard navigation: Arrow keys to move, Enter to select
- `aria-label` on navigation buttons
- Clear button has aria-label
- Supports RTL for Persian/Arabic calendars

## Examples

### Basic Usage

```razor
<SbDatePicker @bind-Value="selectedDate" Label="Birth Date" />
```

### With Constraints

```razor
<SbDatePicker @bind-Value="checkInDate" 
              Label="Check-in Date"
              Min="@DateOnly.FromDateTime(DateTime.Today)"
              Max="@DateOnly.FromDateTime(DateTime.Today.AddMonths(6))" />
```

### Persian Calendar

```razor
<SbDatePicker @bind-Value="persianDate" 
              CalendarSystem="SbCalendarSystem.Persian"
              Label="تاریخ" />
```

### Disable Weekends

```razor
<SbDatePicker @bind-Value="appointmentDate" 
              Label="Appointment Date"
              DisableWeekends="true" />
```

Weekends follow the current calendar (e.g. Saturday/Sunday for Gregorian, Friday only for Persian). For custom rules, use `IsDateDisabled`.

### Custom Format

```razor
<SbDatePicker @bind-Value="eventDate" 
              Label="Event Date"
              Format="MMMM dd, yyyy" />
```

### Required Field

```razor
<SbDatePicker @bind-Value="dueDate" 
              Label="Due Date"
              Clearable="false"
              Placeholder="Select due date (required)" />
```

### Disabled State

```razor
<SbDatePicker Value="@fixedDate" 
              Label="Registration Date"
              Disabled="true" />
```
