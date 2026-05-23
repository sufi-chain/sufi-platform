# Module Architecture

Sufi Platform first-party modules follow a consistent ABP-style structure so teams can build, maintain, and document modules predictably.

## Standard layers

Most modules use these projects:

- `Domain.Shared`
- `Domain`
- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Blazor.Server`
- `Blazor.WebAssembly`
- `EntityFrameworkCore`
- `MongoDB`
- `test/*`

Larger modules may include extra projects, such as rich text integration, bundling, or public UI packages.

## Responsibilities by layer

| Layer | Responsibility |
| --- | --- |
| `Domain.Shared` | Shared constants, localization resources, settings names, and error codes |
| `Domain` | Aggregates, repositories, managers, options, and domain rules |
| `Application.Contracts` | DTOs, service interfaces, permission names, and remote service constants |
| `Application` | Application services, orchestration logic, caching integration, and mapping |
| `HttpApi` | Public controller surface for remote access |
| `HttpApi.Client` | Remote client proxies and integration layer for consumers |
| `Blazor*` | UI pages, menu contributors, components, and host-specific glue |
| `EntityFrameworkCore` / `MongoDB` | Persistence integration for supported data stores |
| `test/*` | Test base plus layer-specific tests |

## Common extension points

Modules usually extend the platform through:

- permissions
- settings
- menu contributors
- toolbar contributors where needed
- Blazor pages and reusable components
- application services and DTOs
- repository implementations for EF Core and MongoDB

## Documentation rule

Every module documentation folder should mirror the same reader journey:

1. overview
2. business features
3. installation
4. configuration
5. usage
6. architecture and extension points
7. permissions, settings, and APIs

This keeps documentation aligned with the module design and makes modules easier to compare.
