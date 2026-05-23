# Audit Logging API

The module contains dedicated APIs for audit logs and entity changes.

## Controllers in source

The HTTP API layer includes:

- `AuditLogController`
- `EntityChangeController`

## Contract surface

The contracts layer includes DTOs for:

- audit-log listing and detail behavior
- entity-change listing and detail behavior
- query/filter inputs for both areas

## Notes

This API surface is designed for administrative and operational tooling rather than public end-user behavior.
