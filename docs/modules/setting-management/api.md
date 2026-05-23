# Setting Management API

The module provides `HttpApi` and `HttpApi.Client` layers, while the Blazor UI uses application services for settings operations.

## Controllers in source

The HTTP API layer includes:

- `EmailSettingsController`
- `TimeZoneSettingsController`

## Notes

The API surface reflects the grouped nature of the UI experience: email-related settings and time-zone settings are exposed as distinct operational areas.
