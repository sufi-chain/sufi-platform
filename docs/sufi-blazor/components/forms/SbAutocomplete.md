# SbAutocomplete

A text input with dropdown suggestions, supporting both static item lists and async search functions.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TItem? | null | The selected item (two-way bindable) |
| ValueChanged | EventCallback\<TItem?\> | - | Callback when selection changes |
| Items | IEnumerable\<TItem\>? | null | Static items to search from |
| SearchFunc | Func\<string, Task\<IEnumerable\<TItem\>\>\>? | null | Async function to fetch items |
| TextField | Func\<TItem, string\> | item => item.ToString() | Function to get display text |
| ValueField | Func\<TItem, object\> | item => item | Function to get value for comparison |
| MinLength | int | 1 | Minimum characters before searching |
| DebounceMs | int | 300 | Debounce delay in milliseconds |
| MaxResults | int | 10 | Maximum results to show |
| Placeholder | string? | null | Placeholder text (when null, uses localized default) |
| Clearable | bool | true | Whether to show clear button when a value is selected |
| Disabled | bool | false | Whether the input is disabled |
| Id | string? | null | Element ID for form association |
| Label | string? | null | Label text displayed above the input |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TItem?\> | Fired when the selected item changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ItemTemplate | RenderFragment\<TItem\> | Custom template for dropdown items |
| NoResultsTemplate | RenderFragment | Template shown when no results found |

### Template Usage Examples

#### Custom ItemTemplate

```razor
<SbAutocomplete TItem="User"
                @bind-Value="selectedUser"
                SearchFunc="SearchUsers"
                TextField="u => u.Name"
                Label="Find User">
    <ItemTemplate Context="user">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <img src="@user.AvatarUrl" alt="" class="avatar-sm" />
            <div>
                <div>@user.Name</div>
                <SbText Size="SbTextSize.Sm" Color="SbColor.Muted">@user.Email</SbText>
            </div>
        </SbStack>
    </ItemTemplate>
</SbAutocomplete>
```

#### NoResultsTemplate

```razor
<SbAutocomplete TItem="Product"
                @bind-Value="selectedProduct"
                Items="@products"
                TextField="p => p.Name">
    <NoResultsTemplate>
        <SbStack Direction="SbStackDirection.Column" Gap="2" Align="SbAlign.Center">
            <SbIcon Name="package-off" Size="SbSize.Lg" />
            <SbText>No products found</SbText>
            <SbLink Href="/products/new">Add new product</SbLink>
        </SbStack>
    </NoResultsTemplate>
</SbAutocomplete>
```

## CSS Classes

- `sb-autocomplete` - Base class
- `sb-autocomplete__label` - Label element
- `sb-autocomplete__required` - Required asterisk
- `sb-autocomplete__input-wrapper` - Input container
- `sb-autocomplete__input` - Text input
- `sb-autocomplete__spinner` - Loading spinner
- `sb-autocomplete__clear` - Clear button
- `sb-autocomplete__icon` - Chevron icon
- `sb-autocomplete__dropdown` - Dropdown panel
- `sb-autocomplete__dropdown--flip-up` - Dropdown flips upward
- `sb-autocomplete__option` - Individual option
- `sb-autocomplete__option--highlighted` - Keyboard-highlighted option
- `sb-autocomplete__option--selected` - Selected option
- `sb-autocomplete__no-results` - No results container

## Accessibility

- Clear button has aria-label
- Keyboard: Arrow keys to navigate, Enter to select, Escape to close

## Examples

### Basic with Static Items

```razor
<SbAutocomplete TItem="string"
                @bind-Value="selectedCity"
                Items="@cities"
                TextField="c => c"
                Label="City" />
```

### Async Search

```razor
<SbAutocomplete TItem="User"
                @bind-Value="selectedUser"
                SearchFunc="SearchUsersAsync"
                TextField="u => u.DisplayName"
                Label="Find User"
                Placeholder="Start typing..." />

@code {
    private async Task<IEnumerable<User>> SearchUsersAsync(string query)
    {
        return await UserService.SearchAsync(query);
    }
}
```

### With Minimum Length

```razor
<SbAutocomplete TItem="Address"
                @bind-Value="selectedAddress"
                SearchFunc="SearchAddresses"
                TextField="a => a.FullAddress"
                MinLength="3"
                DebounceMs="500"
                Label="Address"
                Placeholder="Enter at least 3 characters..." />
```

### Required Field

```razor
<SbAutocomplete TItem="Category"
                @bind-Value="selectedCategory"
                Items="@categories"
                TextField="c => c.Name"
                Label="Category"
                Required="true"
                Clearable="false" />
```

### Disabled State

```razor
<SbAutocomplete TItem="string"
                Value="@lockedValue"
                Items="@options"
                TextField="o => o"
                Disabled="true"
                Label="Locked Selection" />
```
