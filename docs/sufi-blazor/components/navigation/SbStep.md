# SbStep

A single step component used within SbStepper.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string | "" | Step label text |
| Description | string? | null | Step description text |
| Optional | bool | false | Whether the step is optional |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Custom icon for the step indicator |
| ChildContent | RenderFragment | Step content |

## Cascading Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Parent | SbStepper | Parent stepper container |

## CSS Classes

- `sb-step-content` - Step content wrapper

## Examples

### Basic Usage

```razor
<SbStepper>
    <SbStep Label="Step 1">
        <p>Content for step 1</p>
    </SbStep>
    <SbStep Label="Step 2">
        <p>Content for step 2</p>
    </SbStep>
</SbStepper>
```

### With Description

```razor
<SbStep Label="Account Setup" Description="Create your account credentials">
    <SbStack Gap="4">
        <SbTextField Label="Email" @bind-Value="email" />
        <SbTextField Label="Password" Type="password" @bind-Value="password" />
    </SbStack>
</SbStep>
```

### With Custom Icon

```razor
<SbStep Label="Shipping">
    <Icon>
        <SbIcon Name="truck" Size="SbSize.Sm" />
    </Icon>
    <ChildContent>
        <p>Enter your shipping details</p>
    </ChildContent>
</SbStep>
```

### Optional Step

```razor
<SbStepper>
    <SbStep Label="Required Info">
        <p>This step is required</p>
    </SbStep>
    <SbStep Label="Additional Details" Optional="true">
        <p>This step can be skipped</p>
    </SbStep>
    <SbStep Label="Confirmation">
        <p>Review and confirm</p>
    </SbStep>
</SbStepper>
```

### Form Wizard Example

```razor
<SbStepper @bind-ActiveIndex="step">
    <SbStep Label="Personal Info" Description="Your basic information">
        <Icon><SbIcon Name="user" /></Icon>
        <ChildContent>
            <SbGrid Columns="2" Gap="4">
                <SbTextField Label="First Name" @bind-Value="firstName" Required="true" />
                <SbTextField Label="Last Name" @bind-Value="lastName" Required="true" />
                <SbGridItem Span="2">
                    <SbTextField Label="Email" @bind-Value="email" Required="true" />
                </SbGridItem>
                <SbTextField Label="Phone" @bind-Value="phone" />
            </SbGrid>
        </ChildContent>
    </SbStep>
    
    <SbStep Label="Address" Description="Where should we deliver?">
        <Icon><SbIcon Name="map-pin" /></Icon>
        <ChildContent>
            <SbStack Gap="4">
                <SbTextField Label="Street Address" @bind-Value="address" />
                <SbGrid Columns="3" Gap="4">
                    <SbTextField Label="City" @bind-Value="city" />
                    <SbTextField Label="State" @bind-Value="state" />
                    <SbTextField Label="Zip" @bind-Value="zip" />
                </SbGrid>
            </SbStack>
        </ChildContent>
    </SbStep>
    
    <SbStep Label="Preferences" Description="Customize your experience" Optional="true">
        <Icon><SbIcon Name="settings" /></Icon>
        <ChildContent>
            <SbStack Gap="3">
                <SbSwitch Label="Receive promotional emails" @bind-Value="promoEmails" />
                <SbSwitch Label="Enable SMS notifications" @bind-Value="smsNotifications" />
                <SbSelect Label="Preferred Language" @bind-Value="language">
                    <SbSelectOption Value="en">English</SbSelectOption>
                    <SbSelectOption Value="es">Spanish</SbSelectOption>
                    <SbSelectOption Value="fr">French</SbSelectOption>
                </SbSelect>
            </SbStack>
        </ChildContent>
    </SbStep>
    
    <SbStep Label="Review" Description="Confirm your details">
        <Icon><SbIcon Name="check-circle" /></Icon>
        <ChildContent>
            <SbCard>
                <SbStack Gap="2">
                    <SbHeading Level="4">Personal Information</SbHeading>
                    <SbText>@firstName @lastName</SbText>
                    <SbText>@email</SbText>
                    
                    <SbDivider />
                    
                    <SbHeading Level="4">Shipping Address</SbHeading>
                    <SbText>@address</SbText>
                    <SbText>@city, @state @zip</SbText>
                </SbStack>
            </SbCard>
        </ChildContent>
    </SbStep>
</SbStepper>
```

### With Validation

```razor
<SbStep Label="Account">
    <ChildContent>
        <SbForm Model="@accountModel" OnValidSubmit="GoToNextStep">
            <SbStack Gap="4">
                <SbTextField Label="Username" @bind-Value="accountModel.Username" />
                <SbFieldError Field="Username" />
                
                <SbTextField Label="Password" Type="password" @bind-Value="accountModel.Password" />
                <SbFieldError Field="Password" />
                
                <SbButton Type="submit">Continue</SbButton>
            </SbStack>
        </SbForm>
    </ChildContent>
</SbStep>

@code {
    private AccountModel accountModel = new();
    
    private void GoToNextStep()
    {
        currentStep++;
    }
}
```
