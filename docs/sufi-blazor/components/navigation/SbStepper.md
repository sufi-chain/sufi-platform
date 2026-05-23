# SbStepper

A multi-step wizard component for guiding users through a sequential process.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ActiveIndex | int | 0 | Current active step index (0-based) |
| Linear | bool | true | Whether steps must be completed in order |
| Orientation | SbStepperOrientation | Horizontal | Stepper orientation |
| ShowNavigation | bool | true | Whether to show navigation buttons |
| BackText | string? | null | Text for the Back button; when null, uses localized default (e.g. "Previous") |
| NextText | string? | null | Text for the Next button; when null, uses localized default |
| FinishText | string? | null | Text for the Finish button; when null, uses localized default |
| OptionalText | string? | null | Text for optional step label; when null, uses localized default |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ActiveIndexChanged | EventCallback\<int\> | Fired when active step changes |
| OnFinish | EventCallback | Fired when stepper is completed |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Step content (SbStep components) |

## SbStep Parameters

Use `SbStep` inside `SbStepper` to define each step.

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Label | string | "" | Step label shown in the header |
| Description | string? | null | Optional description below the label |
| Optional | bool | false | When true, step is marked as optional (e.g. "Optional" label) |
| Icon | RenderFragment? | null | Custom icon for the step indicator (replaces the default number); completed steps still show a checkmark |
| ChildContent | RenderFragment? | null | Content shown when the step is active |

## SbStepperOrientation Enum

```csharp
public enum SbStepperOrientation
{
    Horizontal,
    Vertical
}
```

## CSS Classes

- `sb-stepper` - Base class
- `sb-stepper--horizontal` - Horizontal orientation
- `sb-stepper--vertical` - Vertical orientation
- `sb-stepper__steps` - Steps container
- `sb-stepper__step` - Individual step
- `sb-stepper__step--completed` - Completed step
- `sb-stepper__step--active` - Active step
- `sb-stepper__step--pending` - Pending step
- `sb-stepper__step--optional` - Optional step
- `sb-stepper__step-button` - Step button
- `sb-stepper__step-indicator` - Step number/icon indicator
- `sb-stepper__step-labels` - Step label container
- `sb-stepper__step-label` - Step label text
- `sb-stepper__step-description` - Step description
- `sb-stepper__step-optional` - Optional text label
- `sb-stepper__connector` - Connector line between steps
- `sb-stepper__connector--completed` - Completed connector
- `sb-stepper__content` - Content container
- `sb-stepper__navigation` - Navigation buttons container
- `sb-stepper__back` - Back button
- `sb-stepper__next` - Next button
- `sb-stepper__finish` - Finish button

## Examples

### Basic Usage

```razor
<SbStepper @bind-ActiveIndex="currentStep" OnFinish="HandleFinish">
    <SbStep Label="Account">
        <SbStack Gap="4">
            <SbTextField Label="Email" @bind-Value="email" />
            <SbTextField Label="Password" Type="password" @bind-Value="password" />
        </SbStack>
    </SbStep>
    <SbStep Label="Profile">
        <SbStack Gap="4">
            <SbTextField Label="First Name" @bind-Value="firstName" />
            <SbTextField Label="Last Name" @bind-Value="lastName" />
        </SbStack>
    </SbStep>
    <SbStep Label="Confirm">
        <SbStack Gap="2">
            <SbText>Email: @email</SbText>
            <SbText>Name: @firstName @lastName</SbText>
        </SbStack>
    </SbStep>
</SbStepper>

@code {
    private int currentStep = 0;
    private string email, password, firstName, lastName;
    
    private async Task HandleFinish()
    {
        // Submit the form
        await SaveUser();
    }
}
```

### With Step Descriptions

Use the `Description` parameter on `SbStep` to show a subtitle under each step label.

```razor
<SbStepper @bind-ActiveIndex="step">
    <SbStep Label="Account Information" Description="Enter your personal details">
        <SbStack Gap="4">
            <SbTextField Label="Full Name" @bind-Value="fullName" />
            <SbTextField Label="Email" @bind-Value="email" />
        </SbStack>
    </SbStep>
    <SbStep Label="Address" Description="Provide your shipping address">
        <SbStack Gap="4">
            <SbTextField Label="Street" @bind-Value="street" />
            <SbTextField Label="City" @bind-Value="city" />
        </SbStack>
    </SbStep>
    <SbStep Label="Review" Description="Confirm your information">
        <SbText>Review and submit.</SbText>
    </SbStep>
</SbStepper>
```

### Vertical Orientation

