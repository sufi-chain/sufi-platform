# SbConfirmDialog

A specialized dialog for confirmation actions with predefined variants for different use cases.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string | "Confirm" | Dialog title |
| Message | string? | null | Dialog message text |
| Variant | SbConfirmDialogVariant | Default | Dialog variant (affects icon and button color) |
| ConfirmText | string | "Confirm" | Text for confirm button |
| CancelText | string | "Cancel" | Text for cancel button |
| ShowCancelButton | bool | true | Whether to show cancel button |
| CloseOnOverlayClick | bool | true | Close when clicking outside |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnConfirm | EventCallback | Fired when confirmed |
| OnCancel | EventCallback | Fired when cancelled |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Custom icon content |
| ChildContent | RenderFragment | Additional content below message |

## Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| ShowAsync() | Task\<bool\> | Show dialog and await result |
| Show() | void | Show dialog (use events for result) |
| Close() | void | Close the dialog |

## SbConfirmDialogVariant Enum

```csharp
public enum SbConfirmDialogVariant
{
    Default,  // Question icon, primary button
    Danger,   // Warning icon, danger button
    Warning,  // Warning icon, warning button
    Info      // Info icon, info button
}
```

## CSS Classes

- `sb-confirm-dialog-overlay` - Backdrop overlay
- `sb-confirm-dialog` - Dialog container
- `sb-confirm-dialog--{variant}` - Variant styles
- `sb-confirm-dialog__icon` - Icon container
- `sb-confirm-dialog__content` - Content area
- `sb-confirm-dialog__title` - Title text
- `sb-confirm-dialog__message` - Message text
- `sb-confirm-dialog__body` - Custom content area
- `sb-confirm-dialog__actions` - Action buttons

## Examples

### Basic Usage

```razor
<SbButton OnClick="() => confirmDialog.Show()">Delete Item</SbButton>

<SbConfirmDialog @ref="confirmDialog"
                 Title="Delete Item"
                 Message="Are you sure you want to delete this item?"
                 OnConfirm="DeleteItem"
                 OnCancel="() => { }" />

@code {
    private SbConfirmDialog confirmDialog;
    
    private async Task DeleteItem()
    {
        await itemService.DeleteAsync(item.Id);
    }
}
```

### Async Usage

```razor
<SbButton OnClick="HandleDelete">Delete</SbButton>

<SbConfirmDialog @ref="confirmDialog"
                 Title="Delete Item"
                 Message="This action cannot be undone."
                 Variant="SbConfirmDialogVariant.Danger"
                 ConfirmText="Delete"
                 CancelText="Keep" />

@code {
    private SbConfirmDialog confirmDialog;
    
    private async Task HandleDelete()
    {
        var confirmed = await confirmDialog.ShowAsync();
        if (confirmed)
        {
            await DeleteItemAsync();
        }
    }
}
```

### Danger Variant

```razor
<SbConfirmDialog @ref="deleteDialog"
                 Title="Delete Account"
                 Message="This will permanently delete your account and all associated data. This action cannot be undone."
                 Variant="SbConfirmDialogVariant.Danger"
                 ConfirmText="Delete Account"
                 CancelText="Cancel"
                 OnConfirm="DeleteAccount" />
```

### Warning Variant

```razor
<SbConfirmDialog @ref="warningDialog"
                 Title="Unsaved Changes"
                 Message="You have unsaved changes. Are you sure you want to leave?"
                 Variant="SbConfirmDialogVariant.Warning"
                 ConfirmText="Leave"
                 CancelText="Stay"
                 OnConfirm="NavigateAway" />
```

### Info Variant

```razor
<SbConfirmDialog @ref="infoDialog"
                 Title="New Version Available"
                 Message="A new version is available. Would you like to update now?"
                 Variant="SbConfirmDialogVariant.Info"
                 ConfirmText="Update Now"
                 CancelText="Later"
                 OnConfirm="UpdateApp" />
```

### Custom Icon

```razor
<SbConfirmDialog @ref="logoutDialog"
                 Title="Sign Out"
                 Message="Are you sure you want to sign out?"
                 ConfirmText="Sign Out"
                 OnConfirm="SignOut">
    <Icon>
        <SbIcon Name="log-out" Size="SbSize.Lg" />
    </Icon>
</SbConfirmDialog>
```

### With Additional Content

```razor
<SbConfirmDialog @ref="deleteUsersDialog"
                 Title="Delete Users"
                 Message="The following users will be deleted:"
                 Variant="SbConfirmDialogVariant.Danger"
                 ConfirmText="Delete All"
                 OnConfirm="DeleteSelectedUsers">
    <ChildContent>
        <ul class="mt-2">
            @foreach (var user in selectedUsers)
            {
                <li>@user.Name (@user.Email)</li>
            }
        </ul>
    </ChildContent>
</SbConfirmDialog>
```

### Without Cancel Button

```razor
<SbConfirmDialog @ref="acknowledgeDialog"
                 Title="Important Notice"
                 Message="Please read and acknowledge the updated terms of service."
                 ShowCancelButton="false"
                 ConfirmText="I Understand"
                 Variant="SbConfirmDialogVariant.Info"
                 OnConfirm="AcknowledgeTerms" />
```

### Prevent Overlay Close

```razor
<SbConfirmDialog @ref="criticalDialog"
                 Title="Confirm Critical Action"
                 Message="This action requires explicit confirmation."
                 CloseOnOverlayClick="false"
                 Variant="SbConfirmDialogVariant.Danger"
                 ConfirmText="I Confirm"
                 OnConfirm="ExecuteCriticalAction" />
```

### Bulk Delete Confirmation

```razor
<SbButton Disabled="@(selectedItems.Count == 0)"
          Color="SbColor.Danger"
          OnClick="() => bulkDeleteDialog.Show()">
    Delete Selected (@selectedItems.Count)
</SbButton>

<SbConfirmDialog @ref="bulkDeleteDialog"
                 Title="Delete Items"
                 Variant="SbConfirmDialogVariant.Danger"
                 ConfirmText="Delete @selectedItems.Count Items"
                 OnConfirm="BulkDelete">
    <ChildContent>
        <SbText>
            Are you sure you want to delete @selectedItems.Count items?
            This action cannot be undone.
        </SbText>
    </ChildContent>
</SbConfirmDialog>
```
