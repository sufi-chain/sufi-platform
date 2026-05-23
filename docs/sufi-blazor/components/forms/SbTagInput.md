# SbTagInput

A text input that converts entered text into removable tags/chips. Supports keyboard delimiters and maximum tag limits.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Tags | List\<string\> | new() | The list of tags (two-way bindable) |
| TagsChanged | EventCallback\<List\<string\>\> | - | Callback when tags change |
| Placeholder | string | "Add tag..." | Placeholder text when the tag list is empty |
| MaxTags | int? | null | Maximum number of tags allowed |
| AllowDuplicates | bool | false | Whether to allow duplicate tags |
| Delimiters | string[] | ["Enter", "Tab", ","] | Keys that trigger tag creation |
| Disabled | bool | false | Whether the input is disabled |
| ReadOnly | bool | false | Whether the input is read-only |
| Id | string? | null | Element ID for the input |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Events

| Event | Type | Description |
|-------|------|-------------|
| TagsChanged | EventCallback\<List\<string\>\> | Fired when the tags list changes |

## CSS Classes

- `sb-tag-input` - Base class
- `sb-tag-input__container` - Container for tags and input
- `sb-tag-input__tag` - Individual tag chip
- `sb-tag-input__tag-text` - Tag text content
- `sb-tag-input__tag-remove` - Remove button
- `sb-tag-input__input` - Text input
- `sb-tag-input__counter` - Tag count display
- `sb-tag-input--disabled` - Disabled state

## Accessibility

- Remove buttons have aria-label "Remove {tag}"
- Keyboard: Backspace removes last tag when input is empty

## Examples

### Basic Usage

```razor
<SbTagInput @bind-Tags="tags" Placeholder="Add tags..." />
```

### With Maximum Tags

```razor
<SbTagInput @bind-Tags="skills" 
            MaxTags="5"
            Placeholder="Add up to 5 skills..." />
```

### Allow Duplicates

```razor
<SbTagInput @bind-Tags="items" 
            AllowDuplicates="true" />
```

### Custom Delimiters

```razor
<SbTagInput @bind-Tags="emails" 
            Delimiters="@(new[] { "Enter", " " })"
            Placeholder="Enter emails separated by space..." />
```

### Disabled State

```razor
<SbTagInput Tags="@lockedTags" Disabled="true" />
```

### Read-Only Mode

```razor
<SbTagInput @bind-Tags="displayTags" ReadOnly="true" />
```

### With Label (using SbFormField)

```razor
<SbFormField Label="Tags">
    <SbTagInput @bind-Tags="articleTags" 
                MaxTags="10"
                Placeholder="Add relevant tags..." />
</SbFormField>
```

### Pre-populated Tags

```razor
<SbTagInput @bind-Tags="categories" />

@code {
    private List<string> categories = new() 
    { 
        "Technology", 
        "Business", 
        "Design" 
    };
}
```
