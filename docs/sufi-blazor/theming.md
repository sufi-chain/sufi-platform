# SufiBlazor Theming

## SbThemeProvider

Wrap app content. Set **Theme** (`Light` / `Dark`) and **Direction** (`Ltr` / `Rtl`).  
Toggle theme (and optionally persist in localStorage) to switch at runtime.

## Tokens

CSS variables under `:root` and `[data-theme="dark"]` control:

- **Colors** — Primary, secondary, semantic (success, warning, danger), surface, background, text, border.  
- **Typography** — Font family, sizes (xs–3xl), weights, line heights.  
- **Spacing** — `--sb-space-0` through `--sb-space-16`.  
- **Radius** — sm, md, lg, xl, full.  
- **Shadows** — sm, md, lg, xl.  
- **Z-index** — Dropdown, modal, popover, tooltip, toast.

Override in your own CSS to customize. Use semantic tokens (e.g. `--sb-color-primary`) rather than raw values.

## RTL

Set `Direction="Rtl"` on `SbThemeProvider`. Components use logical properties so layout flips automatically.

## Custom Themes

Define a new set of variable overrides (e.g. `brand-theme.css`), load after `sufiblazor.css`. No extra C# theme types required unless you add custom theme selection logic.
