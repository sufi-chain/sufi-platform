# Feature Management Overview

The Feature Management module packages ABP feature management in a way that fits Sufi Platform conventions. It is the right place to start when a product needs feature values that can be controlled centrally, especially in tenant-aware or product-tiered scenarios.

## What it enables

- feature definition and value management integration
- administrative access to configurable platform and product behavior
- reuse of ABP feature services in a Sufi-aligned module package

## How it fits the platform

Feature Management is part of the configuration layer of the platform. It often works alongside Tenant Management and Setting Management to shape what a tenant or product edition is allowed to do.

## Where to start in source

Open these packages first:

- `SufiChain.SufiPlatform.FeatureManagement.Blazor` for the management UI
- `SufiChain.SufiPlatform.FeatureManagement.Application.Contracts` for feature contracts and permissions
- `SufiChain.SufiPlatform.FeatureManagement.HttpApi` for the remote surface exposed to hosts
