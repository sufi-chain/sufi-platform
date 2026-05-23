# Package Map

This page explains how to think about package roles across the public Sufi Platform source tree.

## Naming

- `Sufi Platform` is the product/platform name used outside the codebase.
- `SufiAbp` (`Sufi ASP.NET Core Boilerplate `) is the technical framework and package family in `SufiChain.SufiAbp.*`.
- `SufiAbp` is the code prefix used for framework and module identifiers.

## Framework packages

The main framework packages live under `src/framework/` and form the SufiAbp technical foundation of Sufi Platform. They include:

- UI abstraction packages
- Blazor platform integration packages
- authentication packages
- data and storage helpers
- CLI packages

## First-party modules

First-party modules live under `src/modules/` and follow the standard ABP layered structure described in [Module Architecture](../framework/module-architecture.md).

## Independent products in the platform story

SufiBlazor and KomTheme are important platform products, but they are developed and versioned independently.

In this documentation set:

- they can be described at the product and capability level
- their detailed technical documentation should stay in their own repositories and doc sets

## Public documentation scope

This docs set should stay centered on the packages and modules that belong to the public platform source under `/src`.
