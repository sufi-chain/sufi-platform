# Short Link Generator Configuration

The module defines a clear settings surface for URL generation and redirect behavior.

## Defined settings

| Setting | Purpose |
| --- | --- |
| `SufiShortLinks.BaseUrl` | Base URL used when generating full short URLs |
| `SufiShortLinks.ShortUrl.RedirectRoute` | Route prefix used for redirect endpoints |
| `SufiShortLinks.ShortUrl.ShortCodeLength` | Generated short-code length |
| `SufiShortLinks.ShortUrl.CacheExpirationMinutes` | Cache duration for short-link resolution |
| `SufiShortLinks.ShortUrl.DefaultExpirationDays` | Default expiration window for generated links |

## Additional configuration areas

When expanding this page further, document:

- base-URL selection in different environments
- public-route design
- cache behavior in distributed deployments
- tenant-aware or host-aware short-link policies if introduced later
