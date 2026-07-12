# Product Overview

Sufi Platform is the reusable open-source base used to build business applications on top of ABP without rebuilding the same technical foundation for every product. Product owners can use this page to understand what the platform already provides, what kind of needs fit the platform, and how to describe new product requests clearly.

For delivery teams, the platform gives each generated solution a starting point: the SufiAbp framework, a standard host structure, reusable first-party modules, SufiBlazor for interactive UI, and SufiTheme for the application shell.

Sufi Platform is licensed as an LGPL open-source base product. It respects the ABP Framework as the upstream foundation and extends it with a focused Sufi Platform experience for enterprise Blazor applications.

## Naming model

- `Sufi Platform` is the product and platform name.
- `SufiAbp` is the technical foundation and package family behind the platform.
- `SufiAbp` is the code prefix used in framework and module types such as `SufiAbpComponentBase`.

## What a generated product inherits

A generated solution starts with a stable set of layers that are meant to be reused together:

| Layer | Role in a generated product |
| --- | --- |
| ABP | Backend modular architecture, domain and application layering, permissions, settings, tenancy, auditing, persistence |
| SufiAbp Framework | Platform-aware UI abstractions, shared services, Blazor integration, authentication helpers, CLI support |
| SufiBlazor | Default interactive component system for forms, tables, dialogs, navigation, and layout primitives |
| SufiTheme | Default application shell, layout, navigation chrome, and branding surface |
| First-party Modules | Reusable platform capabilities such as identity, tenancy, settings, file management, and short links |
| Product Code | The vertical business workflows, domain rules, and custom UI specific to the generated solution |

## Product-owner view

From a planning perspective, Sufi Platform helps turn a product idea into a structured request:

- choose the business domain and target users
- list the workflows the product must support
- identify reusable platform modules that already cover common needs
- separate reusable capabilities from product-specific behavior
- define roles, permissions, tenant needs, localization needs, and audit requirements early
- prioritize the first useful release before adding advanced workflows

Good product requests do not need to start with technical architecture. They should describe the real users, decisions, screens, data, rules, and integrations that the product needs. The platform then provides a consistent technical base for implementing those needs.

## Why this matters for vertical teams

The point of starting from the platform is speed with consistency.

A product team should not have to rebuild:

- account and identity foundations
- tenant administration
- permissions, settings, and feature management
- localization administration
- audit logging and background-job operations
- file and media management
- short-link handling

When those concerns already exist in the platform, the product team can focus on domain workflows and product behavior.

## Product request checklist

Use this checklist before a new product or module is planned:

- Who are the main user roles?
- What are the top workflows they need to complete?
- Which records, documents, or files does the product manage?
- Does the product need tenants, organizations, branches, or departments?
- Which actions require permissions or approval?
- Which settings should be configurable without code changes?
- Which languages and localization rules are required?
- Which actions must be auditable?
- Which reports, dashboards, or operational views are needed first?

## Horizontal modules and vertical products

This distinction still matters even when the reader is primarily building a vertical solution.

### Horizontal module

A horizontal module is a reusable capability that should be shared across multiple products.

Typical examples:

- CMS
- HelpDesk
- Workflow
- Search
- Billing
- Knowledge Base

### Vertical product

A vertical product is the complete business solution that consumes the platform and may also depend on horizontal modules.

Typical examples:

- school management system
- hospital information system
- government portal
- publishing suite
- industry-specific SaaS product

A vertical team should still ask: is this new feature only for this product, or should it become a reusable module later?

## Default product-building order

For a generated solution, the default order is:

1. generate the solution from the CLI templates
2. start from the baseline modules already included by the platform
3. add reusable capability as horizontal modules when the feature should be shared
4. keep product-specific behavior in the vertical solution itself
5. use SufiBlazor for reusable UI and SufiTheme for the shell unless the product needs a deliberate alternative

## Where to go next

- Read [Getting Started](getting-started.md) if you have not generated and run a solution yet.
- Read [Architecture](architecture.md) to understand the layered platform model.
- Read [Module Catalog](modules/index.md) to see which reusable modules already exist.
- Read [Product Creation Guide](product-creation-guide.md) when you start planning product-specific and reusable features.
