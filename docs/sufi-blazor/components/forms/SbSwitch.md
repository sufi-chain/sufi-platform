# SbSwitch

A toggle switch component for boolean on/off states. More visually distinct than a checkbox for settings and preferences.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | bool | false | Switch on/off state (two-way bindable) |
| ValueChanged | EventCallback\<bool\> | - | Callback when value changes |
| Label | string? | null | Switch label text |
| Disabled | bool | false | Whether the switch is disabled |
| Color | SbColor | SbColor.Primary | Switch accent color when on |
| Size | SbSize | SbSize.Md | Switch size (Sm, Md, Lg) |
| HelperText | string? | null | Helper text shown below the label |
| Id | string? | null | Element ID for the input |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes (CaptureUnmatchedValues) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<bool\> | Fired when the switch value changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Custom label content (takes precedence over Label parameter) |

### Template Usage Examples

#### Custom Label

```razor
<SbSwitch @bind-Value="darkMode">
    <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
        <SbIcon Name="moon" Size="SbSize.Sm" />
        <span>Dark Mode</span>
    </SbStack>
</SbSwitch>
```

## CSS Classes

- `sb-switch` - Base class
- `sb-switch--{color}` - Color variant (primary, success, warning, danger, etc.)
- `sb-switch--{size}` - Size variant (sm, md, lg)
- `sb-switch--checked` - On state
- `sb-switch--disabled` - Disabled state
- `sb-switch__input-wrapper` - Wrapper around input
- `sb-switch__input` - Native input
- `sb-switch__track` - Background track
- `sb-switch__thumb` - Sliding thumb
- `sb-switch__text` - Wrapper for label and helper
- `sb-switch__label` - Label text
- `sb-switch__helper` - Helper text

## Accessibility

- Uses native `<input type="checkbox">` with `role="switch"`
- `aria-checked` indicates current state
- Keyboard accessible (Space to toggle)
- Label wraps input for expanded click area

## Examples

### Basic Usage

```razor
<SbSwitch @bind-Value="isEnabled" Label="Enable feature" />
```

### Color Variants

```razor
<SbSwitch @bind-Value="setting1" Label="Primary" Color="SbColor.Primary" />
<SbSwitch @bind-Value="setting2" Label="Success" Color="SbColor.Success" />
<SbSwitch @bind-Value="setting3" Label="Warning" Color="SbColor.Warning" />
```

### Settings Panel

```razor
<SbCard>
    <Header>
        <SbHeading Level="3">Notifications</SbHeading>
    </Header>
    <SbStack Gap="4">
        <SbSwitch @bind-Value="emailNotifications" Label="Email notifications" />
        <SbSwitch @bind-Value="pushNotifications" Label="Push notifications" />
        <SbSwitch @bind-Value="smsNotifications" Label="SMS notifications" />
    </SbStack>
</SbCard>
```

### Disabled States

```razor
<SbSwitch Value="true" Disabled="true" Label="Always on (locked)" />
<SbSwitch Value="false" Disabled="true" Label="Always off (locked)" />
```

### With Rich Label

```razor
<SbSwitch @bind-Value="autoSave">
    <div>
        <strong>Auto-save</strong>
        <SbText Size="SbTextSize.Sm" Color="SbColor.Muted">
            Automatically save changes every 5 minutes
        </SbText>
    </div>
</SbSwitch>
```

### Controlled Switch

```razor
<SbSwitch Value="@isPublic" 
          ValueChanged="OnPublicChanged"
          Label="Make profile public" />

@code {
    private bool isPublic;
    
    private async Task OnPublicChanged(bool value)
    {
        if (value)
        {
            var confirmed = await DialogService.Confirm(
                "Making your profile public will allow anyone to view it.");
            if (confirmed)
            {
                isPublic = true;
            }
        }
        else
        {
            isPublic = false;
        }
    }
}
```
