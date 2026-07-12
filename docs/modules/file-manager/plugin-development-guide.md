# Plugin Development Guide

## Overview

The SufiChain.SufiPlatform.FileManager module includes a powerful plugin system that allows you to extend the media management functionality with custom processors, editors, and filters.

## Plugin Types

### 1. IMediaPlugin (Base Interface)

All plugins must implement the `IMediaPlugin` interface:

```csharp
public interface IMediaPlugin
{
    string Name { get; }
    string Version { get; }
    string Description { get; }
    Task InitializeAsync();
}
```

### 2. IMediaProcessorPlugin

For custom media processing:

```csharp
public interface IMediaProcessorPlugin : IMediaPlugin
{
    bool CanProcess(MediaType mediaType, string mimeType);
    Task<ProcessingResult> ProcessAsync(Stream inputStream, ProcessingOptions options);
}
```

**Use Cases:**
- Custom video encoding
- Specialized image format conversion
- Document processing
- Audio normalization

### 3. IMediaEditorPlugin

For image editing capabilities:

```csharp
public interface IMediaEditorPlugin : IMediaPlugin
{
    // 50+ editing methods available
    Task<byte[]> CropAsync(byte[] imageData, Rectangle cropArea);
    Task<byte[]> ApplyFilterAsync(byte[] imageData, string filterName, Dictionary<string, object> parameters);
    // ... more methods
}
```

**Use Cases:**
- Custom filters
- Branding/watermarking
- Image enhancements
- Effects and transformations

## Creating a Custom Plugin

### Example: Custom Watermark Plugin

```csharp
using SufiChain.SufiPlatform.FileManager.Plugins;
using SufiChain.SufiPlatform.FileManager.Plugins.Dtos;
using Volo.Abp.DependencyInjection;

namespace MyCompany.CustomPlugins;

public class CompanyWatermarkPlugin : IMediaEditorPlugin, ITransientDependency
{
    private readonly IImageProcessor _imageProcessor;
    private readonly ILogger<CompanyWatermarkPlugin> _logger;
    private readonly byte[] _companyLogo;

    public string Name => "Company Watermark";
    public string Version => "1.0.0";
    public string Description => "Automatically adds company branding to images";

    public CompanyWatermarkPlugin(
        IImageProcessor imageProcessor,
        ILogger<CompanyWatermarkPlugin> logger)
    {
        _imageProcessor = imageProcessor;
        _logger = logger;
        
        // Load company logo
        _companyLogo = LoadCompanyLogo();
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation($"Initialized {Name} v{Version}");
        return Task.CompletedTask;
    }

    public async Task<byte[]> AddWatermarkAsync(
        byte[] imageData, 
        byte[] watermark, 
        WatermarkOptions options)
    {
        // Use company logo instead of provided watermark
        using var image = Image.Load(imageData);
        using var logo = Image.Load(_companyLogo);
        
        // Position at bottom-right with company standards
        var position = CalculatePosition(image, logo);
        
        image.Mutate(ctx =>
        {
            ctx.DrawImage(logo, position, 0.7f); // 70% opacity
        });
        
        using var ms = new MemoryStream();
        await image.SaveAsJpegAsync(ms);
        return ms.ToArray();
    }

    // Implement other IMediaEditorPlugin methods...
    // You can delegate to _imageProcessor or provide custom implementations
}
```

### Example: PDF Processor Plugin

```csharp
public class PdfProcessorPlugin : IMediaProcessorPlugin, ITransientDependency
{
    public string Name => "PDF Processor";
    public string Version => "1.0.0";
    public string Description => "Processes PDF documents with thumbnail generation";

    public bool CanProcess(MediaType mediaType, string mimeType)
    {
        return mediaType == MediaType.Document && 
               mimeType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase);
    }

    public async Task<ProcessingResult> ProcessAsync(
        Stream inputStream, 
        ProcessingOptions options)
    {
        // Use a PDF library to process the document
        var pdfDocument = await PdfDocument.LoadAsync(inputStream);
        
        var result = new ProcessingResult
        {
            Success = true,
            MimeType = "application/pdf",
            Size = inputStream.Length
        };

        if (options.GenerateThumbnail)
        {
            // Generate thumbnail from first page
            var thumbnailData = await GeneratePdfThumbnailAsync(pdfDocument);
            result.ThumbnailData = thumbnailData;
        }

        return result;
    }

    private async Task<byte[]> GeneratePdfThumbnailAsync(PdfDocument pdf)
    {
        // Implementation details...
    }
}
```

## Registering Plugins

Plugins are automatically discovered if they implement `IMediaPlugin` and are registered with the DI container.

### Option 1: Automatic Registration (Recommended)

Use ABP's dependency injection attributes:

```csharp
public class MyPlugin : IMediaEditorPlugin, ITransientDependency
{
    // Plugin will be automatically registered and discovered
}
```

### Option 2: Manual Registration

In your module's `ConfigureServices` method:

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    context.Services.AddTransient<IMediaPlugin, MyCustomPlugin>();
}
```

## Plugin Discovery

The `MediaPluginManager` automatically discovers plugins at application startup:

```csharp
public class MyModule : AbpModule
{
    public override async Task OnApplicationInitializationAsync(
        ApplicationInitializationContext context)
    {
        var pluginManager = context.ServiceProvider
            .GetRequiredService<IMediaPluginManager>();
        
        await pluginManager.DiscoverPluginsAsync();
    }
}
```

## Using Plugins

### Getting Loaded Plugins

```csharp
public class MyService
{
    private readonly IMediaPluginManager _pluginManager;

