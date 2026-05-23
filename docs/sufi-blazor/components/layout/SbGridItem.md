# SbGridItem

A child component of SbGrid that spans a specified number of columns with responsive breakpoint support.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Span | int | 1 | Number of columns this item spans (1-12) |
| SpanSm | int? | null | Column span at small breakpoint (640px+) |
| SpanMd | int? | null | Column span at medium breakpoint (768px+) |
| SpanLg | int? | null | Column span at large breakpoint (1024px+) |
| SpanXl | int? | null | Column span at extra large breakpoint (1280px+) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Content to render inside the grid item |

## CSS Classes

- `sb-grid-item` - Base class
- `sb-grid-item--sm-{n}` - Small breakpoint span
- `sb-grid-item--md-{n}` - Medium breakpoint span
- `sb-grid-item--lg-{n}` - Large breakpoint span
- `sb-grid-item--xl-{n}` - Extra large breakpoint span

## CSS Variables

- `--sb-grid-span` - Number of columns to span

## Examples

### Basic Usage

```razor
<SbGrid Columns="12">
    <SbGridItem Span="6">Half width</SbGridItem>
    <SbGridItem Span="6">Half width</SbGridItem>
</SbGrid>
```

### Responsive Span

```razor
<SbGrid Columns="12">
    <SbGridItem Span="12" SpanMd="6" SpanLg="4">
        <SbCard>Responsive Card</SbCard>
    </SbGridItem>
</SbGrid>
```

### Full Width Header

```razor
<SbGrid Columns="12" Gap="4">
    <SbGridItem Span="12">
        <h1>Page Title</h1>
    </SbGridItem>
    <SbGridItem Span="8">
        <SbCard>Main Content</SbCard>
    </SbGridItem>
    <SbGridItem Span="4">
        <SbCard>Sidebar</SbCard>
    </SbGridItem>
</SbGrid>
```

### Mixed Column Widths

```razor
<SbGrid Columns="12" Gap="4">
    <SbGridItem Span="4">
        <SbCard>1/3 width</SbCard>
    </SbGridItem>
    <SbGridItem Span="8">
        <SbCard>2/3 width</SbCard>
    </SbGridItem>
    <SbGridItem Span="3">
        <SbCard>1/4 width</SbCard>
    </SbGridItem>
    <SbGridItem Span="3">
        <SbCard>1/4 width</SbCard>
    </SbGridItem>
    <SbGridItem Span="6">
        <SbCard>1/2 width</SbCard>
    </SbGridItem>
</SbGrid>
```

### Responsive Dashboard

```razor
<SbGrid Columns="12" Gap="4">
    @* Stats row - 4 columns on large, 2 on medium, 1 on small *@
    <SbGridItem Span="12" SpanMd="6" SpanLg="3">
        <SbStatCard Value="1,234" Label="Users" />
    </SbGridItem>
    <SbGridItem Span="12" SpanMd="6" SpanLg="3">
        <SbStatCard Value="$56K" Label="Revenue" />
    </SbGridItem>
    <SbGridItem Span="12" SpanMd="6" SpanLg="3">
        <SbStatCard Value="89%" Label="Uptime" />
    </SbGridItem>
    <SbGridItem Span="12" SpanMd="6" SpanLg="3">
        <SbStatCard Value="42" Label="Orders" />
    </SbGridItem>
    
    @* Chart - full width on small, 2/3 on large *@
    <SbGridItem Span="12" SpanLg="8">
        <SbCard>Chart Area</SbCard>
    </SbGridItem>
    
    @* Activity feed - full width on small, 1/3 on large *@
    <SbGridItem Span="12" SpanLg="4">
        <SbCard>Activity Feed</SbCard>
    </SbGridItem>
</SbGrid>
```
