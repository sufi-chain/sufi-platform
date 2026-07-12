# SufiChain.SufiPlatform.FileManager – Integration Guide

## Overview

SufiChain.SufiPlatform.FileManager is an ABP module for managing files and media (images, videos, documents, audio) with thumbnails, WebP conversion, structured file definitions, and Blazor UI.

For **full configuration options** (options, blob storage, tiered apps, appsettings), see **[Configuration](configuration.md)**.

---

## Installation

### 1. Install NuGet Packages

```bash
# Host (API) application
dotnet add package SufiChain.SufiPlatform.FileManager.Application
dotnet add package SufiChain.SufiPlatform.FileManager.EntityFrameworkCore
# OR for MongoDB:
# dotnet add package SufiChain.SufiPlatform.FileManager.MongoDB
dotnet add package SufiChain.SufiPlatform.FileManager.HttpApi

# Blazor application
dotnet add package SufiChain.SufiPlatform.FileManager.Blazor
dotnet add package SufiChain.SufiPlatform.FileManager.HttpApi.Client
```

### 2. Add Module Dependencies

**API host module:**

```csharp
[DependsOn(
    typeof(FileManagerApplicationModule),
    typeof(FileManagerEntityFrameworkCoreModule), // or SufiAbpFileManagerMongoDbModule
    typeof(FileManagerHttpApiModule)
)]
public class YourHttpApiHostModule : AbpModule { }
```

**Blazor app module:**

```csharp
[DependsOn(
    typeof(FileManagerBlazorModule),
    typeof(SufiAbpFileManagerHttpApiClientModule)
)]
public class YourBlazorModule : AbpModule { }
```

### 3. Configure Database Context (EF Core)

Add file manager (and blob storing) to your DbContext:

```csharp
protected override void OnModelCreating(ModelBuilder builder)
{
    base.OnModelCreating(builder);
    builder.ConfigureSpFileManager();
    // Required when using default database blob storage:
    builder.ConfigureBlobStoring();
}
```

### 4. Run Migrations

```bash
dotnet ef migrations add AddFileManagerAndBlobStoring
dotnet ef database update
```

---

## Configuration

For **all options** (BaseUrl, quotas, file structures, blob storage, tiered apps), see **[Configuration](configuration.md)**.

### Quick configuration example

```csharp
Configure<SufiAbpFileManagerOptions>(options =>
{
    options.BaseUrl = "https://yourapp.com/";
    options.DefaultStorageQuotaMB = 2048;
    options.AddDefaultStructures(); // optional: "General" structure

    options.DefineStructure("Product.MainImage")
        .WithDisplayName("Product Main Image")
        .ForFileTypes(FileType.Image)
        .AllowExtensions("jpg", "jpeg", "png", "webp")
        .WithMaxSize(5.MB())
        .WithImageDimensions(minWidth: 800, minHeight: 600, maxWidth: 4096, maxHeight: 4096)
        .SingleFile()
        .Required()
        .GenerateThumbnail(true, 200, 200)
        .EnableWebPConversion(true, 80)
        .IsPublic(true);
});
```

