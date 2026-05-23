# SbContainer

A centered content container with responsive max-width presets and optional gutters.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| MaxWidth | SbContainerMaxWidth | Lg | Maximum width preset for the container |
| DisableGutters | bool | false | When true, removes horizontal padding |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Content to render inside the container |

## SbContainerMaxWidth Enum

```csharp
public enum SbContainerMaxWidth
{
    Sm,    // 640px
    Md,    // 768px
    Lg,    // 1024px
    Xl,    // 1280px
    Xxl,   // 1536px
    Fluid  // No max-width (full width)
}
```

## CSS Classes

- `sb-container` - Base class
- `sb-container--sm` - Small max-width (640px)
- `sb-container--md` - Medium max-width (768px)
- `sb-container--lg` - Large max-width (1024px)
- `sb-container--xl` - Extra large max-width (1280px)
- `sb-container--xxl` - 2X large max-width (1536px)
- `sb-container--fluid` - Full width container
- `sb-container--no-gutters` - Removes horizontal padding

## Examples

### Basic Usage

```razor
<SbContainer>
    <h1>Page Title</h1>
    <p>Content is centered with a default max-width of 1024px.</p>
</SbContainer>
```

### Small Container

```razor
<SbContainer MaxWidth="SbContainerMaxWidth.Sm">
    <SbCard>
        <p>Narrow content area, great for forms.</p>
    </SbCard>
</SbContainer>
```

### Full Width

```razor
<SbContainer MaxWidth="SbContainerMaxWidth.Fluid">
    <SbDataGrid Items="@items" />
</SbContainer>
```

### Without Gutters

```razor
<SbContainer DisableGutters="true">
    <div class="full-bleed-banner">
        Edge-to-edge content
    </div>
</SbContainer>
```

### Page Layout

Use `SbContainer` inside your layout (e.g. KomTheme layouts use it for main content):

```razor
<SbContainer MaxWidth="SbContainerMaxWidth.Xl">
    <SbStack Gap="6">
        <SbHeading Level="1">Dashboard</SbHeading>
        <SbGrid Columns="3" Gap="4">
            <SbStatCard Value="1,234" Label="Users" />
            <SbStatCard Value="$56K" Label="Revenue" />
            <SbStatCard Value="89%" Label="Uptime" />
        </SbGrid>
        <SbCard>
            <SbDataGrid Items="@recentOrders" />
        </SbCard>
    </SbStack>
</SbContainer>
```

### Form Container

```razor
<SbContainer MaxWidth="SbContainerMaxWidth.Sm">
    <SbCard>
        <SbStack Gap="4">
            <SbHeading Level="2">Sign In</SbHeading>
            <SbTextField Label="Email" @bind-Value="email" />
            <SbTextField Label="Password" Type="password" @bind-Value="password" />
            <SbButton FullWidth="true">Sign In</SbButton>
        </SbStack>
    </SbCard>
</SbContainer>
```
