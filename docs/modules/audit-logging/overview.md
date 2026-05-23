# Audit Logging Overview

The Audit Logging module exposes the operational record of what users and systems did inside a Sufi Platform application. It is the module to inspect when support teams need request history, when developers need exception context, or when product teams want a reusable audit UI instead of building one inside each host.

## What it enables

- browse audit log entries
- inspect action details for individual requests
- review exceptions tied to a request
- inspect entity changes when the underlying provider captures them

## How it fits the platform

Audit Logging is part of the baseline operations toolset. It complements modules such as Identity and Background Jobs by giving teams the history they need to understand who did what and when.

## Where to start in source

Open these packages first:

- `SufiChain.SufiAbp.AuditLogging.Blazor` for the operator-facing audit pages
- `SufiChain.SufiAbp.AuditLogging.Application.Contracts` for permission names and DTOs
- `SufiChain.SufiAbp.AuditLogging.HttpApi` for the remote API surface
