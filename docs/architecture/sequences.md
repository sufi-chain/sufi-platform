# Sequence Diagrams

Representative platform flows. Module-specific detail belongs in each module’s architecture or usage page.

## Request processing

```
Browser         Blazor Server    SufiController   AppService     Repo         DB
   │                │                 │              │            │           │
   │ GET /entity    │                 │              │            │           │
   │───────────────>│                 │              │            │           │
   │                │ Check Auth      │              │            │           │
   │                │───────────────>│              │            │           │
   │                │   [Authorized]  │              │            │           │
   │                │ GetListAsync()  │              │            │           │
   │                │───────────────────────────────>│            │           │
   │                │                             GetList()       │           │
   │                │                                      │──────────>│
   │                │                                      │<──────────│
   │                │<───────────────────────────────│            │           │
   │<───────────────│                 │              │            │           │
```

## Login (server-side auth)

```
Browser         Login Page    SufiAccountCtrl   OpenIddict    IdentityModule    DB
   │               │              │                │              │               │
   │ POST login     │              │                │              │               │
   │───────────────>│─────────────>│                │              │               │
   │                │              │ ValidateUser() │              │               │
   │                │              │─────────────────────────────>│               │
   │                │              │ CreateToken()                  │               │
   │                │              │────────────────>│              │               │
   │                │              │ Set Cookie     │              │               │
   │<── Redirect────│              │                │              │               │
```

## Blazor component lifecycle (`SufiComponentBase`)

```
Browser         SufiComponentBase    DI Container    Service
   │                   │                  │              │
   │ Navigate to page  │                  │              │
   │──────────────────>│                  │              │
   │                   │ OnInitialized()  │              │
   │                   │ Lazy resolve L, Logger, Auth     │
   │                   │ LoadData()       │              │
   │                   │─────────────────────────────>│
   │                   │ Render()         │              │
   │<──HTML diff──────│                  │              │
```

## AI workspace chat (module)

```
Browser         Chat UI         WorkspaceSync     Kernel / ChatClient    Provider
   │                │               │                 │                │
   │ Send message   │               │                 │                │
   │───────────────>│ Resolve workspace               │                │
   │                │──────────────>│                 │                │
   │                │               │ Build / reuse keyed client        │
   │                │               │────────────────────────────────>│
   │                │               │                 │──Request─────>│
   │                │               │                 │<──Response────│
   │<──Streaming────│               │                 │                │
```

See [AI Architecture](../modules/ai/architecture.md) for workspace, MCP allowlist, and vector-store behavior.

## Related

- [Authentication](../framework/authentication.md)
- [Security](../operations/security.md)
- [Architecture decisions](decisions.md)
