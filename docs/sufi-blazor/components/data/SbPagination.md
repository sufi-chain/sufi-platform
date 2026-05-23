# SbPagination

A pagination component for navigating through paged data. Uses zero-based page index and supports page size selector, first/prev/next/last buttons, and configurable visible page numbers.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| PageIndex | int | 0 | Current zero-based page index |
| PageSize | int | 10 | Items per page |
| TotalCount | long | 0 | Total number of items |
| ShowPageSizeSelector | bool | true | Whether to show the page size dropdown |
| PageSizeOptions | int[] | { 5, 10, 25, 50, 100 } | Options for page size selector |
| ShowPageNumbers | bool | true | Whether to show page number buttons |
| MaxVisiblePages | int | 5 | Max page number buttons to show (desktop) |
| MaxVisiblePagesMobile | int | 3 | Max page number buttons when viewport &lt; 640px |
| PageSizeLabel | string? | null | Label for page size selector (null = localized default) |
| FirstPageLabel | string? | null | First page button (null = localized) |
| PreviousPageLabel | string? | null | Previous button (null = localized) |
| NextPageLabel | string? | null | Next button (null = localized) |
| LastPageLabel | string? | null | Last page button (null = localized) |
| PageAriaLabelFormat | string? | null | Aria format for page buttons, {0} = page number |
| NoItemsText | string? | null | Text when TotalCount is 0 |
| PageInfoFormat | string? | null | Format for "X–Y of Z"; {0}=start, {1}=end, {2}=total |
| Class | string? | null | Additional CSS classes |

## Bindings / Events

| Parameter | Type | Description |
|-----------|------|-------------|
| PageIndexChanged | EventCallback&lt;int&gt; | Fired when page index changes |
| PageSizeChanged | EventCallback&lt;int&gt; | Fired when page size changes |

Use `@bind-PageIndex` and `@bind-PageSize` for two-way binding.

## CSS Classes

- `sb-pagination` - Base class
- `sb-pagination__container` - Inner wrapper
- `sb-pagination__page-size` - Page size selector area
- `sb-pagination__info` - "X–Y of Z" text
- `sb-pagination__controls` - Buttons container
- `sb-pagination__button` - Nav button
- `sb-pagination__pages` - Page numbers wrapper
- `sb-pagination__page` - Page number button
- `sb-pagination__ellipsis` - Ellipsis between page ranges

## Examples

### Basic Usage

```razor
<SbPagination @bind-PageIndex="_pageIndex"
              @bind-PageSize="_pageSize"
              TotalCount="100" />
```

### Without Page Size Selector

```razor
<SbPagination @bind-PageIndex="_pageIndex"
              PageSize="10"
              TotalCount="75"
              ShowPageSizeSelector="false" />
```

### Without Page Numbers

```razor
<SbPagination @bind-PageIndex="_pageIndex"
              @bind-PageSize="_pageSize"
              TotalCount="200"
              ShowPageNumbers="false" />
```

### Custom Page Size Options

```razor
<SbPagination @bind-PageIndex="_pageIndex"
              @bind-PageSize="_pageSize"
              TotalCount="500"
              PageSizeOptions="@(new[] { 10, 20, 50, 100 })" />
```

### Many Pages (MaxVisiblePages)

```razor
<SbPagination @bind-PageIndex="_pageIndex"
              PageSize="10"
              TotalCount="1000"
              MaxVisiblePages="7"
              ShowPageSizeSelector="false" />
```

### With Data List

```razor
<SbStack Gap="2">
    @foreach (var item in GetPagedItems())
    {
        <SbCard>
            <SbStack Direction="SbStackDirection.Row" Justify="SbJustify.SpaceBetween">
                <SbText Weight="SbFontWeight.Semibold">Item @item.Id</SbText>
                <SbText Color="SbColor.Muted" Size="SbSize.Sm">@item.Name</SbText>
            </SbStack>
        </SbCard>
    }
</SbStack>
<SbPagination @bind-PageIndex="_dataPageIndex"
              @bind-PageSize="_dataPageSize"
              TotalCount="@_items.Count" />
```
