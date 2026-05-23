# SufiBlazor Overview

SufiBlazor is an independent Blazor UI component library and design system for ASP.NET Core 10+ applications. You can use it in a plain Blazor project, in an ABP-based solution, or inside Sufi Platform. It does not require Sufi Platform in order to be useful, and it should be understood as a standalone UI product that happens to be the default component base for the platform.

Inside Sufi Platform, SufiBlazor is the shared component system used by generated hosts and first-party modules. Outside the platform, it can still serve as the primary UI layer for forms, tables, overlays, navigation, layout primitives, theming, and RTL-aware applications.

## What the library provides

SufiBlazor gives a Blazor project:

- reusable `Sb*` components for interactive UI
- a design-token-based theming model
- RTL and Persian-friendly behavior alongside standard LTR usage
- date and range picker components with Gregorian, Persian Shamsi, and Hijri support
- a self-contained component model that does not depend on Bootstrap, Tailwind, ABP UI, or Blazorise

## Typical usage scenarios

You can use SufiBlazor in:

- a standalone ASP.NET Core Blazor application
- an ABP-based Blazor application
- a Sufi Platform host or module
- an internal business application that needs a reusable design system without adopting a larger application framework

## Component categories

| Category | Typical use |
| --- | --- |
| Actions | Buttons, icon buttons, and links |
| Forms | Inputs, selectors, date pickers, range pickers, time pickers, and rich-text editing |
| Data | Tables, grids, pagination, metrics, and status displays |
| Layout | Stacks, grids, containers, dividers, and page-composition primitives |
| Navigation | Tabs, breadcrumbs, menus, and accordions |
| Overlays | Dialogs, drawers, popovers, tooltips, and menus |
| Feedback | Alerts, badges, banners, chips, progress, and empty states |
| Surfaces | Cards, surfaces, and papers |
| Typography | Headings and text primitives |
| Common | Icons and markdown rendering |
| Builder | Drag/drop, split panes, property grids, and editor-oriented primitives |

## In Sufi Platform

When SufiBlazor is used inside Sufi Platform:

- it provides the base interactive component layer
- KomTheme builds the host shell and layout on top of it
- SufiAbp provides the platform abstractions, host wiring, and module composition rules around it

That relationship matters for platform developers, but the library remains usable outside that stack.

## Guidance for adopters

### Use SufiBlazor when

- you want a reusable Blazor design system without taking on another UI framework
- you need consistent components for forms, data display, overlays, and navigation
- you want RTL-aware behavior and Persian-friendly date handling
- you need the same component base across multiple Blazor applications or modules

### Be careful when

- your project already depends deeply on another component system and you are planning a gradual migration
- a product-specific requirement is pushing you toward custom UI that may still be better built on top of existing `Sb*` primitives
- your team needs to keep theme, localization, and RTL rules consistent across many pages

## Related docs

- [Installation](installation.md)
- [Theming](theming.md)
- [Localization](localization.md)
- [Components](components.md)
- [DataGrid](data-grid.md)
- [Adding Icons](adding-icons.md)
