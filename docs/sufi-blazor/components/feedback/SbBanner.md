# SbBanner

A prominent message bar for important announcements or alerts that spans the full width of its container. Supports multiple severity levels, custom actions, and dismissible behavior.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string? | null | Banner title displayed prominently |
| Message | string? | null | Banner message text (used when ChildContent is not provided) |
| Severity | SbBannerSeverity | Info | Banner severity (Info, Success, Warning, Error) |
| Dismissible | bool | true | Whether the banner can be dismissed |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnDismiss | EventCallback | Fired when the banner is dismissed |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Custom content for the banner message (takes precedence over Message) |
| Actions | RenderFragment? | Action buttons displayed at the bottom of the banner |

### Template Usage

#### Basic Content

```razor
<SbBanner Title="Notice" Severity="SbBannerSeverity.Info">
    This is custom content inside the banner.
</SbBanner>
```

#### With Actions

```razor
<SbBanner Title="Update Available" Severity="SbBannerSeverity.Info">
    <ChildContent>
        A new version is available. Would you like to update now?
    </ChildContent>
    <Actions>
        <SbButton Variant="SbButtonVariant.Ghost" Size="SbSize.Sm">Later</SbButton>
        <SbButton Size="SbSize.Sm">Update Now</SbButton>
    </Actions>
</SbBanner>
```

## Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| Show() | void | Shows the banner after it has been dismissed |

## CSS Classes

- `sb-banner` - Base class
- `sb-banner--info` - Info severity
- `sb-banner--success` - Success severity
- `sb-banner--warning` - Warning severity
- `sb-banner--error` - Error severity
- `sb-banner__header` - Header row container
- `sb-banner__header-start` - Icon and title container
- `sb-banner__icon` - Icon container
- `sb-banner__title` - Title text
- `sb-banner__close` - Close button
- `sb-banner__content` - Content/message area
- `sb-banner__message` - Message text
- `sb-banner__actions` - Actions container

## Accessibility

- Uses `role="alert"` for important announcements
- Close button has `aria-label="Dismiss"`

## Examples

### Basic Banners

```razor
<SbBanner Title="Information" Message="This is an informational banner." />

<SbBanner Title="Success" 
          Message="Your changes have been saved."
          Severity="SbBannerSeverity.Success" />

<SbBanner Title="Warning" 
          Message="Your session will expire in 5 minutes."
          Severity="SbBannerSeverity.Warning" />

<SbBanner Title="Error" 
          Message="Failed to connect to the server."
          Severity="SbBannerSeverity.Error" />
```

### Non-Dismissible Banner

```razor
<SbBanner Title="System Maintenance"
          Message="Scheduled maintenance tonight at 10 PM."
          Severity="SbBannerSeverity.Warning"
          Dismissible="false" />
```

### With Custom Content and Actions

```razor
<SbBanner Title="New Feature" Severity="SbBannerSeverity.Info">
    <ChildContent>
        <p>We've added dark mode support! Try it out in your settings.</p>
    </ChildContent>
    <Actions>
        <SbButton Variant="SbButtonVariant.Ghost" Size="SbSize.Sm" OnClick="Dismiss">
            Maybe Later
        </SbButton>
        <SbButton Size="SbSize.Sm" OnClick="GoToSettings">
            Go to Settings
        </SbButton>
    </Actions>
</SbBanner>
```

### Handling Dismiss

```razor
<SbBanner @ref="bannerRef"
          Title="Welcome!"
          Message="Thanks for joining us."
          OnDismiss="HandleDismiss" />

<SbButton OnClick="ShowBanner">Show Banner Again</SbButton>

@code {
    private SbBanner? bannerRef;
    
    private void HandleDismiss()
    {
        Console.WriteLine("Banner was dismissed");
    }
    
    private void ShowBanner()
    {
        bannerRef?.Show();
    }
}
```
