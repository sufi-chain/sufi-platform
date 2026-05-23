# SbTextField

A text input component with support for labels, placeholders, adornments, password visibility toggle, and form validation. Can be used with its own **Label** and **Required** parameters, or wrapped in **SbFormField** for label, helper text, and error message layout.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Value | TValue | - | The current value (two-way bindable) |
| ValueChanged | EventCallback\<TValue\> | - | Callback when value changes |
| Label | string? | null | Label text displayed above the input |
| Placeholder | string? | null | Placeholder text |
| Type | string | "text" | Input type (text, password, email, search, etc.) |
| Required | bool | false | Whether the field is required |
| Disabled | bool | false | Whether the input is disabled |
| ReadOnly | bool | false | Whether the input is read-only |
| Clearable | bool | false | Whether to show a clear button |
| Id | string? | auto-generated | Element ID |
| InputMode | string? | null | Input mode hint for mobile keyboards |
| AutoComplete | string? | null | Autocomplete attribute value. When Type is "password" and not set, defaults to "new-password" to prevent browser autofill with cached credentials (e.g., connection strings, API keys). Use "current-password" explicitly for login forms. |
| AriaDescribedBy | string? | null | IDs of elements that describe this input |
| Style | string? | null | Inline styles for the wrapper |
| ClearAriaLabel | string? | null | Aria label for clear button (when null, uses localized default) |
| ShowPasswordAriaLabel | string? | null | Aria label for show password button (when null, uses localized default) |
| HidePasswordAriaLabel | string? | null | Aria label for hide password button (when null, uses localized default) |
| InvalidValueMessage | string | "The value '{0}' is not valid." | Validation error message format (used for non-string TValue parse errors) |
| AdditionalAttributes | Dictionary<string, object>? | - | Additional HTML attributes for the input (from InputBase) |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ValueChanged | EventCallback\<TValue\> | Fired when the value changes |
| OnClear | EventCallback | Fired when the clear button is clicked |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| StartAdornment | RenderFragment | Content shown at the start of the input (e.g., icon) |
| EndAdornment | RenderFragment | Content shown at the end of the input (e.g., icon) |

### Template Usage Examples

#### StartAdornment

```razor
<SbTextField @bind-Value="email" Type="email" Label="Email">
    <StartAdornment>
        <SbIcon Name="mail" />
    </StartAdornment>
</SbTextField>
```

#### EndAdornment

```razor
<SbTextField @bind-Value="search" Placeholder="Search...">
    <EndAdornment>
        <SbIcon Name="search" />
    </EndAdornment>
</SbTextField>
```

## CSS Classes

- `sb-text-field-wrapper` - Outer wrapper
- `sb-text-field` - Input container
- `sb-text-field__label` - Label element
- `sb-text-field__input` - The actual input element
- `sb-text-field__required` - Required asterisk
- `sb-text-field__adornment` - Adornment wrapper
- `sb-text-field__adornment--start` - Start adornment
- `sb-text-field__adornment--end` - End adornment
- `sb-text-field__clear` - Clear button
- `sb-text-field__toggle-password` - Password visibility toggle
- `sb-text-field--disabled` - Disabled state
- `sb-text-field--readonly` - Read-only state
- `sb-text-field--error` - Error state
- `sb-text-field--has-start` - Has start adornment
- `sb-text-field--has-end` - Has end adornment

## Accessibility

- Uses native `<input>` element
- `aria-invalid` set when there are validation errors
- `aria-required` set when Required is true
- `aria-describedby` for error message association
- Password toggle buttons have appropriate aria-labels
- Label properly associated via `for` attribute

## Examples

### Basic Usage (standalone label)

```razor
<SbTextField @bind-Value="name" Label="Name" Placeholder="Enter your name" />
```

### With SbFormField (label, helper text, error)

```razor
<SbFormField Label="Email" HelperText="We'll never share your email" InputId="email">
    <SbTextField TValue="string" @bind-Value="email" Id="email" Type="email" Placeholder="name@domain.com" />
</SbFormField>
```

### Password Field

```razor
<SbTextField @bind-Value="password" Type="password" Label="Password" Required="true" />
```

Password fields default to `autocomplete="new-password"` to prevent browsers from autofilling with cached credentials (useful for connection strings, API keys, and other sensitive non-login fields). For login forms where autofill is desired, set `AutoComplete="current-password"`:

```razor
<SbTextField @bind-Value="password" Type="password" Label="Password" AutoComplete="current-password" />
```

### Email with Icon

```razor
<SbTextField @bind-Value="email" Type="email" Label="Email" Required="true">
    <StartAdornment>
        <SbIcon Name="mail" Size="SbSize.Sm" />
    </StartAdornment>
</SbTextField>
```

### Clearable Search

```razor
<SbTextField @bind-Value="search" 
             Type="search" 
             Placeholder="Search..." 
             Clearable="true"
             OnClear="HandleClear">
    <StartAdornment>
        <SbIcon Name="search" Size="SbSize.Sm" />
    </StartAdornment>
</SbTextField>
```

### With Form Validation

```razor
<EditForm Model="model">
    <DataAnnotationsValidator />
    <SbTextField @bind-Value="model.Username" 
                 Label="Username" 
                 Required="true"
                 AriaDescribedBy="username-error" />
    <SbFieldError For="() => model.Username" Id="username-error" />
</EditForm>
```

### Read-Only Field

```razor
<SbTextField @bind-Value="apiKey" Label="API Key" ReadOnly="true" />
```
