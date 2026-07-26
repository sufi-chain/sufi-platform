# Sufi Platform

**Open-Source Enterprise Platform for Blazor Applications**

Sufi Platform is a comprehensive, modular platform for building enterprise-grade Blazor applications. It provides a complete infrastructure layer — authentication, authorization, multi-tenancy, localization, auditing, AI integration, and more — so you can focus on your domain logic and business features.

Built on top of ABP Framework, Sufi Platform offers a custom Blazor UI system (SufiBlazor component library + SufiTheme), reimplemented modules with clean branding, and a CLI for rapid scaffolding.

---

## What is Sufi Platform?

Sufi Platform is an independent platform built **on top of** ABP Framework. It extends ABP with a custom UI system, branded packages, and original modules:

- **Consumes ABP Framework** as NuGet packages for backend infrastructure (modular architecture, DDD patterns, multi-tenancy, permissions, settings, auditing)
- **Provides ~31 value-add framework packages** under `SufiChain.SufiPlatform.*` (UI system, DDD bases, authentication, AI, communications, captcha, CLI)
- **Replaces ABP's Blazorise-based UI** with a fully custom Blazor component library (SufiBlazor) and theme system (SufiTheme)
- **Ships 18 first-party modules** — reimplemented ABP modules with Sufi Platform UI plus original modules (Calendar, AI, File Manager, Menu Management, etc.)
- **Offers custom tooling** (`sufi` CLI) for scaffolding and code generation
- **Remains fully open-source** under LGPL-3.0 license

### Architecture Stack

```
Host Applications & Products
    |
Sufi Platform Modules (18 modules)
    |-- SufiTheme (Shell, Layout, Navigation, Theming System)
    |
SufiBlazor (Component Library) - Replaces Blazorise
    |
Sufi Platform Framework (~31 packages)
    |-- Value-add: UI system, DDD bases, Auth, SufiAI, SufiCom, Captcha, CLI
    |-- Infrastructure consumed directly from Volo.Abp.* (EF, Mongo, EventBus, Caching, ...)
    |
ABP Framework 10.3.0 (consumed as NuGet packages)
    |
.NET 10.0 + ASP.NET Core 10.0
```

---

## Framework Packages

31 `SufiChain.SufiPlatform.*` packages under `framework/` — each adds real value beyond ABP (UI, branding, AI, communications, captcha, CLI). Thin re-export wrappers were removed as part of Framework Reduction.

| Family | Packages | Description |
|--------|----------|-------------|
| **Core / DDD** | Core, Ddd.Application.Contracts, Ddd.Application | Module base (`SufiModule`), DTO bases, `SufiApplicationService` |
| **UI** | UI.Abstractions, UI.Domain.Shared, UI.Services, UI.Blazor, UI.Blazor.Server, UI.Blazor.WebAssembly | Menu/toolbar/theme contracts, `SufiComponentBase`, localization resources |
| **SufiAI** | SufiAI, SufiAI.Abstractions | Workspace-based AI integration (chat clients, Semantic Kernel, keyed services) |
| **SufiCom** | SufiCom, SufiCom.Abstractions | Email, SMS, voice, channels, notifications |
| **AspNetCore / Auth** | AspNetCore, Mvc, Authentication.Abstractions, Authentication.OpenIdConnect, Authentication.Server, Authentication.WebAssembly, Authorization | `SufiControllerBase`, OIDC, server/WASM auth |
| **Captcha** | Captcha, Captcha.Recaptcha, Captcha.Turnstile | Math captcha, Google reCAPTCHA, Cloudflare Turnstile |
| **Data / Infra** | BlobStoring.S3Provider, Data, Features, Validation, TextTemplating, TextTemplating.Scriban | S3 blob provider, seed helpers, feature flags, templating |
| **CLI** | CLI, CLI.Core | `sufi` global tool, template pipeline, module registry |

Infrastructure packages (EF Core, MongoDB, EventBus, Caching, AutoMapper, etc.) are consumed directly as `Volo.Abp.*` NuGet packages.

---

## Modules

