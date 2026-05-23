# Settings Reference

Settings should be documented in each module under `settings.md`, but every module should answer the same questions:

- what setting names exist?
- what are the default values?
- where are they used?
- are they tenant-scoped, user-scoped, or global?

Recommended format:

| Setting | Default | Scope | Purpose |
| --- | --- | --- | --- |
| `Module.Setting.Name` | `value` | Tenant / Global / User | What it controls |
