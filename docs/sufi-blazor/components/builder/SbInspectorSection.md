# SbInspectorSection

A collapsible section within an SbInspectorPanel. Groups related properties together with an expandable/collapsible header.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string | "" | Section title |
| DefaultExpanded | bool | true | Initial expanded state |
| Class | string? | null | Additional CSS classes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Section content |
| Badge | RenderFragment? | Optional badge shown next to title |

## CSS Classes

- `sb-inspector-section` - Base class
- `sb-inspector-section--expanded` - Expanded state
- `sb-inspector-section__header` - Clickable header
- `sb-inspector-section__chevron` - Expand/collapse indicator
- `sb-inspector-section__title` - Title text
- `sb-inspector-section__badge` - Badge container
- `sb-inspector-section__content` - Content area

## Examples

### Basic Section

```razor
<SbInspectorSection Title="Layout">
    <SbNumberField Label="Width" @bind-Value="width" />
    <SbNumberField Label="Height" @bind-Value="height" />
</SbInspectorSection>
```

### Initially Collapsed

```razor
<SbInspectorSection Title="Advanced" DefaultExpanded="false">
    <SbTextField Label="Custom CSS" @bind-Value="customCss" />
    <SbTextField Label="Custom ID" @bind-Value="customId" />
</SbInspectorSection>
```

### With Badge

```razor
<SbInspectorSection Title="Filters">
    <Badge>
        <SbBadge Value="@activeFilters" Color="SbColor.Primary" />
    </Badge>
    <ChildContent>
        <SbCheckbox Label="Show Active" @bind-Value="showActive" />
        <SbCheckbox Label="Show Completed" @bind-Value="showCompleted" />
    </ChildContent>
</SbInspectorSection>
```

### Multiple Sections in Panel

```razor
<SbInspectorPanel Title="Element Properties">
    <Sections>
        <SbInspectorSection Title="Position">
            <SbStack Direction="SbDirection.Row" Gap="SbSpacing.Sm">
                <SbNumberField Label="X" @bind-Value="x" />
                <SbNumberField Label="Y" @bind-Value="y" />
            </SbStack>
        </SbInspectorSection>
        
        <SbInspectorSection Title="Size">
            <SbStack Direction="SbDirection.Row" Gap="SbSpacing.Sm">
                <SbNumberField Label="W" @bind-Value="width" />
                <SbNumberField Label="H" @bind-Value="height" />
            </SbStack>
            <SbCheckbox Label="Lock Aspect Ratio" @bind-Value="lockRatio" />
        </SbInspectorSection>
        
        <SbInspectorSection Title="Appearance" DefaultExpanded="true">
            <SbColorPicker Label="Fill" @bind-Value="fillColor" />
            <SbSlider Label="Opacity" @bind-Value="opacity" Min="0" Max="100" />
        </SbInspectorSection>
        
        <SbInspectorSection Title="Effects" DefaultExpanded="false">
            <Badge>
                @if (hasEffects)
                {
                    <span class="effect-indicator">●</span>
                }
            </Badge>
            <ChildContent>
                <SbSlider Label="Blur" @bind-Value="blur" Min="0" Max="20" />
                <SbSlider Label="Shadow" @bind-Value="shadow" Min="0" Max="50" />
            </ChildContent>
        </SbInspectorSection>
    </Sections>
</SbInspectorPanel>
```

### Typography Inspector

```razor
<SbInspectorSection Title="Typography">
    <SbSelect Label="Font" @bind-Value="fontFamily">
        <SbSelectOption Value="sans-serif">Sans Serif</SbSelectOption>
        <SbSelectOption Value="serif">Serif</SbSelectOption>
        <SbSelectOption Value="monospace">Monospace</SbSelectOption>
    </SbSelect>
    
    <SbStack Direction="SbDirection.Row" Gap="SbSpacing.Sm">
        <SbNumberField Label="Size" @bind-Value="fontSize" />
        <SbNumberField Label="Line Height" @bind-Value="lineHeight" />
    </SbStack>
    
    <SbSelect Label="Weight" @bind-Value="fontWeight">
        <SbSelectOption Value="300">Light</SbSelectOption>
        <SbSelectOption Value="400">Regular</SbSelectOption>
        <SbSelectOption Value="500">Medium</SbSelectOption>
        <SbSelectOption Value="700">Bold</SbSelectOption>
    </SbSelect>
    
    <SbColorPicker Label="Color" @bind-Value="textColor" />
</SbInspectorSection>
```

### Dynamic Sections

```razor
@foreach (var group in propertyGroups)
{
    <SbInspectorSection Title="@group.Name" DefaultExpanded="@group.IsDefault">
        @foreach (var prop in group.Properties)
        {
            @RenderPropertyEditor(prop)
        }
    </SbInspectorSection>
}
```
