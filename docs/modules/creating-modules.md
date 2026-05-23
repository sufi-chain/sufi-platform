# Creating Modules

Guidelines for building application modules (e.g. Catalog, Orders) that plug into Sufi Platform.

## Structure

- **Blazor project**: Pages, Components (e.g. modals), Menus (menu contributor).  
- **Module class**: Depends on `SufiAbpUiBlazorModule`, your ABP contracts module; configures **RouterOptions** (additional assemblies) and **NavigationOptions** (menu contributors).

## Menu Contributor

Implement **IMenuContributor**. In `ConfigureMenuAsync`, add items to `context.Menu` (often under **GetAdministration()**). Use **ApplicationMenuItem** with name, display text, URL, icon, order. Use **RequirePermissions** for permission-based visibility.

## Pages

- Use **SufiAbpComponentBase**.  
- `@page` for routing; **`[Authorize(Policy = "...")]`** for protection.  
- Load data with **ExecuteWithLoadingAsync**; use **IsOperationLoading** for grid/list loading.  
- Use **SbCard**, **SbDataGrid**, **SbButton**, **SbTextField**, etc.  
- **AuthorizeView** for permission-gated UI (e.g. create button).

## Modals

- **SbDialog** with form content; `@bind-Open`, `Title`, optional `Footer`.  
- **SbForm** / **SbFormField** for inputs.  
- On submit: call app service, **Notify.SuccessAsync**, callback to parent to refresh and close.

## Services

- Use **LazyGetRequiredService** / **LazyGetService** for app services.  
- Inject ABP application contracts (e.g. `I*AppService`); implement use cases in the backend.

## Registration

Add your Blazor module to the host app’s module **DependsOn** so routes and menu contributors are registered.

See [Identity Reference](identity-reference.md) for a concrete example.
