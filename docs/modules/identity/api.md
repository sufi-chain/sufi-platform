# Identity API

The Identity module provides both ABP-aligned and module-specific API behavior.

## Controllers in source

The HTTP API layer includes controllers for:

- `IdentityUserController`
- `IdentityRoleController`
- `IdentityUserLookupController`
- `OrganizationUnitController`
- `SecurityLogController`

## Application-service areas

### Organization units

The module defines `IOrganizationUnitAppService` with operations for:

- tree retrieval
- create, update, delete, and move
- member management
- role management

### Security logs

The contracts layer includes DTOs for security-log querying and listing, including filter input and list/detail DTOs.

## Notes

The module also consumes ABP identity application contracts, which means some identity behavior comes from ABP while Sufi Platform adds additional packaging, UI, and organization-unit/security-log support.