18 first-party modules under `modules/`, each following ABP's layered structure with Sufi Platform UI and branding.

### Reimplemented from ABP

| Module | Folder | Description |
|--------|--------|-------------|
| **Identity** | `identity` | Users, roles, organizational units, security logs, dynamic claims |
| **Account** | `account` | Registration, login, profile, 2FA, captcha, OTP |
| **Tenant Management** | `tenants` | Tenant CRUD, connection strings, database isolation |
| **Permission Management** | `permissions` | Dynamic permission store and API |
| **Setting Management** | `settings` | Hierarchical settings with admin UI (email, timezone, identity) |
| **Feature Management** | `features` | Host/tenant feature flags |
| **Audit Logging** | `audit-logging` | Entity change tracking and audit log viewer |
| **Background Jobs** | `jobs` | Job store, admin UI, retry management |
| **OpenIddict** | `openiddict` | OAuth 2.0 / OpenID Connect application and token management |
| **Users** | `users` | User lookup abstractions and public selector components |

### Sufi Platform Originals

| Module | Folder | Description |
|--------|--------|-------------|
| **AI Management** | `ai` | AI workspaces, RAG with Qdrant/Pgvector, MCP tool registry, usage analytics |
| **Calendar** | `calendar` | Calendars, events, recurrence, availability, free/busy, 12 MCP AI tools |
| **File Manager** | `file-manager` | File/media management with S3, MinIO, FileSystem, Database providers |
| **Menu Management** | `menus` | Dynamic menu CRUD, tree editor, public menu API |
| **Short Links** | `short-links` | URL shortening with click analytics |
| **Tags Management** | `tags` | Tag definitions and tag-to-entity linking |
| **Localization Management** | `localization` | Dynamic localization management and business editor |
| **Blob Storing Database** | `blob-database` | Database-backed blob storage provider (EF Core + MongoDB) |

All modules support both EF Core and MongoDB, use SufiBlazor components, and follow the `SufiComponentBase` / `SufiControllerBase` / Sufi Platform DTO conventions.

---

## Core Features

### Backend Infrastructure (from ABP)

- **Modular Architecture**: Build applications from reusable modules with clear boundaries
- **Domain-Driven Design**: Entities, aggregates, repositories, domain services, specifications
- **Multi-Tenancy**: Database-per-tenant or shared database with tenant isolation
- **Authorization**: Permission-based access control with role and user management
- **Localization**: Multi-language support with JSON resource files
- **Auditing**: Automatic tracking of entity changes and user actions
- **Settings & Features**: Hierarchical configuration system (global, tenant, user)
- **Background Jobs**: Async task processing with retry and scheduling
- **Event Bus**: Local and distributed event handling
- **Caching**: Distributed caching with Redis support
- **Validation**: Fluent validation with automatic DTO validation
- **Exception Handling**: Centralized error handling with localized messages

### Frontend (Sufi Platform Custom)

- **SufiBlazor Component Library**: Custom Blazor components (DataGrid, Form, Modal, Tabs, etc.)
- **SufiTheme**: Dual-layout theme system (collapsed/expanded shells) with LTR/RTL support
- **Responsive Design**: Mobile-first layouts with adaptive navigation
- **Theming System**: CSS variables for easy customization
- **Icon System**: Integrated icon library with consistent styling
- **Localization**: Seamless integration with backend localization

### AI Integration

- **SufiAI Framework**: Workspace-based configuration over Microsoft.Extensions.AI + Semantic Kernel
- **Provider Support**: OpenAI, Ollama, and custom OpenAI-compatible endpoints
- **RAG**: Document indexing and semantic search with Qdrant or Pgvector vector stores
- **MCP Tools**: Model Context Protocol tool registry with Semantic Kernel plugin integration
- **Calendar Copilot**: 12 MCP tools for scheduling, availability, and free/busy queries

### Data Access

- **Entity Framework Core**: Full EF Core support with migrations and LINQ queries
- **MongoDB**: Native MongoDB support with repository pattern
- **Dual Database Support**: All modules support both EF Core and MongoDB
- **Repository Pattern**: Generic repositories with async operations
- **Unit of Work**: Automatic transaction management

