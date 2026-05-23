# SbStatusPill

A compact indicator showing the status of an item with a colored dot and label. Useful for displaying states like Active, Pending, Error, or custom statuses in lists, tables, and cards.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Status | SbStatusType | Default | Status type affecting color (Default, Success, Warning, Error, Info) |
| Label | string? | null | Custom label text (overrides default status label) |
| ShowDot | bool | true | Whether to show the status dot indicator |
| Pulsing | bool | false | Whether to show a pulsing animation on the dot |
| Class | string? | null | Additional CSS classes |

## Status Types and Default Labels

| Status | Default Label | Color |
|--------|---------------|-------|
| Default | "Unknown" | Gray |
| Success | "Active" | Green |
| Warning | "Pending" | Yellow/Orange |
| Error | "Error" | Red |
| Info | "Info" | Blue |

## CSS Classes

- `sb-status-pill` - Base class
- `sb-status-pill--default` - Default status
- `sb-status-pill--success` - Success status
- `sb-status-pill--warning` - Warning status
- `sb-status-pill--error` - Error status
- `sb-status-pill--info` - Info status
- `sb-status-pill--pulsing` - Pulsing animation
- `sb-status-pill__dot` - Status dot indicator
- `sb-status-pill__label` - Label text

## Examples

### Basic Status Pills

```razor
<SbStatusPill Status="SbStatusType.Success" />
<SbStatusPill Status="SbStatusType.Warning" />
<SbStatusPill Status="SbStatusType.Error" />
<SbStatusPill Status="SbStatusType.Info" />
<SbStatusPill Status="SbStatusType.Default" />
```

### Custom Labels

```razor
<SbStatusPill Status="SbStatusType.Success" Label="Online" />
<SbStatusPill Status="SbStatusType.Warning" Label="Away" />
<SbStatusPill Status="SbStatusType.Error" Label="Offline" />
<SbStatusPill Status="SbStatusType.Info" Label="Busy" />
```

### Without Dot

```razor
<SbStatusPill Status="SbStatusType.Success" Label="Completed" ShowDot="false" />
<SbStatusPill Status="SbStatusType.Warning" Label="In Progress" ShowDot="false" />
```

### Pulsing Animation

```razor
<SbStatusPill Status="SbStatusType.Success" Label="Live" Pulsing="true" />
<SbStatusPill Status="SbStatusType.Warning" Label="Processing" Pulsing="true" />
```

### In a Table

```razor
<SbTable Items="@orders">
    <SbColumn Field="OrderId" Title="Order #" />
    <SbColumn Field="Customer" Title="Customer" />
    <SbColumn Field="Status" Title="Status">
        <CellTemplate Context="cell">
            <SbStatusPill Status="@GetStatusType(cell.Item.Status)" 
                          Label="@cell.Item.Status" />
        </CellTemplate>
    </SbColumn>
</SbTable>

@code {
    private SbStatusType GetStatusType(string status) => status switch
    {
        "Completed" => SbStatusType.Success,
        "Processing" => SbStatusType.Warning,
        "Cancelled" => SbStatusType.Error,
        "Shipped" => SbStatusType.Info,
        _ => SbStatusType.Default
    };
}
```

### User Status Indicator

```razor
<div class="user-item">
    <img src="@user.Avatar" alt="@user.Name" />
    <span>@user.Name</span>
    <SbStatusPill Status="@GetUserStatus(user)" 
                  Pulsing="@(user.IsOnline)" />
</div>

@code {
    private SbStatusType GetUserStatus(User user)
    {
        if (user.IsOnline) return SbStatusType.Success;
        if (user.LastSeen > DateTime.UtcNow.AddMinutes(-30)) return SbStatusType.Warning;
        return SbStatusType.Default;
    }
}
```

### Task Status

```razor
@foreach (var task in tasks)
{
    <div class="task-item">
        <span>@task.Title</span>
        <SbStatusPill Status="@GetTaskStatus(task.State)" 
                      Label="@task.State.ToString()" />
    </div>
}

@code {
    private SbStatusType GetTaskStatus(TaskState state) => state switch
    {
        TaskState.Done => SbStatusType.Success,
        TaskState.InProgress => SbStatusType.Warning,
        TaskState.Blocked => SbStatusType.Error,
        TaskState.Review => SbStatusType.Info,
        _ => SbStatusType.Default
    };
}
```

### Server Health Dashboard

```razor
<div class="server-list">
    @foreach (var server in servers)
    {
        <div class="server-item">
            <SbIcon Name="server" />
            <span>@server.Name</span>
            <SbStatusPill Status="@(server.IsHealthy ? SbStatusType.Success : SbStatusType.Error)"
                          Label="@(server.IsHealthy ? "Healthy" : "Down")"
                          Pulsing="@(!server.IsHealthy)" />
        </div>
    }
</div>
```
