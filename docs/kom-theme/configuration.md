# KomTheme Configuration

This page explains the main host-level knobs for KomTheme. Read it when you need to change the default layout, enable or disable shell features, plug in branding, or adjust layout hook behavior for a specific host application.

## `KomThemeBlazorOptions`

| Option | Purpose | Default |
| --- | --- | --- |
| `DefaultLayout` | Layout name such as `SideMenu`, `TopMenu`, `Account`, or `Empty` | `SideMenu` |
| `ShowSidebarToggle` | Shows the sidebar collapse button | `true` |
| `ShowBreadcrumbs` | Shows the breadcrumb bar | `true` |
| `ShowPageToolbar` | Shows the page title and actions area | `true` |
| `ShowFooter` | Shows the footer | `true` |
| `SidebarCollapsed` | Sets the initial sidebar state | `false` |
| `MenuStyle` | Controls menu rendering style such as `Tree`, `Flat`, or `Grouped` | `Tree` |

Configure these options through `Configure<KomThemeBlazorOptions>(...)` in module or host setup.

## Branding

Implement `IBrandingProvider` when the host needs custom branding values such as:

- app name
- logo URL
- reverse logo URL
- favicon URL

If branding varies by tenant, resolve the current tenant in the provider and return tenant-specific values.

## Layout hooks

Use `LayoutHookOptions` to inject shared components into hook points such as `LayoutHooks.Body.First` and `LayoutHooks.Body.Last`. This is the preferred way to add shared host-level UI around the shell without forking the layout itself.

## CSS and font overrides

Override theme variables in host CSS after loading `kom-theme.css`. For font overrides, especially when the host needs custom Latin or RTL typography, use the guidance in [Font Override](font-override.md).
