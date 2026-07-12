# SufiTheme Font Override

This page explains how a host application can override the typography used by SufiTheme and SufiBlazor. Read it when the product needs custom Latin or RTL fonts but should still keep the shared theme behavior for direction, language switching, and layout composition.

## How font selection works

SufiBlazor exposes typography through CSS variables. The most important ones for host applications are:

| Variable | Purpose | Default |
| --- | --- | --- |
| `--sb-font-family-latin` | Latin and LTR scripts | Inter and fallback system fonts |
| `--sb-font-family-farsi` | Persian and other RTL-oriented scripts | Vazirmatn or theme-specific RTL stack |
| `--sb-font-family-mono` | Monospace text such as code blocks | JetBrains Mono and fallbacks |

Language and direction affect which variable is used. In practice, when the host changes the page language to Persian and sets RTL, the RTL font stack becomes active automatically.

## What SufiTheme adds

SufiTheme can layer its own RTL font preference on top of the SufiBlazor variables. That means a host usually does not need to fork the theme just to change typography; it only needs to override the right CSS variables after the theme styles are loaded.

## Host override strategy

Use this order:

1. load SufiBlazor and SufiTheme styles first
2. load host CSS after them
3. define any custom `@font-face` rules in the host CSS
4. override the relevant font variables in the host CSS

## Override only the variables

If the font is already available through the system or another stylesheet, override the variables directly:

```css
:root {
    --sb-font-family-latin: "Your Latin Font", "Segoe UI", sans-serif;
}

:lang(fa),
:lang(ar),
:lang(he),
:lang(ur) {
    --sb-font-family-farsi: "Your RTL Font", Tahoma, Arial, sans-serif;
}
```

## Use custom font files in the host

If the host ships its own font files:

1. place the font files under the host `wwwroot`
2. define `@font-face` rules in host CSS
3. override `--sb-font-family-latin` or `--sb-font-family-farsi` after the theme CSS

A typical host structure looks like this:

```text
YourHost/
  wwwroot/
    fonts/
      my-latin/
      my-rtl/
```

Then define the font families in CSS and assign them through the shared variables.

## Common patterns

### Override RTL only

Use this when the host wants a custom Persian or RTL font but is happy with the default Latin stack.

### Override Latin only

Use this when the host needs a custom brand font for English or other LTR content but wants to keep the default RTL behavior.

### Override both

Use this when the host has a full typography system of its own and still wants SufiTheme and SufiBlazor to respect it through the standard variables.
