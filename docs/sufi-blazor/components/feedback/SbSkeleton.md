# SbSkeleton

A placeholder component that shows a preview of content while data is loading. Reduces perceived loading time by providing visual structure that mimics the final content layout.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Variant | SbSkeletonVariant | Text | Shape variant (Text, Circular, Rectangular) |
| Animation | bool | true | Whether to show the shimmer animation |
| Width | string? | null | CSS width value (e.g., "100%", "200px") |
| Height | string? | null | CSS height value (e.g., "20px", "100px") |
| BorderRadius | string? | null | CSS border-radius value (e.g., "8px", "50%") |
| Class | string? | null | Additional CSS classes |

## CSS Classes

- `sb-skeleton` - Base class
- `sb-skeleton--text` - Text variant (short height, full width)
- `sb-skeleton--circular` - Circular variant (circle shape)
- `sb-skeleton--rectangular` - Rectangular variant (block shape)
- `sb-skeleton--animated` - Shimmer animation applied

## Accessibility

- Uses `aria-hidden="true"` since it's a decorative placeholder

## Examples

### Basic Variants

```razor
@* Text skeleton - simulates a line of text *@
<SbSkeleton Variant="SbSkeletonVariant.Text" />

@* Circular skeleton - simulates an avatar *@
<SbSkeleton Variant="SbSkeletonVariant.Circular" Width="40px" Height="40px" />

@* Rectangular skeleton - simulates an image or card *@
<SbSkeleton Variant="SbSkeletonVariant.Rectangular" Height="200px" />
```

### Custom Dimensions

```razor
<SbSkeleton Variant="SbSkeletonVariant.Text" Width="60%" />
<SbSkeleton Variant="SbSkeletonVariant.Text" Width="80%" />
<SbSkeleton Variant="SbSkeletonVariant.Text" Width="40%" />
```

### Without Animation

```razor
<SbSkeleton Variant="SbSkeletonVariant.Rectangular" 
            Height="100px" 
            Animation="false" />
```

### Custom Border Radius

```razor
<SbSkeleton Variant="SbSkeletonVariant.Rectangular" 
            Height="150px" 
            BorderRadius="16px" />
```

### Card Skeleton

```razor
<div class="card-skeleton">
    @* Image placeholder *@
    <SbSkeleton Variant="SbSkeletonVariant.Rectangular" Height="200px" />
    
    <div style="padding: 16px;">
        @* Title *@
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="80%" Height="24px" />
        
        @* Subtitle *@
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="60%" />
        
        @* Description lines *@
        <div style="margin-top: 16px;">
            <SbSkeleton Variant="SbSkeletonVariant.Text" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="70%" />
        </div>
    </div>
</div>
```

### List Item Skeleton

```razor
@for (int i = 0; i < 5; i++)
{
    <div class="list-item-skeleton" style="display: flex; gap: 16px; padding: 12px;">
        @* Avatar *@
        <SbSkeleton Variant="SbSkeletonVariant.Circular" Width="48px" Height="48px" />
        
        <div style="flex: 1;">
            @* Name *@
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="40%" Height="20px" />
            @* Description *@
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="70%" />
        </div>
    </div>
}
```

### Data Table Skeleton

```razor
<div class="table-skeleton">
    @* Header *@
    <div style="display: flex; gap: 16px; padding: 12px; border-bottom: 1px solid #eee;">
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="20%" />
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="30%" />
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="25%" />
        <SbSkeleton Variant="SbSkeletonVariant.Text" Width="15%" />
    </div>
    
    @* Rows *@
    @for (int i = 0; i < 5; i++)
    {
        <div style="display: flex; gap: 16px; padding: 12px;">
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="20%" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="30%" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="25%" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="15%" />
        </div>
    }
</div>
```

### Profile Page Skeleton

```razor
<div class="profile-skeleton">
    @* Cover image *@
    <SbSkeleton Variant="SbSkeletonVariant.Rectangular" Height="200px" />
    
    <div style="padding: 20px; margin-top: -60px;">
        @* Avatar *@
        <SbSkeleton Variant="SbSkeletonVariant.Circular" 
                    Width="120px" 
                    Height="120px" 
                    Style="border: 4px solid white;" />
        
        @* Name and bio *@
        <div style="margin-top: 16px;">
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="200px" Height="28px" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="150px" />
            <SbSkeleton Variant="SbSkeletonVariant.Text" Width="300px" />
        </div>
    </div>
</div>
```

### Conditional Loading

```razor
@if (isLoading)
{
    <SbSkeleton Variant="SbSkeletonVariant.Text" Width="60%" />
    <SbSkeleton Variant="SbSkeletonVariant.Text" />
    <SbSkeleton Variant="SbSkeletonVariant.Text" Width="80%" />
}
else
{
    <h2>@article.Title</h2>
    <p>@article.Content</p>
}
```
