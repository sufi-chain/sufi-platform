# Image Editor Guide

## Overview

The Image Editor Plugin provides 50+ professional image editing operations organized into 8 categories.

## Features by Category

### 1. Basic Transformations

#### Crop
```csharp
var croppedImage = await editorPlugin.CropAsync(imageData, new Rectangle(50, 50, 300, 300));
```

#### Resize
```csharp
// Resize to exact dimensions
var resized = await editorPlugin.ResizeAsync(imageData, 800, 600, ResizeMode.Stretch);

// Fit within bounds (maintain aspect ratio)
var fitted = await editorPlugin.ResizeAsync(imageData, 800, 600, ResizeMode.Fit);

// Crop to fill
var cropped = await editorPlugin.ResizeAsync(imageData, 800, 600, ResizeMode.Crop);
```

#### Rotate & Flip
```csharp
// Rotate by degrees
var rotated = await editorPlugin.RotateAsync(imageData, 90);

// Flip
var flipped = await editorPlugin.FlipAsync(imageData, FlipMode.Horizontal);
```

### 2. Color Adjustments

```csharp
// Brightness (-100 to +100)
var brighter = await editorPlugin.AdjustBrightnessAsync(imageData, 20);

// Contrast (-100 to +100)
var contrast = await editorPlugin.AdjustContrastAsync(imageData, 15);

// Saturation (-100 to +100)
var saturated = await editorPlugin.AdjustSaturationAsync(imageData, 25);

// Hue (0-360 degrees)
var hueShifted = await editorPlugin.AdjustHueAsync(imageData, 45);

// Temperature (warm/cool)
var warmer = await editorPlugin.AdjustTemperatureAsync(imageData, 30); // Positive = warm
var cooler = await editorPlugin.AdjustTemperatureAsync(imageData, -30); // Negative = cool

// Gamma correction
var gamma = await editorPlugin.AdjustGammaAsync(imageData, 1.2f);

// Exposure
var exposed = await editorPlugin.AdjustExposureAsync(imageData, 10);
```

### 3. Filters & Effects

```csharp
// Grayscale
var grayscale = await editorPlugin.ConvertToGrayscaleAsync(imageData);

// Sepia tone
var sepia = await editorPlugin.ConvertToSepiaAsync(imageData);

// Black & White with threshold
var bw = await editorPlugin.ConvertToBlackAndWhiteAsync(imageData, 0.5f);

// Invert colors
var inverted = await editorPlugin.InvertColorsAsync(imageData);

// Polaroid effect
var polaroid = await editorPlugin.ApplyPolaroidAsync(imageData);

// Vignette
var vignette = await editorPlugin.ApplyVignetteAsync(imageData, 0.7f);

// Blur effects
var gaussianBlur = await editorPlugin.ApplyGaussianBlurAsync(imageData, 3.0f);
var boxBlur = await editorPlugin.ApplyBoxBlurAsync(imageData, 5);
var motionBlur = await editorPlugin.ApplyMotionBlurAsync(imageData, 10, 45);

// Sharpen
var sharpened = await editorPlugin.ApplySharpenAsync(imageData, 2.0f);

// Edge detection
var sobelEdges = await editorPlugin.DetectEdgesSobelAsync(imageData);
var cannyEdges = await editorPlugin.DetectEdgesCannyAsync(imageData);

// Artistic effects
var embossed = await editorPlugin.ApplyEmbossAsync(imageData);
var oilPainting = await editorPlugin.ApplyOilPaintingAsync(imageData, 10, 15);
var pixelated = await editorPlugin.ApplyPixelateAsync(imageData, 20);
```

### 4. Text & Watermark

#### Add Text
```csharp
var textOptions = new TextOptions
{
    Text = "Copyright 2024",
    FontFamily = "Arial",
    FontSize = 24,
    Color = "#FFFFFF",
    X = 100,
    Y = 100,
    Alignment = TextAlignment.Center,
    Bold = true,
    
    // Outline
    EnableOutline = true,
    OutlineColor = "#000000",
    OutlineWidth = 2,
    
    // Shadow
    EnableShadow = true,
    ShadowColor = "#000000",
    ShadowOffsetX = 2,
    ShadowOffsetY = 2,
    ShadowOpacity = 0.5f,
    
    Opacity = 0.9f
};

var withText = await editorPlugin.AddTextAsync(imageData, textOptions);
```

