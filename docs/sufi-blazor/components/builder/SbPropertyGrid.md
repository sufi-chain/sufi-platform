# SbPropertyGrid

A grid-based property editor that automatically generates input controls based on property definitions. Useful for displaying and editing object properties in a consistent format.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Properties | List<SbPropertyDefinition> | [] | Property definitions to display |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnPropertyChanged | EventCallback<SbPropertyChangeEventArgs> | Fired when any property value changes |

## SbPropertyDefinition Class

| Property | Type | Description |
|----------|------|-------------|
| Name | string | Property identifier (used as input id) |
| Label | string | Display label |
| Value | object? | Current value |
| EditorType | SbPropertyEditorType | Type of editor to render |
| Required | bool | Whether property is required |
| Options | SbPropertyOption[]? | Options for Select editor type |

## SbPropertyEditorType Enum

| Value | Description |
|-------|-------------|
| Text | Text input |
| Number | Numeric input |
| Checkbox | Boolean checkbox |
| Select | Dropdown select |
| Color | Color picker |

## SbPropertyOption Class

| Property | Type | Description |
|----------|------|-------------|
| Value | string | Option value |
| Label | string | Display label |

## SbPropertyChangeEventArgs Class

| Property | Type | Description |
|----------|------|-------------|
| Property | SbPropertyDefinition | Changed property definition |
| NewValue | object? | New property value |

## CSS Classes

- `sb-property-grid` - Base class
- `sb-property-grid__row` - Property row
- `sb-property-grid__label` - Property label
- `sb-property-grid__required` - Required indicator
- `sb-property-grid__value` - Value container
- `sb-property-grid__input` - Text/number input
- `sb-property-grid__checkbox` - Checkbox input
- `sb-property-grid__select` - Select input
- `sb-property-grid__color` - Color input

## Examples

### Basic Property Grid

```razor
<SbPropertyGrid Properties="@properties" OnPropertyChanged="HandleChange" />

@code {
    private List<SbPropertyDefinition> properties = new()
    {
        new() { Name = "name", Label = "Name", EditorType = SbPropertyEditorType.Text, Value = "My Element" },
        new() { Name = "width", Label = "Width", EditorType = SbPropertyEditorType.Number, Value = 100 },
        new() { Name = "visible", Label = "Visible", EditorType = SbPropertyEditorType.Checkbox, Value = true }
    };
    
    private void HandleChange(SbPropertyChangeEventArgs args)
    {
        Console.WriteLine($"{args.Property.Name} = {args.NewValue}");
    }
}
```

### With All Editor Types

```razor
<SbPropertyGrid Properties="@allProperties" OnPropertyChanged="HandleChange" />

@code {
    private List<SbPropertyDefinition> allProperties = new()
    {
        new() 
        { 
            Name = "title", 
            Label = "Title", 
            EditorType = SbPropertyEditorType.Text, 
            Value = "Hello",
            Required = true
        },
        new() 
        { 
            Name = "count", 
            Label = "Count", 
            EditorType = SbPropertyEditorType.Number, 
            Value = 42 
        },
        new() 
        { 
            Name = "enabled", 
            Label = "Enabled", 
            EditorType = SbPropertyEditorType.Checkbox, 
            Value = true 
        },
        new() 
        { 
            Name = "size", 
            Label = "Size", 
            EditorType = SbPropertyEditorType.Select,
            Value = "md",
            Options = new SbPropertyOption[]
            {
                new() { Value = "sm", Label = "Small" },
                new() { Value = "md", Label = "Medium" },
                new() { Value = "lg", Label = "Large" }
            }
        },
        new() 
        { 
            Name = "color", 
            Label = "Color", 
            EditorType = SbPropertyEditorType.Color, 
            Value = "#3b82f6" 
        }
    };
}
```

### In Inspector Panel

```razor
<SbInspectorPanel Title="Button Properties">
    <SbPropertyGrid Properties="@buttonProperties" 
                    OnPropertyChanged="UpdateButton" />
</SbInspectorPanel>

@code {
    private List<SbPropertyDefinition> buttonProperties = new()
    {
        new() { Name = "text", Label = "Text", EditorType = SbPropertyEditorType.Text, Value = "Click Me" },
        new() { Name = "variant", Label = "Variant", EditorType = SbPropertyEditorType.Select, 
                Value = "primary",
                Options = new SbPropertyOption[]
                {
                    new() { Value = "primary", Label = "Primary" },
                    new() { Value = "secondary", Label = "Secondary" },
                    new() { Value = "outlined", Label = "Outlined" }
                }
        },
        new() { Name = "disabled", Label = "Disabled", EditorType = SbPropertyEditorType.Checkbox, Value = false }
    };
}
```

### Dynamic Properties from Object

```razor
@code {
    private Element selectedElement;
    
    private List<SbPropertyDefinition> GetPropertiesFromElement()
    {
        return new List<SbPropertyDefinition>
        {
            new() { Name = "id", Label = "ID", EditorType = SbPropertyEditorType.Text, 
                    Value = selectedElement.Id, Required = true },
            new() { Name = "x", Label = "X Position", EditorType = SbPropertyEditorType.Number, 
                    Value = selectedElement.X },
            new() { Name = "y", Label = "Y Position", EditorType = SbPropertyEditorType.Number, 
                    Value = selectedElement.Y },
            new() { Name = "background", Label = "Background", EditorType = SbPropertyEditorType.Color, 
                    Value = selectedElement.BackgroundColor }
        };
    }
    
    private void HandleChange(SbPropertyChangeEventArgs args)
    {
        switch (args.Property.Name)
        {
            case "id": selectedElement.Id = args.NewValue?.ToString(); break;
            case "x": selectedElement.X = Convert.ToDouble(args.NewValue); break;
            case "y": selectedElement.Y = Convert.ToDouble(args.NewValue); break;
            case "background": selectedElement.BackgroundColor = args.NewValue?.ToString(); break;
        }
    }
}
```
