# SbMetric

A component for displaying a single metric value with optional label, trend indicator, and formatting.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | string | - | The metric value to display |
| Label | string? | null | Label text for the metric |
| Trend | decimal? | null | Trend percentage (positive/negative) |
| TrendLabel | string? | null | Custom trend label |
| Format | MetricFormat | Default | Value format (Number, Currency, Percentage) |
| Size | SbSize | Medium | Size variant |
| Color | SbColor? | null | Accent color |
| Class | string? | null | Additional CSS classes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| IconTemplate | RenderFragment | Custom icon content |
| ValueTemplate | RenderFragment | Custom value display |

## CSS Classes

- `sb-metric` - Base class
- `sb-metric--sm` - Small size
- `sb-metric--md` - Medium size
- `sb-metric--lg` - Large size
- `sb-metric__value` - Value display
- `sb-metric__label` - Label text
- `sb-metric__trend` - Trend indicator
- `sb-metric__trend--up` - Positive trend
- `sb-metric__trend--down` - Negative trend
- `sb-metric__icon` - Icon container

## Examples

### Basic Usage

```razor
<SbMetric Value="1,234" Label="Total Users" />
```

### With Trend

```razor
<SbMetric Value="$54,321" 
          Label="Revenue" 
          Trend="12.5m" />
```

### Negative Trend

```razor
<SbMetric Value="89%" 
          Label="Completion Rate" 
          Trend="-3.2m" />
```

### With Icon

```razor
<SbMetric Value="42" Label="Active Tasks">
    <IconTemplate>
        <SbIcon Name="check-circle" Color="SbColor.Success" />
    </IconTemplate>
</SbMetric>
```

### Large Size

```razor
<SbMetric Value="$1.2M" 
          Label="Annual Revenue" 
          Size="SbSize.Lg"
          Trend="8.3m" />
```

### In Dashboard Layout

```razor
<SbGrid Columns="4" Gap="4">
    <SbMetric Value="2,543" Label="Total Orders" Trend="12m" />
    <SbMetric Value="$89,432" Label="Revenue" Trend="8.2m" />
    <SbMetric Value="1,205" Label="New Customers" Trend="23m" />
    <SbMetric Value="98.5%" Label="Satisfaction" Trend="-0.3m" />
</SbGrid>
```
