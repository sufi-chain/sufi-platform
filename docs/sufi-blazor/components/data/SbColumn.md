# SbColumn

Defines a column within an SbDataGrid, with support for custom templates, sorting, filtering, and formatting.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Field | string? | null | Property name to bind to (sorting, filtering, default cell value) |
| Title | string | "" | Column header text |
| Width | string? | null | Column width (e.g., "150px", "20%", "1fr") |
| MinWidth | string? | null | Minimum column width |
| MaxWidth | string? | null | Maximum column width |
| Align | SbColumnAlign | Start | Text alignment (Start, Center, End) |
| Sortable | bool | false | Whether column is sortable |
| Filterable | bool | false | Whether column is filterable |
| Resizable | bool | false | Whether column is resizable |
| Visible | bool | true | Whether column is visible |
| Freeze | SbColumnFreeze | None | Freeze column at start or end (None, Start, End) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles for the column (e.g. "min-width: 160px") |
| Editable | bool | false | Whether the column is editable in-place |
| Format | string? | null | Format string for display (e.g. "C0" currency, "d" short date). Applied when value implements IFormattable and no CellTemplate/FormatFunc is used. |
| FormatFunc | Func\<object?, string\>? | null | Custom formatter for display; overrides Format when set. |
| ValueFunc | Func\<TItem, object?\>? | null | Custom value getter; used when Field is not set or for computed values. |
| SetValueFunc | Action\<TItem, object?\>? | null | Custom value setter for editing. |
| FieldType | Type? | null | Field value type (for default editors); inferred from property if not set. |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| HeaderTemplate | RenderFragment | Custom header content |
| CellTemplate | RenderFragment\<SbCellContext\<TItem\>\> | Custom cell content |
| FooterTemplate | RenderFragment | Custom footer content |
| EditTemplate | RenderFragment\<SbCellContext\<TItem\>\> | Cell content in edit mode |
| FilterTemplate | RenderFragment\<SbColumnFilterMenu\> | Custom filter UI |

### Template Usage Examples

#### Custom CellTemplate

```razor
<SbColumn TItem="User" Field="Name" Title="Name">
    <CellTemplate Context="cell">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <img src="@cell.Item.AvatarUrl" alt="" class="avatar-sm" />
            <span>@cell.Item.Name</span>
        </SbStack>
    </CellTemplate>
</SbColumn>
```

#### Custom HeaderTemplate

```razor
<SbColumn TItem="Product" Field="Status" Title="Status">
    <HeaderTemplate>
        <SbStack Direction="SbStackDirection.Row" Gap="1" Align="SbAlign.Center">
            <SbIcon Name="activity" Size="SbSize.Sm" />
            <span>Status</span>
        </SbStack>
    </HeaderTemplate>
</SbColumn>
```

#### Edit Template

```razor
<SbColumn TItem="Product" Field="Price" Title="Price">
    <CellTemplate Context="cell">
        @cell.Item.Price.ToString("C")
    </CellTemplate>
    <EditTemplate Context="item">
        <SbNumberField TNumber="decimal" @bind-Value="item.Price" />
    </EditTemplate>
</SbColumn>
```

#### Filter Template

```razor
<SbColumn TItem="Order" Field="Status" Title="Status" Filterable="true">
    <FilterTemplate>
        <SbSimpleSelect @bind-Value="statusFilter">
            <SbSelectOption Value="">All</SbSelectOption>
            <SbSelectOption Value="pending">Pending</SbSelectOption>
            <SbSelectOption Value="completed">Completed</SbSelectOption>
            <SbSelectOption Value="cancelled">Cancelled</SbSelectOption>
        </SbSimpleSelect>
    </FilterTemplate>
</SbColumn>
```

## Format behavior

When no `CellTemplate` is used, the displayed value is:

1. **FormatFunc** – if set, `FormatFunc(value)` is used.
2. **Format** – if set and the value implements `IFormattable`, `value.ToString(Format, CurrentCulture)` is used (e.g. `Format="C0"` for currency, `Format="d"` for short date).
3. Otherwise – `value?.ToString() ?? ""`.

## SbCellContext properties

```csharp
public class SbCellContext<TItem>
{
    public TItem Item { get; }
    public int RowIndex { get; }
    public object? Value { get; }
}
```

## CSS Classes

- `sb-column` - Base class
- `sb-column--sortable` - Sortable column
- `sb-column--sorted` - Currently sorted
- `sb-column--sorted-asc` - Sorted ascending
- `sb-column--sorted-desc` - Sorted descending
- `sb-column--filterable` - Filterable column
- `sb-column--align-left` - Left aligned
- `sb-column--align-center` - Center aligned
- `sb-column--align-right` - Right aligned

## Examples

### Basic Columns

```razor
<SbDataGrid TItem="Product" Items="@products">
    <SbColumn TItem="Product" Field="Name" Title="Name" />
    <SbColumn TItem="Product" Field="Price" Title="Price" Format="C2" Align="SbColumnAlign.End" />
    <SbColumn TItem="Product" Field="Stock" Title="Stock" Align="SbColumnAlign.Center" />
</SbDataGrid>
```

### Fixed Width Columns

```razor
<SbColumn TItem="User" Field="Avatar" Title="" Width="60px">
    <CellTemplate Context="cell">
        <img src="@cell.Item.AvatarUrl" alt="" class="avatar" />
    </CellTemplate>
</SbColumn>
```

### Actions Column

```razor
<SbColumn TItem="Order" Title="Actions" Width="120px" Sortable="false">
    <CellTemplate Context="cell">
        <SbStack Direction="SbStackDirection.Row" Gap="1">
            <SbIconButton Icon="eye" Size="SbSize.Sm" OnClick="() => View(cell.Item)" />
            <SbIconButton Icon="edit" Size="SbSize.Sm" OnClick="() => Edit(cell.Item)" />
            <SbIconButton Icon="trash" Size="SbSize.Sm" Color="SbColor.Danger" OnClick="() => Delete(cell.Item)" />
        </SbStack>
    </CellTemplate>
</SbColumn>
```

### Status Badge Column

```razor
<SbColumn TItem="Task" Field="Status" Title="Status">
    <CellTemplate Context="cell">
        <SbStatusPill Status="@GetStatusType(cell.Item.Status)">
            @cell.Item.Status
        </SbStatusPill>
    </CellTemplate>
</SbColumn>
```
