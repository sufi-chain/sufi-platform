# Identity Module Reference

**SufiChain.SufiAbp.Identity.Blazor** provides user and role management UI and is a reference implementation for Sufi Platform modules.

## Scope

- **Users**: List (with search, paging), create, edit, delete. Permission-based access.  
- **Roles**: List, create, edit, delete.  
- **Menus**: Identity group under administration; Users and Roles sub-items.  
- **Backend**: ABP Identity application contracts and services.

## Layout

- **Pages**: UserManagement, RoleManagement.  
- **Components**: UserCreateModal, UserEditModal, RoleCreateModal, RoleEditModal.  
- **Menus**: IdentityMenuContributor.

## Patterns Used

- **SufiAbpComponentBase** — `L`, `Message`, `Notify`, `ExecuteWithLoadingAsync`, `CurrentUser` / `CurrentTenant`.  
- **Lazy services** — `IIdentityUserAppService`, `IIdentityRoleAppService` via `LazyGetRequiredService`.  
- **Loading keys** — Separate keys for load-users, load-roles, delete-user; `IsOperationLoading` on **SbDataGrid**.  
- **Permissions** — `[Authorize(Policy = IdentityPermissions.Users.Default)]` on pages; **AuthorizeView** for create/edit/delete actions.  
- **Modals** — **SbDialog** + **SbForm**; create/edit flow with role selection (users).  
- **CRUD** — List with **SbDataGrid**, **SbTextField** filter, paging; create/edit via modals; delete with **Message.ConfirmAsync**.

## Dependencies

- **SufiChain.SufiAbp.UI.Blazor**, **SufiChain.SufiBlazor**, **Volo.Abp.Identity.Application.Contracts**.

Use Identity as a template for structure, menu contribution, permission checks, and CRUD + modal patterns.
