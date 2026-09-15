---
title: SufiChain.SufiPlatform.Calendar.MongoDB.Tests
type: project-readme
area: platform
status: active
source_path: "sufi-platform/test/calendar/SufiChain.SufiPlatform.Calendar.MongoDB.Tests/SufiChain.SufiPlatform.Calendar.MongoDB.Tests.csproj"
tags:
  - project
  - ai-kb
  - kb/area/platform
---

# SufiChain.SufiPlatform.Calendar.MongoDB.Tests

This README describes the `SufiChain.SufiPlatform.Calendar.MongoDB.Tests` project.
The project source is `sufi-platform/test/calendar/SufiChain.SufiPlatform.Calendar.MongoDB.Tests/SufiChain.SufiPlatform.Calendar.MongoDB.Tests.csproj`.

## Project metadata

| Field | Value |
| --- | --- |
| Project file | `sufi-platform/test/calendar/SufiChain.SufiPlatform.Calendar.MongoDB.Tests/SufiChain.SufiPlatform.Calendar.MongoDB.Tests.csproj` |
| Target framework(s) | `net10.0` |
| Project references | Calendar.TestBase, Calendar.Application, Calendar.MongoDB |

## Business coverage

Six integration tests use the real application services, domain manager, and MongoDB repositories:

- Default uniqueness per tenant and embedded default inheritance.
- Event create, update, and soft deletion.
- Private-calendar visibility and cross-tenant access rejection.
- Embedded recurrence override replacement and clearing.
- Attendee RSVP and reminder persistence and removal.

Each test owns a Mongo2Go server process and a unique database. The service provider disposes the runner after the test. No external MongoDB connection is used. The runtime must support the MongoDB executable bundled with Mongo2Go.

Permission checks are allowed by the fixture; authorization policy denial remains separate coverage. Business visibility and tenant filters execute normally. These scenarios use nontransactional units of work and do not establish transaction rollback guarantees.

## Developer wiki

[Open the canonical developer wiki](../../../../documents/developer-wiki/SufiPlatform/Overview.md).

## Validation

Build and test status is **not verified** by this README. Follow the workspace validation rules before reporting a successful build or test.
