# SbDataGrid

Data grid with sorting, filtering, pagination, selection, editing, and optional virtualization.

## Features

- **Modes**: Client-side (`Items`) or server-side (`ItemsProvider` returning `SbDataResponse<T>`).  
- **Columns**: `SbColumn` with `Field`, `Title`, sortable, filterable, editable, width, align, optional templates (cell, header, footer, filter, edit).  
- **Sorting**: Per-column; `OnSortChanged` for server-side.  
- **Filtering**: Column filters, filter bar, `OnFiltersChanged`; operators and values in `SbFilter`.  
- **Pagination**: Page index/size, total count, optional page-size selector; works with both modes.  
- **Selection**: None, single row, or multiple; `KeySelector`, `SelectedKeys` / `SelectedKeysChanged`, `SelectedItemsChanged`.  
- **Editing**: Row or cell edit mode; optional inline save/cancel; `OnRowEditing`, `OnRowEdited`, `OnRowEditCancelled`, `OnCellEdited`; `CloneItem` for cancel.  
- **Row details**: `DetailTemplate` for expandable master-detail.  
- **Virtualization**: `VirtualizationEnabled`, `Height`, `RowHeight`, `OverscanCount` for large lists.  
- **Export**: `ExportToCsvAsync`, `ExportAllToCsvAsync`.  
- **Misc**: Loading overlay, empty template, density, striped, hoverable, bordered, column resize, frozen columns, keyboard navigation, ARIA.

## Usage Notes

- Use `Loading` with `IsOperationLoading` when data comes from async ops.  
- Prefer `KeySelector` for stable row identity.  
- For server-side, map `SbDataRequest` (paging, sort, filters) to your API and return `SbDataResponse<T>`.

See **DataGridPage** in **SufiChain.SufiBlazorDemo** for examples.
