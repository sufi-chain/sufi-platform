# Framework C4 Model

High-level views of the Sufi Platform framework. Package inventory details live in [Package Map](../reference/package-map.md).

## System context

```
┌──────────────┐    ┌──────────────────┐    ┌──────────────┐
│   End Users  │    │  Sufi Platform   │    │  3rd Party   │
│  (Browsers)  │◄──►│     Framework    │◄──►│ Services     │
└──────────────┘    └──────────────────┘    └──────────────┘
                           │
                    ┌──────┴──────┐
                    │  Modules    │
                    │ (19 first-  │
                    │  party)     │
                    └─────────────┘
                           │
                    ┌──────┴──────┐
                    │  Pro-       │
                    │  Modules    │
                    └─────────────┘
```

## Containers

| Container | Technology | Purpose |
|-----------|------------|---------|
| Blazor Server host | ASP.NET Core 10, Blazor Server | Primary interactive admin/portal host |
| Blazor WASM | ASP.NET Core 10, Blazor WebAssembly | Client-hosted UI variant |
| API layer | ASP.NET Core 10, MVC | HTTP APIs via `SufiControllerBase` and dynamic API |
| Module assemblies | .NET 10 + ABP modules | Domain, application, persistence, Blazor UI |
| PostgreSQL / MongoDB | Host choice | Relational or document persistence |
| S3-compatible storage | AWS SDK / compatible | Blob and file content |
| OpenIddict | OpenIddict module | OAuth2 / OIDC for tiered hosts |
| Semantic Kernel / M.E.AI | Framework SufiAI | Chat, embeddings, and copilot orchestration |

## Framework package families

```
┌─────────────────────────────────────────────────────────┐
│  ASP.NET Core Host (Blazor Server / WebAssembly / MVC)  │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │ SufiCom     │  │ Captcha      │  │ UI Services   │  │
│  └─────────────┘  └──────────────┘  └───────────────┘  │
│  ┌─────────────┐  ┌──────────────┐  ┌───────────────┐  │
│  │ SufiAI      │  │ Auth         │  │ DDD Layer     │  │
│  └─────────────┘  └──────────────┘  └───────────────┘  │
│  ┌─────────────────────────────────────────────────┐   │
│  │               SufiModule (ABP bridge)            │   │
│  └─────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────┘
         │           │           │           │
    ┌────┴──┐   ┌────┴──┐   ┌────┴──┐   ┌────┴──┐
│   EF Core │   │ Mongo │   │ S3    │   │ Cache │
│ Volo.Abp  │   │Volo.* │   │ Blob  │   │Volo.* │
    └───────┘   └───────┘   └───────┘   └───────┘
```

Infrastructure persistence, event bus, and caching are consumed as `Volo.Abp.*` after Framework Reduction — not as Sufi-branded wrapper packages.

## Related

- [Architecture](../architecture.md)
- [Architecture decisions](decisions.md)
- [Modules C4](c4-modules.md)
- [Framework Overview](../framework/overview.md)