Blob storage is configured by the EF Core module (database by default). To override (e.g. file system or Azure), see [Configuration – Blob storage](configuration.md#2-blob-storage).

---

## Usage

### Upload from application service

```csharp
public class YourAppService : ApplicationService
{
    private readonly IFileItemAppService _fileItemAppService;

    public YourAppService(IFileItemAppService fileItemAppService)
    {
        _fileItemAppService = fileItemAppService;
    }

    public async Task<ProductDto> CreateProductAsync(CreateProductInput input)
    {
        var mainImage = await _fileItemAppService.UploadAsync(new UploadFileInput
        {
            FileName = input.MainImageFileName,
            Content = input.MainImageContent,
            MimeType = input.MainImageMimeType,
            StructureKey = "Product.MainImage",
            EntityType = "Product",
            EntityId = productId,
            AutoConfirm = true,
            Alt = input.ProductName
        });

        if (input.GalleryImages?.Any() == true)
        {
            await _fileItemAppService.UploadMultipleAsync(new UploadMultipleFileInput
            {
                Files = input.GalleryImages.Select(img => new FileContentInput
                {
                    FileName = img.FileName,
                    Content = img.Content,
                    MimeType = img.MimeType
                }).ToArray(),
                StructureKey = "Product.Gallery",
                EntityType = "Product",
                EntityId = productId,
                AutoConfirm = true
            });
        }

        var product = new Product { Name = input.ProductName, MainImageId = mainImage.Id };
        return ObjectMapper.Map<Product, ProductDto>(product);
    }

    public async Task<List<FileItemDto>> GetProductFilesAsync(Guid productId)
    {
        var result = await _fileItemAppService.GetListAsync(new GetFileListInput
        {
            EntityType = "Product",
            EntityId = productId
        });
        return result.Items.ToList();
    }
}
```

### Use in Blazor components

```razor
@page "/products/create"
@using SufiChain.SufiPlatform.FileManager.Blazor.Components.Upload
@using SufiChain.SufiPlatform.FileManager.Blazor.Components.Gallery
@using SufiChain.SufiPlatform.FileManager.Blazor.Components.Browser
@using SufiChain.SufiPlatform.FileManager.FileItems

<SbCard>
    <ChildContent>
        <!-- Quick single-image uploader with preview -->
        <SbFormField Label="Main Product Image">
            <SufiAbpQuickImageUploader 
                StructureKey="Product.MainImage"
                AutoConfirm="true"
                OnImageUploaded="@OnMainImageUploaded" />
        </SbFormField>

        <!-- Multi-file uploader with drag-drop and progress -->
        <SbFormField Label="Product Gallery">
            <SufiAbpFileUploader 
                StructureKey="Product.Gallery"
                AllowMultiple="true"
                AutoConfirm="true"
                OnUploadCompleted="@OnGalleryUploaded" />
        </SbFormField>

        <!-- Gallery view of existing files -->
        <SbFormField Label="File Gallery">
            <SufiAbpFileGallery 
                EntityType="Product"
                EntityId="@ProductId"
                Selectable="true"
                OnFileSelected="@OnFileSelected" />
        </SbFormField>

        <!-- File browser with folders, search, bulk operations -->
        <SbFormField Label="File Browser">
            <SufiAbpFileBrowser 
                EntityType="Product"
                EntityId="@ProductId"
                OnFileSelected="@OnFileSelected" />
        </SbFormField>
    </ChildContent>
</SbCard>

@code {
    private Guid ProductId { get; set; }
    private Guid? MainImageId;
    private List<FileItemDto> GalleryFiles = new();

    private void OnMainImageUploaded(FileItemDto file)
    {
        MainImageId = file.Id;
    }

    private void OnGalleryUploaded(List<FileItemDto> files)
    {
        GalleryFiles.AddRange(files);
    }

    private void OnFileSelected(FileItemDto file)
    {
        // Handle selected file
    }
}
```

### Access file URLs

```csharp
var downloadUrl = await _fileItemAppService.GetDownloadUrlAsync(fileId);
var thumbnailUrl = await _fileItemAppService.GetThumbnailUrlAsync(fileId);
var streamUrl = await _fileItemAppService.GetStreamUrlAsync(fileId);
```

**HTTP API base path:** `api/sabp/file-manager/file-items`  
- Download: `GET .../{id}/download` — Thumbnail: `GET .../{id}/thumbnail` — Stream: `GET .../{id}/stream`

In tiered Blazor apps, use **IFileItemUrlProvider** (Blazor.Public) so URLs point to the API. See [Configuration – Tiered applications](configuration.md#3-tiered-applications-blazor-and-api-on-different-urls).

### Query files

```csharp
var result = await _fileItemAppService.GetListAsync(new GetFileListInput
{
    Keyword = "product",
    FileType = FileType.Image,
    StructureKey = "Product.MainImage",
    EntityType = "Product",
    EntityId = productId,
    Sorting = "CreationTime DESC",
    MaxResultCount = 50
});
```

### Storage quota and temporary files

```csharp
var quota = await _fileItemAppService.GetStorageQuotaAsync();
var temp = await _fileItemAppService.UploadAsync(new UploadFileInput { ... AutoConfirm = false });
await _fileItemAppService.ConfirmAsync(temp.Id);
```

---

## Permissions

| Permission | Description |
|------------|-------------|
| **SufiAbpFileManager.FileItems** | View file items |
| **SufiAbpFileManager.FileItems.Create / Update / Delete** | Upload, update metadata, delete |
| **SufiAbpFileManager.FileStructures** | View file structures |
| **SufiAbpFileManager.FileStructures.Create / Update / Delete** | Manage structures |

See [Configuration – Permissions](configuration.md#4-permissions).

---

## Best practices

1. Define **file structures** per use case and use `StructureKey` on uploads.
2. Enable **thumbnails** and **WebP** where appropriate.
3. Set **MaxUploadFileSizeMB** and **DefaultStorageQuotaMB**; see [Configuration](configuration.md).
4. Use **AutoConfirm = false** and **ConfirmAsync** for forms with validation.
5. In **tiered** setups, set **RemoteServices:SufiAbpFileManager:BaseUrl** and **SufiAbpFileManager:BaseUrl**; see [Configuration](configuration.md).

---

## Troubleshooting

| Issue | Check |
|-------|--------|
| **Images/thumbnails 404** | Tiered: set `RemoteServices:SufiAbpFileManager:BaseUrl` in Blazor and `SufiAbpFileManager:BaseUrl` on API. See [Configuration](configuration.md#3-tiered-applications-blazor-and-api-on-different-urls). |
| **Upload fails** | File size vs `MaxUploadFileSizeMB`, structure allowed types/sizes, storage quota. |
| **Blob not found** | Blob storage configured for `SufiAbpFileManagerContainer`; DbContext has `ConfigureBlobStoring()` and migrations applied. |

---

## See also

- [Configuration](configuration.md) – Full options, blob storage, tiered apps
- [Blazor components guide](blazor-components-guide.md) – All UI components
- [API reference](api-reference.md) – IFileItemAppService and HTTP API
- [DevOps Guide](../../devops/file-upload-configuration.md) – Production deployment, SignalR limits, web server config
