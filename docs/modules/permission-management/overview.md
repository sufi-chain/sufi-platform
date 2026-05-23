# Permission Management Overview

The Permission Management module gives Sufi Platform a reusable surface for permission grants and authorization-related administration. It is a foundational module even when it does not carry a large standalone UI, because other modules depend on its contracts and APIs to keep authorization behavior consistent.

## What it enables

- permission grant management
- administrative permission workflows across users, roles, or providers
- a reusable contracts and API layer for other modules that need permission integration

## How it fits the platform

Think of this module as shared infrastructure for administration rather than an isolated business feature. Identity, tenant workflows, and any custom administration surface often depend on it.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.PermissionManagement.Application` and `.Application.Contracts` for grant workflows and DTOs
- `SufiChain.SufiAbp.PermissionManagement.HttpApi` for the remote surface exposed to other parts of the platform
- `SufiChain.SufiAbp.PermissionManagement.Domain.Shared` for shared permission-related definitions
