# SufiBlazor Localization

SufiBlazor uses **Microsoft.Extensions.Localization** so the component library works in any Blazor app—with or without ABP. All user-facing strings in Sb components are localized via a single resource and can be overridden by the host application.

## Overview

- **No ABP dependency** — Sb does not reference ABP; localization is based on `IStringLocalizer<T>` and `.resx` files.
- **Single resource** — `SufiBlazorResource` with embedded `.resx` (default English) and culture-specific `.resx` for **fa** (Persian) and **ar** (Arabic).
- **Host override** — Host apps can keep the built-in strings, add languages, or replace them via their own resource/JSON (e.g. ABP virtual JSON).

## Resource and Files

| Item | Location |
|------|----------|
| Marker class | `SufiChain.SufiBlazor.Localization.SufiBlazorResource` |
| Default (en) | `Localization/SufiBlazorResource.resx` |
| Persian | `Localization/SufiBlazorResource.fa.resx` |
| Arabic | `Localization/SufiBlazorResource.ar.resx` |

Components inject `IStringLocalizer<SufiBlazorResource>` (typically as `L`) and use `L["Key"]` or `L["Key", arg0, arg1]` for parameterized strings.

## How Components Use Localization

### Injecting the localizer

In any Sb component that shows localized text:

```razor
@inject IStringLocalizer<SufiBlazorResource> L
```

`_Imports.razor` already has `@using Microsoft.Extensions.Localization` and `@using SufiChain.SufiBlazor.Localization`, so `L` is available once injected.

### Displaying text

- **Simple:** `@L["Clear"]`, `@L["Close"]`
- **With parameters:** `@L["FilterBy", ColumnTitle]`, `@L["Opacity", _opacity]`
- **Fallback for nullable parameters:** `@(Placeholder ?? L["Select_Placeholder"])`

### Parameter pattern

Text parameters (placeholders, button labels, aria-labels) are `string?`. When null, the component uses the localized default:

```razor
[Parameter]
public string? Placeholder { get; set; }

// In markup:
@(Placeholder ?? L["Select_Placeholder"])
```

Callers can still pass an explicit value to override the default.

## Key Naming Conventions

| Convention | Example | Use |
|------------|---------|-----|
| PascalCase | `Clear`, `Cancel`, `Apply` | Buttons, actions, aria-labels |
| Noun_Placeholder | `Select_Placeholder`, `Search_Placeholder` | Default placeholder when parameter is null |
| Filter* | `FilterEquals`, `FilterContains`, `FilterBy` | Data grid / filter UI |
| Page* | `FirstPage`, `ItemsPerPage`, `PageInfoFormat` | Pagination |
| *Format | `PageAriaLabelFormat`, `PageInfoFormat`, `Opacity` | Strings with `{0}`, `{1}` placeholders |

When adding new keys, add them to all three `.resx` files (default, `.fa.resx`, `.ar.resx`).

## Resource Keys Reference

### Actions and common UI

| Key | Default (en) |
|-----|----------------|
| Clear | Clear |
| Close | Close |
| Cancel | Cancel |
| Apply | Apply |
| Save | Save |
| Expand | Expand |
| Collapse | Collapse |
| Remove | Remove |
| Yes | Yes |
| No | No |
| Confirm | Confirm |
| Back | Back |
| Next | Next |
| Previous | Previous |
| Finish | Finish |
| Optional | Optional |
| ClearAll | Clear all |

### Placeholders

| Key | Default (en) |
|-----|----------------|
| Select_Placeholder | Select... |
| Search_Placeholder | Search... |
| SelectDate_Placeholder | Select date... |
| SelectDateRange_Placeholder | Select date range... |
| SelectTime_Placeholder | Select time... |
| SelectColor_Placeholder | Select color... |
| EnterValue_Placeholder | Enter value... |
| EnterSlug_Placeholder | enter-slug-here |

### Forms and accessibility

