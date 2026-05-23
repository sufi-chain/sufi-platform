# SbTreeView

A hierarchical tree view component for displaying nested data structures.

## Type Parameters

| Parameter | Description |
|-----------|-------------|
| TItem | The type of items in the tree |

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | IEnumerable\<TItem\> | Empty | Root items |
| ChildSelector | Func\<TItem, IEnumerable\<TItem\>\> | Returns empty | Function to get child items |
| TextSelector | Func\<TItem, string\> | ToString() | Function to get item display text |
| IconSelector | Func\<TItem, string?\>? | null | Function to get item icon name |
| IsExpandedSelector | Func\<TItem, bool\>? | null | Function to check if item is expanded |
| IsSelectedSelector | Func\<TItem, bool\>? | null | Function to check if item is selected |
| SelectedItem | TItem? | null | Currently selected item |
| ShowCheckboxes | bool | false | Whether to show checkboxes |
| CheckedItems | HashSet\<TItem\> | new() | Set of checked items |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| SelectedItemChanged | EventCallback\<TItem\> | Fired when item is selected |
| OnToggle | EventCallback\<TItem\> | Fired when item is expanded/collapsed |
| CheckedItemsChanged | EventCallback\<HashSet\<TItem\>\> | Fired when checked items change |

## CSS Classes

- `sb-treeview` - Base class

## Accessibility

- Uses `role="tree"` for the container
- Uses `role="treeitem"` for items
- Uses `role="group"` for child containers
- Uses `aria-level` for nesting depth
- Uses `aria-expanded` for expandable items

## Examples

### Basic Usage

```razor
<SbTreeView TItem="TreeNode"
            Items="@rootNodes"
            ChildSelector="@(n => n.Children)"
            TextSelector="@(n => n.Name)" />

@code {
    private List<TreeNode> rootNodes = new()
    {
        new TreeNode("Documents", new[]
        {
            new TreeNode("Work"),
            new TreeNode("Personal")
        }),
        new TreeNode("Pictures"),
        new TreeNode("Music")
    };
    
    public class TreeNode
    {
        public string Name { get; set; }
        public List<TreeNode> Children { get; set; } = new();
        
        public TreeNode(string name, IEnumerable<TreeNode>? children = null)
        {
            Name = name;
            Children = children?.ToList() ?? new();
        }
    }
}
```

### With Icons

```razor
<SbTreeView TItem="FileNode"
            Items="@files"
            ChildSelector="@(n => n.Children)"
            TextSelector="@(n => n.Name)"
            IconSelector="@(n => n.IsFolder ? "folder" : "file")" />

@code {
    private List<FileNode> files = new()
    {
        new FileNode("src", isFolder: true, children: new[]
        {
            new FileNode("components", isFolder: true),
            new FileNode("App.razor"),
            new FileNode("Program.cs")
        }),
        new FileNode("README.md")
    };
}
```

### Selectable Tree

```razor
<SbTreeView TItem="Category"
            Items="@categories"
            ChildSelector="@(c => c.SubCategories)"
            TextSelector="@(c => c.Name)"
            @bind-SelectedItem="selectedCategory" />

<SbText>Selected: @(selectedCategory?.Name ?? "None")</SbText>

@code {
    private Category? selectedCategory;
    private List<Category> categories = /* ... */;
}
```

### With Checkboxes

```razor
<SbTreeView TItem="Permission"
            Items="@permissions"
            ChildSelector="@(p => p.Children)"
            TextSelector="@(p => p.Name)"
            ShowCheckboxes="true"
            @bind-CheckedItems="checkedPermissions" />

<SbButton OnClick="SavePermissions">
    Save (@checkedPermissions.Count selected)
</SbButton>

@code {
    private HashSet<Permission> checkedPermissions = new();
    private List<Permission> permissions = new()
    {
        new Permission("Users", new[]
        {
            new Permission("View"),
            new Permission("Create"),
            new Permission("Edit"),
            new Permission("Delete")
        }),
        new Permission("Reports", new[]
        {
            new Permission("View"),
            new Permission("Export")
        })
    };
}
```

### Controlled Expansion

```razor
<SbTreeView TItem="TreeNode"
            Items="@nodes"
            ChildSelector="@(n => n.Children)"
            TextSelector="@(n => n.Name)"
            IsExpandedSelector="@(n => expandedNodes.Contains(n.Id))"
            OnToggle="HandleToggle" />

@code {
    private HashSet<int> expandedNodes = new() { 1, 2 };
    
    private void HandleToggle(TreeNode node)
    {
        if (expandedNodes.Contains(node.Id))
            expandedNodes.Remove(node.Id);
        else
            expandedNodes.Add(node.Id);
    }
}
```

### File Explorer

```razor
<SbTreeView TItem="FileSystemItem"
            Items="@fileSystem"
            ChildSelector="@(f => f.Children)"
            TextSelector="@(f => f.Name)"
            IconSelector="@GetIcon"
            @bind-SelectedItem="selectedFile"
            OnToggle="LoadChildren" />

@code {
    private List<FileSystemItem> fileSystem;
    private FileSystemItem? selectedFile;
    
    private string? GetIcon(FileSystemItem item) => item.Type switch
    {
        FileType.Folder => "folder",
        FileType.File => GetFileIcon(item.Extension),
        _ => null
    };
    
    private string GetFileIcon(string? ext) => ext switch
    {
        ".cs" => "file-code",
        ".json" => "file-json",
        ".md" => "file-text",
        ".jpg" or ".png" => "image",
        _ => "file"
    };
    
    private async Task LoadChildren(FileSystemItem folder)
    {
        if (folder.Type == FileType.Folder && !folder.ChildrenLoaded)
        {
            folder.Children = await LoadFromDisk(folder.Path);
            folder.ChildrenLoaded = true;
        }
    }
}
```

### Organization Hierarchy

```razor
<SbTreeView TItem="Employee"
            Items="@executives"
            ChildSelector="@(e => e.DirectReports)"
            TextSelector="@(e => $"{e.Name} - {e.Title}")"
            IconSelector="@(_ => "user")"
            @bind-SelectedItem="selectedEmployee" />

@if (selectedEmployee != null)
{
    <SbCard Class="mt-4">
        <SbStack Gap="2">
            <SbHeading Level="3">@selectedEmployee.Name</SbHeading>
            <SbText Color="muted">@selectedEmployee.Title</SbText>
            <SbText>@selectedEmployee.Email</SbText>
            <SbText>Direct Reports: @selectedEmployee.DirectReports.Count</SbText>
        </SbStack>
    </SbCard>
}
```
