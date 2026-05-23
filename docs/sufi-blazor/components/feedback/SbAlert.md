# SbAlert

Displays important messages to users with contextual severity levels. Alerts can be dismissible and support various severities like info, success, warning, and danger.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Severity | SbAlertSeverity | Info | Alert severity level (Info, Success, Warning, Danger) |
| Title | string? | null | Optional alert title displayed above the message |
| Dismissible | bool | false | Whether the alert can be dismissed by the user |
| Dense | bool | false | Reduces vertical padding for a more compact alert |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnDismiss | EventCallback | Fired when the alert is dismissed |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Main content/message of the alert |

### Template Usage

```razor
<SbAlert Severity="SbAlertSeverity.Info">
    This is the alert message content.
</SbAlert>
```

## CSS Classes

- `sb-alert` - Base class
- `sb-alert--info` - Info severity
- `sb-alert--success` - Success severity
- `sb-alert--warning` - Warning severity
- `sb-alert--danger` - Danger severity
- `sb-alert--dense` - Compact variant
- `sb-alert__icon` - Icon container
- `sb-alert__content` - Content wrapper
- `sb-alert__title` - Title text
- `sb-alert__message` - Message text
- `sb-alert__dismiss` - Dismiss button

## Accessibility

- Uses `role="alert"` for danger severity (assertive)
- Uses `role="status"` for other severities (polite)
- Dismiss button has `aria-label="Dismiss"`

## Examples

### Basic Alerts

```razor
<SbAlert Severity="SbAlertSeverity.Info">
    This is an informational message.
</SbAlert>

<SbAlert Severity="SbAlertSeverity.Success">
    Operation completed successfully!
</SbAlert>

<SbAlert Severity="SbAlertSeverity.Warning">
    Please review before proceeding.
</SbAlert>

<SbAlert Severity="SbAlertSeverity.Danger">
    An error occurred. Please try again.
</SbAlert>
```

### With Title

```razor
<SbAlert Severity="SbAlertSeverity.Info" Title="Information">
    Here are some details you should know about.
</SbAlert>
```

### Dismissible Alert

```razor
<SbAlert Severity="SbAlertSeverity.Warning"
         Dismissible="true"
         OnDismiss="HandleDismiss">
    This alert can be closed by the user.
</SbAlert>

@code {
    private void HandleDismiss()
    {
        // Handle dismissal
    }
}
```

### Dense Variant

```razor
<SbAlert Severity="SbAlertSeverity.Info" Dense="true">
    Compact alert with less vertical padding.
</SbAlert>
```

### Full Example

```razor
<SbAlert Severity="SbAlertSeverity.Success"
         Title="Success!"
         Dismissible="true"
         Dense="false"
         Class="my-custom-alert"
         OnDismiss="() => alertVisible = false">
    Your changes have been saved successfully.
</SbAlert>
```
