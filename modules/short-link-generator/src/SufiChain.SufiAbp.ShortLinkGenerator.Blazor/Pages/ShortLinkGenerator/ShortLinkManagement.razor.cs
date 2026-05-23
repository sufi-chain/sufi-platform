using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using SufiChain.SufiBlazor.Components.Data;
using SufiChain.SufiAbp.ShortLinkGenerator.Permissions;
using SufiChain.SufiBlazor.Contracts.Data;
using Microsoft.AspNetCore.Authorization;

namespace SufiChain.SufiAbp.ShortLinkGenerator.Blazor.Pages.ShortLinkGenerator;

public partial class ShortLinkManagementBase : ShortLinkGeneratorComponentBase
{
    [Inject] protected IShortUrlAppService ShortUrlAppService { get; set; } = null!;
    [Inject] protected IJSRuntime JsRuntime { get; set; } = null!;

    protected IReadOnlyList<ShortUrlDto> ShortUrlList { get; set; } = Array.Empty<ShortUrlDto>();
    protected int TotalCount { get; set; }
    protected int PageSize { get; set; } = 10;
    protected int PageIndex { get; set; }
    protected string CurrentSorting { get; set; } = string.Empty;
    protected string FilterText { get; set; } = string.Empty;
    protected bool IsLoading { get; set; }

    protected CreateShortUrlDto NewEntity { get; set; } = new();
    protected UpdateShortUrlDto EditingEntity { get; set; } = new();
    protected ShortUrlDto EditingEntityDto { get; set; } = new();
    protected Guid EditingEntityId { get; set; }
    protected ShortUrlDto? PendingDeleteEntity { get; set; }

    protected bool IsCreateDialogOpen { get; set; }
    protected bool IsEditDialogOpen { get; set; }
    protected bool IsDeleteDialogOpen { get; set; }

    protected bool HasCreatePermission { get; set; }
    protected bool HasEditPermission { get; set; }
    protected bool HasDeletePermission { get; set; }


    protected DateOnly? NewEntityExpiresAt
    {
        get => NewEntity.ExpiresAt.HasValue ? DateOnly.FromDateTime(NewEntity.ExpiresAt.Value) : null;
        set => NewEntity.ExpiresAt = value?.ToDateTime(TimeOnly.MinValue);
    }

