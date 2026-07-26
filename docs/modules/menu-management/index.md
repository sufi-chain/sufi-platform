# Menu Management Module

Dynamic menu and menu-item CRUD with tree editing, public menu API, and Blazor admin UI.

## Code location

`sufi-platform/modules/menus/`

## Packages

Package segment: **`Menus`** (`SufiChain.SufiPlatform.Menus.*`).

| Layer | Notes |
|-------|-------|
| Domain.Shared / Domain | Menu entities and domain rules |
| Application[.Contracts] | CRUD, reorder, public menu API |
| HttpApi[.Client] | HTTP surface |
| Blazor / Server / WebAssembly | Admin tree, builder, modals |
| EntityFrameworkCore / MongoDB | Dual persistence |

## Capabilities

- Menu and menu-item app services (CRUD, reorder)
- Public menu app service for front-end consumption
- Localization key helpers and registry
- UI: menu tree, sidebar, builder, navigation cards, create/edit modals
- Permission-gated item actions

## Permissions

- `MenuManagement.Menus` — Default, Create, Edit, Delete, ManageItems (confirm in Domain.Shared)

## Related

- [Framework UI abstractions](../../framework/ui-abstractions.md)
- [Package Map](../../reference/package-map.md)
