# SbToastHost

A container component that manages and displays toast notifications. Handles positioning, stacking, auto-dismissal, and animations for multiple toasts. Place one instance in your layout to enable toast notifications throughout your application.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Position | SbToastPosition | TopRight | Position of the toast container |
| DefaultDuration | int | 5000 | Default auto-dismiss duration in milliseconds |
| MaxToasts | int | 5 | Maximum number of toasts to display simultaneously |
| Class | string? | null | Additional CSS classes |

## Position Options

| Value | Description |
|-------|-------------|
| TopLeft | Top-left corner |
| TopCenter | Top-center |
| TopRight | Top-right corner (default) |
| BottomLeft | Bottom-left corner |
| BottomCenter | Bottom-center |
| BottomRight | Bottom-right corner |

## Methods

| Method | Parameters | Description |
|--------|------------|-------------|
| ShowAsync | message, severity?, title?, duration? | Shows a toast with specified options |
| ShowInfoAsync | message, title? | Shows an info toast |
| ShowSuccessAsync | message, title? | Shows a success toast |
| ShowWarningAsync | message, title? | Shows a warning toast |
| ShowErrorAsync | message, title? | Shows an error toast |
| Clear | - | Removes all active toasts |

## CSS Classes

- `sb-toast-host` - Base class
- `sb-toast-host--top-left` - Top-left position
- `sb-toast-host--top-center` - Top-center position
- `sb-toast-host--top-right` - Top-right position
- `sb-toast-host--bottom-left` - Bottom-left position
- `sb-toast-host--bottom-center` - Bottom-center position
- `sb-toast-host--bottom-right` - Bottom-right position
- `sb-toast-host__item` - Individual toast wrapper
- `sb-toast-host__item--entering` - Toast entering animation
- `sb-toast-host__item--leaving` - Toast leaving animation

## Examples

### Basic Setup

Place the toast host in your main layout (e.g., `MainLayout.razor`):

```razor
@inherits LayoutComponentBase

<div class="page">
    <SbToastHost @ref="ToastHost" Position="SbToastPosition.TopRight" />
    
    @Body
</div>

@code {
    public SbToastHost? ToastHost { get; set; }
}
```

### Show Different Toast Types

```razor
<SbButton OnClick="ShowInfo">Info</SbButton>
<SbButton OnClick="ShowSuccess">Success</SbButton>
<SbButton OnClick="ShowWarning">Warning</SbButton>
<SbButton OnClick="ShowError">Error</SbButton>

@code {
    [CascadingParameter]
    public MainLayout? Layout { get; set; }
    
    private SbToastHost? ToastHost => Layout?.ToastHost;
    
    private async Task ShowInfo()
    {
        await ToastHost!.ShowInfoAsync("This is an informational message.");
    }
    
    private async Task ShowSuccess()
    {
        await ToastHost!.ShowSuccessAsync("Operation completed successfully!", "Success");
    }
    
    private async Task ShowWarning()
    {
        await ToastHost!.ShowWarningAsync("Please review your changes.", "Warning");
    }
    
    private async Task ShowError()
    {
        await ToastHost!.ShowErrorAsync("An error occurred. Please try again.", "Error");
    }
}
```

### Custom Duration

```razor
@* Toast that stays for 10 seconds *@
await ToastHost.ShowAsync(
    "This toast will stay longer.",
    SbToastSeverity.Info,
    "Notice",
    duration: 10000
);

@* Toast that doesn't auto-dismiss (duration = 0) *@
await ToastHost.ShowAsync(
    "This toast won't auto-dismiss.",
    SbToastSeverity.Warning,
    "Manual Close Required",
    duration: 0
);
```

### Different Positions

```razor
@* Top-left *@
<SbToastHost Position="SbToastPosition.TopLeft" />

@* Bottom-center *@
<SbToastHost Position="SbToastPosition.BottomCenter" />

@* Bottom-right *@
<SbToastHost Position="SbToastPosition.BottomRight" />
```

### Configuration Options

```razor
<SbToastHost @ref="toastHost"
             Position="SbToastPosition.TopRight"
             DefaultDuration="3000"
             MaxToasts="3" />
```

### Using with a Toast Service

For a more scalable approach, create a toast service:

```csharp
// IToastService.cs
public interface IToastService
{
    event Func<string, SbToastSeverity, string?, int?, Task>? OnShow;
    Task ShowAsync(string message, SbToastSeverity severity = SbToastSeverity.Info, 
                   string? title = null, int? duration = null);
    Task ShowInfoAsync(string message, string? title = null);
    Task ShowSuccessAsync(string message, string? title = null);
    Task ShowWarningAsync(string message, string? title = null);
    Task ShowErrorAsync(string message, string? title = null);
}

// ToastService.cs
public class ToastService : IToastService
{
    public event Func<string, SbToastSeverity, string?, int?, Task>? OnShow;
    
    public Task ShowAsync(string message, SbToastSeverity severity = SbToastSeverity.Info,
                          string? title = null, int? duration = null)
        => OnShow?.Invoke(message, severity, title, duration) ?? Task.CompletedTask;
    
    public Task ShowInfoAsync(string message, string? title = null)
        => ShowAsync(message, SbToastSeverity.Info, title);
    
    public Task ShowSuccessAsync(string message, string? title = null)
        => ShowAsync(message, SbToastSeverity.Success, title);
    
    public Task ShowWarningAsync(string message, string? title = null)
        => ShowAsync(message, SbToastSeverity.Warning, title);
    
    public Task ShowErrorAsync(string message, string? title = null)
        => ShowAsync(message, SbToastSeverity.Error, title);
}
```

Register in DI:

```csharp
builder.Services.AddScoped<IToastService, ToastService>();
```

Connect in layout:

```razor
@inject IToastService ToastService

<SbToastHost @ref="toastHost" />

@code {
    private SbToastHost? toastHost;
    
    protected override void OnInitialized()
    {
        ToastService.OnShow += async (msg, sev, title, dur) =>
        {
            await toastHost!.ShowAsync(msg, sev, title, dur);
        };
    }
}
```

Use anywhere:

```razor
@inject IToastService ToastService

<SbButton OnClick="SaveData">Save</SbButton>

@code {
    private async Task SaveData()
    {
        // ... save logic ...
        await ToastService.ShowSuccessAsync("Data saved successfully!");
    }
}
```

### Clear All Toasts

```razor
<SbButton OnClick="() => toastHost?.Clear()">Clear All Toasts</SbButton>
```

### Form Submission Example

```razor
<SbForm OnValidSubmit="HandleSubmit">
    <SbTextField @bind-Value="name" Label="Name" Required="true" />
    <SbButton Type="submit">Submit</SbButton>
</SbForm>

@code {
    private string name = "";
    
    private async Task HandleSubmit()
    {
        try
        {
            await SaveAsync();
            await ToastHost!.ShowSuccessAsync("Form submitted successfully!");
        }
        catch (Exception ex)
        {
            await ToastHost!.ShowErrorAsync($"Error: {ex.Message}", "Submission Failed");
        }
    }
}
```