#### Add Watermark
```csharp
var watermarkOptions = new WatermarkOptions
{
    Position = WatermarkPosition.BottomRight,
    Opacity = 0.5f,
    Scale = 1.0f,
    MarginX = 20,
    MarginY = 20,
    
    // For tiled watermark
    Tile = false,
    TileSpacingX = 100,
    TileSpacingY = 100
};

var withWatermark = await editorPlugin.AddWatermarkAsync(
    imageData, 
    watermarkImageData, 
    watermarkOptions);
```

### 5. Quality & Optimization

```csharp
// Optimize with quality setting
var optimized = await editorPlugin.OptimizeAsync(imageData, quality: 85);

// Convert format
var asWebP = await editorPlugin.ConvertFormatAsync(imageData, ImageFormat.WebP);
var asPng = await editorPlugin.ConvertFormatAsync(imageData, ImageFormat.Png);
var asJpeg = await editorPlugin.ConvertFormatAsync(imageData, ImageFormat.Jpeg);

// Strip metadata (EXIF, GPS, etc.)
var noMetadata = await editorPlugin.StripMetadataAsync(imageData);
```

### 6. Advanced Editing

```csharp
// Remove red-eye
var eyeArea = new Rectangle(150, 100, 50, 30);
var noRedEye = await editorPlugin.RemoveRedEyeAsync(imageData, eyeArea);

// Auto-enhancements
var autoBright = await editorPlugin.AutoBrightnessAsync(imageData);
var autoContrast = await editorPlugin.AutoContrastAsync(imageData);
var autoLevels = await editorPlugin.AutoLevelsAsync(imageData);

// One-click enhance (applies multiple optimizations)
var enhanced = await editorPlugin.AutoEnhanceAsync(imageData);
```

### 7. Drawing & Shapes

```csharp
var drawOptions = new DrawOptions
{
    Color = "#FF0000",
    Thickness = 3,
    Fill = false,
    FillColor = "#00FF00",
    Opacity = 1.0f
};

// Draw line
var withLine = await editorPlugin.DrawLineAsync(
    imageData, 
    new Point(50, 50), 
    new Point(200, 200), 
    drawOptions);

// Draw rectangle
var withRect = await editorPlugin.DrawRectangleAsync(
    imageData,
    new Rectangle(100, 100, 200, 150),
    drawOptions);

// Draw circle
var withCircle = await editorPlugin.DrawCircleAsync(
    imageData,
    new Point(200, 200),
    radius: 100,
    drawOptions);

// Draw ellipse
var withEllipse = await editorPlugin.DrawEllipseAsync(
    imageData,
    new Rectangle(100, 100, 300, 200),
    drawOptions);
```

### 8. Generic Filter System

```csharp
// Apply named filter with parameters
var parameters = new Dictionary<string, object>
{
    ["radius"] = 10,
    ["intensity"] = 0.8f
};

var filtered = await editorPlugin.ApplyFilterAsync(imageData, "blur", parameters);
```

## Usage Examples

### Example 1: Photo Enhancement Workflow

```csharp
public async Task<byte[]> EnhancePhotoAsync(byte[] originalPhoto)
{
    var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Professional Image Editor");
    
    // Step 1: Auto-enhance
    var enhanced = await plugin.AutoEnhanceAsync(originalPhoto);
    
    // Step 2: Adjust colors
    enhanced = await plugin.AdjustSaturationAsync(enhanced, 10);
    enhanced = await plugin.AdjustContrastAsync(enhanced, 5);
    
    // Step 3: Sharpen
    enhanced = await plugin.ApplySharpenAsync(enhanced, 1.5f);
    
    // Step 4: Add watermark
    var watermark = LoadCompanyLogo();
    var options = new WatermarkOptions
    {
        Position = WatermarkPosition.BottomRight,
        Opacity = 0.6f,
        MarginX = 20,
        MarginY = 20
    };
    enhanced = await plugin.AddWatermarkAsync(enhanced, watermark, options);
    
    // Step 5: Optimize
    enhanced = await plugin.OptimizeAsync(enhanced, 90);
    
    return enhanced;
}
```

### Example 2: Batch Processing

```csharp
public async Task<List<byte[]>> ProcessBatchAsync(List<byte[]> images)
{
    var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Professional Image Editor");
    var results = new List<byte[]>();
    
    foreach (var image in images)
    {
        // Resize all to standard dimensions
        var processed = await plugin.ResizeAsync(image, 1920, 1080, ResizeMode.Fit);
        
        // Apply vintage filter
        processed = await plugin.ConvertToSepiaAsync(processed);
        processed = await plugin.ApplyVignetteAsync(processed, 0.5f);
        
        // Convert to WebP for web use
        processed = await plugin.ConvertFormatAsync(processed, ImageFormat.WebP);
        
        results.Add(processed);
    }
    
    return results;
}
```

