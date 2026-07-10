# SufiBlazor Documentation

Canonical SufiBlazor documentation lives in the **independent product repository**:

```
independent-projects/sufi-blazor/docs/
```

This folder is a pointer only. Do not add or edit component reference files here — they will drift from the source of truth.

## Key pages

| Topic | Path (from repo root) |
| --- | --- |
| Overview | `independent-projects/sufi-blazor/docs/overview.md` |
| Installation | `independent-projects/sufi-blazor/docs/installation.md` |
| Standalone adoption | `independent-projects/sufi-blazor/docs/standalone-adoption.md` |
| Components catalog | `independent-projects/sufi-blazor/docs/components.md` |
| Builder components | `independent-projects/sufi-blazor/docs/builder.md` |
| DataGrid | `independent-projects/sufi-blazor/docs/data-grid.md` |
| Editors & bundling | `independent-projects/sufi-blazor/docs/editors-and-bundling.md` |
| Demo host integration | `independent-projects/sufi-blazor/docs/demo-host-integration.md` |
| Theming | `independent-projects/sufi-blazor/docs/theming.md` |
| Localization | `independent-projects/sufi-blazor/docs/localization.md` |
| Per-component API | `independent-projects/sufi-blazor/docs/components/` |
| Shell ownership (KomTheme) | `independent-projects/sufi-blazor/docs/components/layout/LAYOUT_REVIEW.md` |

## Platform notes

- **SufiBlazor** (`SufiChain.SufiBlazor`) is standalone — no ABP or KomTheme required for the core library.
- **App shell chrome** (navigation frame, sidebars, top bar) lives in **KomTheme**, not SufiBlazor. See `LAYOUT_REVIEW.md`.
- **SufiChain.SufiBlazor.Demo** is a platform-hosted gallery; it references `SufiChain.SufiAbp.UI.Blazor` for menu integration only.

## Obsidian KB

Workspace knowledge-base pages: `.obsidian/IndependentProjects/SufiBlazor.md`
