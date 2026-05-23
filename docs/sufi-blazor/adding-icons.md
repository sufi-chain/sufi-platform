# Adding New SVG Icons (Sufi Icons)

Sufi Icons (si) are the only icon set used in the platform. All icons live in a single registry and are rendered by `SbIcon`. This guide explains how to add a new SVG icon.

## Where icons are defined

| File | Purpose |
|------|---------|
| `SufiChain.SufiBlazor/Icons/SiIconCategory.cs` | Enum of categories (Navigation, Actions, Media, Files, etc.) |
| `SufiChain.SufiBlazor/Icons/SiIconMetadata.cs` | Record: Name, Category, Description, Svg |
| `SufiChain.SufiBlazor/Icons/SufiIcons.cs` | Static registry: dictionary of all icons, `GetIcon`, `GetSvg`, `GetByCategory`, `Search` |
| `SufiChain.SufiBlazor/Components/Common/SbIcon.razor` | Renders icon by name via `SufiIcons.GetSvg(Name)` — **do not add icons here** |

Icons are **not** defined in `SbIcon.razor`. They are defined only in `SufiIcons.cs` inside `CreateIconRegistry()`.

## SVG contract (required for consistency)

All new outline-style icons must follow this contract so they scale and theme correctly:

- **ViewBox**: `viewBox="0 0 24 24"`
- **Fill**: `fill="none"` for outline icons
- **Stroke**: `stroke="currentColor"` and `stroke-width="2"`
- **Caps/joins**: `stroke-linecap="round"` and `stroke-linejoin="round"`
- **No scripts, external refs, or editor metadata**

Example outline SVG:

```xml
<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round">
  <path d="M6 9l6 6 6-6"/>
</svg>
```

For solid/filled icons (e.g. dots, filled shapes), use `fill="currentColor"` and no stroke. The registry defines constants `SvgOutlineAttrs` and `SvgSolidAttrs` in `SufiIcons.cs` — use them when building the SVG string in code.

## Step-by-step: add a new icon

### 1. Choose a name and category

- **Name**: kebab-case (e.g. `file-pdf`, `user-plus`). Must be unique.
- **Category**: Pick one of the values in `SiIconCategory` (e.g. `SiIconCategory.Files`, `SiIconCategory.Actions`). If you need a new category, add it to the enum in `SiIconCategory.cs` first.

### 2. Add the icon to the registry

Open `sufi-platform/src/modules/sufi-blazor/src/SufiChain.SufiBlazor/Icons/SufiIcons.cs` and find the method `CreateIconRegistry()`. Add a new entry to the dictionary:

```csharp
["your-icon-name"] = new("your-icon-name", SiIconCategory.YourCategory, "Short description for search/accessibility",
    $"<svg {SvgOutlineAttrs}><path d=\"M...\"/></svg>"),
```

- Use `SvgOutlineAttrs` for stroke-based icons or `SvgSolidAttrs` for fill-only icons.
- Escape double quotes inside the SVG string (e.g. `\"` for attributes).
- Place the entry in the appropriate comment block (e.g. `// ========== File Icons ==========`) so the file stays organized.

### 3. Verify in the demo

Run the SufiBlazorDemo app and open the Icon page (e.g. `/components/icon`). Your icon should appear in the grid and in search. You can filter by category to confirm it’s in the right group.

### 4. Use the icon in the app

Use the same name (with or without the `si-` prefix) in `SbIcon`:

```razor
<SbIcon Name="your-icon-name" />
```

For menu items and other string-based icon fields, use the same name:

```csharp
icon: "your-icon-name"
```

## Naming conventions

- Use **kebab-case**: `chevron-down`, `folder-open`, `shield-check`.
- Prefer **short, clear names** that match common icon sets (e.g. Lucide/Feather) for easier recognition.
- **No** `si-` prefix in the registry key; the component accepts both `"home"` and `"si-home"` and normalizes internally.

## Optional: AI-generated icons

If you generate SVG with an AI or tooling:

1. Enforce the same contract (24×24 viewBox, stroke/fill, round caps/joins, `currentColor`).
2. Strip scripts, external references, and metadata.
3. Prefer optimizing with SVGO (or similar) before pasting into the registry, and avoid aggressive rounding of path data.

The file `.cursor/plans/icons-list.csv` in the repo contains icon names, categories, and prompts used for the initial pack; you can use it as a reference for naming and categories.

## Summary checklist

- [ ] Name is kebab-case and unique.
- [ ] Category exists in `SiIconCategory` (add enum value if needed).
- [ ] New entry added in `SufiIcons.CreateIconRegistry()` with correct SVG.
- [ ] SVG follows the contract (viewBox, stroke/fill, round caps/joins, `currentColor`).
- [ ] Icon appears and is searchable on the demo Icon page.
- [ ] Usage in app is via `<SbIcon Name="your-icon-name" />` or `icon: "your-icon-name"` only.
