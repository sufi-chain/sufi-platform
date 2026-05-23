# SbDropZone

A designated area that accepts dragged items. Provides visual feedback during drag operations and can filter acceptable item types.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| PlaceholderText | string | "Drag items here" | Text shown when zone is empty |
| DropText | string | "Drop here" | Text shown during drag over |
| AcceptedTypes | string[]? | null | Array of accepted item types (null accepts all) |
| Disabled | bool | false | Whether the zone accepts drops |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnItemDropped | EventCallback<SbDropEventArgs> | Fired when item is dropped |
| OnDragEnter | EventCallback | Fired when drag enters zone |
| OnDragExit | EventCallback | Fired when drag leaves zone |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Content to show inside the zone |

## SbDropEventArgs Class

| Property | Type | Description |
|----------|------|-------------|
| ClientX | double | X coordinate of drop |
| ClientY | double | Y coordinate of drop |
| Data | object? | Dropped data |

## CSS Classes

- `sb-dropzone` - Base class
- `sb-dropzone--drag-over` - Item is being dragged over
- `sb-dropzone--disabled` - Disabled state
- `sb-dropzone__indicator` - Drop indicator
- `sb-dropzone__placeholder` - Empty placeholder
- `sb-dropzone__icon` - Icon element
- `sb-dropzone__text` - Text element

## Examples

### Basic Drop Zone

```razor
<SbDropZone OnItemDropped="HandleDrop" />

@code {
    private void HandleDrop(SbDropEventArgs args)
    {
        Console.WriteLine($"Dropped at ({args.ClientX}, {args.ClientY})");
    }
}
```

### With Custom Placeholder

```razor
<SbDropZone PlaceholderText="Drop components here"
            DropText="Release to add"
            OnItemDropped="HandleDrop" />
```

### With Type Filtering

```razor
<SbDropZone AcceptedTypes="@(new[] { "widget", "component" })"
            PlaceholderText="Drop widgets or components here"
            OnItemDropped="HandleDrop" />
```

### With Content

```razor
<SbDropZone OnItemDropped="HandleDrop">
    @if (droppedItems.Any())
    {
        @foreach (var item in droppedItems)
        {
            <div class="dropped-item">@item.Name</div>
        }
    }
</SbDropZone>
```

### Page Builder Canvas

```razor
<div class="page-builder">
    <aside class="toolbox">
        <SbDraggableItem Data="@textBlock" ItemType="block">
            <div class="tool">📝 Text</div>
        </SbDraggableItem>
        <SbDraggableItem Data="@imageBlock" ItemType="block">
            <div class="tool">🖼️ Image</div>
        </SbDraggableItem>
    </aside>
    
    <main class="canvas">
        <SbDropZone AcceptedTypes="@(new[] { "block" })"
                    PlaceholderText="Drag blocks to build your page"
                    DropText="Drop to add block"
                    OnItemDropped="AddBlock"
                    OnDragEnter="ShowDropGuide"
                    OnDragExit="HideDropGuide">
            @foreach (var block in pageBlocks)
            {
                <div class="page-block">
                    @block.Content
                </div>
            }
        </SbDropZone>
    </main>
</div>

@code {
    private List<Block> pageBlocks = new();
    
    private void AddBlock(SbDropEventArgs args)
    {
        if (args.Data is Block block)
        {
            pageBlocks.Add(block);
        }
    }
}
```

### Multiple Drop Zones

```razor
<div class="kanban-board">
    @foreach (var column in columns)
    {
        <div class="kanban-column">
            <h3>@column.Title</h3>
            <SbDropZone AcceptedTypes="@(new[] { "task" })"
                        OnItemDropped="e => MoveTask(e, column.Id)"
                        PlaceholderText="Drop tasks here">
                @foreach (var task in column.Tasks)
                {
                    <SbDraggableItem Data="@task" ItemType="task">
                        <div class="task-card">@task.Title</div>
                    </SbDraggableItem>
                }
            </SbDropZone>
        </div>
    }
</div>
```

### Disabled Drop Zone

```razor
<SbDropZone Disabled="@isReadOnly"
            PlaceholderText="@(isReadOnly ? "Read-only mode" : "Drop items here")"
            OnItemDropped="HandleDrop" />
```
