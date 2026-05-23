# Tenant Management Overview

The Tenant Management module provides the main administrative workflow for managing tenants in a Sufi Platform host. It is the module to inspect when a product needs host-side tenant operations, tenant-aware settings, or tenant-scoped feature control exposed in a standard way.

## What it enables

- tenant administration
- host-level tenant operations
- feature and settings integration in tenant workflows
- tenant-aware platform management

## How it fits the platform

Tenant Management sits at the center of multi-tenant operations. It collaborates closely with Feature Management and Setting Management, which is why its UI composition is especially important for host applications.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.TenantManagement.Blazor` for the main management UI
- `SufiChain.SufiAbp.TenantManagement.Application` and `.Application.Contracts` for tenant workflows and contracts
- `SufiChain.SufiAbp.TenantManagement.Domain` for tenant-specific domain behavior
- `SufiChain.SufiAbp.TenantManagement.HttpApi` for the remote management surface
