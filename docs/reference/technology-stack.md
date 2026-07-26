# Technology Stack

Exact versions are defined in `sufi-platform/versions.props`. Treat that file as authoritative when this page and a project file disagree.

## Runtime and platform

| Technology | Notes |
|------------|-------|
| .NET 10 | Runtime and SDK for hosts, modules, and framework implementations |
| ASP.NET Core / Blazor 10 | Server and WebAssembly hosts |
| ABP Framework 10.3.0 | Upstream modular platform (`Volo.Abp.*`); Sufi Platform is not a fork |

## Data and storage

| Technology | Purpose |
|------------|---------|
| Entity Framework Core 10 | SQL persistence for modules that ship EF packages |
| PostgreSQL / SQL Server / MySQL / MariaDB / SQLite | EF providers selectable via CLI templates |
| MongoDB | Document persistence option for dual-persistence modules |
| Redis | Optional caching / SignalR backplane |
| Amazon S3 (or compatible) | Blob content via `BlobStoring.S3Provider` |
| Database blobs | `BlobDatabase` module for DB-backed blob provider |

## AI

| Technology | Purpose |
|------------|---------|
| Microsoft.SemanticKernel | Framework SufiAI orchestration |
| Microsoft.Extensions.AI | Chat / embedding abstractions |
| OpenAI-compatible providers | Current AI module provider implementation |
| MongoDB vector store | Current AI module vector implementation |

## Auth and security

| Technology | Purpose |
|------------|---------|
| OpenIddict | OAuth2 / OIDC server (OpenIddict module) |
| Captcha / reCAPTCHA / Turnstile | Bot protection packages |

## Mapping, media, testing, CLI

| Area | Libraries |
|------|-----------|
| Mapping | Mapperly (preferred); AutoMapper where still present |
| Media | ImageSharp, FFMpegCore (File Manager and related) |
| Testing | xUnit, Shouldly, NSubstitute, bUnit, Coverlet |
| CLI UX | Spectre.Console |
| Build weaving | Fody / ConfigureAwait.Fody |

## Package counts

| Layer | Count | Location |
|-------|------:|----------|
| Framework value-add packages | 31 | `framework/` — see [Package Map](package-map.md) |
| First-party modules | 19 | `modules/` — see [Module Catalog](../modules/index.md) |

## Related

- [Dependencies](../dependencies.md)
- [Package Map](package-map.md)
- [Deployment](../operations/deployment.md)
