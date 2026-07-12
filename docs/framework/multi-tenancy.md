# Multi-Tenancy

Tenant resolution via a **middleware** and a **resolver chain**. First resolver to return a tenant wins.

## Built-in Resolvers (by priority)

| Resolver | Source | Typical use |
|----------|--------|-------------|
| **Domain** | Subdomain / custom domain | `tenant.example.com` |
| **Header** | e.g. `X-Tenant-Id` | APIs, SPA |
| **QueryString** | e.g. `?tenant=...` | Links, redirects |
| **Route** | e.g. `/tenants/{tenant}/...` | Multi-tenant routes |
| **Cookie** | Stored tenant | Web apps |
| **CurrentUser** | User’s default tenant | Fallback |

## Features

- **Custom resolvers**: Implement `ITenantResolver` (Name, Priority, `ResolveTenantAsync`), register in DI.  
- **CurrentTenant**: Access via `ICurrentTenant` / SufiComponentBase `CurrentTenant` (Id, Name, IsAvailable).  
- **Data filtering**: Use ABP’s multi-tenant filtering; disable when needed for host-only operations.

## Configuration

Configure each resolver (enabled, header name, cookie name, etc.) through options. Middleware runs after auth, before app logic.