### Example 3: Social Media Preset

```csharp
public async Task<byte[]> ApplySocialMediaPresetAsync(byte[] image, string preset)
{
    var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Professional Image Editor");
    
    return preset.ToLower() switch
    {
        "instagram" => await ApplyInstagramStyleAsync(plugin, image),
        "vintage" => await ApplyVintageStyleAsync(plugin, image),
        "vivid" => await ApplyVividStyleAsync(plugin, image),
        "bw" => await ApplyBlackAndWhiteStyleAsync(plugin, image),
        _ => image
    };
}

private async Task<byte[]> ApplyInstagramStyleAsync(IMediaEditorPlugin plugin, byte[] image)
{
    // Instagram-style processing
    var result = await plugin.AdjustSaturationAsync(image, 15);
    result = await plugin.AdjustContrastAsync(result, 10);
    result = await plugin.AdjustBrightnessAsync(result, 5);
    result = await plugin.ApplyVignetteAsync(result, 0.3f);
    return result;
}

private async Task<byte[]> ApplyVintageStyleAsync(IMediaEditorPlugin plugin, byte[] image)
{
    var result = await plugin.ConvertToSepiaAsync(image);
    result = await plugin.ApplyVignetteAsync(result, 0.6f);
    result = await plugin.AdjustBrightnessAsync(result, -10);
    result = await plugin.ApplyPixelateAsync(result, 2); // Slight grain
    return result;
}
```

### Example 4: Dynamic Watermarking

```csharp
public async Task<byte[]> AddDynamicWatermarkAsync(
    byte[] image, 
    string userName, 
    DateTime timestamp)
{
    var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Professional Image Editor");
    
    // Add username
    var textOptions = new TextOptions
    {
        Text = $"© {userName}",
        FontSize = 18,
        Color = "#FFFFFF",
        X = 20,
        Y = 20,
        EnableShadow = true,
        ShadowColor = "#000000",
        Opacity = 0.8f
    };
    
    var result = await plugin.AddTextAsync(image, textOptions);
    
    // Add timestamp
    textOptions.Text = timestamp.ToString("yyyy-MM-dd HH:mm");
    textOptions.Y = 50;
    textOptions.FontSize = 14;
    
    result = await plugin.AddTextAsync(result, textOptions);
    
    return result;
}
```

## Performance Tips

1. **Batch Similar Operations**: Group similar edits together to minimize processing overhead
2. **Optimize Early**: Apply resize/optimization before expensive filters
3. **Use Appropriate Quality**: Don't use 100% quality unless necessary
4. **Cache Results**: Cache processed images when possible
5. **Async Processing**: Use background jobs for heavy processing

## Error Handling

```csharp
try
{
    var result = await plugin.ApplyFilterAsync(imageData, "custom-filter", parameters);
}
catch (ArgumentException ex)
{
    // Filter not supported
    _logger.LogWarning("Filter not available: {Message}", ex.Message);
}
catch (Exception ex)
{
    // Other processing errors
    _logger.LogError(ex, "Image processing failed");
    throw new UserFriendlyException("Failed to process image");
}
```

## Best Practices

1. **Always validate input**: Check image data before processing
2. **Handle large files carefully**: Consider memory constraints
3. **Provide user feedback**: Show progress for long operations
4. **Test with various formats**: JPEG, PNG, WebP, etc.
5. **Consider mobile devices**: Optimize for mobile performance
6. **Preserve originals**: Keep original images before editing
7. **Use appropriate formats**: WebP for web, PNG for transparency, JPEG for photos

## Integration with Media Management

```csharp
public class MediaEditingService : IMediaEditingService
{
    private readonly IMediaItemAppService _mediaService;
    private readonly IMediaPluginManager _pluginManager;
    
    public async Task<MediaItemDto> EditAndSaveAsync(
        Guid mediaItemId, 
        List<EditOperation> operations)
    {
        // Get original media
        var mediaItem = await _mediaService.GetAsync(mediaItemId);
        var imageData = await DownloadMediaAsync(mediaItem);
        
        // Get editor plugin
        var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Professional Image Editor");
        
        // Apply operations
        var result = imageData;
        foreach (var operation in operations)
        {
            result = await ApplyOperationAsync(plugin, result, operation);
        }
        
        // Upload edited version
        var uploadInput = new UploadMediaInput
        {
            FileName = $"edited_{mediaItem.OriginalName}",
            Content = result,
            MimeType = mediaItem.MimeType,
            EntityType = mediaItem.EntityType,
            EntityId = mediaItem.EntityId
        };
        
        return await _mediaService.UploadAsync(uploadInput);
    }
}
```
