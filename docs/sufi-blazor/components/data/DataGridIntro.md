# Data Grid Overview

**SbDataGrid** is the main component for tabular data: sorting, pagination, selection, virtualization, server-side data, and more.

## Demos

| Demo | Description |
|------|-------------|
| [Basic](/sufi-blazor-demo/data-grid/basic) | Simple client-side grid with columns |
| [Selection](/sufi-blazor-demo/data-grid/selection) | Single or multiple row selection with `KeySelector`, `SelectedKeysChanged`, `SelectedItemsChanged` |
| [Striped & Bordered](/sufi-blazor-demo/data-grid/styled) | Visual variants: striped rows, borders |
| [Compact](/sufi-blazor-demo/data-grid/compact) | Density: compact, default, comfortable |
| [Empty State](/sufi-blazor-demo/data-grid/empty) | Custom content when there are no items |

## API Reference

See [SbDataGrid](SbDataGrid.md) for full parameters, events, and templates.

## Selection API

- **KeySelector** – `Func<TItem, string>` to get a unique key per row (required for selection).
- **SelectedKeys** / **SelectedKeysChanged** – Bind or react to selected row keys.
- **SelectedItemsChanged** – Callback that receives the list of selected items (full objects).

There is no `SelectedItems` property; use `SelectedItemsChanged` to track selected items in your code.
