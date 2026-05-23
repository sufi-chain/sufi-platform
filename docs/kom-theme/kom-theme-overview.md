# KomTheme Overview

KomTheme is the preferred shell and layout layer used by Sufi Platform hosts. Read this section when you need to understand how a host application assembles top bars, side navigation, layout variants, branding, and toolbar composition on top of SufiAbp and SufiBlazor.

KomTheme is not a replacement for SufiBlazor. The two play different roles:

- `SufiBlazor` provides reusable interactive components
- `KomTheme` provides the host shell, page layout, and branded navigation frame

## Packages

| Package | Responsibility |
| --- | --- |
| `SufiChain.KomTheme` | Core options and shared constants |
| `SufiChain.KomTheme.Blazor` | Layouts, `KomAppShell`, top bar, sidebar, and navigation rendering |
| `SufiChain.KomTheme.Blazor.Server` | Server-specific toolbar contributors, branding, and bundling |
| `SufiChain.KomTheme.Blazor.WebAssembly` | WebAssembly-specific toolbar contributors and host integration |

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
- open [Font Override](font-override.md) when the host needs custom Latin or RTL fonts
