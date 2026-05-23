# SbMultiSelect

A dropdown component for selecting multiple values from a list. Selected items are displayed as removable chips.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Values | IReadOnlyList\<TValue\>? | null | Selected values (two-way bindable) |
| ValuesChanged | EventCallback\<IReadOnlyList\<TValue\>\> | - | Callback when selection changes |
| Items | IEnumerable\<TItem\>? | null | Items to select from |
| TextField | Func\<TItem, string\> | item => item.ToString() | Function to get display text from item |
| ValueField | Func\<TItem, TValue\> | item => (TValue)item | Function to get value from item |
| Placeholder | string? | null | Placeholder when no selection (when null, uses localized default) |
| SearchPlaceholder | string? | null | Placeholder for search input (when null, uses localized default) |
| NoResultsText | string? | null | Text when search returns no results (when null, uses localized default) |
| RemoveAriaLabel | string? | null | Aria label for chip remove button (when null, uses localized default) |
| Searchable | bool | false | Whether the select is searchable |
| MaxSelected | int? | null | Maximum number of selections (optional cap) |
| Disabled | bool | false | Whether the select is disabled |
| Id | string? | null | Element ID for form association |
| Label | string? | null | Label text displayed above the select |
| Required | bool | false | Whether the field is required |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValuesChanged | EventCallback\<IReadOnlyList\<TValue\>\> | Fired when the selection changes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ItemTemplate | RenderFragment\<TItem\> | Custom template for dropdown items |
| ChipTemplate | RenderFragment\<TItem\> | Custom template for selected chips |
| NoResultsTemplate | RenderFragment | Template shown when search returns no results |

### Template Usage Examples

#### Custom ItemTemplate

```razor
<SbMultiSelect TItem="Tag" TValue="int"
               Items="@tags"
               @bind-Values="selectedTagIds"
               TextField="t => t.Name"
               ValueField="t => t.Id"
               Label="Tags">
    <ItemTemplate Context="tag">
        <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.Center">
            <span class="tag-color" style="background: @tag.Color"></span>
            <span>@tag.Name</span>
        </SbStack>
    </ItemTemplate>
</SbMultiSelect>
```

#### NoResultsTemplate

```razor
<SbMultiSelect TItem="User" TValue="Guid"
               Items="@users"
               @bind-Values="selectedUserIds"
               Searchable="true">
    <NoResultsTemplate>
        <SbEmptyState>
            <SbText>No users match your search</SbText>
        </SbEmptyState>
    </NoResultsTemplate>
</SbMultiSelect>
```

## CSS Classes

- `sb-select-anchor` - Shared anchor container
- `sb-multi-select-anchor` - Multi-select wrapper (used with sb-select-anchor)
- `sb-multi-select__label` - Label element
- `sb-multi-select__required` - Required asterisk
- `sb-multi-select-trigger` - Trigger container
- `sb-multi-select__chip` - Selected item chip
- `sb-multi-select__chip-remove` - Chip remove button
- `sb-multi-select__placeholder` - Placeholder text
- `sb-multi-select--disabled` - Disabled state
- `sb-select-dropdown` - Dropdown panel
- `sb-select-dropdown--flip-up` - Dropdown flips upward
- `sb-select-search` - Search input container
- `sb-select-options` - Options container
- `sb-select-option` - Individual option
- `sb-select-option--selected` - Selected option
- `sb-select-empty` - Empty state

## Accessibility

- Uses `role="listbox"` with `aria-multiselectable="true"`
- `role="option"` for each item
- `aria-selected` indicates selection state
- Chip remove buttons have aria-labels
- Keyboard: Enter/Space to toggle selection, Escape to close

## Examples

### Basic Usage

```razor
<SbMultiSelect TItem="string" TValue="string"
               Items="@options"
               @bind-Values="selectedOptions"
               Label="Select options" />
```

### With Objects

```razor
<SbMultiSelect TItem="Category" TValue="int"
               Items="@categories"
               @bind-Values="selectedCategoryIds"
               TextField="c => c.Name"
               ValueField="c => c.Id"
               Label="Categories" />
```

### Searchable with Limit

```razor
<SbMultiSelect TItem="User" TValue="Guid"
               Items="@users"
               @bind-Values="assignedUserIds"
               TextField="u => u.DisplayName"
               ValueField="u => u.Id"
               Searchable="true"
               MaxSelected="5"
               Label="Assign to (max 5)" />
```

### Required Field

```razor
<SbMultiSelect TItem="Skill" TValue="int"
               Items="@skills"
               @bind-Values="requiredSkillIds"
               TextField="s => s.Name"
               ValueField="s => s.Id"
               Label="Required Skills"
               Required="true" />
```

### Disabled State

```razor
<SbMultiSelect TItem="string" TValue="string"
               Items="@options"
               Values="@lockedValues"
               Disabled="true"
               Label="Locked Selection" />
```
