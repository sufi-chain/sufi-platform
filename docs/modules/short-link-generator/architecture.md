# Short Link Generator Architecture

The Short Link Generator module is a compact but complete business module.

## Projects

- `Application.Contracts`
- `Application`
- `Domain.Shared`
- `Domain`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Blazor.Server`
- `Blazor.WebAssembly`
- `Blazor.WebAssembly.Bundling`
- `EntityFrameworkCore`
- `MongoDB`
- `test/*`

## Domain concepts

### `ShortUrl`

The main aggregate for short-code, destination URL, state, expiration, and click counters.

### `ShortUrlClick`

A click-tracking entity used for analytics and recent activity.

## Public behavior

The module includes `ShortUrlRedirectController`, which resolves short codes, checks cache and validity, and issues redirects.

## Administrative behavior

The module includes management pages and permissions for creation, editing, deletion, and analytics visibility.
