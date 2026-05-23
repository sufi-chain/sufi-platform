# SbColumnFilterMenu

A dropdown menu component for column filtering within data grids.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ColumnTitle | string | - | Title of the column being filtered |
| FilterValue | string? | null | Current filter value |
| FilterOperator | FilterOperator | Contains | Filter comparison operator |
| Values | IEnumerable\<string\>? | null | Distinct values for checkbox filtering |
| ShowOperators | bool | true | Whether to show operator selection |
| ShowDistinctValues | bool | true | Whether to show distinct value checkboxes |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnApply | EventCallback\<FilterEventArgs\> | Fired when filter is applied |
| OnClear | EventCallback | Fired when filter is cleared |

## FilterOperator Enum

```csharp
public enum FilterOperator
{
    Contains,
    Equals,
    NotEquals,
    StartsWith,
    EndsWith,
    GreaterThan,
    LessThan,
    GreaterThanOrEqual,
    LessThanOrEqual
}
```

## CSS Classes

- `sb-column-filter-menu` - Base class
- `sb-column-filter-menu__header` - Menu header
- `sb-column-filter-menu__operators` - Operator selection
- `sb-column-filter-menu__input` - Filter input
- `sb-column-filter-menu__values` - Distinct values list
- `sb-column-filter-menu__value-item` - Individual value checkbox
- `sb-column-filter-menu__footer` - Footer with buttons

## Examples

### Basic Usage

```razor
<SbColumnFilterMenu ColumnTitle="Name"
                    FilterValue="@nameFilter"
                    OnApply="ApplyNameFilter"
                    OnClear="ClearNameFilter" />
```

### With Distinct Values

```razor
<SbColumnFilterMenu ColumnTitle="Status"
                    Values="@statusValues"
                    OnApply="ApplyStatusFilter"
                    OnClear="ClearStatusFilter" />

@code {
    private IEnumerable<string> statusValues = new[] { "Active", "Inactive", "Pending" };
}
```

### Text Filter Only

```razor
<SbColumnFilterMenu ColumnTitle="Email"
                    FilterOperator="FilterOperator.Contains"
                    ShowDistinctValues="false"
                    OnApply="ApplyEmailFilter" />
```

### Numeric Filter

```razor
<SbColumnFilterMenu ColumnTitle="Price"
                    FilterOperator="FilterOperator.GreaterThan"
                    ShowDistinctValues="false"
                    OnApply="ApplyPriceFilter" />
```

### Inside DataGrid Column

```razor
<SbColumn TItem="Product" Field="Category" Title="Category" Filterable="true">
    <FilterTemplate>
        <SbColumnFilterMenu ColumnTitle="Category"
                            Values="@categoryValues"
                            OnApply="FilterByCategory"
                            OnClear="ClearCategoryFilter" />
    </FilterTemplate>
</SbColumn>
```
