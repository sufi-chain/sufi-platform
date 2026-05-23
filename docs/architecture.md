# Sufi Platform Architecture

This document explains how the public source is organized and how the platform is meant to be extended. Read it when you are deciding where a change belongs, reviewing a new module proposal, or tracing how a host application is assembled from shared platform pieces.

## Terminology

- `Sufi Platform` is the platform and product name.
- `SufiAbp` is the technical base and the package family under `SufiChain.SufiAbp.*`.
- `SufiAbp` is the code prefix used in framework and module types.
- `ABP Framework` is the upstream open-source framework that provides the core backend architecture. Website: [abp.io](https://abp.io). Source: [github.com/abpframework/abp](https://github.com/abpframework/abp).

## Relationship with ABP

Sufi Platform is built on top of ABP Framework rather than trying to hide that heritage. ABP provides the proven modular architecture, DDD layering, multi-tenancy, authorization, settings, localization, auditing, and persistence model.

Sufi Platform extends that foundation with SufiAbp-branded framework surfaces, SufiBlazor components, KomTheme, first-party modules, templates, and the `sufi` CLI. The goal is to remain compatible with ABP's architectural strengths while providing a focused enterprise Blazor platform and a consistent Sufi Platform developer experience.

## Layered model

Sufi Platform is not just a repository layout. It is a layered application model built so teams can reuse the same technical base across multiple products.

From bottom to top:

1. `ABP`
   - modular backend architecture
   - domain/application layering
   - permissions, settings, tenancy, auditing, persistence

2. `SufiAbp Framework`
   - UI abstractions and default UI services
   - Blazor composition and base components
   - ABP integration points
   - authentication, data, storage, and CLI support

3. `SufiBlazor`
   - default component system used by platform modules and hosts

4. `KomTheme`
   - preferred shell, layout, toolbar, and navigation surface

5. first-party modules
   - reusable horizontal capabilities such as identity, settings, file management, and short links

6. host applications and vertical products
   - business-specific solutions composed from reusable platform modules plus domain-specific code

## Repository map

| Path | What lives there |
| --- | --- |
| `src/framework` | SufiAbp framework packages and CLI |
| `src/modules` | First-party reusable modules |
| `src/templates` | Template assets used by the CLI |
| `docs` | Canonical long-form documentation |
| `independent-projects/sufi-blazor` | Source dependency for the default component system |
| `independent-projects/kom-theme` | Source dependency for the preferred shell and theme |

For most platform work, start in `src/framework` or `src/modules`. Reach for the independent products when you need to understand how the platform consumes them, not to turn this repository into their internal reference manual.

## Framework direction

The architectural rule is simple:

- keep backend behavior aligned with ABP layering
- keep UI composition aligned with SufiAbp abstractions
- use SufiBlazor for reusable interactive components
- use KomTheme for standard shell and layout behavior

This keeps the platform consistent across both shared modules and host applications.

## Standard module shape

Most reusable modules follow the standard ABP split, with optional UI and storage variants when the module needs them:

- `Domain.Shared`
- `Domain`
- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- optional `Blazor.Server` and `Blazor.WebAssembly`
- optional `EntityFrameworkCore` and `MongoDB`
- `test/*`

You can see this shape clearly in modules such as `src/modules/file-manager` and `src/modules/short-link-generator`.

## Baseline modules

These modules form the baseline platform catalog in `src/modules`:

- `account`
- `audit-logging`
- `background-jobs`
- `feature-management`
- `file-manager`
- `identity`
- `localization-management`
- `permission-management`
- `setting-management`
- `short-link-generator`
- `tenant-management`

For a new product, the default assumption is that these are available unless a host intentionally excludes them.

## Horizontal modules and vertical products

This distinction matters when deciding where to add new work.

### Horizontal module

Create a horizontal module when the capability is reusable across many products, has a clear bounded context, and can stand as a shared platform asset.

Examples include:

- CMS
- HelpDesk
- Workflow
- Billing
- Search
- Knowledge Base

### Vertical product

Create a vertical product when the goal is a complete domain solution that happens to reuse the platform.

Examples include:

- hospital information systems
- school management systems
- government service portals
- real-estate solutions
- domain-specific SaaS products

## Composition rule

When building a new product, follow this order:

1. reuse the baseline platform modules
2. add new horizontal modules when the capability should be shared
3. keep truly domain-specific behavior in vertical modules or host code
4. assemble the host application on top of the shared layers

That order preserves reuse and keeps the platform catalog from filling up with one-off business logic.
