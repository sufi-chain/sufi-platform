# SbSelect

A dropdown select component for choosing a single value from a list of items. Supports search filtering, custom item templates, and keyboard navigation.

**Two variants:** **SbSelect** (data-driven: bind to `Items` with `TextField`/`ValueField`) and **SbSimpleSelect** (declarative: use **SbSelectOption** children). For clearable selection (clear button), use **SbSimpleSelect** with `Clearable="true"`.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue? | null | The selected value (two-way bindable) |
| ValueChanged | EventCallback\<TValue?\> | - | Callback when selection changes |
| Items | IEnumerable\<TItem\>? | null | Items to select from |
| TextField | Func\<TItem, string\> | item => item.ToString() | Function to get display text from item |
| ValueField | Func\<TItem, TValue\> | item => (TValue)item | Function to get value from item |
| Placeholder | string? | null | Placeholder when no selection (when null, uses localized default) |
| Searchable | bool | false | Whether the select is searchable |
| Clearable | bool | false | Reserved; for clearable UI use SbSimpleSelect with Clearable="true" |
| Disabled | bool | false | Whether the select is disabled |
| Id | string? | null | Element ID for form association |
| Label | string? | null | Label text displayed above the select |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TValue?\> | Fired when the selection changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ItemTemplate | RenderFragment\<TItem\> | Custom template for rendering each item in the dropdown |
| NoResultsTemplate | RenderFragment | Template shown when search returns no results |

### Template Usage Examples

#### Custom ItemTemplate

```razor
<SbSelect TItem="Country" TValue="string"
          Items="@countries"
          @bind-Value="selectedCountryCode"
          TextField="c => c.Name"
          ValueField="c => c.Code"
          Label="Country">
    <ItemTemplate Context="country">
        <div class="country-item">
            <img src="@country.FlagUrl" alt="" class="flag-icon" />
            <span>@country.Name</span>
        </div>
    </ItemTemplate>
</SbSelect>
```

#### NoResultsTemplate

```razor
<SbSelect TItem="User" TValue="int"
          Items="@users"
          @bind-Value="selectedUserId"
          TextField="u => u.Name"
          ValueField="u => u.Id"
          Searchable="true">
    <NoResultsTemplate>
        <div class="no-results">
            <SbIcon Name="search-off" />
            <span>No users found</span>
        </div>
    </NoResultsTemplate>
</SbSelect>
```

## CSS Classes

- `sb-select-anchor` - Anchor container for dropdown positioning
- `sb-select__label` - Label element
- `sb-select__required` - Required asterisk
- `sb-select-trigger` - Button that opens the dropdown
- `sb-select-trigger__value` - Selected value display
- `sb-select-trigger__placeholder` - Placeholder text
- `sb-select-trigger__icon` - Chevron icon
- `sb-select-trigger__icon--open` - Icon rotation when open
- `sb-select-dropdown` - Dropdown panel
- `sb-select-dropdown--flip-up` - Dropdown flips upward
- `sb-select-search` - Search input container
- `sb-select-search__input` - Search input element
- `sb-select-options` - Options container
- `sb-select-option` - Individual option
- `sb-select-option--selected` - Selected option state
- `sb-select-empty` - Empty/no results state

## Accessibility

- Uses `role="listbox"` for the dropdown
- `role="option"` for each item
- `aria-selected` indicates current selection
- `aria-haspopup="listbox"` on trigger
- `aria-expanded` indicates dropdown state
- Keyboard navigation: Enter/Space to open, Escape to close, Arrow keys to navigate

## Examples

### Basic Usage

```razor
<SbSelect TItem="string" TValue="string"
          Items="@options"
          @bind-Value="selectedOption"
          Label="Choose an option" />
```

### With Objects

```razor
<SbSelect TItem="Product" TValue="int"
          Items="@products"
          @bind-Value="selectedProductId"
          TextField="p => p.Name"
          ValueField="p => p.Id"
          Label="Product"
          Placeholder="Select a product..." />
```

### Searchable Select

```razor
<SbSelect TItem="User" TValue="Guid"
          Items="@users"
          @bind-Value="selectedUserId"
          TextField="u => $"{u.FirstName} {u.LastName}"
          ValueField="u => u.Id"
          Searchable="true"
          Label="Assign to" />
```

### With Custom Template

```razor
<SbSelect TItem="Priority" TValue="int"
          Items="@priorities"
          @bind-Value="priority"
          TextField="p => p.Name"
          ValueField="p => p.Level"
          Label="Priority">
    <ItemTemplate Context="p">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <span class="priority-dot" style="background: @p.Color"></span>
            <span>@p.Name</span>
        </SbStack>
    </ItemTemplate>
</SbSelect>
```

### Disabled Select

```razor
<SbSelect TItem="string" TValue="string"
          Items="@options"
          Value="@defaultValue"
          Disabled="true"
          Label="Status (locked)" />
```

## See also

- [SbSimpleSelect](./SbSimpleSelect.md) – Declarative select with SbSelectOption children and clear button support
- [SbSelectOption](./SbSelectOption.md) – Option component for use inside SbSimpleSelect
