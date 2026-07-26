# Security Architecture

## Authentication

| Mode | Package / module | Notes |
|------|------------------|-------|
| OpenID Connect | `AspNetCore.Authentication.OpenIdConnect` + OpenIddict module | Authorization Code with PKCE for tiered / AuthServer hosts |
| Server-side session | `AspNetCore.Authentication.Server` | Cookie auth; `SufiAccountController` for login/logout |
| WASM OIDC | `AspNetCore.Authentication.WebAssembly` | SPA-style OIDC with silent refresh |

See [Authentication](../framework/authentication.md) and [OpenIddict](../modules/openiddict/index.md).

## Authorization

- Permission definitions via module `PermissionDefinitionProvider` classes
- `[Authorize]` / policy checks on controllers and Blazor components
- `IAuthorizationService` available from `SufiComponentBase`
- Multi-tenant entities implement `IMultiTenant`; use data filters deliberately for host-only queries

## Encryption and secrets

- TLS 1.2+ in transit; database and S3 encryption at rest are deployment choices
- Development: ASP.NET Core user secrets
- Production: Key Vault / Secrets Manager / Kubernetes secrets
- Sensitive ABP settings may use settings encryption where configured

## API and bot protection

- ASP.NET Core rate limiting is host-owned (not a framework package)
- Captcha packages: `Captcha`, `Captcha.Recaptcha`, `Captcha.Turnstile`
- Model validation plus branded validation localization (`Validation` package)
- CORS is host-configured; the framework does not enable permissive CORS by default

## Audit and exceptions

- Audit Logging module records user, action, entity changes, and timestamps
- Prefer structured Serilog sinks in hosts; keep stack traces out of end-user responses

## Related

- [Deployment](deployment.md)
- [Runbook](runbook.md)
- [Architecture decisions](../architecture/decisions.md)