```razor
<SbStepper Orientation="SbStepperOrientation.Vertical" @bind-ActiveIndex="step">
    <SbStep Label="Step 1" Description="Enter basic information">
        Content for step 1
    </SbStep>
    <SbStep Label="Step 2" Description="Configure settings">
        Content for step 2
    </SbStep>
    <SbStep Label="Step 3" Description="Review and submit">
        Content for step 3
    </SbStep>
</SbStepper>
```

### With Optional Steps

```razor
<SbStepper @bind-ActiveIndex="step">
    <SbStep Label="Required Info">
        Required step content
    </SbStep>
    <SbStep Label="Additional Info" Optional="true">
        Optional step content - can be skipped
    </SbStep>
    <SbStep Label="Confirmation">
        Final step content
    </SbStep>
</SbStepper>
```

### Non-Linear Navigation

```razor
<SbStepper Linear="false" @bind-ActiveIndex="step">
    <SbStep Label="General">
        General settings
    </SbStep>
    <SbStep Label="Advanced">
        Advanced settings
    </SbStep>
    <SbStep Label="Review">
        Review settings
    </SbStep>
</SbStepper>
```

### Custom Button Text

```razor
<SbStepper 
    BackText="Previous"
    NextText="Continue"
    FinishText="Complete Setup"
    OptionalText="(Optional)"
    @bind-ActiveIndex="step">
    <SbStep Label="Step 1">Content 1</SbStep>
    <SbStep Label="Step 2" Optional="true">Content 2</SbStep>
    <SbStep Label="Step 3">Content 3</SbStep>
</SbStepper>
```

### Without Built-in Navigation

```razor
<SbStepper ShowNavigation="false" @bind-ActiveIndex="step">
    <SbStep Label="Step 1">
        <SbStack Gap="4">
            <p>Step 1 content</p>
            <SbButton OnClick="() => step++">Continue</SbButton>
        </SbStack>
    </SbStep>
    <SbStep Label="Step 2">
        <SbStack Gap="4">
            <p>Step 2 content</p>
            <SbStack Direction="SbStackDirection.Row" Gap="2">
                <SbButton Variant="SbButtonVariant.Outline" OnClick="() => step--">Back</SbButton>
                <SbButton OnClick="() => step++">Continue</SbButton>
            </SbStack>
        </SbStack>
    </SbStep>
</SbStepper>

@code {
    private int step = 0;
}
```

### Checkout Flow

```razor
<SbStepper @bind-ActiveIndex="checkoutStep" OnFinish="PlaceOrder">
    <SbStep Label="Cart" Description="Review your items">
        <Icon><SbIcon Name="shopping-cart" /></Icon>
        <ChildContent>
            <SbStack Gap="4">
                @foreach (var item in cartItems)
                {
                    <SbCard>
                        <SbStack Direction="SbStackDirection.Row" Justify="SbJustify.SpaceBetween">
                            <span>@item.Name</span>
                            <span>@item.Price.ToString("C")</span>
                        </SbStack>
                    </SbCard>
                }
                <SbText Weight="semibold">Total: @cartTotal.ToString("C")</SbText>
            </SbStack>
        </ChildContent>
    </SbStep>
    <SbStep Label="Shipping" Description="Enter shipping address">
        <Icon><SbIcon Name="truck" /></Icon>
        <ChildContent>
            <SbGrid Columns="2" Gap="4">
                <SbTextField Label="First Name" @bind-Value="shipping.FirstName" />
                <SbTextField Label="Last Name" @bind-Value="shipping.LastName" />
                <SbGridItem Span="2">
                    <SbTextField Label="Address" @bind-Value="shipping.Address" />
                </SbGridItem>
                <SbTextField Label="City" @bind-Value="shipping.City" />
                <SbTextField Label="Zip Code" @bind-Value="shipping.ZipCode" />
            </SbGrid>
        </ChildContent>
    </SbStep>
    <SbStep Label="Payment" Description="Enter payment details">
        <Icon><SbIcon Name="credit-card" /></Icon>
        <ChildContent>
            <SbStack Gap="4">
                <SbTextField Label="Card Number" @bind-Value="payment.CardNumber" />
                <SbGrid Columns="2" Gap="4">
                    <SbTextField Label="Expiry" Placeholder="MM/YY" @bind-Value="payment.Expiry" />
                    <SbTextField Label="CVC" @bind-Value="payment.CVC" />
                </SbGrid>
            </SbStack>
        </ChildContent>
    </SbStep>
    <SbStep Label="Confirm" Description="Review your order">
        <Icon><SbIcon Name="check-circle" /></Icon>
        <ChildContent>
            <SbAlert Severity="SbAlertSeverity.Success">
                Order ready to be placed!
            </SbAlert>
        </ChildContent>
    </SbStep>
</SbStepper>
```
