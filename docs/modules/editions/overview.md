# Editions Overview

## Role

- **Edition** aggregate: `Name`, `DisplayName`, `Code`, `IsActive`
- Admin CRUD at `/panel/admin/editions`
- Entitlement resolution through `IEntitlementSource` with `FeatureCheckerEntitlementSource` at runtime
- `ILicenseApiEntitlementSource` exists as a contract for on-prem license confirmation (not implemented yet)

## Relationship to SaaS and copilots

- Commercial SufiSaas plans can reference `EditionId` and seed feature caps (for example AI Copilot limits)
- There is no Editions-specific copilot in the current alpha
- Do not expose connection strings, license secrets, or ManageFeatures mutations through copilots without a separate approval packet

## CLI note

Editions ships in `modules/editions/` but is **not** in the default CLI module registry today. Add package references and module depends manually when a host needs editions.

## Related

- [Features](features.md)
- [Installation](installation.md)
- [Permissions](permissions.md)
