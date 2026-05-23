using System;
using System.Linq;
using System.Threading.Tasks;
using SufiChain.SufiAbp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.ObjectMapping;
using System.Linq.Dynamic.Core;


namespace SufiChain.SufiAbp.Application.Services;

/// <summary>
/// Base class for CRUD application services with standard entity DTO.
/// </summary>
public abstract class CrudAppService<TEntity, TEntityDto, TKey>
    : CrudAppService<TEntity, TEntityDto, TKey, PagedAndSortedResultRequestDto>
    where TEntity : class, IEntity<TKey>
{
    protected CrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }
}

/// <summary>
/// Base class for CRUD application services with custom get list input.
/// </summary>
public abstract class CrudAppService<TEntity, TEntityDto, TKey, TGetListInput>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TEntityDto>
    where TEntity : class, IEntity<TKey>
{
    protected CrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }
}

/// <summary>
/// Base class for CRUD application services with custom create input.
/// </summary>
public abstract class CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>
    where TEntity : class, IEntity<TKey>
{
    protected CrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }
}

/// <summary>
/// Base class for CRUD application services with separate create and update inputs.
/// </summary>
public abstract class CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : CrudAppService<TEntity, TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>,
      ISufiAbpCrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntity : class, IEntity<TKey>
{
    protected CrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }

    public new virtual async Task<TEntityDto> GetAsync(TKey id)
    {
        return await base.GetAsync(id);
    }

    public new virtual async Task<PagedResultDto<TEntityDto>> GetListAsync(TGetListInput input)
    {
        return await base.GetListAsync(input);
    }

    public new virtual async Task<TEntityDto> CreateAsync(TCreateInput input)
    {
        return await base.CreateAsync(input);
    }

    public new virtual async Task<TEntityDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        return await base.UpdateAsync(id, input);
    }

    public new virtual async Task DeleteAsync(TKey id)
    {
        await base.DeleteAsync(id);
    }

    protected override Task<TEntityDto> MapToGetListOutputDtoAsync(TEntity entity)
    {
        return MapToGetOutputDtoAsync(entity);
    }

    protected override TEntityDto MapToGetListOutputDto(TEntity entity)
    {
        return MapToGetOutputDto(entity);
    }
}

