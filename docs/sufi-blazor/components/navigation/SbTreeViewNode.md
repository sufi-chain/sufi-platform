# SbTreeViewNode

An individual node component used internally by SbTreeView to render tree items recursively.

## Type Parameters

| Parameter | Description |
|-----------|-------------|
| TItem | The type of item in the tree |

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Item | TItem | required | The item to display |
| Level | int | 0 | Nesting level (0 for root) |
| ChildSelector | Func\<TItem, IEnumerable\<TItem\>\> | Returns empty | Function to get child items |
| TextSelector | Func\<TItem, string\> | ToString() | Function to get item display text |
| IconSelector | Func\<TItem, string?\>? | null | Function to get item icon name |
| IsExpandedSelector | Func\<TItem, bool\>? | null | Function to check expansion state |
| IsSelectedSelector | Func\<TItem, bool\>? | null | Function to check selection state |

## Cascading Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Parent | SbTreeView\<TItem\> | Parent tree view component |

## CSS Classes

- `sb-treeview-node` - Base class
- `sb-treeview-node--expanded` - When node is expanded
- `sb-treeview-node__content` - Content container
- `sb-treeview-node__content--selected` - When node is selected
- `sb-treeview-node__toggle` - Expand/collapse button
- `sb-treeview-node__spacer` - Spacer for leaf nodes
- `sb-treeview-node__checkbox` - Checkbox element
- `sb-treeview-node__icon` - Icon container
- `sb-treeview-node__text` - Text content
- `sb-treeview-node__chevron` - Expand indicator chevron
- `sb-treeview-node__children` - Children container

## Notes

This component is typically not used directly. It's automatically created by SbTreeView for each item in the tree. However, understanding its structure helps with custom styling.

## Internal Behavior

- Indentation is calculated based on `Level * 20 + 8` pixels
- Toggle button only appears for nodes with children
- Checkbox state is managed by the parent SbTreeView
- Selection click triggers parent's `SelectItem` method
- Toggle click triggers parent's `ToggleItem` method

## Styling Examples

### Custom Node Styling

```css
/* Highlight selected nodes */
.sb-treeview-node__content--selected {
    background-color: var(--sb-color-primary-light);
    border-radius: 4px;
}

/* Custom hover effect */
.sb-treeview-node__content:hover {
    background-color: var(--sb-color-surface-hover);
}

/* Increase indentation */
.sb-treeview-node__content {
    padding-left: calc(var(--level) * 24px + 12px);
}

/* Style the expand icon */
.sb-treeview-node__chevron {
    transition: transform 0.2s ease;
}

.sb-treeview-node--expanded .sb-treeview-node__chevron {
    transform: rotate(90deg);
}

/* Style icons by type */
.sb-treeview-node[data-type="folder"] .sb-treeview-node__icon {
    color: var(--sb-color-warning);
}

.sb-treeview-node[data-type="file"] .sb-treeview-node__icon {
    color: var(--sb-color-muted);
}
```

### Animation for Expand/Collapse

```css
.sb-treeview-node__children {
    overflow: hidden;
    animation: expandChildren 0.2s ease-out;
}

@keyframes expandChildren {
    from {
        opacity: 0;
        max-height: 0;
    }
    to {
        opacity: 1;
        max-height: 1000px;
    }
}
```

## Usage Context

Since SbTreeViewNode is used internally, here's how it fits into the tree rendering:

```razor
@* This is how SbTreeView uses SbTreeViewNode internally *@
<div class="sb-treeview">
    @foreach (var item in Items)
    {
        <SbTreeViewNode TItem="TItem"
                        Item="item"
                        Level="0"
                        ChildSelector="ChildSelector"
                        TextSelector="TextSelector"
                        IconSelector="IconSelector"
                        IsExpandedSelector="IsExpandedSelector"
                        IsSelectedSelector="IsSelectedSelector" />
    }
</div>

@* SbTreeViewNode then recursively renders children *@
<div class="sb-treeview-node">
    <div class="sb-treeview-node__content">
        <button class="sb-treeview-node__toggle">▶</button>
        <span class="sb-treeview-node__icon">📁</span>
        <span class="sb-treeview-node__text">Item Name</span>
    </div>
    <div class="sb-treeview-node__children">
        @* Recursive SbTreeViewNode for each child *@
    </div>
</div>
```
