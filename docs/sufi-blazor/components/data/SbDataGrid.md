# SbDataGrid

A powerful data grid component for displaying tabular data with support for sorting, filtering, pagination, selection, and custom templates.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Items | IEnumerable\<TItem\>? | null | Data items to display (client-side) |
| ItemsProvider | Func\<SbDataRequest, Task\<SbDataResponse\<TItem\>\>\>? | null | Async data provider for server-side paging/sorting |
| PageIndex | int | 0 | Current page index (0-based) |
| PageIndexChanged | EventCallback\<int\> | - | Callback when page index changes |
| PageSize | int | 10 | Items per page |
| PageSizeChanged | EventCallback\<int\> | - | Callback when page size changes |
| TotalCount | long | 0 | Total item count (for server-side paging) |
| Loading | bool | false | Whether data is loading |
| SelectionMode | SbSelectionMode | None | Selection mode (None, SingleRow, MultipleRows) |
| SelectedKeys | IReadOnlySet\<string\>? | null | Currently selected row keys |
| SelectedKeysChanged | EventCallback\<IReadOnlySet\<string\>\> | - | Callback when selected keys change |
| SelectedItemsChanged | EventCallback\<IReadOnlyList\<TItem\>\> | - | Callback when selected items change |
| KeySelector | Func\<TItem, string\>? | null | Function to get the unique key for each row (required for selection) |
| ShowPagination | bool | true | Whether to show pagination |
| ShowPageSizeSelector | bool | true | Whether to show page size dropdown |
| PageSizeOptions | int[] | { 5, 10, 25, 50, 100 } | Page size options |
| EmptyTemplate | RenderFragment? | null | Content shown when no data |
| OnRowClicked | EventCallback\<TItem\> | - | Callback when a row is clicked |
| OnSortChanged | EventCallback\<SbSort?\> | - | Callback when sort changes |
| Class | string? | null | Additional CSS classes |
| RightToLeft | bool? | null | Whether the grid is RTL |
| Density | SbDataGridDensity | Default | Row density (Default, Compact) |
| Striped | bool | false | Whether to show striped rows |
| Hoverable | bool | true | Whether rows highlight on hover |
| Bordered | bool | true | Whether to show borders |
| ShowColumnFilters | bool | true | Whether to show column filters |
| AllowColumnResize | bool | false | Whether columns can be resized |
| DetailTemplate | RenderFragment\<TItem\>? | null | Expandable detail row content |
| ShowFilterBar | bool | true | Whether to show filter bar |
| Height | string? | null | Fixed height (e.g. for virtualization) |

Additional parameters include VirtualizationEnabled, RowHeight, OverscanCount, EditMode, OnRowEditing, OnRowEdited, OnColumnResized, ExpandedRowKeys, OnRowExpandChanged, and others.

## Events

| Event | Type | Description |
|-------|------|-------------|
| SelectedKeysChanged | EventCallback\<IReadOnlySet\<string\>\> | Fired when selected keys change |
| SelectedItemsChanged | EventCallback\<IReadOnlyList\<TItem\>\> | Fired when selection changes (provides selected items) |
| OnSortChanged | EventCallback\<SbSort?\> | Fired when sort changes |
| PageIndexChanged | EventCallback\<int\> | Fired when page index changes |
| PageSizeChanged | EventCallback\<int\> | Fired when page size changes |
| OnRowClicked | EventCallback\<TItem\> | Fired when a row is clicked |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | SbColumn definitions |
| EmptyTemplate | RenderFragment | Content shown when no data |
| DetailTemplate | RenderFragment\<TItem\> | Expandable detail row content |

### Template Usage Examples

#### Column Definitions

```razor
<SbDataGrid TItem="Product" Items="@products">
    <SbColumn TItem="Product" Field="Name" Title="Product Name" />
    <SbColumn TItem="Product" Field="Price" Title="Price" Format="C2" />
    <SbColumn TItem="Product" Field="Stock" Title="Stock" />
    <SbColumn TItem="Product" Title="Actions">
        <CellTemplate Context="product">
            <SbButton Size="SbSize.Sm" OnClick="() => Edit(product)">Edit</SbButton>
        </CellTemplate>
    </SbColumn>
</SbDataGrid>
```

#### EmptyTemplate

```razor
<SbDataGrid TItem="Order" Items="@orders">
    <EmptyTemplate>
        <SbEmptyState>
            <SbIcon Name="package" Size="SbSize.Xl" />
            <SbHeading Level="4">No orders found</SbHeading>
            <SbText>Create your first order to get started.</SbText>
            <SbButton OnClick="CreateOrder">Create Order</SbButton>
        </SbEmptyState>
    </EmptyTemplate>
    <ChildContent>
        <!-- Column definitions -->
    </ChildContent>
</SbDataGrid>
```

#### DetailTemplate (Expandable Rows)

```razor
<SbDataGrid TItem="Order" Items="@orders">
    <DetailTemplate Context="order">
        <SbCard>
            <SbHeading Level="5">Order Items</SbHeading>
            @foreach (var item in order.Items)
            {
                <div>@item.Name - @item.Quantity x @item.Price.ToString("C")</div>
            }
        </SbCard>
    </DetailTemplate>
    <!-- Columns -->
</SbDataGrid>
```

## CSS Classes

- `sb-data-grid` - Base class
- `sb-data-grid--striped` - Striped rows
- `sb-data-grid--hoverable` - Hoverable rows
- `sb-data-grid--bordered` - Bordered style
- `sb-data-grid--dense` - Compact density (when Density is Compact)
- `sb-data-grid--loading` - Loading state
- `sb-data-grid__table` - Table element
- `sb-data-grid__header` - Header row
- `sb-data-grid__body` - Body container
- `sb-data-grid__row` - Data row
- `sb-data-grid__row--selected` - Selected row
- `sb-data-grid__cell` - Table cell
- `sb-data-grid__empty` - Empty state container
- `sb-data-grid__loading` - Loading state container
- `sb-data-grid__pagination` - Pagination container

## Examples

### Basic Usage

```razor
<SbDataGrid TItem="User" Items="@users">
    <SbColumn TItem="User" Field="Name" Title="Name" />
    <SbColumn TItem="User" Field="Email" Title="Email" />
    <SbColumn TItem="User" Field="Role" Title="Role" />
</SbDataGrid>
```

### With Selection

Use `KeySelector` to identify rows, then bind or handle `SelectedKeysChanged` and/or `SelectedItemsChanged`:

```razor
<SbDataGrid TItem="Product" 
            Items="@products"
            KeySelector="@(p => p.Name)"
            SelectionMode="SbSelectionMode.MultipleRows"
            SelectedKeysChanged="OnSelectedKeysChanged"
            SelectedItemsChanged="OnSelectedItemsChanged">
    <SbColumn TItem="Product" Field="Name" Title="Name" />
    <SbColumn TItem="Product" Field="Price" Title="Price" />
</SbDataGrid>
```

### Server-Side Data

Use `ItemsProvider` with `SbDataRequest` / `SbDataResponse` for server-side paging and sorting. Bind `PageIndex`, `PageSize`, `TotalCount`, and handle `PageIndexChanged` / `PageSizeChanged` when using server-side data with manual load.
