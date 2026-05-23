# SbProgress

Displays progress indication for ongoing operations. Supports both linear (bar) and circular styles, with determinate and indeterminate modes.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Type | SbProgressType | Linear | Progress type (Linear, Circular) |
| Value | double | 0 | Progress value from 0 to 100 |
| Indeterminate | bool | false | Shows continuous animation without specific progress |
| Color | SbColor | Primary | Progress bar color |
| Size | SbSize | Md | Size for circular progress (Sm, Md, Lg) |
| ShowLabel | bool | false | Whether to show the percentage label |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## CSS Classes

- `sb-progress` - Base class
- `sb-progress--linear` - Linear (bar) type
- `sb-progress--circular` - Circular type
- `sb-progress--sm` - Small size
- `sb-progress--md` - Medium size
- `sb-progress--lg` - Large size
- `sb-progress--primary` - Primary color
- `sb-progress--secondary` - Secondary color
- `sb-progress--success` - Success color
- `sb-progress--warning` - Warning color
- `sb-progress--danger` - Danger color
- `sb-progress--indeterminate` - Indeterminate animation
- `sb-progress__track` - Track/background (linear)
- `sb-progress__bar` - Progress bar (linear)
- `sb-progress__label` - Percentage label (linear)
- `sb-progress__track-circle` - Track circle (circular)
- `sb-progress__bar-circle` - Progress circle (circular)
- `sb-progress__label-text` - Percentage label (circular)

## Accessibility

- Uses `role="progressbar"`
- Sets `aria-valuenow`, `aria-valuemin`, `aria-valuemax` for determinate progress
- Omits `aria-valuenow` for indeterminate progress

## Examples

### Linear Progress

```razor
<SbProgress Value="25" />
<SbProgress Value="50" />
<SbProgress Value="75" />
<SbProgress Value="100" />
```

### Linear with Label

```razor
<SbProgress Value="65" ShowLabel="true" />
```

### Indeterminate Linear

```razor
<SbProgress Indeterminate="true" />
```

### Different Colors

```razor
<SbProgress Value="50" Color="SbColor.Primary" />
<SbProgress Value="50" Color="SbColor.Success" />
<SbProgress Value="50" Color="SbColor.Warning" />
<SbProgress Value="50" Color="SbColor.Danger" />
```

### Circular Progress

```razor
<SbProgress Type="SbProgressType.Circular" Value="25" />
<SbProgress Type="SbProgressType.Circular" Value="50" />
<SbProgress Type="SbProgressType.Circular" Value="75" />
```

### Circular with Label

```razor
<SbProgress Type="SbProgressType.Circular" Value="75" ShowLabel="true" />
```

### Circular Sizes

```razor
<SbProgress Type="SbProgressType.Circular" Value="50" Size="SbSize.Sm" />
<SbProgress Type="SbProgressType.Circular" Value="50" Size="SbSize.Md" />
<SbProgress Type="SbProgressType.Circular" Value="50" Size="SbSize.Lg" />
```

### Indeterminate Circular

```razor
<SbProgress Type="SbProgressType.Circular" Indeterminate="true" />
```

### File Upload Example

```razor
@if (isUploading)
{
    <SbProgress Value="@uploadProgress" ShowLabel="true" Color="SbColor.Primary" />
    <p>Uploading: @fileName</p>
}

@code {
    private bool isUploading;
    private double uploadProgress;
    private string fileName = "";
    
    private async Task UploadFile()
    {
        isUploading = true;
        fileName = "document.pdf";
        
        for (int i = 0; i <= 100; i += 10)
        {
            uploadProgress = i;
            StateHasChanged();
            await Task.Delay(200);
        }
        
        isUploading = false;
    }
}
```

### Loading Overlay with Circular Progress

```razor
@if (isLoading)
{
    <div class="loading-overlay">
        <SbProgress Type="SbProgressType.Circular" 
                    Indeterminate="true" 
                    Size="SbSize.Lg" />
        <p>Loading data...</p>
    </div>
}
```

### Multi-Step Progress

```razor
<div class="step-progress">
    <span>Step @currentStep of @totalSteps</span>
    <SbProgress Value="@((double)currentStep / totalSteps * 100)" 
                ShowLabel="true" 
                Color="SbColor.Success" />
</div>

@code {
    private int currentStep = 2;
    private int totalSteps = 5;
}
```
