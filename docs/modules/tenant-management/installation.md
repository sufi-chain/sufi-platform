# Tenant Management Installation

## Main packages

- `SufiChain.SufiAbp.TenantManagement.Application.Contracts`
- `SufiChain.SufiAbp.TenantManagement.Application`
- `SufiChain.SufiAbp.TenantManagement.Domain.Shared`
- `SufiChain.SufiAbp.TenantManagement.Domain`
- `SufiChain.SufiAbp.TenantManagement.HttpApi`
- `SufiChain.SufiAbp.TenantManagement.HttpApi.Client`
- `SufiChain.SufiAbp.TenantManagement.Blazor`

## Important dependency pattern

The Blazor package references:

- `SufiChain.SufiAbp.FeatureManagement.Blazor`
- `SufiChain.SufiAbp.SettingManagement.Blazor`

This indicates that tenant administration is a central place where related administrative capabilities are composed.
