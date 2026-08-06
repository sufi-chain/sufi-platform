using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.Features;
using SufiChain.SufiPlatform.Features.Blazor.Components;

namespace SufiChain.SufiPlatform.Editions.Blazor.Pages;

public partial class Editions
{
    private const string EditionFeatureProviderName = EditionFeatureValueProvider.ProviderName;

    [Inject] protected IEditionAppService EditionAppService { get; set; } = default!;
    [Inject] protected IAuthorizationService AuthorizationService { get; set; } = default!;

    private FeaturesModal _featuresModal = default!;
    private List<EditionDto>? _items;
    private bool _showCreate;
    private bool _showEdit;
    private bool _loading;
    private bool _saving;
    private Guid? _editingId;
    private string? _concurrencyStamp;
    private EditionFormModel _form = new();

    private bool HasCreatePermission { get; set; }
    private bool HasUpdatePermission { get; set; }
    private bool HasDeletePermission { get; set; }
    private bool HasManageFeaturesPermission { get; set; }

    protected override async Task OnInitializedAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Create);
        HasUpdatePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Update);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.Delete);
        HasManageFeaturesPermission = await AuthorizationService.IsGrantedAsync(EditionsPermissions.Editions.ManageFeatures);
        await ReloadAsync();
    }

    private async Task ReloadAsync()
    {
        _loading = true;
        try
        {
            var result = await EditionAppService.GetListAsync(new GetEditionsInput { MaxResultCount = 100 });
            _items = result.Items.ToList();
        }
        finally
        {
            _loading = false;
        }
    }

    private void ShowCreateModal()
    {
        _form = new EditionFormModel { IsActive = true };
        _editingId = null;
        _concurrencyStamp = null;
        _showCreate = true;
        _showEdit = false;
    }

    private void ShowEditModal(EditionDto edition)
    {
        _form = new EditionFormModel
        {
            Name = edition.Name,
            DisplayName = edition.DisplayName,
            Code = edition.Code,
            IsActive = edition.IsActive
        };
        _editingId = edition.Id;
        _concurrencyStamp = edition.ConcurrencyStamp;
        _showEdit = true;
        _showCreate = false;
    }

    private void CloseModal()
    {
        _showCreate = false;
        _showEdit = false;
    }

    private async Task SaveAsync()
    {
        _saving = true;
        try
        {
            if (_showCreate)
            {
                await EditionAppService.CreateAsync(new EditionCreateDto
                {
                    Name = _form.Name,
                    DisplayName = _form.DisplayName,
                    Code = _form.Code,
                    IsActive = _form.IsActive
                });
            }
            else if (_editingId.HasValue)
            {
                await EditionAppService.UpdateAsync(_editingId.Value, new EditionUpdateDto
                {
                    Name = _form.Name,
                    DisplayName = _form.DisplayName,
                    Code = _form.Code,
                    IsActive = _form.IsActive,
                    ConcurrencyStamp = _concurrencyStamp ?? string.Empty
                });
            }

            CloseModal();
            await ReloadAsync();
        }
        finally
        {
            _saving = false;
        }
    }

    private async Task DeleteAsync(EditionDto edition)
    {
        await EditionAppService.DeleteAsync(edition.Id);
        await ReloadAsync();
    }

    private Task ShowEditionFeatures(EditionDto edition)
    {
        return _featuresModal.OpenAsync(EditionFeatureProviderName, edition.Id.ToString(), edition.DisplayName);
    }

    private sealed class EditionFormModel
    {
        public string Name { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
