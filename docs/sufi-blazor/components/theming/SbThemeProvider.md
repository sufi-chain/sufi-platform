# SbThemeProvider

The root theming component that provides theme context to all child SufiBlazor components. Manages dark/light mode and text direction (LTR/RTL).

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Theme | SbTheme? | null | Theme configuration to apply |
| IsDark | bool | false | Enables dark mode (overrides Theme.IsDark) |
| Direction | SbDirection | Ltr | Text and layout direction (Ltr, Rtl) |
| Class | string? | null | Additional CSS classes |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Application content wrapped by the provider |

## Theme Presets

| Value | Description |
|-------|-------------|
| SbTheme.Light | Light theme preset |
| SbTheme.Dark | Dark theme preset |

## Direction Options

| Value | Description |
|-------|-------------|
| SbDirection.Ltr | Left-to-right (default) |
| SbDirection.Rtl | Right-to-left |

## Cascading Value

The component cascades an `SbThemeContext` object containing:
- `Theme` - Current theme settings
- `IsDark` - Whether dark mode is active
- `Direction` - Current text direction
- `DirectionAttribute` - "ltr" or "rtl" string

## CSS Classes Applied

- `sb-theme-root` - Always applied
- `sb-theme-light` - Light mode
- `sb-theme-dark` - Dark mode
- `sb-rtl` - RTL direction

## HTML Attributes

- `dir="ltr"` or `dir="rtl"` - Direction attribute

## Examples

### Basic Setup

Place `SbThemeProvider` at the root of your application:

```razor
@* App.razor or MainLayout.razor *@
<SbThemeProvider>
    <Router AppAssembly="@typeof(App).Assembly">
        <!-- ... -->
    </Router>
</SbThemeProvider>
```

### Dark Mode

```razor
<SbThemeProvider IsDark="true">
    @* All components will use dark theme *@
    <SbCard>
        <p>This card uses dark theme</p>
    </SbCard>
</SbThemeProvider>
```

### Toggle Dark Mode

```razor
<SbThemeProvider IsDark="@isDarkMode">
    <SbSwitch Label="Dark Mode" @bind-Value="isDarkMode" />
    
    <SbCard>
        <p>Theme-aware content</p>
    </SbCard>
</SbThemeProvider>

@code {
    private bool isDarkMode = false;
}
```

### RTL Support

```razor
<SbThemeProvider Direction="SbDirection.Rtl">
    <SbCard>
        <p>محتوای راست به چپ</p>
    </SbCard>
</SbThemeProvider>
```

### Using Theme Preset

```razor
<SbThemeProvider Theme="SbTheme.Dark">
    <SbCard>
        <p>Using dark theme preset</p>
    </SbCard>
</SbThemeProvider>
```

### Full Application Setup

App shell and top bar are provided by the **KomTheme** (e.g. `KomAppShell`, `KomTopBar`). For a minimal layout using only SufiBlazor primitives:

```razor
@* Minimal layout using design system only *@
@inherits LayoutComponentBase

<SbThemeProvider IsDark="@_isDarkMode" Direction="@_direction">
    <SbContainer MaxWidth="SbContainerMaxWidth.Lg">
        <SbStack Gap="4">
            <SbStack Direction="SbStackDirection.Row" Justify="SbJustify.End" Gap="2">
                <SbIconButton Icon="@(_isDarkMode ? "sun" : "moon")" OnClick="ToggleTheme" />
                <SbSelect @bind-Value="_directionValue" Style="width: 80px;">
                    <SbSelectOption Value="ltr">LTR</SbSelectOption>
                    <SbSelectOption Value="rtl">RTL</SbSelectOption>
                </SbSelect>
            </SbStack>
            @Body
        </SbStack>
    </SbContainer>
</SbThemeProvider>

@code {
    private bool _isDarkMode = false;
    private string _directionValue = "ltr";
    
    private SbDirection _direction => 
        _directionValue == "rtl" ? SbDirection.Rtl : SbDirection.Ltr;
    
    private void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
    }
}
```

### Accessing Theme Context

Child components can access the theme context via cascading parameter:

```razor
@code {
    [CascadingParameter]
    public SbThemeContext? ThemeContext { get; set; }
    
    protected override void OnParametersSet()
    {
        if (ThemeContext?.IsDark == true)
        {
            // Adjust component for dark mode
        }
    }
}
```

### Nested Theme Providers

You can nest providers for local theme overrides:

```razor
<SbThemeProvider IsDark="false">
    <SbCard>
        <p>Light theme card</p>
    </SbCard>
    
    <SbThemeProvider IsDark="true">
        <SbCard>
            <p>This card is dark themed</p>
        </SbCard>
    </SbThemeProvider>
</SbThemeProvider>
```

### Persisting Theme Preference

```razor
@inject IJSRuntime JS

<SbThemeProvider IsDark="@_isDarkMode">
    <SbSwitch Label="Dark Mode" 
              Value="@_isDarkMode"
              ValueChanged="SetDarkMode" />
</SbThemeProvider>

@code {
    private bool _isDarkMode;
    
    protected override async Task OnInitializedAsync()
    {
        var stored = await JS.InvokeAsync<string>("localStorage.getItem", "theme");
        _isDarkMode = stored == "dark";
    }
    
    private async Task SetDarkMode(bool value)
    {
        _isDarkMode = value;
        await JS.InvokeVoidAsync("localStorage.setItem", "theme", value ? "dark" : "light");
    }
}
```
