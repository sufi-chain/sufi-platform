# Short Link Generator API

## Application-service surface

The contracts layer includes `IShortUrlAppService` and DTOs for:

- creation
- update
- listing and filtering
- analytics
- click-related data

## HTTP API surface

The module includes controllers for:

- short-link management
- short-link API behavior
- public redirect handling through `ShortUrlRedirectController`

## Important behavior

The redirect flow is part of the public-facing module behavior, not only administrative UI behavior. It is therefore one of the most important pieces to validate in host applications.
