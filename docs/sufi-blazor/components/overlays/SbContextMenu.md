# SbContextMenu

A right-click context menu component that appears at the cursor position.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Disabled | bool | false | Whether the context menu is disabled |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnOpen | EventCallback\<SbContextMenuEventArgs\> | Fired before menu opens (can cancel) |
| OnClose | EventCallback | Fired when menu closes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Content that triggers the context menu |
| MenuContent | RenderFragment | Menu items |

## Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| Close() | Task | Close the context menu |

## SbContextMenuEventArgs Class

```csharp
public class SbContextMenuEventArgs
{
    public double X { get; set; }   // X coordinate
    public double Y { get; set; }   // Y coordinate
    public bool Cancel { get; set; } // Set to true to cancel opening
}
```

## CSS Classes

- `sb-context-menu-trigger` - Trigger wrapper
- `sb-context-menu-overlay` - Backdrop overlay
- `sb-context-menu` - Menu container

## Examples

### Basic Usage

```razor
<SbContextMenu>
    <ChildContent>
        <div class="content-area">
            Right-click here for options
        </div>
    </ChildContent>
    <MenuContent>
        <SbMenuItem Text="Cut" OnClick="Cut">
            <Icon><SbIcon Name="SufiChainsors" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Copy" OnClick="Copy">
            <Icon><SbIcon Name="copy" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Paste" OnClick="Paste">
            <Icon><SbIcon Name="clipboard" /></Icon>
        </SbMenuItem>
    </MenuContent>
</SbContextMenu>
```

### File Browser Context Menu

```razor
<SbContextMenu>
    <ChildContent>
        <div class="file-item">
            <SbIcon Name="file" />
            <span>document.pdf</span>
        </div>
    </ChildContent>
    <MenuContent>
        <SbMenuItem Text="Open" OnClick="OpenFile">
            <Icon><SbIcon Name="external-link" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Download" OnClick="DownloadFile">
            <Icon><SbIcon Name="download" /></Icon>
        </SbMenuItem>
        <SbDivider />
        <SbMenuItem Text="Rename" OnClick="RenameFile">
            <Icon><SbIcon Name="edit" /></Icon>
        </SbMenuItem>
        <SbMenuItem Text="Move to..." OnClick="MoveFile">
            <Icon><SbIcon Name="folder" /></Icon>
        </SbMenuItem>
        <SbDivider />
        <SbMenuItem Text="Delete" OnClick="DeleteFile">
            <Icon><SbIcon Name="trash" Color="SbColor.Danger" /></Icon>
        </SbMenuItem>
    </MenuContent>
</SbContextMenu>
```

### DataGrid Row Context Menu

```razor
<SbDataGrid Items="@items">
    <SbColumn Field="Name" Title="Name" />
    <SbColumn Field="Status" Title="Status" />
    <RowTemplate Context="row">
        <SbContextMenu>
            <ChildContent>
                <tr>
                    <td>@row.Item.Name</td>
                    <td>@row.Item.Status</td>
                </tr>
            </ChildContent>
            <MenuContent>
                <SbMenuItem Text="View" OnClick="() => View(row.Item)" />
                <SbMenuItem Text="Edit" OnClick="() => Edit(row.Item)" />
                <SbDivider />
                <SbMenuItem Text="Delete" OnClick="() => Delete(row.Item)" />
            </MenuContent>
        </SbContextMenu>
    </RowTemplate>
</SbDataGrid>
```

### Conditional Menu Items

```razor
<SbContextMenu OnOpen="HandleOpen">
    <ChildContent>
        <div class="content">@selectedItem?.Name</div>
    </ChildContent>
    <MenuContent>
        <SbMenuItem Text="View" OnClick="View" />
        @if (canEdit)
        {
            <SbMenuItem Text="Edit" OnClick="Edit" />
        }
        @if (canDelete)
        {
            <SbDivider />
            <SbMenuItem Text="Delete" OnClick="Delete" />
        }
    </MenuContent>
</SbContextMenu>

@code {
    private void HandleOpen(SbContextMenuEventArgs args)
    {
        // Determine permissions based on context
        canEdit = currentUser.HasPermission("edit");
        canDelete = currentUser.HasPermission("delete");
    }
}
```

### Cancel Opening

```razor
<SbContextMenu OnOpen="HandleOpen">
    <ChildContent>
        <div>Right-click when enabled</div>
    </ChildContent>
    <MenuContent>
        <SbMenuItem Text="Action" OnClick="DoAction" />
    </MenuContent>
</SbContextMenu>

@code {
    private bool isEnabled = false;
    
    private void HandleOpen(SbContextMenuEventArgs args)
    {
        if (!isEnabled)
        {
            args.Cancel = true; // Prevent menu from opening
        }
    }
}
```

### Disabled Context Menu

```razor
<SbContextMenu Disabled="@isReadOnly">
    <ChildContent>
        <div class="document">
            Document content (context menu disabled in read-only mode)
        </div>
    </ChildContent>
    <MenuContent>
        <SbMenuItem Text="Edit" OnClick="Edit" />
        <SbMenuItem Text="Delete" OnClick="Delete" />
    </MenuContent>
</SbContextMenu>
```

### Canvas/Drawing Context Menu

```razor
<SbContextMenu @ref="canvasContextMenu" OnOpen="HandleCanvasContextMenu">
    <ChildContent>
        <canvas @ref="canvasRef" width="800" height="600" />
    </ChildContent>
    <MenuContent>
        @if (selectedShape != null)
        {
            <SbMenuItem Text="Edit Shape" OnClick="EditShape" />
            <SbMenuItem Text="Duplicate" OnClick="DuplicateShape" />
            <SbMenuItem Text="Bring to Front" OnClick="BringToFront" />
            <SbMenuItem Text="Send to Back" OnClick="SendToBack" />
            <SbDivider />
            <SbMenuItem Text="Delete" OnClick="DeleteShape" />
        }
        else
        {
            <SbMenuItem Text="Add Rectangle" OnClick="AddRectangle" />
            <SbMenuItem Text="Add Circle" OnClick="AddCircle" />
            <SbMenuItem Text="Add Text" OnClick="AddText" />
            <SbDivider />
            <SbMenuItem Text="Paste" OnClick="Paste" Disabled="@(!hasClipboard)" />
        }
    </MenuContent>
</SbContextMenu>

@code {
    private SbContextMenu canvasContextMenu;
    private Shape? selectedShape;
    
    private void HandleCanvasContextMenu(SbContextMenuEventArgs args)
    {
        // Check if a shape is at the click position
        selectedShape = FindShapeAt(args.X, args.Y);
    }
}
```

### Tree View Context Menu

```razor
<SbTreeView Items="@folders" ChildSelector="f => f.Children" TextSelector="f => f.Name">
    <ItemTemplate Context="folder">
        <SbContextMenu>
            <ChildContent>
                <span>@folder.Name</span>
            </ChildContent>
            <MenuContent>
                <SbMenuItem Text="New Folder" OnClick="() => NewFolder(folder)" />
                <SbMenuItem Text="New File" OnClick="() => NewFile(folder)" />
                <SbDivider />
                <SbMenuItem Text="Rename" OnClick="() => Rename(folder)" />
                <SbMenuItem Text="Delete" OnClick="() => Delete(folder)" 
                            Disabled="@(folder.Children.Any())" />
            </MenuContent>
        </SbContextMenu>
    </ItemTemplate>
</SbTreeView>
```
