# SufiTheme Public Navigation

SufiTheme does **not** ship a separate public layout component. `StandardLayouts.Public` resolves to the same shell as `StandardLayouts.Application` (see [Layouts](layouts.md)).

Public-facing products differentiate UX through **navigation data**, not a different layout file.

## IPublicMenuProvider

SufiTheme defines `IPublicMenuProvider` for DB-driven or host-specific public menus (knowledge base, marketing site nav, etc.).

| Implementation | Behavior |
| --- | --- |
| `NullPublicMenuProvider` | Default — returns empty menu |
| Host replacement | Register via DI `Replace(ServiceDescriptor.Singleton<IPublicMenuProvider, ...>())` |

Registration in `SufiThemeBlazorModule`:

```csharp
services.AddSingleton<IPublicMenuProvider, NullPublicMenuProvider>();
```

Hosts replace this when public routes need a menu tree separate from the admin `ApplicationMenuItem` graph.

## Console host example

SufiChane.SufiPlatform registers `ConsolePublicMenuProvider` in `ConsoleModule`:

```csharp
context.Services.Replace(ServiceDescriptor.Scoped<IPublicMenuProvider, ConsolePublicMenuProvider>());
```

That provider loads public/KB menu items from seeded data while admin navigation continues to use module `IMenuContributor` entries.

## When to use Public vs Application layout name

Use `StandardLayouts.Public` in theme/routing when a page should be tagged as public for layout resolution — it still gets the same `SideMenuLayout` / `TopMenuLayout` / `DualSidebarLayout` component configured in `SufiThemeBlazorOptions.Layout`.

Combine with:

- `IPublicMenuProvider` for public nav content
- Host zone/routing logic (e.g. `ZoneLayoutResolver`) for account vs app vs public areas

## Related

- [Layouts](layouts.md)
- [Configuration](configuration.md)
- SufiBlazor shell ownership: `independent-projects/sufi-blazor/docs/components/layout/LAYOUT_REVIEW.md`
