# Developer Conventions

This page defines the documentation and implementation conventions that should stay consistent across Sufi Platform.

## Naming conventions

- Use `Sufi Platform` for product, business, planning, and marketing language.
- Use `SufiAbp` (`Sufi ASP.NET Core Boilerplate `) for the technical framework, package family, and namespace root `SufiChain.SufiAbp.*`.
- Use `SufiAbp` for type, module, and resource prefixes in code examples and technical docs.
- Avoid `SufiAbp` in new docs because it is ambiguous.

## Documentation conventions

- `docs/` is the canonical documentation source.
- Root and module READMEs should be concise entry points, not full documentation sets.
- Each module should use the same docs shape so readers always know where to find installation, configuration, usage, permissions, and API information.
- Product-facing explanations should appear before deep technical details when possible.

## Module conventions

- Use the ABP layered structure consistently.
- Keep permission names, settings names, and remote service constants in contracts or shared projects where appropriate.
- Register UI navigation through contributors instead of hard-coded page composition.
- Keep settings and permissions documented in their module docs.
- Prefer shared framework abstractions when integrating menus, notifications, user context, and theming behavior.

## UI conventions

- Reuse SufiBlazor components where available.
- Keep page-level module UI in the module's Blazor project.
- Use the shared platform theme and menu composition model.
- Document user-visible pages and workflows in module `usage.md` pages.

## Authoring conventions for new docs

Every new module should add or update:

- `docs/modules/index.md`
- `docs/modules/<module>/index.md`
- at least `overview.md`, `installation.md`, `configuration.md`, and `usage.md`
- product-facing feature descriptions, not just package references

## Naming and path notes

Use real repository paths in documentation even when a path contains a temporary naming issue. For example, the short-link module currently lives under `src/modules/short-link-generator/`, and docs should stay accurate until that path is intentionally renamed.
