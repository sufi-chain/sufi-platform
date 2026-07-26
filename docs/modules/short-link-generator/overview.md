# Short Link Generator Overview

The Short Link Generator module provides a compact but complete example of a reusable business capability in Sufi. It combines management UI, public redirect handling, settings, permissions, analytics, and both EF Core and MongoDB persistence, which makes it useful both as a product feature and as a reference implementation.

## What it enables

- creation and management of short links
- public redirect handling by short code
- click counting and recent click analytics
- active, inactive, and expiration-based control over links

## How it fits the platform

Treat this module as a horizontal capability. It is useful in marketing flows, invitations, support links, navigation shortcuts, and any product that needs trackable shared URLs.

## Where to start in source

Open these packages first:

- `SufiChain.SufiPlatform.ShortLinks.Blazor` for the management UI
- `SufiChain.SufiPlatform.ShortLinks.Application` and `.Application.Contracts` for the main use cases and DTOs
- `SufiChain.SufiPlatform.ShortLinks.HttpApi` for both management and redirect-related API behavior
- `SufiChain.SufiPlatform.ShortLinks.Domain`, `.EntityFrameworkCore`, and `.MongoDB` for the persistence model
