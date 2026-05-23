# SbFormField

A wrapper component that provides consistent label, help text, and error display for form inputs.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string? | null | Label text for the field |
| HelpText | string? | null | Help text displayed below the input |
| Required | bool | false | Whether to show required indicator |
| Id | string? | null | ID to associate label with input |
| Error | string? | null | Error message to display |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | The form input to wrap |
| LabelTemplate | RenderFragment | Custom label content (overrides Label) |
| HelpTemplate | RenderFragment | Custom help content (overrides HelpText) |

### Template Usage Examples

#### Custom LabelTemplate

```razor
<SbFormField>
    <LabelTemplate>
        <span>Email</span>
        <SbBadge Color="SbColor.Primary" Size="SbSize.Sm">Required</SbBadge>
    </LabelTemplate>
    <ChildContent>
        <SbTextField @bind-Value="email" Type="email" />
    </ChildContent>
</SbFormField>
```

#### Custom HelpTemplate

```razor
<SbFormField Label="Password">
    <ChildContent>
        <SbTextField @bind-Value="password" Type="password" />
    </ChildContent>
    <HelpTemplate>
        <ul class="password-requirements">
            <li class="@(hasMinLength ? "valid" : "")">At least 8 characters</li>
            <li class="@(hasNumber ? "valid" : "")">Contains a number</li>
            <li class="@(hasSpecial ? "valid" : "")">Contains a special character</li>
        </ul>
    </HelpTemplate>
</SbFormField>
```

## CSS Classes

- `sb-form-field` - Base class
- `sb-form-field__label` - Label element
- `sb-form-field__required` - Required asterisk
- `sb-form-field__input` - Input wrapper
- `sb-form-field__help` - Help text
- `sb-form-field__error` - Error message
- `sb-form-field--error` - Error state

## Accessibility

- Label associated with input via `for` attribute when Id is provided
- Help text can be linked via aria-describedby
- Error messages displayed with appropriate styling

## Examples

### Basic Usage

```razor
<SbFormField Label="Username" HelpText="Choose a unique username">
    <SbTextField @bind-Value="username" />
</SbFormField>
```

### Required Field

```razor
<SbFormField Label="Email" Required="true">
    <SbTextField @bind-Value="email" Type="email" />
</SbFormField>
```

### With Error

```razor
<SbFormField Label="Password" Error="@passwordError">
    <SbTextField @bind-Value="password" Type="password" />
</SbFormField>
```

### With ID Association

```razor
<SbFormField Label="Name" Id="name-field">
    <SbTextField @bind-Value="name" Id="name-field" />
</SbFormField>
```

### Wrapping Other Components

```razor
<SbFormField Label="Color Theme">
    <SbColorPicker @bind-Value="themeColor" />
</SbFormField>

<SbFormField Label="Priority Level">
    <SbSlider TNumber="int" @bind-Value="priority" Min="1" Max="5" />
</SbFormField>

<SbFormField Label="Skills" HelpText="Select up to 5 skills">
    <SbMultiSelect TItem="string" TValue="string" 
                   Items="@skills" 
                   @bind-Values="selectedSkills"
                   MaxSelected="5" />
</SbFormField>
```
