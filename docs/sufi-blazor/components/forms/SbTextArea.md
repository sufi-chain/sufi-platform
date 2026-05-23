# SbTextArea

A multi-line text input component with support for labels, character counting, and resizable behavior.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | string? | null | The current text value (two-way bindable) |
| ValueChanged | EventCallback\<string?\> | - | Callback when value changes |
| Placeholder | string? | null | Placeholder text |
| Rows | int | 3 | Number of visible text rows |
| MaxLength | int? | null | Maximum character length |
| ShowCharacterCount | bool | false | Whether to show character count |
| Resizable | bool | true | Whether the textarea is resizable |
| Disabled | bool | false | Whether the textarea is disabled |
| ReadOnly | bool | false | Whether the textarea is read-only |
| Error | bool | false | Whether the textarea has an error |
| Id | string? | null | Element ID |
| Label | string? | null | Label text displayed above the textarea |
| Required | bool | false | Whether the field is required |
| HelperText | string? | null | Helper or hint text below the textarea (hidden when Error is true) |
| ErrorMessage | string? | null | Error message shown when Error is true |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes for the textarea |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<string?\> | Fired when the value changes |

## CSS Classes

- `sb-textarea` - Base class
- `sb-textarea__label` - Label element
- `sb-textarea__required` - Required asterisk
- `sb-textarea__input` - The textarea element
- `sb-textarea__helper` - Helper text container
- `sb-textarea__error` - Error message container
- `sb-textarea__counter` - Character count display
- `sb-textarea__resize-handle` - Visual resize indicator
- `sb-textarea--error` - Error state
- `sb-textarea--disabled` - Disabled state
- `sb-textarea--readonly` - Read-only state

## Accessibility

- Uses native `<textarea>` element
- `aria-invalid` set when Error is true
- `aria-describedby` links to helper text or error message ID for screen readers
- Label properly associated via `for` attribute

## Examples

### Basic Usage

```razor
<SbTextArea @bind-Value="description" 
            Label="Description" 
            Placeholder="Enter a description..." />
```

### With Character Limit

```razor
<SbTextArea @bind-Value="bio" 
            Label="Bio"
            MaxLength="500"
            ShowCharacterCount="true"
            Placeholder="Tell us about yourself..." />
```

### Larger Text Area

```razor
<SbTextArea @bind-Value="content" 
            Label="Article Content"
            Rows="10"
            Resizable="true" />
```

### Non-Resizable

```razor
<SbTextArea @bind-Value="note" 
            Label="Quick Note"
            Rows="3"
            Resizable="false" />
```

### With Helper Text

```razor
<SbTextArea @bind-Value="comments" 
            Label="Comments"
            HelperText="Maximum 500 characters"
            Placeholder="Enter comments..." />
```

### With Error State

```razor
<SbTextArea @bind-Value="comments" 
            Label="Feedback"
            Error="true"
            ErrorMessage="This field contains invalid content"
            Placeholder="Enter feedback..." />
```

### Disabled and Read-Only

```razor
<SbTextArea Value="This content is locked" 
            Label="Locked Content"
            Disabled="true" />

<SbTextArea @bind-Value="viewOnlyText" 
            Label="View Only"
            ReadOnly="true" />
```

### Required Field

```razor
<SbTextArea @bind-Value="feedback" 
            Label="Feedback"
            Required="true"
            Placeholder="Your feedback is required..." />
```
