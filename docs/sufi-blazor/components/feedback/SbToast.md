# SbToast

A temporary notification message that appears briefly to provide feedback about an operation. Toasts auto-dismiss and can show various severity levels. Typically used via SbToastHost for proper positioning and management.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string? | null | Toast title/heading |
| Message | string? | null | Toast message text (used when ChildContent is not provided) |
| Severity | SbToastSeverity | Info | Toast severity (Info, Success, Warning, Error) |
| ShowCloseButton | bool | true | Whether to show the close button |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClose | EventCallback | Fired when the toast is closed |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Custom content (takes precedence over Message) |

### Template Usage

```razor
<SbToast Title="Success" Severity="SbToastSeverity.Success">
    <p>Your profile has been updated.</p>
    <a href="/profile">View Profile</a>
</SbToast>
```

## CSS Classes

- `sb-toast` - Base class
- `sb-toast--info` - Info severity
- `sb-toast--success` - Success severity
- `sb-toast--warning` - Warning severity
- `sb-toast--error` - Error severity
- `sb-toast__icon` - Icon container
- `sb-toast__content` - Content wrapper
- `sb-toast__title` - Title text
- `sb-toast__message` - Message text
- `sb-toast__close` - Close button

## Accessibility

- Uses `role="alert"` for important notifications
- Uses `aria-live="polite"` for non-intrusive announcements
- Close button has `aria-label="Close"`

## Examples

### Basic Toast (Standalone)

```razor
<SbToast Severity="SbToastSeverity.Info" Message="This is an info toast." />
<SbToast Severity="SbToastSeverity.Success" Message="Operation successful!" />
<SbToast Severity="SbToastSeverity.Warning" Message="Please review your input." />
<SbToast Severity="SbToastSeverity.Error" Message="An error occurred." />
```

### With Title

```razor
<SbToast Title="Success" 
         Message="Your changes have been saved."
         Severity="SbToastSeverity.Success" />
```

### With Custom Content

```razor
<SbToast Title="New Message" Severity="SbToastSeverity.Info">
    <p>You have a new message from <strong>John</strong>.</p>
    <SbButton Size="SbSize.Sm" Variant="SbButtonVariant.Ghost">View</SbButton>
</SbToast>
```

### Without Close Button

```razor
<SbToast Message="Auto-saving..." 
         Severity="SbToastSeverity.Info"
         ShowCloseButton="false" />
```

### Handling Close

```razor
<SbToast Title="Notification"
         Message="Click to dismiss"
         Severity="SbToastSeverity.Info"
         OnClose="HandleClose" />

@code {
    private void HandleClose()
    {
        Console.WriteLine("Toast closed");
    }
}
```

## Recommended Usage with SbToastHost

For proper toast management (positioning, stacking, auto-dismiss), use `SbToastHost`:

```razor
@* In your layout or main component *@
<SbToastHost @ref="toastHost" Position="SbToastPosition.TopRight" />

@* Trigger toasts from your component *@
<SbButton OnClick="ShowSuccessToast">Show Success</SbButton>

@code {
    private SbToastHost? toastHost;
    
    private async Task ShowSuccessToast()
    {
        await toastHost!.ShowSuccessAsync("Operation completed!", "Success");
    }
}
```

See `SbToastHost` documentation for full details on toast management.
