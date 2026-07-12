using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using SufiChain.SufiPlatform.FileManager.FileItems;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Public.Components.Upload;

public partial class PublicFileUploader
{
    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;

    [Parameter]
    public string? StructureKey { get; set; }

    [Parameter]
    public string? EntityType { get; set; }

    [Parameter]
    public Guid? EntityId { get; set; }

    [Parameter]
    public string? FolderPath { get; set; }

    [Parameter]
    public bool AutoConfirm { get; set; } = true;

    [Parameter]
    public bool AllowMultiple { get; set; } = true;

    [Parameter]
    public long MaxFileSize { get; set; } = 100 * 1024 * 1024;

    [Parameter]
    public int MaxFiles { get; set; } = 20;

    [Parameter]
    public string Accept { get; set; } = "image/*,application/pdf,video/*,.docx,.html,.htm,.txt,.md";

    [Parameter]
    public string Title { get; set; } = "Upload files";

    [Parameter]
    public string Description { get; set; } = "Drag files here or browse from your device.";

    [Parameter]
    public string BrowseButtonText { get; set; } = "Browse files";

    [Parameter]
    public string RemoveButtonText { get; set; } = "Remove file";

    [Parameter]
    public string? Class { get; set; }

    [Parameter]
    public string? Style { get; set; }

    [Parameter]
    public EventCallback<List<FileItemDto>> Uploaded { get; set; }

    [Parameter]
    public EventCallback<string> UploadFailed { get; set; }

    private readonly string _fileInputId = $"public-file-uploader-{Guid.NewGuid():N}";
    private readonly List<SelectedUploadFile> _selectedFiles = new();
    private readonly List<string> _errors = new();
    private bool _isDragging;

    protected virtual void HandleDragEnter()
    {
        _isDragging = true;
    }

    protected virtual void HandleDragLeave()
    {
        _isDragging = false;
    }

    protected virtual Task HandleDrop(DragEventArgs args)
    {
        _isDragging = false;
        return Task.CompletedTask;
    }

    protected virtual async Task OnFilesSelectedAsync(InputFileChangeEventArgs args)
    {
        _errors.Clear();
        _selectedFiles.Clear();

        var browserFiles = args.GetMultipleFiles(AllowMultiple ? MaxFiles : 1).ToList();
        foreach (var browserFile in browserFiles)
        {
            var selectedFile = new SelectedUploadFile(browserFile.Name, browserFile.Size, browserFile.ContentType);
            _selectedFiles.Add(selectedFile);

            if (browserFile.Size > MaxFileSize)
            {
                selectedFile.HasError = true;
                selectedFile.ErrorMessage = $"File '{browserFile.Name}' exceeds the maximum size of {FormatFileSize(MaxFileSize)}.";
                _errors.Add(selectedFile.ErrorMessage);
                continue;
            }

            await UploadFileAsync(browserFile, selectedFile);
        }

        var uploadedFiles = _selectedFiles.Where(x => x.FileItem != null).Select(x => x.FileItem!).ToList();
        if (uploadedFiles.Any())
        {
            await Uploaded.InvokeAsync(uploadedFiles);
        }
    }

    protected virtual async Task UploadFileAsync(IBrowserFile browserFile, SelectedUploadFile selectedFile)
    {
        try
        {
            selectedFile.IsUploading = true;
            selectedFile.Progress = 10;

            await using var stream = browserFile.OpenReadStream(MaxFileSize);
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            selectedFile.Progress = 75;

            selectedFile.FileItem = await FileItemAppService.UploadAsync(new UploadFileInput
            {
                FileName = browserFile.Name,
                Content = memoryStream.ToArray(),
                MimeType = browserFile.ContentType,
                StructureKey = StructureKey,
                EntityType = EntityType,
                EntityId = EntityId,
                FolderPath = FolderPath,
                AutoConfirm = AutoConfirm
            });

            selectedFile.Progress = 100;
            selectedFile.IsUploaded = true;
        }
        catch (Exception ex)
        {
            selectedFile.HasError = true;
            selectedFile.ErrorMessage = ex.Message;
            _errors.Add(ex.Message);
            await UploadFailed.InvokeAsync(ex.Message);
        }
        finally
        {
            selectedFile.IsUploading = false;
        }
    }

    protected virtual void RemoveFile(SelectedUploadFile file)
    {
        _selectedFiles.Remove(file);
    }

    protected virtual string GetFileIcon(SelectedUploadFile file)
    {
        if (file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            return "image";
        }

        if (file.ContentType.StartsWith("video/", StringComparison.OrdinalIgnoreCase))
        {
            return "video";
        }

        if (file.ContentType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
        {
            return "file-text";
        }

        return "file";
    }

    protected virtual string FormatFileSize(long bytes)
    {
        if (bytes < 1024)
        {
            return $"{bytes} B";
        }

        if (bytes < 1024 * 1024)
        {
            return $"{bytes / 1024d:0.#} KB";
        }

        return $"{bytes / 1024d / 1024d:0.#} MB";
    }

    protected sealed class SelectedUploadFile
    {
        public SelectedUploadFile(string name, long size, string contentType)
        {
            Name = name;
            Size = size;
            ContentType = contentType;
        }

        public string Name { get; }
        public long Size { get; }
        public string ContentType { get; }
        public int Progress { get; set; }
        public bool IsUploading { get; set; }
        public bool IsUploaded { get; set; }
        public bool HasError { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
        public FileItemDto? FileItem { get; set; }
    }
}
