# Audit Logging Architecture

The module follows the standard administrative module shape:

- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Domain.Shared`

It wraps audit-log related contracts and presents them through a platform-aligned UI.
