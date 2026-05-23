# SbInspectorPanel

A collapsible properties panel for inspecting and editing selected elements. Commonly used as a sidebar in visual builders and design tools.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string | "Properties" | Panel title |
| Collapsible | bool | true | Whether panel can be collapsed |
| Collapsed | bool | false | Current collapsed state |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| CollapsedChanged | EventCallback<bool> | Fired when collapsed state changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Panel content (used when Sections is not provided) |
| Sections | RenderFragment? | Container for SbInspectorSection components |
| HeaderActions | RenderFragment? | Action buttons in the header |

## CSS Classes

- `sb-inspector-panel` - Base class
- `sb-inspector-panel--collapsed` - Collapsed state
- `sb-inspector-panel__header` - Header container
- `sb-inspector-panel__title` - Title text
- `sb-inspector-panel__toggle` - Collapse toggle button
- `sb-inspector-panel__header-actions` - Header actions container
- `sb-inspector-panel__body` - Body content area

## Examples

### Basic Inspector Panel

```razor
<SbInspectorPanel Title="Properties">
    <SbTextField Label="Name" @bind-Value="name" />
    <SbNumberField Label="Width" @bind-Value="width" />
    <SbNumberField Label="Height" @bind-Value="height" />
</SbInspectorPanel>
```

### With Sections

```razor
<SbInspectorPanel Title="Element Inspector">
    <Sections>
        <SbInspectorSection Title="Layout" DefaultExpanded="true">
            <SbNumberField Label="X" @bind-Value="x" />
            <SbNumberField Label="Y" @bind-Value="y" />
            <SbNumberField Label="Width" @bind-Value="width" />
            <SbNumberField Label="Height" @bind-Value="height" />
        </SbInspectorSection>
        
        <SbInspectorSection Title="Appearance">
            <SbColorPicker Label="Background" @bind-Value="bgColor" />
            <SbSlider Label="Opacity" @bind-Value="opacity" Min="0" Max="100" />
            <SbNumberField Label="Border Radius" @bind-Value="borderRadius" />
        </SbInspectorSection>
        
        <SbInspectorSection Title="Typography" DefaultExpanded="false">
            <SbSelect Label="Font Family" @bind-Value="fontFamily" Items="@fonts" />
            <SbNumberField Label="Font Size" @bind-Value="fontSize" />
            <SbColorPicker Label="Color" @bind-Value="textColor" />
        </SbInspectorSection>
    </Sections>
</SbInspectorPanel>
```

### With Header Actions

```razor
<SbInspectorPanel Title="Component Settings">
    <HeaderActions>
        <SbIconButton Icon="refresh" Size="SbSize.Sm" OnClick="Reset" />
        <SbIconButton Icon="copy" Size="SbSize.Sm" OnClick="CopyStyles" />
    </HeaderActions>
    <ChildContent>
        <!-- Properties -->
    </ChildContent>
</SbInspectorPanel>
```

### Controlled Collapse

```razor
<SbInspectorPanel Title="Details"
                  Collapsible="true"
                  Collapsed="@isCollapsed"
                  CollapsedChanged="@(v => isCollapsed = v)">
    <p>Panel content here</p>
</SbInspectorPanel>

<SbButton OnClick="() => isCollapsed = !isCollapsed">
    @(isCollapsed ? "Expand" : "Collapse")
</SbButton>

@code {
    private bool isCollapsed = false;
}
```

### Non-Collapsible Panel

```razor
<SbInspectorPanel Title="Required Settings" Collapsible="false">
    <SbTextField Label="ID" @bind-Value="elementId" Required="true" />
</SbInspectorPanel>
```

### Visual Builder Sidebar

```razor
<aside class="builder-sidebar">
    @if (selectedElement != null)
    {
        <SbInspectorPanel Title="@selectedElement.Type">
            <HeaderActions>
                <SbIconButton Icon="trash" 
                              Size="SbSize.Sm" 
                              OnClick="DeleteElement" />
            </HeaderActions>
            <Sections>
                <SbInspectorSection Title="Content">
                    @switch (selectedElement.Type)
                    {
                        case "text":
                            <SbTextArea Label="Text" @bind-Value="selectedElement.Content" />
                            break;
                        case "image":
                            <SbTextField Label="Image URL" @bind-Value="selectedElement.Src" />
                            <SbTextField Label="Alt Text" @bind-Value="selectedElement.Alt" />
                            break;
                    }
                </SbInspectorSection>
                
                <SbInspectorSection Title="Style">
                    <SbPropertyGrid Properties="@GetStyleProperties()" 
                                    OnPropertyChanged="UpdateStyle" />
                </SbInspectorSection>
            </Sections>
        </SbInspectorPanel>
    }
    else
    {
        <SbEmptyState Title="No Selection"
                      Description="Select an element to edit its properties" />
    }
</aside>
```

### Multiple Panels

```razor
<div class="inspector-container">
    <SbInspectorPanel Title="Transform" Collapsed="false">
        <SbNumberField Label="X" @bind-Value="x" />
        <SbNumberField Label="Y" @bind-Value="y" />
    </SbInspectorPanel>
    
    <SbInspectorPanel Title="Fill" Collapsed="true">
        <SbColorPicker Label="Color" @bind-Value="fillColor" />
    </SbInspectorPanel>
    
    <SbInspectorPanel Title="Stroke" Collapsed="true">
        <SbColorPicker Label="Color" @bind-Value="strokeColor" />
        <SbNumberField Label="Width" @bind-Value="strokeWidth" />
    </SbInspectorPanel>
</div>
```
