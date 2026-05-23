# SbTable

A simple table component for displaying data with `TItem` and `Items`, without the advanced features of SbDataGrid.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| TItem | (generic) | - | Item type for data and row template |
| Items | IEnumerable&lt;TItem&gt;? | null | Data items to render |
| Striped | bool | false | Whether to show striped rows |
| Hover | bool | true | Whether rows highlight on hover |
| Bordered | bool | false | Whether to show borders |
| Compact | bool | false | Whether to use compact spacing |
| OnRowClick | EventCallback&lt;TItem&gt; | - | Callback when a row is clicked |
| Class | string? | null | Additional CSS classes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| HeaderContent | RenderFragment | Table header (e.g. `<tr><th>...</th></tr>`) |
| RowTemplate | RenderFragment&lt;TItem&gt; | Row content; receives the item as `context` |
| FooterContent | RenderFragment? | Optional table footer |
| EmptyContent | RenderFragment? | Content when `Items` is null or empty |

## Usage

Bind a list and define the header and row template:

```razor
<SbTable TItem="Product" Items="_products">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
</SbTable>
```

## CSS Classes

- `sb-table` - Base class
- `sb-table--striped` - Striped rows
- `sb-table--hover` - Hoverable rows
- `sb-table--bordered` - Bordered style
- `sb-table--compact` - Compact spacing

## Examples

### Basic Table

```razor
<SbTable TItem="Product" Items="_products">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
            <th>Stock</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
        <td>@context.Stock</td>
    </RowTemplate>
</SbTable>
```

### Striped Table

```razor
<SbTable TItem="Product" Items="_products" Striped="true">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
</SbTable>
```

### Bordered Table

```razor
<SbTable TItem="Product" Items="_products" Bordered="true">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
</SbTable>
```

### Compact Table

```razor
<SbTable TItem="Product" Items="_products" Compact="true">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
</SbTable>
```

### Empty State

When `Items` is null or empty, use `EmptyContent` to show a message:

```razor
<SbTable TItem="Product" Items="@(new List<Product>())">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
    <EmptyContent>
        <div style="padding: 40px; text-align: center; color: var(--sb-color-text-muted);">
            <SbIcon Name="search" Size="SbSize.Lg" style="margin-bottom: 8px; opacity: 0.5;" />
            <p>No products found.</p>
        </div>
    </EmptyContent>
</SbTable>
```

### Clickable Rows

Use `OnRowClick` to handle row clicks:

```razor
<SbTable TItem="Product" Items="_products" OnRowClick="OnProductClick">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Category</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>@context.Category</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
</SbTable>
<SbText Variant="SbTextVariant.Caption">Clicked: @(_clickedProduct?.Name ?? "(none)")</SbText>
```

### With Footer

Use `FooterContent` for a footer row (e.g. totals):

```razor
<SbTable TItem="Product" Items="_products">
    <HeaderContent>
        <tr>
            <th>Name</th>
            <th>Price</th>
        </tr>
    </HeaderContent>
    <RowTemplate>
        <td>@context.Name</td>
        <td>$@context.Price.ToString("N2")</td>
    </RowTemplate>
    <FooterContent>
        <tr>
            <td><strong>Total</strong></td>
            <td><strong>@_products.Sum(p => p.Price).ToString("N2")</strong></td>
        </tr>
    </FooterContent>
</SbTable>
```
