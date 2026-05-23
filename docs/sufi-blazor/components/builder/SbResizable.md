# SbResizable

A container that allows users to resize its content by dragging edge and corner handles. Useful for resizable panels, widgets, and elements in visual builders.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Width | double | 0 | Current width in pixels |
| Height | double | 0 | Current height in pixels |
| MinWidth | double | 50 | Minimum allowed width |
| MinHeight | double | 50 | Minimum allowed height |
| MaxWidth | double? | null | Maximum allowed width (null = unlimited) |
| MaxHeight | double? | null | Maximum allowed height (null = unlimited) |
| EnableTop | bool | false | Enable top edge resize |
| EnableRight | bool | true | Enable right edge resize |
| EnableBottom | bool | true | Enable bottom edge resize |
| EnableLeft | bool | false | Enable left edge resize |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| WidthChanged | EventCallback<double> | Fired when width changes |
| HeightChanged | EventCallback<double> | Fired when height changes |
| OnResizeStart | EventCallback | Fired when resize begins |
| OnResizeEnd | EventCallback | Fired when resize ends |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Content to make resizable |

## Properties

| Property | Type | Description |
|----------|------|-------------|
| IsResizing | bool | Indicates if currently resizing |

## Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| StopResize() | Task | Programmatically stops resize operation |

## CSS Classes

- `sb-resizable` - Base class
- `sb-resizable--resizing` - During resize
- `sb-resizable__handle` - Base handle class
- `sb-resizable__handle--top` - Top edge handle
- `sb-resizable__handle--right` - Right edge handle
- `sb-resizable__handle--bottom` - Bottom edge handle
- `sb-resizable__handle--left` - Left edge handle
- `sb-resizable__handle--top-right` - Top-right corner handle
- `sb-resizable__handle--bottom-right` - Bottom-right corner handle
- `sb-resizable__handle--bottom-left` - Bottom-left corner handle
- `sb-resizable__handle--top-left` - Top-left corner handle

## Examples

### Basic Resizable

```razor
<SbResizable @bind-Width="width" @bind-Height="height">
    <div class="resizable-content">
        Resize me! (@width x @height)
    </div>
</SbResizable>

@code {
    private double width = 200;
    private double height = 150;
}
```

### All Directions

```razor
<SbResizable @bind-Width="width" 
             @bind-Height="height"
             EnableTop="true"
             EnableRight="true"
             EnableBottom="true"
             EnableLeft="true">
    <div class="content">
        Resizable from all sides and corners
    </div>
</SbResizable>
```

### With Constraints

```razor
<SbResizable @bind-Width="width"
             @bind-Height="height"
             MinWidth="100"
             MinHeight="100"
             MaxWidth="500"
             MaxHeight="400">
    <div class="constrained-box">
        Min: 100x100, Max: 500x400
    </div>
</SbResizable>
```

### Horizontal Only

```razor
<SbResizable @bind-Width="width"
             @bind-Height="height"
             EnableRight="true"
             EnableLeft="true"
             EnableTop="false"
             EnableBottom="false">
    <div class="horizontal-resize">
        Horizontal resize only
    </div>
</SbResizable>
```

### Vertical Only

```razor
<SbResizable @bind-Width="width"
             @bind-Height="height"
             EnableRight="false"
             EnableLeft="false"
             EnableTop="true"
             EnableBottom="true">
    <div class="vertical-resize">
        Vertical resize only
    </div>
</SbResizable>
```

### With Resize Events

```razor
<SbResizable @bind-Width="width"
             @bind-Height="height"
             OnResizeStart="HandleResizeStart"
             OnResizeEnd="HandleResizeEnd">
    <div class="@(isResizing ? "resizing" : "")">
        @(isResizing ? "Resizing..." : "Ready")
    </div>
</SbResizable>

@code {
    private double width = 200, height = 200;
    private bool isResizing;
    
    private void HandleResizeStart()
    {
        isResizing = true;
    }
    
    private void HandleResizeEnd()
    {
        isResizing = false;
        Console.WriteLine($"Final size: {width}x{height}");
    }
}
```

### Resizable Panel in Layout

```razor
<div class="layout">
    <SbResizable @bind-Width="sidebarWidth"
                 Height="@double.MaxValue"
                 MinWidth="200"
                 MaxWidth="400"
                 EnableRight="true"
                 EnableTop="false"
                 EnableBottom="false"
                 EnableLeft="false">
        <nav class="sidebar">
            <h3>Sidebar</h3>
            <SbNavMenu Items="@menuItems" />
        </nav>
    </SbResizable>
    
    <main class="main-content">
        Main Content Area
    </main>
</div>

@code {
    private double sidebarWidth = 280;
}
```

### Resizable Widget

```razor
<SbResizable @bind-Width="widgetWidth"
             @bind-Height="widgetHeight"
             MinWidth="150"
             MinHeight="100"
             Class="dashboard-widget">
    <SbCard>
        <Header>
            <SbHeading Level="4">Widget</SbHeading>
        </Header>
        <ChildContent>
            <SbText>Resizable widget content</SbText>
        </ChildContent>
    </SbCard>
</SbResizable>
```
