# SbForm

A form container component that wraps Blazor's EditForm with consistent styling and structure.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Model | object | - | The form model for data binding |
| OnValidSubmit | EventCallback | - | Callback when form submits with valid data |
| OnInvalidSubmit | EventCallback | - | Callback when form submits with invalid data |
| Class | string? | null | Additional CSS classes |

## Templates / Slots (RenderFragments)

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Form fields and content |

## CSS Classes

- `sb-form` - Base class

## Examples

### Basic Usage

```razor
<SbForm Model="loginModel" OnValidSubmit="HandleLogin">
    <DataAnnotationsValidator />
    
    <SbFormField Label="Email" Required="true">
        <SbTextField @bind-Value="loginModel.Email" Type="email" />
        <SbFieldError For="() => loginModel.Email" />
    </SbFormField>
    
    <SbFormField Label="Password" Required="true">
        <SbTextField @bind-Value="loginModel.Password" Type="password" />
        <SbFieldError For="() => loginModel.Password" />
    </SbFormField>
    
    <SbButton Type="submit">Login</SbButton>
</SbForm>

@code {
    private LoginModel loginModel = new();
    
    private async Task HandleLogin()
    {
        await AuthService.LoginAsync(loginModel);
    }
}
```

### With Validation Summary

```razor
<SbForm Model="registration" OnValidSubmit="Register">
    <DataAnnotationsValidator />
    <SbValidationSummary />
    
    <!-- Form fields -->
    
    <SbButton Type="submit">Register</SbButton>
</SbForm>
```

### Handling Invalid Submit

```razor
<SbForm Model="data" 
        OnValidSubmit="Save" 
        OnInvalidSubmit="ShowValidationErrors">
    <!-- Form content -->
</SbForm>

@code {
    private void ShowValidationErrors()
    {
        ToastService.Warning("Please fix the validation errors");
    }
}
```
