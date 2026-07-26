# ABP Integration

Sufi Platform UI layers integrate with ABP backend services through adapters in **SufiChain.SufiPlatform.UI.Services** and host packages such as **SufiChain.SufiPlatform.UI.Blazor.Server**. See [Package Map](../reference/package-map.md) for the current framework package list.

## Adapters

| Sufi Platform contract | ABP adapter |
|-------------|-------------|
| ICurrentUserAccessor | AbpCurrentUserAccessor |
| ICurrentTenant | AbpCurrentTenantAdapter |
| IClock | AbpClockAdapter |
| IMenuContributor | AbpMenuContributorAdapter (wraps ABP menu contributors) |
| IToolbarContributor | AbpToolbarContributorAdapter |
| ISufiPermissionChecker | AbpPermissionCheckerAdapter (delegates to ABP `IPermissionChecker`) |

## Features

- **Backend stays ABP**: Domain, application services, DTOs, permissions.  
- **UI uses Sufi Platform**: `SufiComponentBase`, SufiBlazor, and SufiTheme; components depend on Sufi Platform interfaces, not ABP directly.  
- **Menus/toolbars**: Existing ABP menu/toolbar contributors can be adapted so they feed Sufi Platform’s menu/toolbar system.

## Usage

- Add platform UI host modules (for example **SufiThemeBlazorServerModule**) and **AbpUiModule** to the Blazor app.  
- Use **SufiComponentBase** and Sufi Platform services (`Message`, `Notify`, `CurrentUser`, etc.); they are backed by ABP where adapters exist.  
- Use ABP app services via DI (e.g. `IIdentityUserAppService`).  
- Use **AuthorizeView** and **`[Authorize(Policy = "...")]`** with ABP permissions.
