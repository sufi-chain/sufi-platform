# Sufi Platform

Sufi Platform is the reusable base for building Blazor business applications on top of the [ABP Framework](https://abp.io). It gives teams a consistent starting point with framework packages, reusable modules, a shared UI system, and the `sufi` CLI.

## Relationship with ABP

SufiAbp is derived from and built on the ABP Framework ecosystem. ABP provides the proven modular architecture, DDD foundation, multi-tenancy, authorization, settings, localization, auditing, and persistence patterns.

The upstream ABP Framework source code is available on GitHub at [abpframework/abp](https://github.com/abpframework/abp). Sufi Platform extends this foundation with SufiAbp-branded APIs, SufiBlazor UI components, KomTheme, additional modules, and platform-specific tooling focused on enterprise Blazor applications.

## What lives here

This `src/` workspace contains the public source for the platform itself:

- `framework/` for the core SufiAbp packages
- `modules/` for reusable platform modules such as identity, file management, settings, tenants, and short links
- `templates/` for the `sufi` CLI templates

## How the platform fits together

1. `ABP` provides the backend architecture and layering.
2. `SufiAbp` adds platform-specific abstractions and services.
3. `SufiBlazor` provides the component library.
4. `KomTheme` provides the shell, layout, and navigation.
5. Modules add reusable business and administration features.
6. Host products combine those pieces into a finished application.

## Start here

- Read `docs/index.md` for the main documentation hub.
- Read `docs/getting-started.md` to install the CLI and generate a solution.
- Read `docs/architecture.md` to understand how the pieces fit together.
- Read `docs/modules/index.md` for the module catalog.
- Read `docs/contributing/documentation-guide.md` if you are updating docs.

## Build notes

- Build the CLI with `dotnet build src/framework/SufiChain.SufiAbp.CLI/SufiChain.SufiAbp.CLI.csproj`.
- Build the file manager solution with `dotnet build src/modules/file-manager/SufiChain.SufiAbp.FileManager.slnx`.
- Build the KomTheme solution with `dotnet build src/modules/kom-theme/SufiChain.KomTheme.slnx`.
- Build the short link generator solution with `dotnet build src/modules/short-link-generator/SCIS.SP.ShortLinkGenerator.slnx`.

## License

This source is available under the LGPL license. See `LICENSE`.
