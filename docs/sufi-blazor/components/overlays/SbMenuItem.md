# SbMenuItem

A menu item component used within SbMenu for individual menu options.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Text | string? | null | Menu item text |
| Shortcut | string? | null | Keyboard shortcut hint text |
| Disabled | bool | false | Whether the item is disabled |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClick | EventCallback | Fired when the item is clicked |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Icon content displayed before text |
| ChildContent | RenderFragment | Custom content (replaces Text) |

## Cascading Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| ParentMenu | SbMenu | Parent menu container |

## CSS Classes

- `sb-menu-item` - Base class
- `sb-menu-item--disabled` - When disabled
- `sb-menu-item__icon` - Icon container
- `sb-menu-item__text` - Text container
- `sb-menu-item__shortcut` - Shortcut hint

## Accessibility

- Uses `role="menuitem"`
- Supports keyboard activation (Enter/Space)
- `tabindex` managed for keyboard navigation

## Examples

### Basic Usage

```razor
<SbMenu @bind-Open="isOpen">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Menu</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Option 1" OnClick="HandleOption1" />
        <SbMenuItem Text="Option 2" OnClick="HandleOption2" />
        <SbMenuItem Text="Option 3" OnClick="HandleOption3" />
    </ChildContent>
</SbMenu>
```

### With Icon

```razor
<SbMenuItem Text="Edit" OnClick="Edit">
    <Icon>
        <SbIcon Name="edit" Size="SbSize.Sm" />
    </Icon>
</SbMenuItem>
```

### With Shortcut

```razor
<SbMenuItem Text="Save" Shortcut="Ctrl+S" OnClick="Save" />
<SbMenuItem Text="Copy" Shortcut="Ctrl+C" OnClick="Copy" />
<SbMenuItem Text="Paste" Shortcut="Ctrl+V" OnClick="Paste" />
```

### Disabled Item

```razor
<SbMenuItem Text="Delete" Disabled="@(!canDelete)" OnClick="Delete">
    <Icon><SbIcon Name="trash" /></Icon>
</SbMenuItem>

@code {
    private bool canDelete = false;
}
```

### Custom Content

```razor
<SbMenuItem OnClick="SelectUser">
    <ChildContent>
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <img src="/avatar.jpg" alt="John" class="avatar-sm" />
            <SbStack Gap="0">
                <SbText>John Doe</SbText>
                <SbText Size="xs" Color="muted">john@example.com</SbText>
            </SbStack>
        </SbStack>
    </ChildContent>
</SbMenuItem>
```

### With Status Indicator

```razor
<SbMenuItem OnClick="() => SetStatus('online')">
    <Icon>
        <span class="status-dot status-dot--online"></span>
    </Icon>
    <ChildContent>Online</ChildContent>
</SbMenuItem>

<SbMenuItem OnClick="() => SetStatus('away')">
    <Icon>
        <span class="status-dot status-dot--away"></span>
    </Icon>
    <ChildContent>Away</ChildContent>
</SbMenuItem>

<SbMenuItem OnClick="() => SetStatus('dnd')">
    <Icon>
        <span class="status-dot status-dot--dnd"></span>
    </Icon>
    <ChildContent>Do Not Disturb</ChildContent>
</SbMenuItem>
```

### Danger Item

```razor
<SbMenuItem Text="Delete" OnClick="Delete" Class="menu-item-danger">
    <Icon><SbIcon Name="trash" Color="SbColor.Danger" /></Icon>
</SbMenuItem>
```

### Complete File Menu Example

```razor
<SbMenu @bind-Open="fileMenuOpen">
    <AnchorContent>
        <SbButton Variant="SbButtonVariant.Ghost" OnClick="() => fileMenuOpen = !fileMenuOpen">
            File
        </SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="New" Shortcut="Ctrl+N" OnClick="NewFile">
            <Icon><SbIcon Name="file-plus" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Open" Shortcut="Ctrl+O" OnClick="OpenFile">
            <Icon><SbIcon Name="folder-open" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Open Recent">
            <Icon><SbIcon Name="clock" /></Icon>
        </SbMenuItem>
        
        <SbDivider />
        
        <SbMenuItem Text="Save" Shortcut="Ctrl+S" OnClick="SaveFile">
            <Icon><SbIcon Name="save" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Save As..." Shortcut="Ctrl+Shift+S" OnClick="SaveFileAs">
            <Icon><SbIcon Name="save" /></Icon>
        </SbMenuItem>
        
        <SbDivider />
        
        <SbMenuItem Text="Export" OnClick="Export">
            <Icon><SbIcon Name="download" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Print" Shortcut="Ctrl+P" OnClick="Print">
            <Icon><SbIcon Name="printer" /></Icon>
        </SbMenuItem>
        
        <SbDivider />
        
        <SbMenuItem Text="Close" Shortcut="Ctrl+W" OnClick="CloseFile">
            <Icon><SbIcon Name="x" /></Icon>
        </SbMenuItem>
    </ChildContent>
</SbMenu>
```
