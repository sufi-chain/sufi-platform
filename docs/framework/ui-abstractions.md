# UI Abstractions

Contracts in **SufiChain.SufiPlatform.UI.Abstractions** define the UI surface. Implementations live in UI.Services, UI.Blazor, or UI.Abp.

## Theming

- **ITheme**, **IThemeManager**, **IThemeSelector** — Current theme, layout selection (Application, Account, Empty).  
- **ThemingOptions** — Register themes, default theme.

## Navigation

- **ApplicationMenu**, **ApplicationMenuItem** — Hierarchical menus; items can have URL, icon, order, children, `RequirePermissions`.  
- **IMenuContributor** — Contribute items via `ConfigureMenuAsync(MenuConfigurationContext)`.  
- **IMenuManager** — `GetAsync(name)`, `GetMainMenuAsync()`.  
- **StandardMenus** — Main, User.

## Toolbars

- **Toolbar**, **ToolbarItem** — Named toolbars; items are component types + order.  
- **IToolbarContributor** — Add items via `ConfigureToolbarAsync(IToolbarConfigurationContext)`.  
- **IToolbarManager** — `GetAsync(name)`.

## Page Toolbars & Layout Hooks

- **IPageToolbarManager**, **PageToolbar** — Page-specific actions.  
- **ILayoutHookManager**, **LayoutHookInfo** — Inject components at **LayoutHooks** (e.g. Body.First, Body.Last).

## Alerts, Messages, Notifications

- **IAlertManager** — Page-level alerts (Info, Success, Warning, Danger).  
- **IUiMessageService** — Confirmations, info, success, warn, error.  
- **IUiNotificationService** — Toasts.

## User, Tenant, Auth, Branding

- **ICurrentUserAccessor**, **CurrentUserInfo** — Current user.  
- **ICurrentTenant** — Current tenant.  
- **ISufiAbpPermissionChecker** — Permission checks.  
- **IBrandingProvider** — App name, logo, favicon.  
- **ICookieService**, **ILocalStorageService** — Browser storage.
