# SbStatCard

A card component for displaying key metrics and statistics with optional icon, prefix/suffix, trend, description, and actions.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string | - | Label describing the statistic |
| Value | string | - | The statistic value (displayed as-is) |
| Prefix | string? | null | Text before the value (e.g. "$") |
| Suffix | string? | null | Text after the value (e.g. "%", "min") |
| Trend | double? | null | Trend percentage (positive or negative) |
| TrendLabel | string? | null | Trend description (e.g. "vs last month") |
| ShowTrend | bool | true | Whether to show trend when Trend is set |
| Description | string? | null | Additional description text below trend |
| Class | string? | null | Additional CSS classes |

## Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment? | Icon content (e.g. `<Icon><SbIcon Name="users" /></Icon>`) |
| Actions | RenderFragment? | Action buttons or links (e.g. "View All" button) |

## CSS Classes

- `sb-stat-card` - Base class
- `sb-stat-card__icon` - Icon container
- `sb-stat-card__content` - Content area
- `sb-stat-card__label` - Label text
- `sb-stat-card__value` - Value display
- `sb-stat-card__prefix` - Value prefix
- `sb-stat-card__suffix` - Value suffix
- `sb-stat-card__trend` - Trend row
- `sb-stat-card__trend--positive` - Positive trend (e.g. arrow up)
- `sb-stat-card__trend--negative` - Negative trend (e.g. arrow down)
- `sb-stat-card__trend-icon` - Trend arrow
- `sb-stat-card__trend-value` - Trend percentage
- `sb-stat-card__trend-label` - Trend label text
- `sb-stat-card__description` - Description text
- `sb-stat-card__actions` - Actions area

## Examples

### Basic Usage

```razor
<SbStatCard Label="Total Users" Value="2,543" />

<SbStatCard Label="Revenue" Value="98,234" Prefix="$" />

<SbStatCard Label="Growth Rate" Value="12.5" Suffix="%" />
```

### With Icons

```razor
<SbStatCard Label="Total Sales" Value="1,249" Prefix="$">
    <Icon>
        <SbIcon Name="dollar-sign" Size="SbSize.Xl" Color="SbColor.Success" />
    </Icon>
</SbStatCard>

<SbStatCard Label="Active Users" Value="8,459">
    <Icon>
        <SbIcon Name="users" Size="SbSize.Xl" Color="SbColor.Primary" />
    </Icon>
</SbStatCard>
```

### With Trends

```razor
<SbStatCard Label="Total Orders" Value="3,456"
            Trend="12.5" TrendLabel="vs last month">
    <Icon>
        <SbIcon Name="shopping-cart" Size="SbSize.Xl" Color="SbColor.Primary" />
    </Icon>
</SbStatCard>

<SbStatCard Label="Bounce Rate" Value="23.4" Suffix="%"
            Trend="-5.3" TrendLabel="vs last month">
    <Icon>
        <SbIcon Name="trend-down" Size="SbSize.Xl" Color="SbColor.Warning" />
    </Icon>
</SbStatCard>
```

### With Descriptions

```razor
<SbStatCard Label="Page Views" Value="234,567"
            Trend="15.3" TrendLabel="from last month"
            Description="Total page views across all pages">
    <Icon>
        <SbIcon Name="eye" Size="SbSize.Xl" Color="SbColor.Info" />
    </Icon>
</SbStatCard>
```

### With Actions

```razor
<SbStatCard Label="Pending Tasks" Value="12"
            Description="Tasks awaiting review">
    <Icon>
        <SbIcon Name="clipboard-list" Size="SbSize.Xl" Color="SbColor.Warning" />
    </Icon>
    <Actions>
        <SbButton Size="SbSize.Sm" Variant="SbButtonVariant.Outline">
            View All
        </SbButton>
    </Actions>
</SbStatCard>
```

### Without Trend Display

```razor
<SbStatCard Label="Total Products" Value="1,234"
            ShowTrend="false"
            Description="Items in inventory">
    <Icon>
        <SbIcon Name="package" Size="SbSize.Xl" Color="SbColor.Secondary" />
    </Icon>
</SbStatCard>
```
