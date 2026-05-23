# Permission Management Installation

## Main packages

- `SufiChain.SufiAbp.PermissionManagement.Application.Contracts`
- `SufiChain.SufiAbp.PermissionManagement.Application`
- `SufiChain.SufiAbp.PermissionManagement.HttpApi`
- `SufiChain.SufiAbp.PermissionManagement.HttpApi.Client`
- `SufiChain.SufiAbp.PermissionManagement.Domain.Shared`

## Notable dependency pattern

Other modules, such as Identity, can reference permission-management contracts to embed permission assignment workflows into their own UI.
