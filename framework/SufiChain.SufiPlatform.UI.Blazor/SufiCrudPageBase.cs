using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using SufiChain.SufiPlatform.UI.Extensibility.EntityActions;
using SufiChain.SufiPlatform.UI.Extensibility.TableColumns;
using SufiChain.SufiPlatform.UI.Layout;

namespace SufiChain.SufiPlatform.UI.Blazor;

/// <summary>
/// Base class for CRUD pages with common functionality for listing, creating, updating, and deleting entities.
/// </summary>
/// <typeparam name="TEntityDto">The entity DTO type for display.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
public abstract class SufiCrudPageBase<TEntityDto, TKey> 
    : SufiCrudPageBase<TEntityDto, TKey, TEntityDto, TEntityDto>
    where TEntityDto : class, new()
{
}

/// <summary>
/// Base class for CRUD pages with separate create and update input types.
/// </summary>
/// <typeparam name="TEntityDto">The entity DTO type for display.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
/// <typeparam name="TCreateInput">The type for create operations.</typeparam>
/// <typeparam name="TUpdateInput">The type for update operations.</typeparam>
public abstract class SufiCrudPageBase<TEntityDto, TKey, TCreateInput, TUpdateInput>
    : SufiCrudPageBase<TEntityDto, TEntityDto, TKey, TCreateInput, TUpdateInput>
    where TEntityDto : class, new()
    where TCreateInput : class, new()
    where TUpdateInput : class, new()
{
}

