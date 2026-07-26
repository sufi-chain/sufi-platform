# Product Creation Guide

This guide gives product owners, solution teams, and developers a practical process for describing and creating new capabilities on top of the open-source Sufi Platform base. Use it when the request is something like a vertical industry product, or when you need to decide whether to reuse open-source modules, consume licensed **Pro** NuGet packages (CMS, HelpDesk, Finance, and similar), or build product-specific code.

Read this guide together with:

- `docs/architecture.md` for platform layering and composition rules
- `docs/modules/creating-modules.md` for module and UI conventions

## Terminology

| Term | Meaning |
|------|---------|
| **Sufi Platform** | Product and application-platform offering |
| **`SufiChain.SufiPlatform.*`** | Technical framework and package family |
| **`Sufi*` types** | Module, type, and code prefixes |
| **ABP Framework** | Upstream open-source foundation |

Keep planning notes focused on user needs, workflows, roles, permissions, data, and delivery priorities.

## Product-owner brief

Before implementation starts, describe the request in plain product language:

- product goal and target users
- main workflows and expected outcomes
- user roles and permission boundaries
- records, files, documents, and reference data managed by the product
- tenant, organization, branch, or department structure if needed
- localization, audit, reporting, and integration needs
- first-release scope versus later enhancements

This brief helps the team decide what should reuse existing platform modules, what should become a reusable module, and what should stay specific to the product.

## Step 1: Classify the request

Before you create anything, decide whether the request should become a reusable horizontal module or a vertical product.

### Horizontal module

Choose this path when the capability:

- is reusable across many products
- has a bounded business context
- should become part of the Sufi Platform catalog
- can be plugged into many host applications

Examples:

- CMS, HelpDesk, Chat, CRM (Phase 2) and Finance (Phase 3) → prefer licensed **Pro** NuGet packages (not open source; free tier via [sufichain.com](https://sufichain.com))
- Commerce, ERP, HR → planned Pro phases; see [Roadmap](roadmap.md) before inventing parallel OSS modules
- Domain-only features for one customer → vertical product code

### Vertical product

Choose this path when the request is for a full domain-specific solution assembled from multiple capabilities.

Examples:

- school management system
- hospital information system
- marketplace solution
- government service portal
- industry-specific CRM or ERP product

### Interpretation examples

- "Create a CMS module" -> use the licensed Pro CMS NuGet (Phase 2), not a new open-source fork
- "Add HelpDesk to the platform" -> consume Pro HelpDesk packages under a Sufi Platform license
- "Add ERP workflows / HR payroll" -> Phase 5–6 roadmap Pro capabilities; check [Roadmap](roadmap.md) before building a one-off
- "Generate an ecommerce product" -> vertical product or host (open-source base + Phase 4 Commerce Pro when available)
- "Build a complete hospital system" -> vertical product

## Step 2: Start from the platform baseline

Do not start from a blank ABP solution unless there is a clear reason to do so.

Always begin by identifying which existing Sufi Platform capabilities and first-party modules can be reused.

### Baseline modules to consider first

- `account`
- `identity`
- `tenant-management`
- `permission-management`
- `setting-management`
- `feature-management`
- `localization-management`
- `audit-logging`
- `background-jobs`

### Utility modules to add when needed

- `file-manager` for media, documents, uploads, galleries, attachments, and file browsing
- `short-link-generator` for public redirects, tracked links, and shareable short URLs

## Step 3: Define the delivery shape

Use this table to keep scope clear.

| If the request means... | Build... |
| --- | --- |
| reusable business capability for many future products | one or more horizontal modules |
| complete solution for one business domain | a vertical host product |
| reusable capability now, standalone product later | module first, host composition second |

## Step 4: Break the capability into module boundaries

Prefer modules that have:

- one main business language
- clear permissions and settings
- clean DTO and app-service boundaries
- reusable administration UI where appropriate
- minimal coupling to one specific host product

### Example: CMS module family

- content types
- content items
- page builder
- media integration
- routing and site structure
- SEO and publishing settings

### Example: HelpDesk module family

- tickets
- queues
- SLA rules
- agent assignment
- comments and attachments
- canned responses
- knowledge base integration

### Example: E-commerce module family

- catalog
- inventory
- pricing
- cart
- checkout
- orders
- promotions
- storefront content integration

## Step 5: Reuse existing platform modules explicitly

For each new capability, decide which existing modules become part of the solution.

| Need | Reuse |
| --- | --- |
| users, roles, claims | `identity` |
| login and profile flows | `account` |
| multi-tenant administration | `tenant-management` |
| permission UI and assignment | `permission-management` |
| configurable behavior | `setting-management` |
| tenant or product toggles | `feature-management` |
| language and translations | `localization-management` |
| files, images, attachments, media browser | `file-manager` |
| operational traceability | `audit-logging` |
| async jobs and processing | `background-jobs` |
| public short URLs and redirects | `short-link-generator` |

The default rule is simple: do not rebuild those concerns inside a new module unless the product truly needs a replacement or a deep specialization.

## Step 6: Choose the UI model

Use these defaults unless the product explicitly needs something different:

- `Sufi Platform Framework` for platform-aware UI behavior
- `SufiBlazor` for interactive components
- `SufiTheme` for the full app shell, navigation, and branded layout

### Prefer SufiBlazor for

- forms
- grids and tables
- filters
- dialogs and drawers
- tabs and menus
- status indicators
- dashboards and metrics
- builder or editor UI

### Prefer SufiTheme when

- the app needs a complete shell
- the product needs sidebars, topbars, and branded navigation chrome
- the host should feel like a cohesive Sufi Platform application

### Prefer File Manager integration when

- the product needs file browsing
- the product needs media selection
- the product needs uploads, galleries, thumbnails, or shared asset rules

## Step 7: Create the backend shape

For each reusable module, default to the standard ABP split:

- `Domain.Shared`
- `Domain`
- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- optional `EntityFrameworkCore`
- optional `MongoDB`
- `test/*`

### Layer responsibilities

- `Domain.Shared` - constants, enums, localization resources, permission names, setting names
- `Domain` - entities, aggregates, managers, domain services, business rules
- `Application.Contracts` - DTOs, service interfaces, public contracts
- `Application` - orchestration, validation, mapping, and app-service workflows
- `HttpApi` - controllers and remote API surface
- `HttpApi.Client` - remote integration support
- `Blazor` - admin or product UI, contributors, pages, dialogs, forms
- persistence packages - provider-specific data access

## Step 8: Make the module feel native to the platform

A new Sufi Platform module should:

- use permissions consistently
- define settings clearly
- respect tenancy where appropriate
- localize all user-facing text
- plug into menus and toolbars through contributor patterns
- expose clean application contracts
- use Sufi Platform and SufiBlazor conventions in the UI
- follow the standard documentation structure under `docs/modules/`

If the capability is admin-facing or operational, it should normally include a Blazor management experience.

## Step 9: Assemble the host product

If the request is for a full product, do not stop at the module level.

Create a host composition that:

1. includes the needed baseline Sufi Platform modules
2. includes any new horizontal modules
3. includes domain-specific product modules where required
4. applies SufiTheme when the product should follow the standard shell model

That keeps the solution reusable where it should be reusable and domain-specific only where it has to be.
