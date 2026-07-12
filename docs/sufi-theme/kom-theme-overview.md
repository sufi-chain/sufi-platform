# SufiTheme Overview

SufiTheme is the preferred shell and layout layer used by Sufi Platform hosts. Read this section when you need to understand how a host application assembles top bars, side navigation, layout variants, branding, and toolbar composition on top of Sufi Platform and SufiBlazor.

SufiTheme is not a replacement for SufiBlazor. The two play different roles:

- `SufiBlazor` provides reusable interactive components (standalone — no ABP required)
- `SufiTheme` provides the host shell, page layout, and branded navigation frame

## Dependencies

SufiTheme **requires Sufi Platform UI**, not SufiBlazor alone:

- `SufiChain.SufiPlatform.UI.Blazor` — menus, toolbars, page layout, `AccountLayout`
- `SufiChain.SufiPlatform.Core`, `SufiChain.SufiPlatform.Security` — module infrastructure

Product source: `independent-projects/sufi-theme/` (independently versioned NuGet packages).

## Packages

| Package | Responsibility |
| --- | --- |
| `SufiChain.SufiTheme` | Core options and shared constants |
| `SufiChain.SufiTheme.Blazor` | Layouts, `SufiAppShell`, top bar, sidebar, and navigation rendering |
| `SufiChain.SufiTheme.Blazor.Server` | Server-specific toolbar contributors, branding, and bundling |
| `SufiChain.SufiTheme.Blazor.WebAssembly` | WebAssembly-specific toolbar contributors and host integration |

## What it gives a host

- standard layouts for app, account, and minimal pages
- contributor-driven toolbars
- navigation rendering on top of `IMenuManager` and `IMenuContributor`
- branding through `IBrandingProvider`
- layout hook points for shared host composition
- integration with `SbThemeProvider` for dark mode and RTL behavior

## Where developers usually start

- open [Layouts](layouts.md) when choosing or changing the shell structure of a host
- open [Toolbars](toolbars.md) when adding top-bar actions or user-specific tools
- open [Configuration](configuration.md) when changing default layout behavior or branding
- open [Public navigation](public-navigation.md) when public/KB menus differ from admin navigation
- open [Font Override](font-override.md) when the host needs custom Latin or RTL fonts
