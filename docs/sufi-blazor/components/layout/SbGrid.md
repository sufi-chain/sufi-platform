# SbGrid

A responsive CSS Grid layout component with configurable columns and breakpoints.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Columns | int | 12 | Number of columns (base/mobile-first value) |
| ColumnsSm | int? | null | Number of columns at sm breakpoint (640px+) |
| ColumnsMd | int? | null | Number of columns at md breakpoint (768px+) |
| ColumnsLg | int? | null | Number of columns at lg breakpoint (1024px+) |
| ColumnsXl | int? | null | Number of columns at xl breakpoint (1280px+) |
| Gap | int | 4 | Gap between grid items (uses spacing scale 0-16) |
| RowGap | int? | null | Row gap if different from Gap |
| ColumnGap | int? | null | Column gap if different from Gap |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Grid items (typically SbGridItem components) |

## CSS Classes

- `sb-grid` - Base class
- `sb-grid--sm-{n}` - Small breakpoint columns
- `sb-grid--md-{n}` - Medium breakpoint columns
- `sb-grid--lg-{n}` - Large breakpoint columns
- `sb-grid--xl-{n}` - Extra large breakpoint columns

## CSS Variables

- `--sb-grid-columns` - Number of grid columns

## Examples

### Basic 3-Column Grid

```razor
<SbGrid Columns="3" Gap="4">
    <SbCard>Card 1</SbCard>
    <SbCard>Card 2</SbCard>
    <SbCard>Card 3</SbCard>
</SbGrid>
```

### Responsive Grid

```razor
<SbGrid Columns="1" ColumnsSm="2" ColumnsMd="3" ColumnsLg="4" Gap="4">
    @foreach (var item in items)
    {
        <SbCard>@item.Name</SbCard>
    }
</SbGrid>
```

### Dashboard Layout

```razor
<SbGrid Columns="12" Gap="4">
    <SbGridItem Span="12">
        <SbCard>Header Section</SbCard>
    </SbGridItem>
    <SbGridItem Span="8">
        <SbCard>Main Content</SbCard>
    </SbGridItem>
    <SbGridItem Span="4">
        <SbCard>Sidebar</SbCard>
    </SbGridItem>
</SbGrid>
```

### Stats Grid

```razor
<SbGrid Columns="4" Gap="4">
    <SbStatCard Value="1,234" Label="Users" Icon="users" />
    <SbStatCard Value="$56K" Label="Revenue" Icon="dollar-sign" />
    <SbStatCard Value="89%" Label="Uptime" Icon="activity" />
    <SbStatCard Value="42" Label="Orders" Icon="shopping-cart" />
</SbGrid>
```

### Different Row and Column Gaps

```razor
<SbGrid Columns="3" RowGap="6" ColumnGap="4">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
    <div>Item 4</div>
    <div>Item 5</div>
    <div>Item 6</div>
</SbGrid>
```

### Form Grid

```razor
<SbGrid Columns="2" Gap="4">
    <SbTextField Label="First Name" @bind-Value="firstName" />
    <SbTextField Label="Last Name" @bind-Value="lastName" />
    <SbGridItem Span="2">
        <SbTextField Label="Email" @bind-Value="email" />
    </SbGridItem>
    <SbGridItem Span="2">
        <SbTextArea Label="Address" @bind-Value="address" />
    </SbGridItem>
</SbGrid>
```
