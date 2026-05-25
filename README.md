# SufiAbp – Sufi ASP.NET Core Platform

**Open-Source Enterprise Platform for Blazor Applications**

SufiAbp is a comprehensive, modular platform for building enterprise-grade Blazor applications. It provides a complete infrastructure layer—authentication, authorization, multi-tenancy, localization, auditing, and more—so you can focus on your domain logic and business features.

Built on top of ABP Framework , SufiAbp offers a custom Blazor UI system (SufiBlazor component library + KomTheme), reimplemented modules with clean branding, and a powerful CLI for rapid development.

---

## What is SufiAbp?

SufiAbp is **not a fork** of ABP Framework. It is a strategic platform layer that:

- **Consumes ABP Framework** as NuGet packages for backend infrastructure (modular architecture, DDD patterns, multi-tenancy, permissions, settings, auditing)
- **Provides 63 SufiAbp packages** (43 reimplemented from ABP with custom branding + 20 innovations including custom UI system and authentication)
- **Replaces ABP's Blazorise-based UI** with a fully custom Blazor component library (SufiBlazor) and theme system (KomTheme)
- **Reimplements core ABP modules** (Identity, Tenant Management, etc.) with SufiAbp UI and branding
- **Offers custom tooling** (`sufi` CLI) for scaffolding and code generation
- **Remains fully open-source** under LGPL-3.0 license

### Architecture Stack

```
Host Applications & Products
    ↓
SufiAbp Modules (Identity, Tenant Management, File Manager, AI Management, etc.)
    ├─ KomTheme Module (Shell, Layout, Navigation, Theming System)
    ↓
SufiBlazor (Component Library) - Replaces Blazorise
    ↓
SufiAbp Framework (63 packages)
    ├─ 43 reimplemented from ABP (with SufiAbp branding & customizations)
    └─ 20 SufiAbp innovations (UI system, Auth, CLI)
    ↓
ABP Framework  (consumed as NuGet packages)
    ↓
.NET 10.0 + ASP.NET Core 10.0
```

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

### Frontend (SufiAbp Custom)

- **SufiBlazor Component Library**: Custom Blazor components (DataGrid, Form, Modal, Tabs, etc.)
- **KomTheme**: Dual-layout theme system (collapsed/expanded shells) with LTR/RTL support
- **Responsive Design**: Mobile-first layouts with adaptive navigation
- **Theming System**: CSS variables for easy customization
- **Icon System**: Integrated icon library with consistent styling
- **Localization**: Seamless integration with backend localization

### Data Access

- **Entity Framework Core**: Full EF Core support with migrations and LINQ queries
- **MongoDB**: Native MongoDB support with repository pattern
- **Dual Database Support**: Modules can support both EF Core and MongoDB
- **Repository Pattern**: Generic repositories with async operations
- **Unit of Work**: Automatic transaction management

### SufiAbp Modules

**Core Modules (Open Source - LGPL-3.0):**

- **Identity**: User, role, and claim management with ASP.NET Core Identity integration
- **Tenant Management**: Multi-tenant administration with database isolation
- **Permission Management**: Dynamic permission system with UI
- **Setting Management**: Hierarchical settings with UI
- **Feature Management**: Feature flags and tenant features
- **Audit Logging**: Entity change tracking and audit log viewer
- **File Manager**: File upload, storage, and management with multiple providers
- **Blob Storage**: Abstract blob storage with FileSystem, Database, MinIO, and S3 providers
- **OpenIddict Integration**: OAuth 2.0 and OpenID Connect server
- **Background Jobs**: Job management and monitoring
- **AI Management**: AI service integration and management (NEW)
- **Chat**: Real-time chat infrastructure (SignalR-based, basic features)
- **Channels**: Multi-channel communication infrastructure (Email only)

These infrastructure modules provide the foundation for vertical applications but are not end-user products themselves.


---

## SufiAbp vs ABP Framework

| Aspect | ABP Framework | SufiAbp Platform |
|--------|---------------|------------------|
| **UI System** | Blazorise (3rd party) | SufiBlazor (owned) |
| **Component Base** | `AbpComponentBase` | `SufiAbpComponentBase` |
| **DTO Branding** | `Volo.Abp.Application.Dtos.*` | `SufiChain.SufiAbp.Application.Dtos.*` |
| **Controller Base** | `AbpControllerBase` | `SufiAbpControllerBase` |
| **Theme** | LeptonX | KomTheme  |
| **CLI** | `abp` command | `sufi` command |
| **License** | LGPL-3.0 (framework only) | LGPL-3.0 (framework + modules) |
| **Commercial** | ABP Commercial for modules | Infrastructure open-source, vertical apps commercial |
| **Object Mapping** | AutoMapper | Mapperly (compile-time) |
| **Module UI** | Blazorise-based | SufiBlazor-based |

