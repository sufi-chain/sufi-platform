# SufiBlazor Installation

This page explains how to add SufiBlazor to a Blazor application. The same installation flow works whether you are using Sufi Platform, ABP, or a plain ASP.NET Core 10+ Blazor project.

## Add the package

Add the package to the Blazor project that will render the UI:

```bash
dotnet add package SufiChain.SufiBlazor
```

## Add the static assets

Add the stylesheet to the application head:

```html
<link href="_content/SufiChain.SufiBlazor/sufiblazor.css" rel="stylesheet" />
```

Add the script before the closing `</body>` if your project uses the JavaScript-backed behaviors:

```html
<script src="_content/SufiChain.SufiBlazor/sufiblazor.js"></script>
```

## Add the namespaces

In `_Imports.razor`, add the namespaces your app uses most often. A typical starting point is:

```razor
@using SufiChain.SufiBlazor
@using SufiChain.SufiBlazor.Components
@using SufiChain.SufiBlazor.Theming
```

You can add more specific component namespaces as your project grows.

## Wrap the app with the theme provider

Wrap the application root or root layout with `SbThemeProvider` so the components receive theme and direction context:

```razor
<SbThemeProvider IsDark="false" Direction="SbDirection.Ltr">
    <Router AppAssembly="@typeof(App).Assembly">
        ...
    </Router>
</SbThemeProvider>
```

This step matters even in plain Blazor apps because the component library expects a shared theme and direction context.

## Verify the setup

Create a simple page with a few components such as `SbCard`, `SbButton`, and `SbAlert`. If the styles load and the components render correctly, the library is wired in.

## Inside Sufi Platform

If the host already uses Sufi Platform conventions:

- SufiBlazor is usually part of the expected UI stack already
- KomTheme builds on top of SufiBlazor rather than replacing it
- the same package, static asset, and theme-provider concepts still apply

## Optional notes

- If your app uses dialog, toast, or similar UI services, follow the host-specific registration guidance used by your application.
- If KomTheme is part of the host, it still depends on SufiBlazor as the base component system.
