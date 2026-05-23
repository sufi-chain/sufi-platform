# SbDateRangePicker

A dual-calendar date range picker with preset options, supporting multiple calendar systems.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | SbDateRange? | null | The selected date range (two-way bindable) |
| ValueChanged | EventCallback\<SbDateRange?\> | - | Callback when range changes |
| CalendarSystem | SbCalendarSystem? | null | Calendar system; when null, uses current UI culture |
| Min | DateOnly? | null | Minimum selectable date |
| Max | DateOnly? | null | Maximum selectable date |
| DisableWeekends | bool | false | When true, disables weekend days according to the current calendar |
| IsDateDisabled | Func\<DateOnly, bool\>? | null | Function to disable specific dates |
| Format | string? | null | Date format string |
| Placeholder | string? | null | Placeholder when no range selected; when null, uses localized default |
| Clearable | bool | true | Whether to show clear button |
| Disabled | bool | false | Whether the picker is disabled |
| ShowPresets | bool | true | Whether to show quick preset options |
| Id | string? | null | Element ID |
| Label | string? | null | Label text displayed above the picker |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<SbDateRange?\> | Fired when the date range changes |

## SbDateRange Type

```csharp
public record SbDateRange(DateOnly? Start, DateOnly? End);
```

## CSS Classes

- `sb-daterangepicker` - Base class
- `sb-daterangepicker__label` - Label element
- `sb-daterangepicker__required` - Required asterisk
- `sb-daterangepicker__trigger` - Button that opens the picker
- `sb-daterangepicker__trigger--disabled` - Disabled trigger
- `sb-daterangepicker__value` - Selected range display
- `sb-daterangepicker__placeholder` - Placeholder text
- `sb-daterangepicker__clear` - Clear button
- `sb-daterangepicker__icon` - Calendar icon
- `sb-daterangepicker__dropdown` - Dropdown panel
- `sb-daterangepicker__dropdown--flip-up` - Dropdown flips upward
- `sb-daterangepicker__calendars` - Calendars container
- `sb-daterangepicker__calendar` - Individual calendar
- `sb-daterangepicker__header` - Calendar header
- `sb-daterangepicker__nav-btn` - Navigation buttons
- `sb-daterangepicker__title` - Month/year title
- `sb-daterangepicker__day-names` - Day names row
- `sb-daterangepicker__days` - Days grid
- `sb-daterangepicker__day` - Individual day button
- `sb-daterangepicker__day--start` - Range start date
- `sb-daterangepicker__day--end` - Range end date
- `sb-daterangepicker__day--in-range` - Days within range
- `sb-daterangepicker__day--preview` - Hover preview
- `sb-daterangepicker__day--today` - Today's date
- `sb-daterangepicker__day--disabled` - Disabled date
- `sb-daterangepicker__presets` - Preset buttons container
- `sb-daterangepicker__preset` - Preset button
- `sb-daterangepicker__footer` - Footer with actions
- `sb-daterangepicker__selection` - Selection summary text
- `sb-daterangepicker__cancel` - Cancel button
- `sb-daterangepicker__apply` - Apply button

## Preset Options

When ShowPresets is true, the following presets are available:
- Today
- Last 7 days
- Last 30 days
- Last 90 days
- This month
- Last month

## Accessibility

- Navigation buttons have aria-labels
- Clear button has aria-label
- Supports RTL for Persian/Arabic calendars

## Examples

### Basic Usage

```razor
<SbDateRangePicker @bind-Value="dateRange" Label="Date Range" />
```

### Without Presets

```razor
<SbDateRangePicker @bind-Value="customRange" 
                   Label="Custom Range"
                   ShowPresets="false" />
```

### With Constraints

```razor
<SbDateRangePicker @bind-Value="bookingRange" 
                   Label="Booking Period"
                   Min="@DateOnly.FromDateTime(DateTime.Today)"
                   Max="@DateOnly.FromDateTime(DateTime.Today.AddYears(1))" />
```

### Persian Calendar

```razor
<SbDateRangePicker @bind-Value="persianRange" 
                   CalendarSystem="SbCalendarSystem.Persian"
                   Label="بازه تاریخ" />
```

### Disable Weekends

```razor
<SbDateRangePicker @bind-Value="workingDays" 
                   Label="Working Days"
                   IsDateDisabled="d => d.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday" />
```

### Custom Format

```razor
<SbDateRangePicker @bind-Value="reportRange" 
                   Label="Report Period"
                   Format="MMM d, yyyy" />
```

### Required Field

```razor
<SbDateRangePicker @bind-Value="requiredRange" 
                   Label="Period (required)"
                   Required="true"
                   Clearable="false" />
```