| Key | Default (en) |
|-----|----------------|
| ShowPassword | Show password |
| HidePassword | Hide password |
| ClearTime | Clear time |
| ClearColor | Clear color |
| ClearDate | Clear date |
| ClearDates | Clear dates |
| Hour | Hour |
| Minute | Minute |
| Second | Second |
| Period | Period |
| Now | Now |
| CustomColor | Custom Color |
| Opacity | Opacity: {0}% |
| Preview | Preview: |
| GenerateSlug | Generate |
| GenerateFromTitle | Generate from title |
| NoResultsFound | No results found |

### Data grid and pagination

| Key | Default (en) |
|-----|----------------|
| FirstPage | First page |
| PreviousPage | Previous page |
| NextPage | Next page |
| LastPage | Last page |
| ItemsPerPage | Items per page: |
| NoItems | No items |
| PageAriaLabelFormat | Page {0} |
| PageInfoFormat | {0}-{1} of {2} |
| SaveChanges | Save changes |
| CancelEditing | Cancel editing |
| EditRow | Edit row |

### Filter menu

| Key | Default (en) |
|-----|----------------|
| Operator | Operator |
| Value | Value |
| And | And |
| FilterBy | Filter: {0} |
| FilterOptions | Filter options |
| FilterEquals | Equals |
| FilterNotEquals | Not equals |
| FilterContains | Contains |
| FilterStartsWith | Starts with |
| FilterEndsWith | Ends with |
| FilterGreaterThan | Greater than |
| FilterGreaterThanOrEqual | Greater than or equal |
| FilterLessThan | Less than |
| FilterLessThanOrEqual | Less than or equal |
| FilterBetween | Between |
| FilterInList | In list |
| FilterIsEmpty | Is empty |
| FilterIsNotEmpty | Is not empty |

## Localized Components

Components that use `L` and the keys above include:

- **Forms:** SbTextField, SbAutocomplete, SbSelect, SbMultiSelect, SbSimpleSelect, SbTimePicker, SbColorPicker, SbDatePicker, SbDateRangePicker, SbSlugEditor, SbRichTextEditor (link/image dialog Cancel/Apply)
- **Overlays:** SbDialog, SbDrawer, SbToast, SbConfirmDialog
- **Data:** SbDataGrid (row edit Save/Cancel/Edit), SbPagination, SbColumnFilterMenu, SbFilterBar
- **Navigation:** SbTreeViewNode (Expand/Collapse), SbStepper (Back/Next/Finish/Optional)

## Host Application Setup

### Non-ABP Blazor app

1. Call `AddSufiBlazor()` — it registers `AddLocalization()`.
2. Configure request localization (e.g. middleware and `RequestLocalizationOptions`) as needed for your app.
3. Sb components will use `IStringLocalizer<SufiBlazorResource>` and the embedded `.resx` for the current culture.

### ABP host

1. ABP already registers localization; using `AddSufiBlazor()` is enough for the built-in Sb strings.
2. To **override** Sb strings with your own (e.g. JSON):

   ```csharp
   Configure<AbpLocalizationOptions>(options =>
   {
       options.Resources
           .Add<SufiBlazorResource>("en")
           .AddVirtualJson("/Localization/SufiBlazor");
   });
   ```

   Then add your JSON files under the path you specified; they will take precedence over the embedded `.resx` for that culture.

## Adding a New Localized String

1. **Choose a key** — Use PascalCase or the existing conventions (e.g. `Noun_Placeholder`, `Filter*`).
2. **Add to all three .resx files** in `src/modules/sufi-blazor/src/SufiChain.SufiBlazor/Localization/`:
   - `SufiBlazorResource.resx` (default/en)
   - `SufiBlazorResource.fa.resx`
   - `SufiBlazorResource.ar.resx`
3. **Use in component:** `@L["YourKey"]` or `@(YourParameter ?? L["YourKey"])`.
4. If the component does not yet inject the localizer, add `@inject IStringLocalizer<SufiBlazorResource> L`.

## RTL

Persian (fa) and Arabic (ar) are RTL. Sb layout and theme handle RTL via `SbThemeProvider` and `Direction`; localization only provides the translated strings. Ensure the app sets the correct culture and direction for the UI.
