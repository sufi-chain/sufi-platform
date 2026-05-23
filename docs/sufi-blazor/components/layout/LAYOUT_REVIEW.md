# SufiBlazor Layout Components – Review vs KomTheme

## Summary

The **KomTheme uses its own layout components** (`KomAppShell`, `KomDualSidebar`, `KomTopBar`, `KomSidebar`, `KomIconRail`, `KomExpandPanel`) in `SufiChain.KomTheme.Blazor/Components/Layout/`. The SufiBlazor design system shell components (`SbAppShell`, `SbDualSidebar`, `SbTopBar`, `SbSidebar`, `SbIconRail`, `SbExpandPanel`) are **not used** in any application or in the KomTheme.

## Usage by Component

| Component | Used in KomTheme? | Used elsewhere? | Verdict |
|-----------|---------------------|-----------------|---------|
| **SbContainer** | Yes – `TopMenuLayout.razor` | SufiBlazorDemo | **KEEP** |
| **SbContainerMaxWidth** | Yes – with SbContainer | SufiBlazorDemo | **KEEP** |
| **SbAppShell** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomAppShell |
| **SbAppShellVariant** | No | No (not even used by SbAppShell) | **REMOVED** – dead code |
| **SbDualSidebar** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomDualSidebar |
| **SbTopBar** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomTopBar |
| **SbSidebar** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomSidebar |
| **SbIconRail** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomIconRail |
| **SbExpandPanel** | No | No (docs only) | **REMOVED** – redundant; Kom uses KomExpandPanel |
| **SbStack** | Yes (class names in layouts) | Identity, FileManager, Audit, Feature, Setting modules | **KEEP** |
| **SbAlign, SbJustify, SbStackDirection** | Yes | Many modules | **KEEP** |
| **SbGrid, SbGridItem** | No | SufiBlazorDemo only | **KEEP** – design system primitives |
| **SbSpacer** | No | SufiBlazorDemo only | **KEEP** – design system primitive |

## KomTheme Layouts

- **DualSidebarLayout.razor** – Uses `KomAppShell`, `KomDualSidebar`, `KomIconRail`, `KomExpandPanel`, `KomTopBar`.
- **SideMenuLayout.razor** – Uses `KomAppShell`, `KomSidebar`, `KomTopBar`.
- **TopMenuLayout.razor** – Uses `KomTopBar`, **SbContainer** (only Sb layout component used), `SbDrawer`, `SbNavMenu`, etc.

## Conclusion

- **Keep:** SbContainer, SbContainerMaxWidth, SbStack, SbAlign, SbJustify, SbStackDirection, SbGrid, SbGridItem, SbSpacer (used by apps or by design-system demo).
- **Removed:** SbAppShell, SbAppShellVariant, SbDualSidebar, SbTopBar, SbSidebar, SbIconRail, SbExpandPanel – unused and redundant with KomTheme components; removing them avoids confusion and mistaken use during feature development.
