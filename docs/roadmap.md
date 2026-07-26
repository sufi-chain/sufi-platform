# Roadmap

Product roadmap for Sufi Platform. Canonical short form also lives in the repository [README](../README.md#roadmap).

**Current position:** Phase 1 (open-source foundation) is in **alpha**. **Phases 2–3 — Pro Products and Finance** are the active development focus.

| Phase | Focus | Status |
| --- | --- | --- |
| 1 | Foundation (identity, tenants, audit, jobs, settings, SufiBlazor, SufiTheme, files, calendar, AI, tags, menus, …) | Alpha |
| 2 | Pro Products (Chat, HelpDesk, messaging, Copilots, CRM, CMS) | **Alpha · active now** |
| 3 | Finance (wallets, invoices, payments, accounting, inventory) | **Alpha · active now** |
| 4 | Commerce (subscriptions, booking, events, channels) | Soon |
| 5 | ERP (workflows, approvals, procurement, projects, documents) | Future |
| 6 | HR (employees, attendance, leave, payroll, org structure) | Future |
| 7 | Scale & Enterprise (microservices, custom apps) | Future |

## Scope notes

| Phase | Distribution | Detail |
| --- | --- | --- |
| 1 | Open-source (`sufi-platform/`) | Identity, tenancy, editions, permissions, features, settings, localization, OpenIddict, audit, jobs, SufiBlazor, SufiTheme, File Manager, calendar, AI workspaces, tags, menus, short links, blob database |
| 2 | Pro NuGet (licensed; free tier) | SufiCom/Chat, HelpDesk, AI Copilots, Calendar Copilot, CRM, CMS, Forms, Branding, Dashboard |
| 3 | Pro NuGet (licensed; free tier) | Payments, wallets, invoicing, accounting, inventory |
| 4 | Pro NuGet (planned) | Subscriptions, services, booking, events, dynamic channels |
| 5 | Pro NuGet (planned) | Workflows, approvals, procurement, projects, documents |
| 6 | Pro NuGet (planned) | Employees, attendance, leave, payroll, org structure |
| 7 | Architecture | Microservice boundaries and custom enterprise apps on the same core |

## How to propose items

Describe the user problem, expected outcome, affected modules, and whether the need is reusable or product-specific. See [Product Creation Guide](product-creation-guide.md) and [Product Overview](product-overview.md).

## Notes

- Prefer product language over implementation detail.
- Phase 1 ships as open-source source in this repository. Phases 2+ **Pro** capabilities are **not open source** — they are delivered as **NuGet packages** under a license from [sufichain.com](https://sufichain.com), including a **free tier** for every licensee.
- Link roadmap items to canonical docs once the open-source feature ships, or to Pro package docs once the NuGet product is published.
- Open-source base work (CLI templates, Outbox/Inbox, editions, tests) continues under Phase 1 while Phases 2–3 Pro products and Finance are the licensed NuGet focus.
