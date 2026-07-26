# Dependencies and Libraries

Libraries and packages used by the public Sufi Platform source under `sufi-platform/`.

- **Version pinning:** `versions.props`
- **Package inventory:** [Package Map](reference/package-map.md)
- **Stack overview:** [Technology stack](reference/technology-stack.md)

## Sufi Platform packages

| Group | Role |
| --- | --- |
| Core / DDD | `SufiModule`, `SufiApplicationService`, CRUD contracts |
| UI.* | Abstractions, services, `SufiComponentBase`, Server/WASM |
| SufiAI.* | Cross-module chat/kernel workspace options |
| SufiCom.* | Email, SMS, voice, channels |
| AspNetCore / Authentication / Authorization | MVC base, OIDC, server/WASM auth |
| Captcha.* | Math, reCAPTCHA, Turnstile |
| Data, Features, Validation, TextTemplating.* | Seeds, features, validation, templates |
| BlobStoring.S3Provider | S3-compatible blobs |
| CLI / CLI.Core | `sufi` tool |
| Modules under `modules/` | 19 first-party modules (`SufiChain.SufiPlatform.{Segment}.*`) |

Infrastructure persistence, event bus, and caching come from **`Volo.Abp.*`** after Framework Reduction.

## Independent products

SufiBlazor and SufiTheme are developed and versioned independently. Detailed component/layout docs stay in their own repositories under `independent-projects/`.

## Key third-party libraries

| Area | Libraries | Notes |
| --- | --- | --- |
| Framework | ABP Framework (`Volo.Abp.*`) 10.3.0 | Modularity, permissions, tenancy, persistence foundations |
| Runtime | ASP.NET Core 10 and Blazor | Main runtime stack |
| Data | Entity Framework Core 10, MongoDB driver | Dual persistence option |
| AI | Semantic Kernel, Microsoft.Extensions.AI | Framework SufiAI + AI module |
| Logging | Serilog packages | Host-configured structured logging |
| Testing | xUnit, NSubstitute, Shouldly, bUnit, Coverlet | Unit, integration, UI tests |
| Build | Fody / ConfigureAwait.Fody | Build-time weaving |
| Media | ImageSharp, FFMpegCore | File Manager and related |

## Notes

- Exact versions are defined in `versions.props`.
- Module-specific third-party dependencies should also appear in that module’s docs when they affect installation or behavior.
