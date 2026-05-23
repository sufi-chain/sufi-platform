# Identity Overview

The Identity module is the main operator-facing module for user and role administration in Sufi Platform. It is the module to inspect when a host needs standard administration screens for identity data or when a team needs to extend identity workflows without owning the full stack itself.

## What it enables

- user management
- role management
- organization unit management
- security log viewing

## How it fits the platform

Identity is one of the core baseline modules. It works alongside Account for end-user flows, Permission Management for grants, and Tenant Management for host-level administration in multi-tenant products.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.Identity.Blazor` for pages such as `UserManagement`, `RoleManagement`, and `OrganizationUnitManagement`
- `SufiChain.SufiAbp.Identity.Public.Blazor` when the scenario touches public-facing identity UI
- `SufiChain.SufiAbp.Identity.Application.Contracts` for DTOs, permissions, and remote service contracts
- `SufiChain.SufiAbp.Identity.HttpApi` for the remote management surface
