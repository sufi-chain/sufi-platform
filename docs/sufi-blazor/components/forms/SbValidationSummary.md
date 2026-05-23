# SbValidationSummary

Displays all validation errors for the current form in a summary list.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Class | string? | null | Additional CSS classes |

## CSS Classes

- `sb-validation-summary` - Base class
- `sb-validation-summary__list` - Error list
- `sb-validation-summary__item` - Individual error item

## Examples

### Basic Usage

```razor
<EditForm Model="model">
    <DataAnnotationsValidator />
    <SbValidationSummary />
    
    <SbFormField Label="Name">
        <SbTextField @bind-Value="model.Name" />
    </SbFormField>
    
    <SbFormField Label="Email">
        <SbTextField @bind-Value="model.Email" Type="email" />
    </SbFormField>
    
    <SbButton Type="submit">Submit</SbButton>
</EditForm>
```

### At Top of Form

```razor
<SbCard>
    <Header>
        <SbHeading Level="2">Create Account</SbHeading>
    </Header>
    <EditForm Model="user">
        <DataAnnotationsValidator />
        <SbValidationSummary />
        
        <!-- Form fields here -->
    </EditForm>
</SbCard>
```

### Custom Styling

```razor
<SbValidationSummary Class="form-errors-compact" />
```
