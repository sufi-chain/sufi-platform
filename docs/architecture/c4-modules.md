# Modules C4 Model

High-level views of the 19 first-party modules under `modules/`. Per-module detail lives under [Module Catalog](../modules/index.md).

## System context

```
┌──────────┐    ┌──────────────────────────────┐    ┌──────────────┐
│  End     │    │  Sufi Platform Modules        │    │  External    │
│  Users   │◄──►│  (19 modules)                │───►│  Services    │
└──────────┘    └──────────────────────────────┘    └──────────────┘
```

## Container diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                    Host Application                                  │
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────────┐ │
│  │ Blazor UI   │  │ REST API    │  │  Background Workers         │ │
│  │ (SignalR)   │  │ (Controllers)│  │  (Jobs, Events)             │ │
│  └─────────────┘  └─────────────┘  └─────────────────────────────┘ │
│                                                                      │
│  ┌─────────────────────────────────────────────────────────────────┐│
│  │                    Module Layer (19 modules)                     ││
│  │  Account · Identity · Tenants · Editions · Features · …          ││
│  └─────────────────────────────────────────────────────────────────┘│
└─────────────────────────────────────────────────────────────────────┘
         │                   │                    │
    ┌────┴────┐        ┌─────┴─────┐       ┌────┴────┐
    │PostgreSQL│        │  MongoDB  │       │   S3    │
    └──────────┘        └───────────┘       └─────────┘
```

## Calendar module (example)

```
Calendar Module
├── Calendar.Domain — Event, Calendar, recurrence value objects
├── Calendar.Application — app services, MCP seed contributor
├── Calendar.AI — MCP tool implementations
├── Calendar.Blazor / Blazor.Public — admin + public scheduler UI
└── Calendar.EntityFrameworkCore / MongoDB
```

## AI module (example)

```
AI Module (SufiChain.SufiPlatform.SufiAI.*)
├── Domain — Workspace, AIModelConfiguration, MCPServer, DocumentChunk
├── Application — Workspace, RAG, MCP, usage app services
├── HttpApi — OpenAI-compatible `/v1` endpoints + dynamic API
├── Blazor — admin UI
└── EntityFrameworkCore / MongoDB — persistence; vectors via MongoDB today
```

Prefer [AI Architecture](../modules/ai/architecture.md) and [Calendar](../modules/calendar/index.md) for current entity and package names.

## Related

- [Architecture decisions](decisions.md)
- [Framework C4](c4-framework.md)
- [Module Architecture](../framework/module-architecture.md)
