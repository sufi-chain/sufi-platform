# Setting Management Architecture

This module wraps ABP setting management and adds a composable Blazor UI.

## Projects

- `Application.Contracts`
- `Application`
- `HttpApi`
- `HttpApi.Client`
- `Blazor`
- `Domain.Shared`

## UI extension model

The Blazor project contains an `ISettingComponentContributor` abstraction and `SettingManagementComponentOptions`, allowing settings groups to be registered and ordered.
