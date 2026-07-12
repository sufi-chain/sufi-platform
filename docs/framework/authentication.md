# Authentication

Modular auth for different hosting and identity scenarios in the Sufi Platform foundation of Sufi.

## Packages

| Package | Use case |
|---------|----------|
| **Authentication.Abstractions** | Shared options; dependency of others |
| **Authentication.Server** | Tiered API host (login, logout, tokens) |
| **Authentication.WebAssembly** | WASM client (token provider, auth handler) |
| **Authentication.OpenIdConnect** | OIDC / external IdP |

## Features

- **Tiered**: Blazor calls API with Bearer tokens; Server module handles login/logout and token issuance.  
- **Single**: Auth typically cookie-based in the same process.  
- **WASM**: Token storage (e.g. localStorage), `SufiAbpAccessTokenProvider`, optional auth callback page.  
- **OIDC**: Authority, client id/secret, scopes; standard OIDC flows.

## Integration

- Use **SufiAbpAuthorizationMessageHandler** (or equivalent) to attach tokens to HTTP calls when tiered.  
- Use **AuthorizeView** and **`[Authorize]`** for protected UI.  
- ABP Identity can back user validation and token creation; auth modules bridge Sufi Platform UI and ABP.
