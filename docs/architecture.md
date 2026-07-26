# Sufi Platform Architecture

This document explains how the public source is organized and how the platform is meant to be extended. Read it when you are deciding where a change belongs, reviewing a new module proposal, or tracing how a host application is assembled from shared platform pieces.

## Terminology

| Term | Meaning |
|------|---------|
| **Sufi Platform** | Product and platform name |
| **`SufiChain.SufiPlatform.*`** | Technical package family |
| **`Sufi*` types** | Code prefix (`SufiModule`, `SufiComponentBase`, …) |
| **ABP Framework** | Upstream modular backend ([abp.io](https://abp.io), [github.com/abpframework/abp](https://github.com/abpframework/abp)) |

## Relationship with ABP

Sufi Platform is built on top of ABP Framework rather than trying to hide that heritage. ABP provides the proven modular architecture, DDD layering, multi-tenancy, authorization, settings, localization, auditing, and persistence model.

Sufi Platform extends that foundation with Sufi Platform-branded framework surfaces, SufiBlazor components, SufiTheme, first-party modules, templates, and the `sufi` CLI. The goal is to remain compatible with ABP's architectural strengths while providing a focused enterprise Blazor platform and a consistent Sufi Platform developer experience.

## Layered model

Sufi Platform is not just a repository layout. It is a layered application model built so teams can reuse the same technical base across multiple products.

From bottom to top:

1. `ABP`
   - modular backend architecture
   - domain/application layering
   - permissions, settings, tenancy, auditing, persistence

2. `Sufi Platform Framework`
   - UI abstractions and default UI services
   - Blazor composition and base components
   - ABP integration points
   - authentication, data, storage, and CLI support

3. `SufiBlazor`
   - default component system used by platform modules and hosts

4. `SufiTheme`
   - preferred shell, layout, toolbar, and navigation surface

5. first-party modules
   - reusable horizontal capabilities such as identity, settings, file management, and short links

6. host applications and vertical products
   - business-specific solutions composed from reusable platform modules plus domain-specific code

## Repository map

| Path | What lives there |
| --- | --- |
| `framework/` | Sufi Platform framework packages and CLI |
| `modules/` | First-party reusable modules |
| `templates/` | Template assets used by the CLI |
| `docs` | Canonical long-form documentation |
| `independent-projects/sufi-blazor` | Source dependency for the default component system |
| `independent-projects/sufi-theme` | Source dependency for the preferred shell and theme |

For most platform work, start in `framework/` or `modules/`. Reach for the independent products when you need to understand how the platform consumes them, not to turn this repository into their internal reference manual.

## Framework direction

The architectural rule is simple:

- keep backend behavior aligned with ABP layering
- keep UI composition aligned with Sufi Platform abstractions
- use SufiBlazor for reusable interactive components
- use SufiTheme for standard shell and layout behavior

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

You can see this shape clearly in modules such as `modules/file-manager` and `modules/short-links`.

## First-party modules

There are **19** first-party modules under `modules/`. Source folders use short names (`tenants`, `jobs`, `short-links`, …); docs folders may keep longer display names. See the [Module Catalog](modules/index.md) and [Package Map](reference/package-map.md).

Typical CLI baseline includes account, identity, tenants, permissions, features, settings, OpenIddict, audit logging, background jobs, localization, file manager, calendar, menus, tags, short links, blob database, users, and AI. **Editions** ships in source but is not in the default CLI registry yet.

## Deeper architecture docs

- [Architecture decisions](architecture/decisions.md)
- [Framework C4](architecture/c4-framework.md)
- [Modules C4](architecture/c4-modules.md)
- [Sequence diagrams](architecture/sequences.md)
- [Deployment](operations/deployment.md)
- [Security](operations/security.md)
- [Operational runbook](operations/runbook.md)

## Horizontal modules and vertical products

This distinction matters when deciding where to add new work.

### Horizontal module

Create a horizontal module when the capability is reusable across many products, has a clear bounded context, and can stand as a shared platform asset.

Examples include:

- Open-source baseline capabilities (identity, files, calendar, AI, …) — Phase 1
- Licensed Pro packages such as CMS, HelpDesk, Chat, CRM, Forms — Phase 2 ([Roadmap](roadmap.md))
- Finance / billing — Phase 3; Commerce, ERP (workflows), HR — later Pro phases

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

That order preserves reuse and keeps the platform catalog from filling up with one-off business logic. For delivery order across Foundation, Pro, Finance, Commerce, ERP, HR, and Scale, see [Roadmap](roadmap.md).