---

## Sufi Platform vs ABP Framework

| Aspect | ABP Framework | Sufi Platform |
|--------|---------------|---------------|
| **UI System** | Blazorise (3rd party) | SufiBlazor (owned) |
| **Component Base** | `AbpComponentBase` | `SufiComponentBase` |
| **DTO Branding** | `Volo.Abp.Application.Dtos.*` | `SufiChain.SufiPlatform.Application.Dtos.*` |
| **Controller Base** | `AbpControllerBase` | `SufiControllerBase` |
| **Theme** | LeptonX | SufiTheme |
| **CLI** | `abp` command | `sufi` command |
| **License** | LGPL-3.0 (framework only) | LGPL-3.0 (framework + modules) |
| **Object Mapping** | AutoMapper | Mapperly (compile-time) |
| **AI Integration** | — (not in ABP OSS) | SufiAI (workspaces, RAG, MCP) |

---

## Technology Stack

### Backend

- **.NET 10.0** and **ASP.NET Core 10.0**
- **Entity Framework Core 10.0** with migrations and LINQ
- **MongoDB Driver** with native repository support
- **OpenIddict** for OAuth 2.0 and OpenID Connect
- **Microsoft.Extensions.AI** + **Semantic Kernel** for AI integration
- **SignalR** for real-time communication
- **Mapperly** for compile-time object mapping
- **Serilog** for structured logging

### Frontend

- **Blazor Server** and **Blazor WebAssembly** support
- **SufiBlazor** custom component library (`Sb*` prefix)
- **SufiTheme** layout and theming system
- **CSS Variables** for dynamic theming
- **LTR/RTL** bidirectional layout support

### Infrastructure

- **Redis** for distributed caching and session storage
- **RabbitMQ** for distributed events
- **MinIO / S3** for object storage
- **Qdrant / Pgvector** for vector search (AI RAG)
- **PostgreSQL / SQL Server / MySQL / SQLite** relational databases
- **MongoDB** NoSQL database

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Node.js 20+ (for frontend tooling)
- Docker (optional, for Redis, RabbitMQ, databases)
- Visual Studio 2026 / VS Code / Rider

### Installation

1. **Install the Sufi Platform CLI:**

   ```bash
   dotnet tool install -g SufiChain.SufiPlatform.Cli
   ```

2. **Create a new application:**

   ```bash
   sufi new MyApp -t app
   ```

   Architecture variants:

   | Variant | CLI flags |
   |---------|-----------|
   | WebApp (all-in-one) | `--solution-kind webapp` |

   Database options: `-d ef` (default) or `-d mongodb`

3. **Run database migrations:**

   ```bash
   cd MyApp/src/MyApp.DbMigrator
   dotnet run
   ```

4. **Run the application:**

   ```bash
   cd ../MyApp.Blazor
   dotnet run
   ```

5. **Open in browser:**

   Navigate to `https://localhost:44300`

   Default credentials: `admin` / `1q2w3E*`

---

## Project Structure

```
sufi-platform/
  framework/        # ~31 SufiChain.SufiPlatform.* packages
  modules/          # 18 first-party modules (short folders)
  templates/        # CLI solution templates
  docs/             # Module and framework documentation
```

---

## License

All components are licensed under **LGPL-3.0** — framework and modules alike. You can use Sufi Platform in both open-source and commercial projects.

---

## Community & Support

- **Website**: https://sufiplatform.com
- **Documentation**: https://docs.sufiplatform.com
- **GitHub**: https://github.com/sufi-chain/sufi-platform

### Commercial Support

- Priority support via email and chat
- Consulting services for architecture and implementation
- Custom module development
- Training and workshops

---

## Contributing

Sufi Platform is an open-source project and welcomes contributions:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write tests
5. Submit a pull request

---

## Acknowledgments

Sufi Platform is built on top of [ABP Framework](https://abp.io) and would not be possible without the excellent work of the ABP team. We consume ABP as NuGet packages and extend it with our custom UI system, modules, and tooling.

---

**Built with care by the Sufi Chain Team**
