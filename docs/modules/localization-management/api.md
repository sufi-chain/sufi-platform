# Localization Management API

The module provides a substantial API surface for localization resources and texts.

## Controllers in source

The HTTP API layer includes:

- `LocalizationResourceController`
- `LocalizationTextController`

## Contract surface

The contracts layer includes DTOs for:

- create/update resource operations
- create/update text operations
- querying localization texts
- import/export workflows

## Notes

This module is richer than a simple admin wrapper because it also includes caching and external-store integration in the application layer.
