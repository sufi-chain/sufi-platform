# SbMenu

A dropdown menu component with keyboard navigation support.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Open | bool | false | Whether the menu is open |
| Placement | SbPlacement | BottomStart | Menu placement relative to anchor |
| CloseOnItemClick | bool | true | Close when clicking a menu item |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OpenChanged | EventCallback\<bool\> | Fired when open state changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| AnchorContent | RenderFragment | Content that triggers the menu |
| ChildContent | RenderFragment | Menu items (SbMenuItem components) |

## Keyboard Navigation

- **ArrowDown**: Move to next item
- **ArrowUp**: Move to previous item
- **Home**: Move to first item
- **End**: Move to last item
- **Enter/Space**: Activate focused item
- **Escape**: Close menu
- **Tab**: Close menu

## CSS Classes

- `sb-menu-anchor` - Anchor wrapper
- `sb-menu` - Menu container
- `sb-menu--{placement}` - Placement variants

## Accessibility

- Uses `role="menu"` on the menu
- Uses `role="menuitem"` on items
- Supports keyboard navigation

## Examples

### Basic Usage

```razor
<SbMenu @bind-Open="isOpen">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">
            Options <SbIcon Name="chevron-down" />
        </SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Edit" OnClick="Edit" />
        <SbMenuItem Text="Duplicate" OnClick="Duplicate" />
        <SbMenuItem Text="Delete" OnClick="Delete" />
    </ChildContent>
</SbMenu>

@code {
    private bool isOpen = false;
}
```

### With Icons

```razor
<SbMenu @bind-Open="isOpen">
    <AnchorContent>
        <SbIconButton Icon="more-vertical" OnClick="() => isOpen = !isOpen" />
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Edit" OnClick="Edit">
            <Icon><SbIcon Name="edit" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Copy" OnClick="Copy">
            <Icon><SbIcon Name="copy" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Delete" OnClick="Delete">
            <Icon><SbIcon Name="trash" /></Icon>
        </SbMenuItem>
    </ChildContent>
</SbMenu>
```

### With Keyboard Shortcuts

```razor
<SbMenu @bind-Open="isOpen">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">File</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="New" Shortcut="Ctrl+N" OnClick="New" />
        <SbMenuItem Text="Open" Shortcut="Ctrl+O" OnClick="Open" />
        <SbMenuItem Text="Save" Shortcut="Ctrl+S" OnClick="Save" />
        <SbMenuItem Text="Save As..." Shortcut="Ctrl+Shift+S" OnClick="SaveAs" />
    </ChildContent>
</SbMenu>
```

### With Disabled Items

```razor
<SbMenu @bind-Open="isOpen">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Actions</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="View" OnClick="View" />
        <SbMenuItem Text="Edit" OnClick="Edit" Disabled="@(!canEdit)" />
        <SbMenuItem Text="Delete" OnClick="Delete" Disabled="@(!canDelete)" />
    </ChildContent>
</SbMenu>
```

### Different Placements

```razor
<SbMenu @bind-Open="isOpen" Placement="SbPlacement.BottomEnd">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Bottom End</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Option 1" />
        <SbMenuItem Text="Option 2" />
    </ChildContent>
</SbMenu>

<SbMenu @bind-Open="isOpen2" Placement="SbPlacement.TopStart">
    <AnchorContent>
        <SbButton OnClick="() => isOpen2 = !isOpen2">Top Start</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Option 1" />
        <SbMenuItem Text="Option 2" />
    </ChildContent>
</SbMenu>
```

### User Menu

```razor
<SbMenu @bind-Open="userMenuOpen">
    <AnchorContent>
        <button class="user-avatar-btn" @onclick="() => userMenuOpen = !userMenuOpen">
            <img src="/avatar.jpg" alt="@user.Name" />
            <span>@user.Name</span>
            <SbIcon Name="chevron-down" />
        </button>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem Text="Profile" OnClick="GoToProfile">
            <Icon><SbIcon Name="user" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Settings" OnClick="GoToSettings">
            <Icon><SbIcon Name="settings" /></Icon>
        </SbMenuItem>
        <SbDivider />
        <SbMenuItem Text="Sign Out" OnClick="SignOut">
            <Icon><SbIcon Name="log-out" /></Icon>
        </SbMenuItem>
    </ChildContent>
</SbMenu>
```

### Keep Open After Click

```razor
<SbMenu @bind-Open="isOpen" CloseOnItemClick="false">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Multi-Select</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbMenuItem OnClick="() => ToggleOption(1)">
            <SbCheckbox Value="@options.Contains(1)" />
            Option 1
        </SbMenuItem>
        <SbMenuItem OnClick="() => ToggleOption(2)">
            <SbCheckbox Value="@options.Contains(2)" />
            Option 2
        </SbMenuItem>
        <SbMenuItem OnClick="() => ToggleOption(3)">
            <SbCheckbox Value="@options.Contains(3)" />
            Option 3
        </SbMenuItem>
    </ChildContent>
</SbMenu>
```

### Row Actions Menu

```razor
<SbDataGrid Items="@items">
    <SbColumn Field="Name" Title="Name" />
    <SbColumn Field="Status" Title="Status" />
    <SbColumn Title="Actions" Width="60px">
        <CellTemplate Context="cell">
            <SbMenu>
                <AnchorContent>
                    <SbIconButton Icon="more-vertical" Size="SbSize.Sm" />
                </AnchorContent>
                <ChildContent>
                    <SbMenuItem Text="View" OnClick="() => View(cell.Item)" />
                    <SbMenuItem Text="Edit" OnClick="() => Edit(cell.Item)" />
                    <SbDivider />
                    <SbMenuItem Text="Delete" OnClick="() => Delete(cell.Item)">
                        <Icon><SbIcon Name="trash" Color="SbColor.Danger" /></Icon>
                    </SbMenuItem>
                </ChildContent>
            </SbMenu>
        </CellTemplate>
    </SbColumn>
</SbDataGrid>
```
