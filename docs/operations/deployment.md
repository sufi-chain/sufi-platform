# Deployment Architecture

## Package distribution

The framework publishes **31** value-add NuGet packages (`SufiChain.SufiPlatform.*`). Hosts and modules consume those packages plus `Volo.Abp.*` infrastructure packages. Exact versions are pinned in `sufi-platform/versions.props`.

## Host deployment shape

```
┌─────────────────────────────────────────────────────────────────┐
│                    Host Application                             │
│  (Blazor Server / ASP.NET Core / Docker / Kubernetes)           │
│                                                                 │
│  Blazor Hub · REST API · Static files                           │
│  Module assemblies (19 first-party + optional Pro)              │
│  Framework assemblies (UI, Auth, SufiAI, SufiCom, Captcha, …) │
└─────────────────────────────────────────────────────────────────┘
           │                   │                  │
     ┌─────┴─────┐      ┌─────┴─────┐     ┌─────┴─────┐
     │ PostgreSQL │      │ MongoDB   │     │   S3      │
     │  or SQL    │      │ (option)  │     │ (Blob)    │
     └───────────┘      └───────────┘     └───────────┘
           │
     ┌─────┴─────┐
     │ Redis     │ (optional cache / SignalR backplane)
     └───────────┘
```

## Container sketch

Use the SDK and ASP.NET runtime images that match the host’s target framework (currently .NET 10). Restore and publish the host solution; inject connection strings and blob credentials from secrets at runtime.

## Scaling notes

- **Blazor Server:** scale horizontally behind a load balancer; sticky sessions or a Redis SignalR backplane are required.
- **Stateless API / WASM hosts:** no Blazor sticky-session requirement.
- **Databases:** rely on provider connection pooling; size pools for concurrent Blazor circuits and background jobs.

## Related

- [Operational runbook](runbook.md)
- [Security](security.md)
- [Getting Started](../getting-started.md)
- [Technology stack](../reference/technology-stack.md)