    protected DateOnly? EditingEntityExpiresAt
    {
        get => EditingEntity.ExpiresAt.HasValue ? DateOnly.FromDateTime(EditingEntity.ExpiresAt.Value) : null;
        set => EditingEntity.ExpiresAt = value?.ToDateTime(TimeOnly.MinValue);
    }

    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
        await GetShortUrlsAsync();
        await base.OnInitializedAsync();
    }

    protected virtual async Task SetPermissionsAsync()
    {
        HasCreatePermission = await AuthorizationService.IsGrantedAnyAsync(ShortLinkGeneratorPermissions.ShortLinks.Create);
        HasEditPermission = await AuthorizationService.IsGrantedAsync(ShortLinkGeneratorPermissions.ShortLinks.Edit);
        HasDeletePermission = await AuthorizationService.IsGrantedAsync(ShortLinkGeneratorPermissions.ShortLinks.Delete);
    }

    protected virtual async Task GetShortUrlsAsync()
    {
        try
        {
            IsLoading = true;

            var input = new GetShortUrlListDto
            {
                Filter = string.IsNullOrWhiteSpace(FilterText) ? null : FilterText,
                MaxResultCount = PageSize,
                SkipCount = PageIndex * PageSize,
                Sorting = string.IsNullOrWhiteSpace(CurrentSorting) ? nameof(ShortUrlDto.CreationTime) + " DESC" : CurrentSorting
            };

            var result = await ShortUrlAppService.GetListAsync(input);
            ShortUrlList = result.Items;
            TotalCount = (int)result.TotalCount;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
        finally
        {
            IsLoading = false;
        }
    }

    protected virtual async Task OnPageIndexChangedAsync(int pageIndex)
    {
        PageIndex = pageIndex;
        await GetShortUrlsAsync();
    }

    protected virtual async Task OnSortChangedAsync(SbSort? sort)
    {
        CurrentSorting = sort == null || string.IsNullOrWhiteSpace(sort.Field)
            ? string.Empty
            : sort.Field + (sort.Direction == SbSortDirection.Descending ? " DESC" : string.Empty);

        PageIndex = 0;
        await GetShortUrlsAsync();
    }

    protected virtual async Task ApplyFiltersAsync()
    {
        PageIndex = 0;
        await GetShortUrlsAsync();
    }

    protected virtual async Task ClearFiltersAsync()
    {
        FilterText = string.Empty;
        PageIndex = 0;
        await GetShortUrlsAsync();
    }

    protected virtual async Task RefreshAsync()
    {
        await GetShortUrlsAsync();
    }

    protected virtual Task OpenCreateModalAsync()
    {
        NewEntity = new CreateShortUrlDto();
        IsCreateDialogOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CloseCreateModalAsync()
    {
        NewEntity = new CreateShortUrlDto();
        IsCreateDialogOpen = false;
        return Task.CompletedTask;
    }

    protected Task OnNewEntityExpiresAtChanged(DateOnly? value)
    {
        NewEntityExpiresAt = value;
        return Task.CompletedTask;
    }

    protected Task OnEditingEntityExpiresAtChanged(DateOnly? value)
    {
        EditingEntityExpiresAt = value;
        return Task.CompletedTask;
    }

    protected virtual async Task CreateShortUrlAsync()
    {
        try
        {
            var result = await ShortUrlAppService.CreateAsync(NewEntity);
            IsCreateDialogOpen = false;
            await GetShortUrlsAsync();

            if (!string.IsNullOrWhiteSpace(result.FullShortUrl))
            {
                await Message.SuccessAsync(L["ShortLinkCreatedSuccessfully"] + Environment.NewLine + result.FullShortUrl);
                await CopyShortLinkAsync(result.FullShortUrl, false);
            }
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual Task OpenEditModalAsync(ShortUrlDto entity)
    {
        EditingEntityId = entity.Id;
        EditingEntityDto = entity;
        EditingEntity = new UpdateShortUrlDto
        {
            DestinationUrl = entity.DestinationUrl,
            Description = entity.Description,
            IsActive = entity.IsActive,
            ExpiresAt = entity.ExpiresAt
        };

        IsEditDialogOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CloseEditModalAsync()
    {
        EditingEntity = new UpdateShortUrlDto();
        EditingEntityDto = new ShortUrlDto();
        EditingEntityId = Guid.Empty;
        IsEditDialogOpen = false;
        return Task.CompletedTask;
    }

    protected virtual async Task UpdateShortUrlAsync()
    {
        try
        {
            await ShortUrlAppService.UpdateAsync(EditingEntityId, EditingEntity);
            IsEditDialogOpen = false;
            await Message.Success(L["ShortLinkUpdatedSuccessfully"]);
            await GetShortUrlsAsync();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual Task PromptDeleteAsync(ShortUrlDto entity)
    {
        PendingDeleteEntity = entity;
        IsDeleteDialogOpen = true;
        return Task.CompletedTask;
    }

    protected virtual Task CancelDelete()
    {
        PendingDeleteEntity = null;
        IsDeleteDialogOpen = false;
        return Task.CompletedTask;
    }

    protected void OnDeleteDialogOpenChanged(bool open)
    {
        IsDeleteDialogOpen = open;
        if (!open)
        {
            PendingDeleteEntity = null;
        }
    }

    protected virtual async Task DeleteConfirmedAsync()
    {
        if (PendingDeleteEntity == null)
        {
            IsDeleteDialogOpen = false;
            return;
        }

        try
        {
            await ShortUrlAppService.DeleteAsync(PendingDeleteEntity.Id);
            await Message.SuccessAsync(L["ShortLinkDeletedSuccessfully"]);
            await CancelDelete();
            await GetShortUrlsAsync();
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    protected virtual async Task CopyShortLinkAsync(string? fullShortUrl, bool showSuccessMessage = true)
    {
        if (string.IsNullOrWhiteSpace(fullShortUrl))
        {
            return;
        }

        try
        {
            await JsRuntime.InvokeVoidAsync("navigator.clipboard.writeText", fullShortUrl);
            if (showSuccessMessage)
            {
                await Message.SuccessAsync(L["LinkCopiedToClipboard"]);
            }
        }
        catch
        {
            await Message.WarnAsync(L["FailedToCopyLink"]);
        }
    }

    protected virtual string GetTrimmedUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Length > 60 ? value[..60] + "..." : value;
    }

    protected override async Task HandleErrorAsync(Exception exception)
    {
        await Message.ErrorAsync(exception.Message);
    }
}
