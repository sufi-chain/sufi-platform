---
title: SufiChain.SufiPlatform.EventBus.Abstractions
type: project-readme
area: platform
status: active
source_path: "sufi-platform/framework/SufiChain.SufiPlatform.EventBus.Abstractions/SufiChain.SufiPlatform.EventBus.Abstractions.csproj"
tags:
  - project
  - ai-kb
  - kb/area/platform
---

# SufiChain.SufiPlatform.EventBus.Abstractions

This README describes the `SufiChain.SufiPlatform.EventBus.Abstractions` project.
The project source is `sufi-platform/framework/SufiChain.SufiPlatform.EventBus.Abstractions/SufiChain.SufiPlatform.EventBus.Abstractions.csproj`.

`SufiIntegrationEto` is the shared base for distributed event contracts. In
addition to the stable event id and tenant scope, it carries event version,
correlation, causation, source, source aggregate, and optional W3C trace
metadata. The envelope fields are additive so existing event contracts remain
deserializable while new publishers can provide the metadata required for
Inbox/Outbox reliability and future service extraction.

`IEventInboxStore` is the framework port for durable consumer receipts. Its
implementation must atomically claim the `(consumer, tenant, event id)` key,
record retries, and move poison messages to a dead-letter state. The current
host uses ABP's EF Core Inbox/Outbox records as the physical persistence
mechanism; this abstraction does not introduce a second store.

`IEventEnvelopeEnricher` and the default `EventEnvelopeEnricher` provide the
publication-boundary metadata policy. They fill missing event id, occurrence
time, tenant, source, correlation, causation, and W3C trace values while
preserving explicit contract values. Wiring the enricher into every publisher
and validating it in the host remain required runtime work.

`IIntegrationEventPublisher` and `IntegrationEventPublisher` provide the
shared publication boundary for new cross-module events. The publisher enriches
the envelope and delegates to ABP's configured `IDistributedEventBus`; it does
not create a second Outbox or broker abstraction. When called inside a Unit of
Work, ABP's existing host configuration persists the event in the same
transaction through `OutgoingEventRecord`.

## Project metadata

| Field | Value |
| --- | --- |
| Project file | `sufi-platform/framework/SufiChain.SufiPlatform.EventBus.Abstractions/SufiChain.SufiPlatform.EventBus.Abstractions.csproj` |
| Target framework(s) | `netstandard2.1;net10.0` |
| Project references | None declared |

## Developer wiki

[Open the canonical developer wiki](../../documents/developer-wiki/SufiPlatform/Overview.md).

## Validation

Build and test status is **not verified** by this README. Follow the workspace validation rules before reporting a successful build or test.
