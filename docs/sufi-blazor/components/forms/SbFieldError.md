# SbFieldError

Displays validation error messages for a specific form field, integrating with Blazor's EditContext.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| For | Expression\<Func\<object?\>\> | - | Lambda expression identifying the field |
| Id | string? | null | Element ID (for aria-describedby association) |
| Class | string? | null | Additional CSS classes |

## CSS Classes

- `sb-field-error` - Base class
- `sb-field-error__message` - Individual error message

## Accessibility

- Use with aria-describedby on the input for screen reader support

## Examples

### Basic Usage

```razor
<EditForm Model="model">
    <DataAnnotationsValidator />
    
    <SbTextField @bind-Value="model.Email" 
                 Type="email" 
                 Label="Email"
                 AriaDescribedBy="email-error" />
    <SbFieldError For="() => model.Email" Id="email-error" />
</EditForm>
```

### Multiple Fields

```razor
<EditForm Model="user">
    <DataAnnotationsValidator />
    
    <SbFormField Label="First Name">
        <SbTextField @bind-Value="user.FirstName" Id="first-name" />
        <SbFieldError For="() => user.FirstName" />
    </SbFormField>
    
    <SbFormField Label="Last Name">
        <SbTextField @bind-Value="user.LastName" Id="last-name" />
        <SbFieldError For="() => user.LastName" />
    </SbFormField>
    
    <SbFormField Label="Email">
        <SbTextField @bind-Value="user.Email" Type="email" Id="email" />
        <SbFieldError For="() => user.Email" />
    </SbFormField>
</EditForm>
```

### With Custom Styling

```razor
<SbFieldError For="() => model.Password" Class="password-error" />
```
