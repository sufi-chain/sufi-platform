# SbBreadcrumb

A breadcrumb navigation component showing the current page location within a hierarchy.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | List\<SbBreadcrumbItem\> | new() | List of breadcrumb items |
| Separator | string | "/" | Separator character between items |
| Class | string? | null | Additional CSS classes |

## SbBreadcrumbItem Class

Each item has `Text`, optional `Href` (null = current page, not a link), and optional `Icon`. The `Icon` value is rendered as text before the label (e.g. an icon name or character); for full icon components you would extend the item type or use a custom template.

```csharp
public class SbBreadcrumbItem
{
    public string Text { get; set; } = "";
    public string? Href { get; set; }
    public string? Icon { get; set; }
    
    public SbBreadcrumbItem() { }
    public SbBreadcrumbItem(string text, string? href = null) { }
}
```

## CSS Classes

- `sb-breadcrumb` - Base class
- `sb-breadcrumb__list` - Ordered list container
- `sb-breadcrumb__item` - Individual breadcrumb item
- `sb-breadcrumb__item--current` - Current/last item
- `sb-breadcrumb__link` - Clickable link
- `sb-breadcrumb__text` - Non-clickable text
- `sb-breadcrumb__icon` - Icon container
- `sb-breadcrumb__separator` - Separator element

## Accessibility

- Uses `nav` element with `aria-label="Breadcrumb"`
- Uses `ol` (ordered list) for proper semantics
- Last item has `aria-current="page"`
- Separators have `aria-hidden="true"`

## Examples

### Basic Usage

```razor
<SbBreadcrumb Items="@breadcrumbs" />

@code {
    private List<SbBreadcrumbItem> breadcrumbs = new()
    {
        new("Home", "/"),
        new("Products", "/products"),
        new("Electronics", "/products/electronics"),
        new("Laptops")  // Current page - no href
    };
}
```

### With Custom Separator

```razor
<SbBreadcrumb Items="@items" Separator=">" />
<SbBreadcrumb Items="@items" Separator="→" />
<SbBreadcrumb Items="@items" Separator="•" />
```

### With Icons

```razor
@code {
    private List<SbBreadcrumbItem> breadcrumbs = new()
    {
        new SbBreadcrumbItem { Text = "Home", Href = "/", Icon = "home" },
        new SbBreadcrumbItem { Text = "Users", Href = "/users", Icon = "users" },
        new SbBreadcrumbItem { Text = "John Doe", Icon = "user" }
    };
}
```

### Dynamic Breadcrumbs

```razor
<SbBreadcrumb Items="@BuildBreadcrumbs()" />

@code {
    private Category currentCategory;
    
    private List<SbBreadcrumbItem> BuildBreadcrumbs()
    {
        var items = new List<SbBreadcrumbItem>
        {
            new("Home", "/"),
            new("Categories", "/categories")
        };
        
        // Add parent categories
        var parent = currentCategory?.Parent;
        var ancestors = new List<Category>();
        while (parent != null)
        {
            ancestors.Insert(0, parent);
            parent = parent.Parent;
        }
        
        foreach (var ancestor in ancestors)
        {
            items.Add(new(ancestor.Name, $"/categories/{ancestor.Id}"));
        }
        
        // Add current category (no href)
        items.Add(new(currentCategory.Name));
        
        return items;
    }
}
```

### In Page Layout

Breadcrumbs are typically rendered in the layout's top bar (e.g. KomTheme's `KomTopBar`). Content area example:

```razor
<SbContainer>
    <SbStack Gap="4">
        <SbBreadcrumb Items="@breadcrumbs" />
        <h1>Page Content</h1>
    </SbStack>
</SbContainer>
```

### Admin Panel Style

```razor
<SbContainer>
    <SbStack Gap="4">
        <SbBreadcrumb Items="@new List<SbBreadcrumbItem>
        {
            new(\"Dashboard\", \"/admin\"),
            new(\"Content\", \"/admin/content\"),
            new(\"Articles\", \"/admin/content/articles\"),
            new(\"Edit Article\")
        }" />
        
        <SbCard>
            <SbHeading Level="1">Edit Article</SbHeading>
            <!-- Form content -->
        </SbCard>
    </SbStack>
</SbContainer>
```

### E-Commerce Category Navigation

```razor
@page "/products/{**path}"

<SbBreadcrumb Items="@BuildCategoryPath()" Separator="›" />

<SbGrid Columns="4" Gap="4">
    @foreach (var product in products)
    {
        <SbCard>
            <img src="@product.Image" alt="@product.Name" />
            <SbHeading Level="4">@product.Name</SbHeading>
            <SbText>@product.Price.ToString("C")</SbText>
        </SbCard>
    }
</SbGrid>

@code {
    [Parameter] public string? Path { get; set; }
    
    private List<SbBreadcrumbItem> BuildCategoryPath()
    {
        var items = new List<SbBreadcrumbItem> { new("Shop", "/products") };
        
        if (!string.IsNullOrEmpty(Path))
        {
            var segments = Path.Split('/');
            var currentPath = "/products";
            
            for (int i = 0; i < segments.Length; i++)
            {
                currentPath += "/" + segments[i];
                var isLast = i == segments.Length - 1;
                items.Add(new(segments[i].Replace("-", " ").ToTitleCase(), 
                              isLast ? null : currentPath));
            }
        }
        
        return items;
    }
}
```
