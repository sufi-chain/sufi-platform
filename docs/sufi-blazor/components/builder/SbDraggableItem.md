# SbDraggableItem

A wrapper component that makes its content draggable via HTML5 drag and drop. Used in visual builders for drag-and-drop interfaces.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Data | object? | null | Data to transfer during drag operation |
| ItemType | string? | null | Type identifier for filtering drop targets |
| Disabled | bool | false | Whether dragging is disabled |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnDragStarted | EventCallback<SbDragStartEventArgs> | Fired when drag begins |
| OnDragEnded | EventCallback | Fired when drag ends |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Content to make draggable |

## SbDragStartEventArgs Class

| Property | Type | Description |
|----------|------|-------------|
| Data | object? | The data being dragged |
| ItemType | string? | The item type identifier |

## CSS Classes

- `sb-draggable-item` - Base class
- `sb-draggable-item--dragging` - Applied during drag
- `sb-draggable-item--disabled` - Disabled state

## Examples

### Basic Draggable Item

```razor
<SbDraggableItem Data="@item" OnDragStarted="HandleDragStart">
    <div class="my-item">
        Drag me!
    </div>
</SbDraggableItem>

@code {
    private object item = new { Id = 1, Name = "Item 1" };
    
    private void HandleDragStart(SbDragStartEventArgs args)
    {
        Console.WriteLine($"Dragging: {args.Data}");
    }
}
```

### With Item Type

```razor
<SbDraggableItem Data="@widget" ItemType="widget" OnDragStarted="HandleDragStart">
    <div class="widget-card">
        <SbIcon Name="grid" />
        <span>Widget</span>
    </div>
</SbDraggableItem>
```

### Palette of Draggable Components

```razor
<div class="component-palette">
    @foreach (var component in availableComponents)
    {
        <SbDraggableItem Data="@component" 
                         ItemType="@component.Type"
                         OnDragStarted="HandleDragStart"
                         OnDragEnded="HandleDragEnd">
            <div class="palette-item">
                <SbIcon Name="@component.Icon" />
                <span>@component.Name</span>
            </div>
        </SbDraggableItem>
    }
</div>

@code {
    private List<ComponentInfo> availableComponents = new()
    {
        new() { Type = "button", Name = "Button", Icon = "square" },
        new() { Type = "text", Name = "Text", Icon = "type" },
        new() { Type = "image", Name = "Image", Icon = "image" }
    };
    
    private ComponentInfo? draggedComponent;
    
    private void HandleDragStart(SbDragStartEventArgs args)
    {
        draggedComponent = args.Data as ComponentInfo;
    }
    
    private void HandleDragEnd()
    {
        draggedComponent = null;
    }
}
```

### Disabled State

```razor
<SbDraggableItem Data="@item" Disabled="@isLocked">
    <div class="locked-item">
        @if (isLocked)
        {
            <SbIcon Name="lock" />
        }
        Item Content
    </div>
</SbDraggableItem>
```

### Combined with SbDropZone

```razor
<div class="builder-layout">
    <div class="toolbox">
        <SbDraggableItem Data="@textBlock" ItemType="block">
            <div class="tool">Text Block</div>
        </SbDraggableItem>
        <SbDraggableItem Data="@imageBlock" ItemType="block">
            <div class="tool">Image Block</div>
        </SbDraggableItem>
    </div>
    
    <SbDropZone AcceptedTypes="@(new[] { "block" })"
                OnItemDropped="HandleDrop"
                PlaceholderText="Drag blocks here">
        @foreach (var block in pageBlocks)
        {
            <div class="placed-block">@block.Name</div>
        }
    </SbDropZone>
</div>
```