---

## Technology Stack

### Backend

- **.NET 10.0**: Latest .NET runtime with performance improvements
- **ASP.NET Core 10.0**: Web framework with Blazor Server and WebAssembly support
- **Entity Framework Core 10.0**: ORM with migrations and LINQ
- **MongoDB Driver**: Native MongoDB support
- **OpenIddict**: OAuth 2.0 and OpenID Connect server
- **SignalR**: Real-time communication
- **MediatR**: CQRS and mediator pattern
- **FluentValidation**: Validation library
- **Serilog**: Structured logging
- **Mapperly**: Compile-time object mapping

### Frontend

- **Blazor Server**: Server-side rendering with SignalR
- **Blazor WebAssembly**: Client-side SPA with .NET in browser
- **SufiBlazor**: Custom component library
- **KomTheme**: Layout and theming system
- **CSS Variables**: Dynamic theming
- **Responsive Design**: Mobile-first layouts

### Infrastructure

- **Redis**: Distributed caching and session storage
- **RabbitMQ**: Message broker for distributed events
- **MinIO / S3**: Object storage for files and blobs
- **PostgreSQL / SQL Server / MySQL**: Relational database options
- **MongoDB**: NoSQL database option

---

## Getting Started

### Prerequisites

- .NET 10.0 SDK
- Node.js 20+ (for frontend tooling)
- Docker (optional, for Redis, RabbitMQ, databases)
- Visual Studio 2026 / VS Code / Rider

### Installation

1. **Install the SufiAbp CLI:**

   ```bash
   dotnet tool install -g SufiChain.SufiAbp.Cli
   ```

2. **Create a new application:**

   ```bash
   sufi new MyApp -t app
   ```

   Options:
   - `-t app`: Application template (single-layer, layered, or tiered)
   - `-t module`: Reusable module template
   - `-d ef`: Entity Framework Core (default)
   - `-d mongodb`: MongoDB

3. **Navigate to the solution:**

   ```bash
   cd MyApp
   ```

4. **Run database migrations:**

   ```bash
   cd src/MyApp.DbMigrator
   dotnet run
   ```

5. **Run the application:**

   ```bash
   cd ../MyApp.Blazor
   dotnet run
   ```

6. **Open in browser:**

   Navigate to `https://localhost:44300`

   Default credentials:
   - Username: `admin`
   - Password: `1q2w3E*`

---

## Roadmap

- ✅ Core framework
- ✅ Identity module
- ✅ Tenant Management module
- ✅ File Manager module
- ✅ AI Management module
- ✅ Tags Management module
- ✅ Blob Storage with multiple providers
- ✅ CLI tooling


### Next Milestone 

- 🔄 Chat - Basic real-time chat infrastructure
- 🔄 Chat.Channels - Basic multi-channel communication 
- 🔄 Messaging 
- 🔄 TextTemplating
   
---

## License

### Open Source Components (LGPL-3.0)

The following components are licensed under LGPL-3.0 and are free to use:

- **SufiAbp Framework**
- **Core Modules**: Identity, Tenant Management, File Manager, AI Management, Audit Logging, etc.

You can use these components in both open-source and commercial projects without restrictions.

---

## Community & Support

- **Documentation**: https://docs.sabp.ir
- **Git**: https://git.sabp.ir/sufi-chain/sufi-abp

### Commercial Support

- Priority support via email and chat
- Consulting services for architecture and implementation
- Custom module development
- Training and workshops


---

## Contributing

SufiAbp is an open-source project and welcomes contributions:

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Write tests
5. Submit a pull request

---

## Acknowledgments

SufiAbp is built on top of [ABP Framework](https://abp.io) and would not be possible without the excellent work of the ABP team. We consume ABP as NuGet packages and extend it with our custom UI system, modules, and tooling.

Special thanks to:
- ABP Framework team for the solid foundation
- .NET team for the amazing platform
- Open-source community for feedback and contributions

---

**Built with ❤️ by the Sufi Chain Team**
