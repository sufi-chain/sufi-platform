# KomTheme Layouts

This page explains the layout variants available in KomTheme and when each one fits a host application. Read it when you are selecting the default shell for a product or changing the layout for a specific route such as account pages or embedded screens.

## Available layouts

| Layout | When to use it |
| --- | --- |
| `SideMenuLayout` | Default application shell with collapsible sidebar, top bar, breadcrumbs, and optional footer |
| `TopMenuLayout` | Hosts with a smaller navigation surface that works better in the header |
| `DualSidebarLayout` | Products that need a two-level navigation pattern with an icon rail and expandable panel |
| `AccountLayout` | Authentication and account pages such as login or reset flows |
| `EmptyLayout` | Minimal wrapper for full-screen, embedded, or print-oriented pages |

## Common behavior

All layouts share a few important platform behaviors:

- responsive navigation behavior
- dark-mode and RTL support through `SbThemeProvider`
- menu rendering through `IMenuManager`
- the ability to participate in shared layout hooks and toolbar composition

## How to choose a layout

Choose the default layout in `KomThemeBlazorOptions`, then override it per page only when the route truly needs a different shell.

Typical examples:

- use `SideMenuLayout` for most admin and back-office hosts
- use `TopMenuLayout` for simpler products with limited navigation depth
- use `AccountLayout` for sign-in, register, and recovery flows
- use `EmptyLayout` when the page should not inherit the normal app chrome
