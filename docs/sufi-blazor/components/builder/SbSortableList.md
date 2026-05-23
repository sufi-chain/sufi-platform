# SbSortableList

A generic list component that supports drag-and-drop reordering. Items can be sorted by dragging, with optional remove functionality.

## Type Parameters

| Parameter | Description |
|-----------|-------------|
| TItem | Type of items in the list |

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List<TItem> | [] | List of items to display |
| ItemTemplate | RenderFragment<TItem> | required | Template for rendering each item |
| ShowHandles | bool | true | Whether to show drag handles |
| Removable | bool | false | Whether items can be removed |
| Orientation | SbSortableOrientation | Vertical | List orientation |
| Disabled | bool | false | Whether sorting is disabled |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ItemsChanged | EventCallback<List<TItem>> | Fired when items are reordered |
| OnItemRemoved | EventCallback<TItem> | Fired when an item is removed |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ItemTemplate | RenderFragment<TItem> | Required template for each item |
| EmptyTemplate | RenderFragment? | Content shown when list is empty |

## Orientation Options

| Value | Description |
|-------|-------------|
| Vertical | Items stacked vertically (default) |
| Horizontal | Items arranged horizontally |

## CSS Classes

- `sb-sortable-list` - Base class
- `sb-sortable-list--horizontal` - Horizontal orientation
- `sb-sortable-list__item` - Item wrapper
- `sb-sortable-list__item--dragging` - Being dragged
- `sb-sortable-list__item--drop-target` - Drop target
- `sb-sortable-list__handle` - Drag handle container
- `sb-sortable-list__content` - Item content
- `sb-sortable-list__remove` - Remove button
- `sb-sortable-list__empty` - Empty state container

## Examples

### Basic Sortable List

```razor
<SbSortableList Items="@items" @bind-Items="items">
    <ItemTemplate Context="item">
        <span>@item.Name</span>
    </ItemTemplate>
</SbSortableList>

@code {
    private List<Item> items = new()
    {
        new() { Id = 1, Name = "First Item" },
        new() { Id = 2, Name = "Second Item" },
        new() { Id = 3, Name = "Third Item" }
    };
}
```

### With Remove Button

```razor
<SbSortableList Items="@tasks" 
                @bind-Items="tasks"
                Removable="true"
                OnItemRemoved="HandleRemove">
    <ItemTemplate Context="task">
        <SbCheckbox Label="@task.Title" @bind-Value="task.IsComplete" />
    </ItemTemplate>
</SbSortableList>

@code {
    private List<Task> tasks = new() { /* ... */ };
    
    private void HandleRemove(Task task)
    {
        Console.WriteLine($"Removed: {task.Title}");
    }
}
```

### Without Handles

```razor
<SbSortableList Items="@items" @bind-Items="items" ShowHandles="false">
    <ItemTemplate Context="item">
        <SbCard>
            <ChildContent>@item.Content</ChildContent>
        </SbCard>
    </ItemTemplate>
</SbSortableList>
```

### Horizontal Orientation

```razor
<SbSortableList Items="@tabs" 
                @bind-Items="tabs"
                Orientation="SbSortableOrientation.Horizontal">
    <ItemTemplate Context="tab">
        <SbChip>@tab.Label</SbChip>
    </ItemTemplate>
</SbSortableList>
```

### With Empty Template

```razor
<SbSortableList Items="@items" @bind-Items="items" Removable="true">
    <ItemTemplate Context="item">
        <div class="list-item">@item.Name</div>
    </ItemTemplate>
    <EmptyTemplate>
        <SbEmptyState IconText="📝"
                      Title="No items"
                      Description="Add some items to get started" />
    </EmptyTemplate>
</SbSortableList>
```

### Disabled State

```razor
<SbSortableList Items="@items" 
                @bind-Items="items" 
                Disabled="@isLocked">
    <ItemTemplate Context="item">
        <span>@item.Name</span>
    </ItemTemplate>
</SbSortableList>

<SbSwitch Label="Lock Order" @bind-Value="isLocked" />
```

### Task Prioritization

```razor
<SbCard>
    <Header>
        <SbHeading Level="4">Priority Tasks</SbHeading>
    </Header>
    <ChildContent>
        <SbSortableList Items="@priorityTasks" 
                        @bind-Items="priorityTasks"
                        Removable="true"
                        OnItemRemoved="ArchiveTask">
            <ItemTemplate Context="task">
                <div class="task-item">
                    <span class="priority-badge">#@(priorityTasks.IndexOf(task) + 1)</span>
                    <span class="task-title">@task.Title</span>
                    <SbStatusPill Status="@GetStatus(task)" />
                </div>
            </ItemTemplate>
            <EmptyTemplate>
                <SbEmptyState Title="All caught up!" 
                              Description="No priority tasks remaining" />
            </EmptyTemplate>
        </SbSortableList>
    </ChildContent>
</SbCard>
```

### Layer Ordering

```razor
<SbInspectorPanel Title="Layers">
    <SbSortableList Items="@layers" @bind-Items="layers">
        <ItemTemplate Context="layer">
            <div class="layer-item">
                <SbIcon Name="@(layer.Visible ? "eye" : "eye-off")" 
                        Size="SbSize.Sm"
                        @onclick="() => layer.Visible = !layer.Visible" />
                <span>@layer.Name</span>
                <SbIcon Name="@(layer.Locked ? "lock" : "unlock")" 
                        Size="SbSize.Sm"
                        @onclick="() => layer.Locked = !layer.Locked" />
            </div>
        </ItemTemplate>
    </SbSortableList>
</SbInspectorPanel>
```
