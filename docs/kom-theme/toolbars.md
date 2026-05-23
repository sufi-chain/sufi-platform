# KomTheme Toolbars

KomTheme toolbars are built through contributor composition rather than by hard-coding buttons into the shell. Read this page when a host needs to add a top-bar action, show different items based on permissions, or keep Server and WebAssembly toolbar behavior aligned.

## Toolbar model

Toolbars are assembled from `IToolbarContributor` implementations. Each contributor decides whether it should add components to a named toolbar and in what order they should appear.

## Default behavior

Theme packages usually contribute items such as:

- theme switch
- language switch
- user menu

The exact composition depends on the host package and contributor registration.

## Custom contributors

Implement `IToolbarContributor` when you need a host-specific toolbar item.

In `ConfigureToolbarAsync`:

- check the target toolbar name, such as `KomToolbars.Main`
- add your component through the toolbar context
- choose an order that keeps the top bar predictable for users

## When to customize

Use custom contributors when a product needs:

- permission-aware toolbar actions
- tenant-aware shortcuts
- environment-specific commands
- different behavior between Server and WebAssembly hosts
