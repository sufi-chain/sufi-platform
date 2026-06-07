# SbDialog

A modal dialog component built on the native HTML `<dialog>` element with header, body, and footer slots.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Open | bool | false | Whether the dialog is open |
| Title | string? | null | Dialog title displayed in header |
| Size | SbDialogSize | Md | Dialog size preset |
| Width | string? | null | Override width; value applied as-is to CSS (e.g. "800px", "80%") |
| MaxWidth | string? | null | Override max-width; value applied as-is to CSS (e.g. "800px", "80%") |
| Height | string? | null | Override height; value applied as-is to CSS (e.g. "80vh", "600px", "80%") |
| MaxHeight | string? | null | Override max-height; value applied as-is to CSS (e.g. "90vh", "800px", "80%") |
| CloseOnEscape | bool | true | Whether to close on Escape key |
| CloseOnBackdropClick | bool | true | Whether to close when clicking backdrop |
| ShowCloseButton | bool | true | Whether to show the close button |
| OnCloseButtonClick | EventCallback | - | Optional hook invoked before the standard header close (X) flow. `OpenChanged(false)` still fires. |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OpenChanged | EventCallback\<bool\> | Fired when open state changes |
| OnClose | EventCallback\<SbDialogCloseReason\> | Fired when dialog closes with reason |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Header | RenderFragment | Custom header content (replaces Title) |
| ChildContent | RenderFragment | Dialog body content |
| Footer | RenderFragment | Footer content for action buttons |

## SbDialogSize Enum

```csharp
public enum SbDialogSize
{
    Sm,         // 400px max-width
    Md,         // 600px max-width
    Lg,         // 800px max-width
    Xl,         // 1024px max-width
    FullScreen  // Full screen
}
```

## SbDialogCloseReason Enum

```csharp
public enum SbDialogCloseReason
{
    Escape,       // User pressed Escape key
    Backdrop,     // User clicked the backdrop
    CloseButton,  // User clicked the close button
    Programmatic  // Closed via code
}
```

## CSS Classes

- `sb-dialog` - Base class
- `sb-dialog--sm|md|lg|xl|fullscreen` - Size variants
- `sb-dialog__container` - Inner container
- `sb-dialog__header` - Header section
- `sb-dialog__title` - Title text
- `sb-dialog__close-btn` - Close button
- `sb-dialog__body` - Body content
- `sb-dialog__footer` - Footer section

## Accessibility

- Uses native `<dialog>` element
- `aria-labelledby` links to title
- `aria-modal="true"` indicates modal
- Close button has `aria-label="Close"`

## Examples

### Basic Usage

```razor
<SbButton OnClick="() => isOpen = true">Open Dialog</SbButton>

<SbDialog @bind-Open="isOpen" Title="Hello">
    <p>This is the dialog content.</p>
</SbDialog>

@code {
    private bool isOpen = false;
}
```

### With Footer Actions

```razor
<SbDialog @bind-Open="isOpen" Title="Save Changes?">
    <ChildContent>
        <p>You have unsaved changes. Would you like to save them?</p>
    </ChildContent>
    <Footer>
        <SbStack Direction="SbStackDirection.Row" Gap="2" Justify="SbJustify.End">
            <SbButton Variant="SbButtonVariant.Outline" OnClick="() => isOpen = false">
                Discard
            </SbButton>
            <SbButton OnClick="Save">Save Changes</SbButton>
        </SbStack>
    </Footer>
</SbDialog>
```

### Different Sizes

```razor
<SbDialog @bind-Open="smallOpen" Title="Small Dialog" Size="SbDialogSize.Sm">
    Small content
</SbDialog>

<SbDialog @bind-Open="largeOpen" Title="Large Dialog" Size="SbDialogSize.Lg">
    Large content
</SbDialog>

<SbDialog @bind-Open="fullOpen" Title="Full Screen" Size="SbDialogSize.FullScreen">
    Full screen content
</SbDialog>
```

### Custom Dimensions (Width, MaxWidth, Height, MaxHeight)

Override the Size preset with inline dimensions. Inline styles override the size class (e.g. `sb-dialog--lg`):

```razor
<SbDialog @bind-Open="isOpen" Title="Wide Modal" Width="900px" MaxWidth="80%">
    Content with custom width
</SbDialog>

<SbDialog @bind-Open="tallOpen" Title="Tall Modal" Height="80vh" MaxHeight="90vh">
    Content with custom height
</SbDialog>
```

### Custom Header

```razor
<SbDialog @bind-Open="isOpen">
    <Header>
        <SbStack Direction="SbStackDirection.Row" Align="SbAlign.Center" Gap="2">
            <SbIcon Name="warning" Color="SbColor.Warning" />
            <h2>Warning</h2>
        </SbStack>
    </Header>
    <ChildContent>
        <p>This action cannot be undone.</p>
    </ChildContent>
</SbDialog>
```

### Prevent Accidental Close

```razor
<SbDialog @bind-Open="isOpen" 
          Title="Important Form"
          CloseOnEscape="false"
          CloseOnBackdropClick="false"
          ShowCloseButton="false">
    <SbForm Model="@model" OnValidSubmit="Submit">
        <SbTextField Label="Name" @bind-Value="model.Name" />
        <Footer>
            <SbButton Type="submit">Submit</SbButton>
        </Footer>
    </SbForm>
</SbDialog>
```

### Custom Close Button Handler

Use `OnCloseButtonClick` for cleanup or confirmation-side effects that should run before the header X closes the dialog. The dialog still performs the standard close flow and fires `OpenChanged(false)` afterward:

```razor
<SbDialog @bind-Open="isOpen" OnCloseButtonClick="OnHeaderClose" Title="Create Item">
    <ChildContent>...</ChildContent>
</SbDialog>

@code {
    private bool isOpen;

    private Task OnHeaderClose()
    {
        // Optional cleanup before the dialog closes.
        return Task.CompletedTask;
    }
}
```

### Handle Close Reason

```razor
<SbDialog @bind-Open="isOpen" 
          Title="Form"
          OnClose="HandleClose">
    <p>Content</p>
</SbDialog>

@code {
    private void HandleClose(SbDialogCloseReason reason)
    {
        Console.WriteLine($"Dialog closed: {reason}");
        if (reason == SbDialogCloseReason.Backdrop)
        {
            // Show warning about unsaved changes
        }
    }
}
```

### Form Dialog

```razor
<SbDialog @bind-Open="isOpen" Title="Add User" Size="SbDialogSize.Md">
    <ChildContent>
        <SbStack Gap="4">
            <SbTextField Label="Name" @bind-Value="user.Name" />
            <SbTextField Label="Email" @bind-Value="user.Email" />
            <SbSelect Label="Role" @bind-Value="user.Role">
                <SbSelectOption Value="user">User</SbSelectOption>
                <SbSelectOption Value="admin">Admin</SbSelectOption>
            </SbSelect>
        </SbStack>
    </ChildContent>
    <Footer>
        <SbStack Direction="SbStackDirection.Row" Gap="2" Justify="SbJustify.End">
            <SbButton Variant="SbButtonVariant.Ghost" OnClick="() => isOpen = false">
                Cancel
            </SbButton>
            <SbButton OnClick="SaveUser">Add User</SbButton>
        </SbStack>
    </Footer>
</SbDialog>
```