    public async Task ProcessImageAsync(byte[] imageData)
    {
        // Get all editor plugins
        var editorPlugins = _pluginManager.GetPlugins<IMediaEditorPlugin>();
        
        foreach (var plugin in editorPlugins)
        {
            Console.WriteLine($"Available: {plugin.Name} v{plugin.Version}");
        }

        // Get specific plugin by name
        var watermarkPlugin = _pluginManager.GetPlugin<IMediaEditorPlugin>("Company Watermark");
        
        if (watermarkPlugin != null)
        {
            var result = await watermarkPlugin.AddWatermarkAsync(imageData, null, new WatermarkOptions());
        }
    }
}
```

## Plugin Configuration

### Custom Configuration Options

```csharp
public class MyPluginOptions
{
    public bool Enabled { get; set; } = true;
    public string ApiKey { get; set; }
    public int MaxSize { get; set; } = 10 * 1024 * 1024;
}

public class MyPlugin : IMediaPlugin, ITransientDependency
{
    private readonly MyPluginOptions _options;

    public MyPlugin(IOptions<MyPluginOptions> options)
    {
        _options = options.Value;
    }
}
```

Configure in your module:

```csharp
public override void ConfigureServices(ServiceConfigurationContext context)
{
    Configure<MyPluginOptions>(options =>
    {
        options.Enabled = true;
        options.ApiKey = "your-api-key";
    });
}
```

## Best Practices

### 1. Error Handling

Always handle errors gracefully:

```csharp
public async Task<byte[]> ProcessAsync(byte[] data)
{
    try
    {
        // Processing logic
        return processedData;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Failed to process with {PluginName}", Name);
        
        // Return original data or throw, depending on requirements
        throw new UserFriendlyException($"Plugin {Name} failed: {ex.Message}");
    }
}
```

### 2. Performance Considerations

- Use async/await properly
- Dispose resources correctly
- Consider memory usage for large files
- Implement timeout mechanisms

### 3. Logging

Use structured logging:

```csharp
_logger.LogInformation("Processing {FileName} with {PluginName}", 
    fileName, Name);
```

### 4. Testing

Create unit tests for your plugins:

```csharp
public class MyPluginTests
{
    [Fact]
    public async Task Should_Apply_Watermark()
    {
        // Arrange
        var plugin = new CompanyWatermarkPlugin(/*...*/);
        var testImage = GetTestImage();

        // Act
        var result = await plugin.AddWatermarkAsync(testImage, null, new WatermarkOptions());

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Length > 0);
    }
}
```

## Advanced Topics

### Chaining Plugins

Process media through multiple plugins:

```csharp
public async Task<byte[]> ApplyPluginChainAsync(byte[] data, List<string> pluginNames)
{
    var result = data;
    
    foreach (var pluginName in pluginNames)
    {
        var plugin = _pluginManager.GetPlugin<IMediaEditorPlugin>(pluginName);
        if (plugin != null)
        {
            result = await plugin.ProcessAsync(result);
        }
    }
    
    return result;
}
```

### Plugin Marketplace

Create distributable plugins:

1. Package as NuGet package
2. Include plugin DLL and dependencies
3. Document installation and configuration
4. Provide usage examples

## Troubleshooting

### Plugin Not Discovered

- Ensure plugin implements `IMediaPlugin`
- Check DI registration (use `ITransientDependency` or manual registration)
- Verify `InitializeAsync` doesn't throw exceptions
- Check application logs during startup

### Plugin Errors

- Enable detailed logging
- Check `IMediaPluginManager.GetAllPlugins()` to see loaded plugins
- Verify plugin dependencies are available
- Test plugin in isolation

## Example: Complete Custom Filter Plugin

```csharp
public class VintageFilterPlugin : IMediaEditorPlugin, ITransientDependency
{
    public string Name => "Vintage Filter";
    public string Version => "1.0.0";
    public string Description => "Applies vintage photo effects";

    private readonly ILogger<VintageFilterPlugin> _logger;

    public VintageFilterPlugin(ILogger<VintageFilterPlugin> logger)
    {
        _logger = logger;
    }

    public Task InitializeAsync()
    {
        _logger.LogInformation("Vintage Filter Plugin initialized");
        return Task.CompletedTask;
    }

    public async Task<byte[]> ApplyFilterAsync(
        byte[] imageData, 
        string filterName, 
        Dictionary<string, object> parameters)
    {
        if (!filterName.Equals("vintage", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Filter '{filterName}' not supported by this plugin");
        }

        using var image = Image.Load<Rgba32>(imageData);
        
        // Apply vintage effect
        image.Mutate(ctx => ctx
            .Saturate(0.7f)        // Reduce saturation
            .Brightness(0.95f)     // Slightly darker
            .Sepia()               // Sepia tone
            .Vignette()            // Add vignette
            .GaussianSharpen(0.5f) // Slight grain effect
        );

        using var ms = new MemoryStream();
        await image.SaveAsJpegAsync(ms, new JpegEncoder { Quality = 85 });
        return ms.ToArray();
    }

    // Implement other required methods from IMediaEditorPlugin
    // Can throw NotImplementedException for unused methods
}
```

## Resources

- [ImageSharp Documentation](https://docs.sixlabors.com/articles/imagesharp/index.html)
- [ABP Dependency Injection](https://docs.abp.io/en/abp/latest/Dependency-Injection)
- [Plugin Pattern Best Practices](https://martinfowler.com/articles/plugins.html)
