# SbSimpleSelect

A dropdown select that uses **SbSelectOption** children instead of a data-bound list. Supports search, clear button, and generic value type (TValue).

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue? | null | The selected value (two-way bindable) |
| ValueChanged | EventCallback\<TValue?\> | - | Callback when selection changes |
| ChildContent | RenderFragment? | null | SbSelectOption children defining the options |
| Label | string? | null | Label text displayed above the select |
| Placeholder | string? | null | Placeholder when no selection (when null, uses localized default) |
| SearchPlaceholder | string? | null | Placeholder for the search input when Searchable (when null, uses localized default) |
| NoResultsText | string? | null | Text when search has no results (when null, uses localized default) |
| Searchable | bool | false | Whether the dropdown is searchable |
| Clearable | bool | false | Whether to show a clear button when a value is selected |
| Disabled | bool | false | Whether the select is disabled |
| NoResultsTemplate | RenderFragment? | null | Custom template when search returns no results |
| Id | string? | null | Element ID for form association |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TValue?\> | Fired when the selection changes |

## CSS Classes

- `sb-select-anchor` - Anchor container for dropdown positioning
- `sb-select-label` - Label element
- `sb-select-trigger` - Trigger wrapper
- `sb-select-trigger__main` - Button that opens the dropdown
- `sb-select-trigger__value` - Selected value display
- `sb-select-trigger__placeholder` - Placeholder text
- `sb-select-trigger__icon` - Chevron icon
- `sb-select-trigger__icon--open` - Icon when open
- `sb-select-trigger__clear` - Clear button
- `sb-select-dropdown` - Dropdown panel
- `sb-select-dropdown--flip-up` - Dropdown flips upward
- `sb-select-search` / `sb-select-search__input` - Search box
- `sb-select-options` - Options container
- `sb-select-option` - Option button
- `sb-select-option--selected` - Selected option
- `sb-select-option--disabled` - Disabled option
- `sb-select-empty` - No results state

## Accessibility

- Uses `role="listbox"` and `role="option"`
- `aria-selected` on options
- `aria-expanded` on trigger
- Keyboard: Enter/Space to open, Escape to close

## Examples

### Basic Usage

```razor
<SbSimpleSelect TValue="string" @bind-Value="selectedStatus" Label="Status">
    <SbSelectOption TValue="string" Value="draft">Draft</SbSelectOption>
    <SbSelectOption TValue="string" Value="published">Published</SbSelectOption>
    <SbSelectOption TValue="string" Value="archived">Archived</SbSelectOption>
</SbSimpleSelect>
```

### With Placeholder and Clearable

```razor
<SbSimpleSelect TValue="string" @bind-Value="selectedCountry" 
                Label="Country"
                Placeholder="Choose a country..."
                Clearable="true">
    <SbSelectOption TValue="string" Value="us">United States</SbSelectOption>
    <SbSelectOption TValue="string" Value="uk">United Kingdom</SbSelectOption>
    <SbSelectOption TValue="string" Value="ca">Canada</SbSelectOption>
    <SbSelectOption TValue="string" Value="au">Australia</SbSelectOption>
</SbSimpleSelect>
```

### Searchable

```razor
<SbSimpleSelect TValue="string" @bind-Value="selectedItem" 
                Label="Search"
                Searchable="true"
                Placeholder="Type to search...">
    <SbSelectOption TValue="string" Value="a">Apple</SbSelectOption>
    <SbSelectOption TValue="string" Value="b">Banana</SbSelectOption>
    <SbSelectOption TValue="string" Value="c">Cherry</SbSelectOption>
</SbSimpleSelect>
```

### Disabled Option

```razor
<SbSimpleSelect TValue="string" @bind-Value="selectedPlan" Label="Plan">
    <SbSelectOption TValue="string" Value="free">Free</SbSelectOption>
    <SbSelectOption TValue="string" Value="basic">Basic - $9/mo</SbSelectOption>
    <SbSelectOption TValue="string" Value="pro" Disabled="true">Pro - Coming Soon</SbSelectOption>
</SbSimpleSelect>
```

### Disabled Select

```razor
<SbSimpleSelect TValue="string" @bind-Value="lockedValue" 
                Label="Status"
                Disabled="true">
    <SbSelectOption TValue="string" Value="active">Active</SbSelectOption>
</SbSimpleSelect>
```

### Dynamic Options

```razor
<SbSimpleSelect TValue="string" @bind-Value="selectedCategory" Label="Category">
    @foreach (var category in categories)
    {
        <SbSelectOption TValue="string" Value="@category.Id">
            @category.Name
        </SbSelectOption>
    }
</SbSimpleSelect>
```
