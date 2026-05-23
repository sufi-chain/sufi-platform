# SbDragHandle

A dedicated drag handle element for reordering items. Provides both mouse drag and keyboard navigation support for accessibility.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Data | object? | null | Data to transfer during drag |
| Disabled | bool | false | Whether the handle is disabled |
| AriaLabel | string | "Drag to reorder" | Accessibility label |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnDragStarted | EventCallback<DragEventArgs> | Fired when drag begins |
| OnDragEnded | EventCallback<DragEventArgs> | Fired when drag ends |
| OnMoveUp | EventCallback | Fired on ArrowUp key press |
| OnMoveDown | EventCallback | Fired on ArrowDown key press |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Custom handle content (default: ⋮⋮) |

## CSS Classes

- `sb-drag-handle` - Base class
- `sb-drag-handle--disabled` - Disabled state
- `sb-drag-handle__icon` - Default icon element

## Accessibility

- Uses `role="button"` for screen readers
- Supports `tabindex` for keyboard focus
- Arrow keys trigger `OnMoveUp`/`OnMoveDown` for keyboard reordering
- `aria-label` describes the handle's purpose

## Examples

### Basic Handle

```razor
<div class="list-item">
    <SbDragHandle />
    <span>Item content</span>
</div>
```

### With Keyboard Support

```razor
@foreach (var (item, index) in items.Select((item, i) => (item, i)))
{
    <div class="list-item">
        <SbDragHandle OnMoveUp="() => MoveUp(index)"
                      OnMoveDown="() => MoveDown(index)"
                      OnDragStarted="e => StartDrag(index)"
                      OnDragEnded="e => EndDrag()" />
        <span>@item.Name</span>
    </div>
}

@code {
    private List<Item> items = new() { /* ... */ };
    
    private void MoveUp(int index)
    {
        if (index > 0)
        {
            var temp = items[index];
            items[index] = items[index - 1];
            items[index - 1] = temp;
        }
    }
    
    private void MoveDown(int index)
    {
        if (index < items.Count - 1)
        {
            var temp = items[index];
            items[index] = items[index + 1];
            items[index + 1] = temp;
        }
    }
}
```

### Custom Handle Icon

```razor
<SbDragHandle>
    <SbIcon Name="grip-vertical" />
</SbDragHandle>
```

### Disabled Handle

```razor
<SbDragHandle Disabled="@item.IsLocked" />
```

### In a Sortable Card List

```razor
<div class="card-list">
    @foreach (var card in cards)
    {
        <SbCard Class="sortable-card">
            <Header>
                <SbStack Direction="SbDirection.Row" Justify="SbJustify.SpaceBetween">
                    <SbDragHandle />
                    <SbHeading Level="4">@card.Title</SbHeading>
                    <SbIconButton Icon="more-vertical" />
                </SbStack>
            </Header>
            <ChildContent>
                @card.Content
            </ChildContent>
        </SbCard>
    }
</div>
```

### With Data Transfer

```razor
<SbDragHandle Data="@item"
              OnDragStarted="HandleDragStart"
              OnDragEnded="HandleDragEnd">
    <span class="grip">⠿</span>
</SbDragHandle>

@code {
    private void HandleDragStart(DragEventArgs args)
    {
        // Store dragged item reference
    }
    
    private void HandleDragEnd(DragEventArgs args)
    {
        // Clean up
    }
}
```
