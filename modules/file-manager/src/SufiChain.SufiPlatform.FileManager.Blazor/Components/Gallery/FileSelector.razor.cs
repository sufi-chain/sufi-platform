using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Application.Dtos;
using SufiChain.SufiPlatform.FileManager.FileItems;
using SufiChain.SufiPlatform.FileManager.FileStructures;
using SufiChain.SufiPlatform.FileManager.FileTypes;
using SufiChain.SufiPlatform.FileManager.Localization;
using SufiChain.SufiPlatform.UI.Blazor;

namespace SufiChain.SufiPlatform.FileManager.Blazor.Components.Gallery;

/// <summary>
/// Modal dialog component for selecting files from the file manager.
/// </summary>
public partial class FileSelector : FileManagerComponentBase
{
    private record StructureFilterOption(string? Key, string DisplayName);

    private static readonly FileType?[] _fileTypeOptions =
        new FileType?[] { null, FileType.Image, FileType.Video, FileType.Document };

    private string GetFileTypeLabel(FileType? type) => type?.ToString() ?? L["AllTypes"];

    [Inject]
    protected IFileItemAppService FileItemAppService { get; set; } = default!;

    [Inject]
    protected IFileStructureAppService FileStructureAppService { get; set; } = default!;

    [Parameter] public bool AllowMultiple { get; set; } = false;
    [Parameter] public FileType? FilterFileType { get; set; }
    [Parameter] public string? StructureKey { get; set; }
    [Parameter] public EventCallback<List<FileItemDto>> OnFileSelected { get; set; }

    private bool _isOpen = false;
    private List<FileItemDto> _fileItems = new();
    private HashSet<Guid> _selectedItems = new();
    private bool _isLoading = false;
    private string? _searchKeyword;
    private string? _filterStructureKey;
    private List<StructureFilterOption> _structureFilterOptions = new();
    private FileType? _selectedFileType;
    private int _currentPage = 1;
    private int _totalCount = 0;
    private int _totalPages = 0;
    private const int PageSize = 12;

    protected override void OnInitialized()
    {
        _selectedFileType = FilterFileType;
    }

    public async Task ShowAsync()
    {
        _selectedItems.Clear();
        _filterStructureKey = StructureKey;
        _isOpen = true;
        await LoadStructureFilterOptions();
        await LoadFileItems();
    }

    public void Hide()
    {
        _isOpen = false;
    }

    private async Task LoadFileItems()
    {
        _isLoading = true;
        StateHasChanged();

        try
        {
            var input = new GetFileListInput
            {
                Keyword = _searchKeyword,
                FileType = _selectedFileType,
                StructureKey = _filterStructureKey,
                SkipCount = (_currentPage - 1) * PageSize,
                MaxResultCount = PageSize,
                Sorting = "CreationTime DESC"
            };

            var result = await FileItemAppService.GetListAsync(input);
            _fileItems = result.Items.ToList();
            _totalCount = (int)result.TotalCount;
            _totalPages = (int)Math.Ceiling((double)_totalCount / PageSize);
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadStructureFilterOptions()
    {
        try
        {
            var result = await FileStructureAppService.GetListAsync(new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 100,
                Sorting = "DisplayName"
            });
            _structureFilterOptions = result.Items
                .Select(s => new StructureFilterOption(s.Key, ResolveStructureText(s)))
                .Prepend(new StructureFilterOption(null, L["AllStructures"].Value!))
                .ToList();
        }
        catch
        {
            _structureFilterOptions = new List<StructureFilterOption> { new(null, L["AllStructures"].Value!) };
        }
    }

    private async Task OnStructureFilterChanged()
    {
        _currentPage = 1;
        await LoadFileItems();
    }

    private async Task LoadPage(int page)
    {
        _currentPage = page;
        await LoadFileItems();
    }

    private void ToggleSelection(FileItemDto item)
    {
        if (AllowMultiple)
        {
            if (_selectedItems.Contains(item.Id))
                _selectedItems.Remove(item.Id);
            else
                _selectedItems.Add(item.Id);
        }
        else
        {
            _selectedItems.Clear();
            _selectedItems.Add(item.Id);
        }
    }

    private async Task ConfirmSelection()
    {
        var selectedFiles = _fileItems.Where(m => _selectedItems.Contains(m.Id)).ToList();
        await OnFileSelected.InvokeAsync(selectedFiles);
        Hide();
    }
}