/// <summary>
/// Base class for CRUD application services with separate get and get list output DTOs.
/// </summary>
public abstract class CrudAppService<TEntity, TGetOutputDto, TGetListOutputDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : SufiAbpApplicationService
    where TEntity : class, IEntity<TKey>
{
    protected IRepository<TEntity, TKey> Repository { get; }

    protected virtual string? GetPolicyName { get; set; }
    protected virtual string? GetListPolicyName { get; set; }
    protected virtual string? CreatePolicyName { get; set; }
    protected virtual string? UpdatePolicyName { get; set; }
    protected virtual string? DeletePolicyName { get; set; }

    protected CrudAppService(IRepository<TEntity, TKey> repository)
    {
        Repository = repository;
    }

    protected virtual async Task<TGetOutputDto> GetAsync(TKey id)
    {
        await CheckGetPolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        return await MapToGetOutputDtoAsync(entity);
    }

    protected virtual async Task<PagedResultDto<TGetListOutputDto>> GetListAsync(TGetListInput input)
    {
        await CheckGetListPolicyAsync();

        var query = await CreateFilteredQueryAsync(input);
        var totalCount = await AsyncExecuter.CountAsync(query);

        query = ApplySorting(query, input);
        query = ApplyPaging(query, input);

        var entities = await AsyncExecuter.ToListAsync(query);
        var entityDtos = await MapToGetListOutputDtosAsync(entities);

        return new PagedResultDto<TGetListOutputDto>(
            totalCount,
            entityDtos
        );
    }

    protected virtual async Task<TGetOutputDto> CreateAsync(TCreateInput input)
    {
        await CheckCreatePolicyAsync();

        var entity = await MapToEntityAsync(input);

        TryToSetTenantId(entity);

        await Repository.InsertAsync(entity, autoSave: true);

        return await MapToGetOutputDtoAsync(entity);
    }

    protected virtual async Task<TGetOutputDto> UpdateAsync(TKey id, TUpdateInput input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await GetEntityByIdAsync(id);
        await MapToEntityAsync(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);

        return await MapToGetOutputDtoAsync(entity);
    }

    protected virtual async Task DeleteAsync(TKey id)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteAsync(id);
    }

    protected virtual async Task<TEntity> GetEntityByIdAsync(TKey id)
    {
        return await Repository.GetAsync(id);
    }

    protected virtual async Task CheckGetPolicyAsync()
    {
        await CheckPolicyAsync(GetPolicyName);
    }

    protected virtual async Task CheckGetListPolicyAsync()
    {
        await CheckPolicyAsync(GetListPolicyName);
    }

    protected virtual async Task CheckCreatePolicyAsync()
    {
        await CheckPolicyAsync(CreatePolicyName);
    }

    protected virtual async Task CheckUpdatePolicyAsync()
    {
        await CheckPolicyAsync(UpdatePolicyName);
    }

    protected virtual async Task CheckDeletePolicyAsync()
    {
        await CheckPolicyAsync(DeletePolicyName);
    }

    protected virtual async Task<IQueryable<TEntity>> CreateFilteredQueryAsync(TGetListInput input)
    {
        return await Repository.GetQueryableAsync();
    }

    protected virtual IQueryable<TEntity> ApplySorting(IQueryable<TEntity> query, TGetListInput input)
    {
        if (input is Volo.Abp.Application.Dtos.ISortedResultRequest sortInput && !sortInput.Sorting.IsNullOrWhiteSpace())
        {
            return query.OrderBy(sortInput.Sorting!);
        }

        return ApplyDefaultSorting(query);
    }

    protected virtual IQueryable<TEntity> ApplyDefaultSorting(IQueryable<TEntity> query)
    {
        if (typeof(TEntity).IsAssignableTo(typeof(IHasCreationTime)))
        {
            return query.OrderByDescending(e => ((IHasCreationTime)e).CreationTime);
        }
        else
        {
            return query.OrderByDescending(e => e.Id);
        }
    }

    protected virtual IQueryable<TEntity> ApplyPaging(IQueryable<TEntity> query, TGetListInput input)
    {
        if (input is Volo.Abp.Application.Dtos.IPagedResultRequest pagedInput)
        {
            return query.PageBy(pagedInput);
        }

        if (input is Volo.Abp.Application.Dtos.ILimitedResultRequest limitedInput)
        {
            return query.Take(limitedInput.MaxResultCount);
        }

        return query;
    }

    protected virtual async Task<TGetOutputDto> MapToGetOutputDtoAsync(TEntity entity)
    {
        return await Task.FromResult(MapToGetOutputDto(entity));
    }

    protected virtual TGetOutputDto MapToGetOutputDto(TEntity entity)
    {
        return ObjectMapper.Map<TEntity, TGetOutputDto>(entity);
    }

    protected virtual async Task<List<TGetListOutputDto>> MapToGetListOutputDtosAsync(List<TEntity> entities)
    {
        var dtos = new List<TGetListOutputDto>();

        foreach (var entity in entities)
        {
            dtos.Add(await MapToGetListOutputDtoAsync(entity));
        }

        return dtos;
    }

    protected virtual async Task<TGetListOutputDto> MapToGetListOutputDtoAsync(TEntity entity)
    {
        return await Task.FromResult(MapToGetListOutputDto(entity));
    }

    protected virtual TGetListOutputDto MapToGetListOutputDto(TEntity entity)
    {
        return ObjectMapper.Map<TEntity, TGetListOutputDto>(entity);
    }

    protected virtual async Task<TEntity> MapToEntityAsync(TCreateInput createInput)
    {
        return await Task.FromResult(MapToEntity(createInput));
    }

    protected virtual TEntity MapToEntity(TCreateInput createInput)
    {
        var entity = ObjectMapper.Map<TCreateInput, TEntity>(createInput);
        SetIdForGuids(entity);
        return entity;
    }

    protected virtual void SetIdForGuids(TEntity entity)
    {
        if (entity is IEntity<Guid> entityWithGuidId && entityWithGuidId.Id == Guid.Empty)
        {
            EntityHelper.TrySetId(
                entityWithGuidId,
                () => GuidGenerator.Create(),
                true
            );
        }
    }

    protected virtual async Task MapToEntityAsync(TUpdateInput updateInput, TEntity entity)
    {
        if (updateInput is Volo.Abp.Application.Dtos.IEntityDto<TKey> entityDto)
        {
            entityDto.Id = entity.Id;
        }

        MapToEntity(updateInput, entity);
        await Task.CompletedTask;
    }

    protected virtual void MapToEntity(TUpdateInput updateInput, TEntity entity)
    {
        ObjectMapper.Map(updateInput, entity);
    }

    protected virtual void TryToSetTenantId(TEntity entity)
    {
        if (entity is IMultiTenant && HasTenantIdProperty(entity))
        {
            var tenantId = CurrentTenant.Id;

            if (!tenantId.HasValue)
            {
                return;
            }

            var propertyInfo = entity.GetType().GetProperty(nameof(IMultiTenant.TenantId));

            if (propertyInfo == null || propertyInfo.GetSetMethod(true) == null)
            {
                return;
            }

            propertyInfo.SetValue(entity, tenantId);
        }
    }

    protected virtual bool HasTenantIdProperty(TEntity entity)
    {
        return entity.GetType().GetProperty(nameof(IMultiTenant.TenantId)) != null;
    }
}

/// <summary>
/// SufiAbp-branded CRUD application service base class.
/// </summary>
public abstract class SufiAbpCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput>
    : SufiAbpCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>
    where TEntity : class, IEntity<TKey>
{
    protected SufiAbpCrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }
}

/// <summary>
/// SufiAbp-branded CRUD application service with full customization.
/// </summary>
public abstract class SufiAbpCrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>,
      ISufiAbpCrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>
    where TEntity : class, IEntity<TKey>
{
    protected SufiAbpCrudAppService(IRepository<TEntity, TKey> repository)
        : base(repository)
    {
    }
}
