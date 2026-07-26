# Architecture Decision Records

Accepted decisions for the Sufi Platform framework and first-party modules. Historical detail and residual debt live in the workspace knowledge base (`.obsidian/Framework Reduction Plan`, `.obsidian/SufiPlatform Naming Refactor Plan`, `.obsidian/Debt and Attention Log`).

## Framework

### ADR-001: Framework Reduction — direct ABP consumption

**Status:** Accepted

**Context:** Thin `SufiChain.SufiPlatform.*` wrappers around ABP infrastructure added maintenance cost without product value.

**Decision:** Consume `Volo.Abp.*` directly for infrastructure (EF Core providers, MongoDB, EventBus, Caching, and similar). Keep `SufiChain.SufiPlatform.*` only for value-add surfaces: module base, UI abstractions, branding, SufiAI, SufiCom, captcha, AspNetCore/MVC bases, and CLI.

**Consequences:** Documented framework package count is **31** value-add packages. Developers must use the approved ABP surfaces described in [ABP Integration](../framework/abp-integration.md).

### ADR-002: Naming — `SufiChain.SufiPlatform.*`

**Status:** Accepted (Naming Stages 0–8 complete)

**Context:** Legacy naming mixed product labels and inconsistent NuGet prefixes.

**Decision:** Canonical package and project family is `SufiChain.SufiPlatform.*`. Module source folders use short names (`tenants`, `permissions`, `jobs`, …) while docs may keep long display names (Tenant Management, Permission Management, Background Jobs).

**Consequences:** Docs and install guides must cite the short package segments (`Tenants`, `Permissions`, `ShortLinks`, `BlobDatabase`, …), not obsolete `*Management` / `ShortLinkGenerator` package names.

### ADR-003: SufiAI stays in the framework

**Status:** Accepted

**Context:** Many modules need AI contracts. Putting those packages only in a business module creates circular dependencies.

**Decision:** Keep framework SufiAI packages (`SufiAI`, `SufiAI.Abstractions`, and related abstractions) under `framework/` for cross-module access. The **AI module** under `modules/ai/` owns database-driven workspaces, RAG, MCP, and admin UI.

**Consequences:** Framework carries Semantic Kernel / Microsoft.Extensions.AI dependencies; modules consume keyed chat and kernel services.

### ADR-004: Blazor as primary UI

**Status:** Accepted

**Context:** Product UI must be consistent across modules and hosts.

**Decision:** Product UI is Blazor (Server and/or WebAssembly) with `SufiComponentBase`, SufiBlazor (`Sb*` components), and SufiTheme. Do not introduce Blazorise or ABP UI components for new work.

**Consequences:** Shared chrome and interactive controls stay in independent products; modules compose them through platform UI abstractions.

### ADR-005: Versions centralized in `versions.props`

**Status:** Accepted

**Context:** Hard-coded versions in project files drifted across packages.

**Decision:** Pin dependency versions in `sufi-platform/versions.props` and reference MSBuild properties from projects.

**Consequences:** [Technology stack](../reference/technology-stack.md) and [Dependencies](../dependencies.md) defer to `versions.props` as the source of truth.

### ADR-006: Standard module project structure

**Status:** Accepted

**Context:** Contributors need a predictable layout across first-party modules.

**Decision:** Modules follow Domain.Shared → Domain → Application.Contracts → Application → HttpApi / HttpApi.Client → EntityFrameworkCore / MongoDB → Blazor (plus optional Server/WebAssembly/Public variants). Layers may be omitted when a module has no UI or no application surface.

**Consequences:** See [Module Architecture](../framework/module-architecture.md) and [Module Catalog](../modules/index.md).

## Modules

### ADR-M001: Dual persistence

**Status:** Accepted

**Decision:** Where the module architecture requires it, ship both EF Core and MongoDB packages. Hosts choose one persistence stack at generation time (`-d ef|mongo`).

### ADR-M002: AI module persistence and vectors

**Status:** Accepted (current implementation)

**Decision:** The AI module persists workspaces and related configuration through the module’s EF Core / MongoDB packages. Vector search is implemented via MongoDB (`VectorStoreType.MongoDB`). Pgvector and Qdrant remain future options, not current defaults.

**Consequences:** Prefer [AI Architecture](../modules/ai/architecture.md) over older notes that assume pgvector-first or `CopilotDefinition` aggregates.

### ADR-M003: Calendar recurrence

**Status:** Accepted

**Decision:** Use iCalendar RRULE for recurring events so calendars stay interoperable with external systems.

### ADR-M004: Editions for feature policy binding

**Status:** Accepted

**Decision:** The Editions module defines named editions that bind tenants to feature policy values used by Feature Management and SaaS plans. Admin UI lives at `/panel/admin/editions`. Editions is not yet in the default CLI module registry.

**Consequences:** See [Editions](../modules/editions/index.md).