/// <summary>
/// Full CRUD page base class with all customizable type parameters.
/// </summary>
/// <typeparam name="TGetOutputDto">The DTO type for single entity retrieval.</typeparam>
/// <typeparam name="TGetListOutputDto">The DTO type for list retrieval.</typeparam>
/// <typeparam name="TKey">The primary key type.</typeparam>
/// <typeparam name="TCreateInput">The type for create operations.</typeparam>
/// <typeparam name="TUpdateInput">The type for update operations.</typeparam>
public abstract class SufiCrudPageBase<TGetOutputDto, TGetListOutputDto, TKey, TCreateInput, TUpdateInput>
    : SufiComponentBase
    where TGetOutputDto : class
    where TGetListOutputDto : class
    where TCreateInput : class, new()
    where TUpdateInput : class, new()
{
    // ====== STATE ======

    /// <summary>
    /// Default page size for list queries.
    /// </summary>
    protected virtual int PageSize { get; } = 10;

    /// <summary>
    /// Current page number (1-based).
    /// </summary>
    protected int CurrentPage = 1;

    /// <summary>
    /// Current sorting expression.
    /// </summary>
    protected string? CurrentSorting;

    /// <summary>
    /// Total count of entities.
    /// </summary>
    protected int? TotalCount;

    /// <summary>
    /// The current list of entities.
    /// </summary>
    protected IReadOnlyList<TGetListOutputDto> Entities = Array.Empty<TGetListOutputDto>();

    /// <summary>
    /// The entity being created.
    /// </summary>
    protected TCreateInput NewEntity = new();

    /// <summary>
    /// The ID of the entity being edited.
    /// </summary>
    protected TKey EditingEntityId = default!;

    /// <summary>
    /// The entity being edited.
    /// </summary>
    protected TUpdateInput EditingEntity = new();

    /// <summary>
    /// Breadcrumb items for the page.
    /// </summary>
    protected List<BreadcrumbItem> BreadcrumbItems = new(2);

    /// <summary>
    /// Entity actions dictionary.
    /// </summary>
    protected EntityActionDictionary EntityActions { get; set; } = new();

    /// <summary>
    /// Table columns dictionary.
    /// </summary>
    protected TableColumnDictionary TableColumns { get; set; } = new();

    // ====== PERMISSIONS ======

    /// <summary>
    /// Policy name required for creating entities.
    /// </summary>
    protected string? CreatePolicyName { get; set; }

    /// <summary>
    /// Policy name required for updating entities.
    /// </summary>
    protected string? UpdatePolicyName { get; set; }

    /// <summary>
    /// Policy name required for deleting entities.
    /// </summary>
    protected string? DeletePolicyName { get; set; }

    /// <summary>
    /// Whether the user has permission to create entities.
    /// </summary>
    public bool HasCreatePermission { get; set; }

    /// <summary>
    /// Whether the user has permission to update entities.
    /// </summary>
    public bool HasUpdatePermission { get; set; }

    /// <summary>
    /// Whether the user has permission to delete entities.
    /// </summary>
    public bool HasDeletePermission { get; set; }

    // ====== MODAL STATE ======

    /// <summary>
    /// Whether the create modal is visible.
    /// </summary>
    protected bool IsCreateModalVisible { get; set; }

    /// <summary>
    /// Whether the edit modal is visible.
    /// </summary>
    protected bool IsEditModalVisible { get; set; }

    // ====== LIFECYCLE ======

    protected override async Task OnInitializedAsync()
    {
        await SetPermissionsAsync();
        await SetEntityActionsAsync();
        await SetTableColumnsAsync();
        await InvokeAsync(StateHasChanged);
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await SetToolbarItemsAsync();
            await SetBreadcrumbItemsAsync();
        }
        await base.OnAfterRenderAsync(firstRender);
    }

    // ====== PERMISSIONS ======

    /// <summary>
    /// Sets permissions based on policy names.
    /// </summary>
    protected virtual async Task SetPermissionsAsync()
    {
        if (CreatePolicyName != null)
        {
            HasCreatePermission = await IsGrantedAsync(CreatePolicyName);
        }

        if (UpdatePolicyName != null)
        {
            HasUpdatePermission = await IsGrantedAsync(UpdatePolicyName);
        }

        if (DeletePolicyName != null)
        {
            HasDeletePermission = await IsGrantedAsync(DeletePolicyName);
        }
    }

    /// <summary>
    /// Checks a policy and throws if not granted.
    /// </summary>
    protected virtual async Task CheckPolicyAsync(string? policyName)
    {
        if (string.IsNullOrEmpty(policyName))
        {
            return;
        }

        if (!await IsGrantedAsync(policyName))
        {
            throw new UnauthorizedAccessException($"Policy '{policyName}' is not granted.");
        }
    }

    protected virtual Task CheckCreatePolicyAsync() => CheckPolicyAsync(CreatePolicyName);
    protected virtual Task CheckUpdatePolicyAsync() => CheckPolicyAsync(UpdatePolicyName);
    protected virtual Task CheckDeletePolicyAsync() => CheckPolicyAsync(DeletePolicyName);

    // ====== DATA OPERATIONS ======

    /// <summary>
    /// Gets the list of entities. Override to implement data fetching.
    /// </summary>
    protected abstract Task<(IReadOnlyList<TGetListOutputDto> Items, int TotalCount)> GetEntitiesAsync(
        int skipCount,
        int maxResultCount,
        string? sorting);

    /// <summary>
    /// Gets a single entity by ID. Override to implement data fetching.
    /// </summary>
    protected abstract Task<TGetOutputDto> GetEntityAsync(TKey id);

    /// <summary>
    /// Creates a new entity. Override to implement data creation.
    /// </summary>
    protected abstract Task<TGetOutputDto> CreateEntityAsync(TCreateInput input);

    /// <summary>
    /// Updates an existing entity. Override to implement data updating.
    /// </summary>
    protected abstract Task<TGetOutputDto> UpdateEntityAsync(TKey id, TUpdateInput input);

    /// <summary>
    /// Deletes an entity. Override to implement data deletion.
    /// </summary>
    protected abstract Task DeleteEntityAsync(TKey id);

    /// <summary>
    /// Refreshes the entity list.
    /// </summary>
    protected virtual async Task RefreshEntitiesAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            var skipCount = (CurrentPage - 1) * PageSize;
            var result = await GetEntitiesAsync(skipCount, PageSize, CurrentSorting);
            Entities = result.Items;
            TotalCount = result.TotalCount;
        }, "load");
    }

    /// <summary>
    /// Searches and refreshes the entity list from the first page.
    /// </summary>
    protected virtual async Task SearchEntitiesAsync()
    {
        var currentPage = CurrentPage;
        CurrentPage = 1;
        if (currentPage == 1)
        {
            await RefreshEntitiesAsync();
        }
        await InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Handles page change in the data grid.
    /// </summary>
    protected virtual async Task OnPageChangedAsync(int page)
    {
        CurrentPage = page;
        await RefreshEntitiesAsync();
    }

    /// <summary>
    /// Handles sort change in the data grid.
    /// </summary>
    protected virtual async Task OnSortChangedAsync(string? sorting)
    {
        CurrentSorting = sorting;
        await RefreshEntitiesAsync();
    }

    // ====== CREATE OPERATIONS ======

    /// <summary>
    /// Opens the create modal.
    /// </summary>
    protected virtual async Task OpenCreateModalAsync()
    {
        try
        {
            await CheckCreatePolicyAsync();
            NewEntity = new TCreateInput();
            IsCreateModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    /// <summary>
    /// Closes the create modal.
    /// </summary>
    protected virtual Task CloseCreateModalAsync()
    {
        IsCreateModalVisible = false;
        NewEntity = new TCreateInput();
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Creates a new entity from the modal.
    /// </summary>
    protected virtual async Task CreateEntityFromModalAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await OnCreatingEntityAsync();
            await CheckCreatePolicyAsync();
            await CreateEntityAsync(NewEntity);
            await OnCreatedEntityAsync();
        }, "create");
    }

    /// <summary>
    /// Called before creating an entity.
    /// </summary>
    protected virtual Task OnCreatingEntityAsync() => Task.CompletedTask;

    /// <summary>
    /// Called after creating an entity.
    /// </summary>
    protected virtual async Task OnCreatedEntityAsync()
    {
        await RefreshEntitiesAsync();
        await CloseCreateModalAsync();
        await Notify.SuccessAsync(L["CreatedSuccessfully"]);
    }

    // ====== UPDATE OPERATIONS ======

    /// <summary>
    /// Opens the edit modal for the specified entity.
    /// </summary>
    protected virtual async Task OpenEditModalAsync(TGetListOutputDto entity)
    {
        try
        {
            await CheckUpdatePolicyAsync();
            
            EditingEntityId = GetEntityId(entity);
            var fullEntity = await GetEntityAsync(EditingEntityId);
            EditingEntity = MapToEditingEntity(fullEntity);
            
            IsEditModalVisible = true;
            await InvokeAsync(StateHasChanged);
        }
        catch (Exception ex)
        {
            await HandleErrorAsync(ex);
        }
    }

    /// <summary>
    /// Closes the edit modal.
    /// </summary>
    protected virtual Task CloseEditModalAsync()
    {
        IsEditModalVisible = false;
        EditingEntity = new TUpdateInput();
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>
    /// Updates the entity from the modal.
    /// </summary>
    protected virtual async Task UpdateEntityFromModalAsync()
    {
        await ExecuteWithLoadingAsync(async () =>
        {
            await OnUpdatingEntityAsync();
            await CheckUpdatePolicyAsync();
            await UpdateEntityAsync(EditingEntityId, EditingEntity);
            await OnUpdatedEntityAsync();
        }, "update");
    }

    /// <summary>
    /// Called before updating an entity.
    /// </summary>
    protected virtual Task OnUpdatingEntityAsync() => Task.CompletedTask;

    /// <summary>
    /// Called after updating an entity.
    /// </summary>
    protected virtual async Task OnUpdatedEntityAsync()
    {
        await RefreshEntitiesAsync();
        await CloseEditModalAsync();
        await Notify.SuccessAsync(L["SavedSuccessfully"]);
    }

    // ====== DELETE OPERATIONS ======

    /// <summary>
    /// Deletes the specified entity with confirmation.
    /// </summary>
    protected virtual async Task DeleteEntityWithConfirmationAsync(TGetListOutputDto entity)
    {
        var confirmed = await Message.ConfirmAsync(
            GetDeleteConfirmationMessage(entity),
            L["AreYouSure"]);

        if (!confirmed)
        {
            return;
        }

        await ExecuteWithLoadingAsync(async () =>
        {
            await OnDeletingEntityAsync();
            await CheckDeletePolicyAsync();
            await DeleteEntityAsync(GetEntityId(entity));
            await OnDeletedEntityAsync();
        }, "delete");
    }

    /// <summary>
    /// Called before deleting an entity.
    /// </summary>
    protected virtual Task OnDeletingEntityAsync() => Task.CompletedTask;

    /// <summary>
    /// Called after deleting an entity.
    /// </summary>
    protected virtual async Task OnDeletedEntityAsync()
    {
        // Go to previous page if this was the last item
        if (Entities.Count == 1 && CurrentPage > 1)
        {
            CurrentPage -= 1;
        }

        await RefreshEntitiesAsync();
        await Notify.SuccessAsync(L["DeletedSuccessfully"]);
    }

    /// <summary>
    /// Gets the confirmation message for deleting an entity.
    /// </summary>
    protected virtual string GetDeleteConfirmationMessage(TGetListOutputDto entity)
    {
        return L["ItemWillBeDeletedMessage"];
    }

    // ====== MAPPING ======

    /// <summary>
    /// Gets the ID from an entity DTO. Override if your DTO doesn't have an Id property.
    /// </summary>
    protected virtual TKey GetEntityId(TGetListOutputDto entity)
    {
        var idProperty = typeof(TGetListOutputDto).GetProperty("Id");
        if (idProperty == null)
        {
            throw new InvalidOperationException(
                $"Type {typeof(TGetListOutputDto).Name} does not have an Id property. " +
                "Override GetEntityId method to provide custom ID extraction.");
        }
        return (TKey)idProperty.GetValue(entity)!;
    }

    /// <summary>
    /// Maps a get output DTO to an update input. Override for custom mapping.
    /// </summary>
    protected virtual TUpdateInput MapToEditingEntity(TGetOutputDto entity)
    {
        // Simple property copy - override for custom mapping
        var updateInput = new TUpdateInput();
        var sourceProps = typeof(TGetOutputDto).GetProperties();
        var targetProps = typeof(TUpdateInput).GetProperties();

        foreach (var targetProp in targetProps.Where(p => p.CanWrite))
        {
            var sourceProp = sourceProps.FirstOrDefault(p => 
                p.Name == targetProp.Name && p.PropertyType == targetProp.PropertyType);
            
            if (sourceProp != null)
            {
                targetProp.SetValue(updateInput, sourceProp.GetValue(entity));
            }
        }

        return updateInput;
    }

    // ====== CONFIGURATION ======

    /// <summary>
    /// Sets entity actions. Override to configure actions.
    /// </summary>
    protected virtual ValueTask SetEntityActionsAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Sets table columns. Override to configure columns.
    /// </summary>
    protected virtual ValueTask SetTableColumnsAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Sets toolbar items. Override to configure toolbar.
    /// </summary>
    protected virtual ValueTask SetToolbarItemsAsync() => ValueTask.CompletedTask;

    /// <summary>
    /// Sets breadcrumb items. Override to configure breadcrumbs.
    /// </summary>
    protected virtual ValueTask SetBreadcrumbItemsAsync() => ValueTask.CompletedTask;
}
