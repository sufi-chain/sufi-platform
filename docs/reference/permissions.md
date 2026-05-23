# Permissions Reference

Permissions should be documented in each module under `permissions.md`, but every module should answer the same questions:

- what permissions exist?
- what UI or API capabilities do they protect?
- which user roles typically need them?
- which permissions are read-only versus administrative?

Use a table format whenever possible:

| Permission | Description | Typical users |
| --- | --- | --- |
| `Module.Feature.Action` | What it allows | Admin, operator, etc. |
