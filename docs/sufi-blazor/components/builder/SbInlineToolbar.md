# SbInlineToolbar

A floating toolbar for context-specific actions. Typically appears near selected elements in visual editors and builders.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Actions | List<SbToolbarAction> | [] | Toolbar action buttons |
| Visible | bool | true | Whether the toolbar is visible |
| Position | SbToolbarPosition | Top | Position relative to content |
| X | double? | null | Absolute X position (when Position is Float) |
| Y | double? | null | Absolute Y position (when Position is Float) |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnAction | EventCallback<SbToolbarAction> | Fired when any action is clicked |

## SbToolbarAction Class

| Property | Type | Description |
|----------|------|-------------|
| Id | string | Unique identifier |
| Label | string? | Button label text |
| IconText | string? | Icon text (emoji or symbol) |
| Icon | RenderFragment? | Custom icon content |
| Tooltip | string? | Tooltip text |
| Disabled | bool | Whether action is disabled |
| IsActive | bool | Whether action is active/selected |
| IsDanger | bool | Whether action is destructive |
| IsSeparator | bool | Renders as separator instead of button |
| OnClick | Action? | Direct click handler |

## Position Options

| Value | Description |
|-------|-------------|
| Top | Above the content |
| Bottom | Below the content |
| Left | Left side of content |
| Right | Right side of content |
| Float | Absolute positioning using X/Y |

## Static Methods

```csharp
SbToolbarAction.Separator() // Creates a separator action
```

## CSS Classes

- `sb-inline-toolbar` - Base class
- `sb-inline-toolbar--visible` - Visible state
- `sb-inline-toolbar__separator` - Separator element
- `sb-inline-toolbar__btn` - Action button
- `sb-inline-toolbar__btn--active` - Active state
- `sb-inline-toolbar__btn--danger` - Danger variant
- `sb-inline-toolbar__icon` - Icon element
- `sb-inline-toolbar__label` - Label element

## Examples

### Basic Toolbar

```razor
<SbInlineToolbar Actions="@toolbarActions" OnAction="HandleAction" />

@code {
    private List<SbToolbarAction> toolbarActions = new()
    {
        new() { Id = "bold", IconText = "B", Tooltip = "Bold" },
        new() { Id = "italic", IconText = "I", Tooltip = "Italic" },
        new() { Id = "underline", IconText = "U", Tooltip = "Underline" }
    };
    
    private void HandleAction(SbToolbarAction action)
    {
        Console.WriteLine($"Action: {action.Id}");
    }
}
```

### With Separators and Danger Actions

```razor
<SbInlineToolbar Actions="@editActions" />

@code {
    private List<SbToolbarAction> editActions = new()
    {
        new() { Id = "edit", IconText = "✏️", Tooltip = "Edit" },
        new() { Id = "duplicate", IconText = "📋", Tooltip = "Duplicate" },
        SbToolbarAction.Separator(),
        new() { Id = "delete", IconText = "🗑️", Tooltip = "Delete", IsDanger = true }
    };
}
```

### With Active State

```razor
<SbInlineToolbar Actions="@formatActions" />

@code {
    private List<SbToolbarAction> formatActions = new()
    {
        new() { Id = "bold", IconText = "B", IsActive = isBold },
        new() { Id = "italic", IconText = "I", IsActive = isItalic },
        new() { Id = "underline", IconText = "U", IsActive = isUnderline }
    };
    
    private bool isBold, isItalic, isUnderline;
}
```

### Floating Position

```razor
<SbInlineToolbar Actions="@actions"
                 Position="SbToolbarPosition.Float"
                 X="@toolbarX"
                 Y="@toolbarY"
                 Visible="@showToolbar" />

@code {
    private double toolbarX, toolbarY;
    private bool showToolbar;
    
    private void ShowToolbarAt(double x, double y)
    {
        toolbarX = x;
        toolbarY = y;
        showToolbar = true;
    }
}
```

### Rich Text Editor Toolbar

```razor
<div class="editor-container">
    <SbInlineToolbar Actions="@editorActions"
                     Position="SbToolbarPosition.Top"
                     Visible="@hasSelection"
                     OnAction="ApplyFormat" />
    
    <div class="editor-content" @onmouseup="CheckSelection">
        @content
    </div>
</div>

@code {
    private bool hasSelection;
    
    private List<SbToolbarAction> editorActions = new()
    {
        new() { Id = "bold", IconText = "B", Tooltip = "Bold (Ctrl+B)" },
        new() { Id = "italic", IconText = "I", Tooltip = "Italic (Ctrl+I)" },
        SbToolbarAction.Separator(),
        new() { Id = "link", IconText = "🔗", Tooltip = "Insert Link" },
        new() { Id = "highlight", IconText = "🖍️", Tooltip = "Highlight" }
    };
}
```

### Element Selection Toolbar

```razor
@if (selectedElement != null)
{
    <SbInlineToolbar Actions="@elementActions"
                     Position="SbToolbarPosition.Top"
                     OnAction="HandleElementAction">
    </SbInlineToolbar>
}

@code {
    private object? selectedElement;
    
    private List<SbToolbarAction> elementActions = new()
    {
        new() { Id = "move", IconText = "↕️", Tooltip = "Move" },
        new() { Id = "resize", IconText = "↔️", Tooltip = "Resize" },
        new() { Id = "settings", IconText = "⚙️", Tooltip = "Settings" },
        SbToolbarAction.Separator(),
        new() { Id = "delete", IconText = "🗑️", Tooltip = "Delete", IsDanger = true }
    };
}
```

### Hidden/Visible Toggle

```razor
<SbInlineToolbar Actions="@actions"
                 Visible="@isToolbarVisible" />

<SbButton OnClick="() => isToolbarVisible = !isToolbarVisible">
    Toggle Toolbar
</SbButton>
```
