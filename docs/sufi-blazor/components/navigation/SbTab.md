# SbTab

A single tab panel component used within SbTabs.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Id | string? | null | Unique identifier for ID-based tab selection |
| Label | string | "" | Tab label text displayed in the tab button |
| Disabled | bool | false | Whether the tab is disabled |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Icon content displayed in the tab button |
| Badge | RenderFragment | Badge content displayed in the tab button |
| ChildContent | RenderFragment | Tab panel content |

## Cascading Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Parent | SbTabs | Parent tabs container |

## CSS Classes

- `sb-tab-panel` - Base class for panel
- `sb-tab-panel--active` - When panel is active
- `sb-tabs__panel` - Tabs panel container class

## Examples

### Basic Usage

```razor
<SbTabs>
    <SbTab Label="First Tab">
        <p>Content for the first tab</p>
    </SbTab>
    <SbTab Label="Second Tab">
        <p>Content for the second tab</p>
    </SbTab>
</SbTabs>
```

### With ID for Controlled Selection

```razor
<SbTabs @bind-ActiveTab="activeTabId">
    <SbTab Id="overview" Label="Overview">
        Overview content
    </SbTab>
    <SbTab Id="details" Label="Details">
        Details content
    </SbTab>
    <SbTab Id="history" Label="History">
        History content
    </SbTab>
</SbTabs>

@code {
    private string? activeTabId = "overview";
    
    private void GoToHistory() => activeTabId = "history";
}
```

### With Icon

```razor
<SbTab Label="Settings">
    <Icon>
        <SbIcon Name="settings" Size="SbSize.Sm" />
    </Icon>
    <ChildContent>
        <p>Settings content goes here</p>
    </ChildContent>
</SbTab>
```

### With Badge

```razor
<SbTab Label="Messages">
    <Badge>
        <SbBadge Value="3" Color="SbColor.Primary" />
    </Badge>
    <ChildContent>
        <p>You have 3 new messages</p>
    </ChildContent>
</SbTab>
```

### Icon and Badge Combined

```razor
<SbTab Label="Notifications">
    <Icon>
        <SbIcon Name="bell" Size="SbSize.Sm" />
    </Icon>
    <Badge>
        <SbBadge Value="12" Color="SbColor.Danger" />
    </Badge>
    <ChildContent>
        <ul>
            @foreach (var notification in notifications)
            {
                <li>@notification.Message</li>
            }
        </ul>
    </ChildContent>
</SbTab>
```

### Disabled Tab

```razor
<SbTabs>
    <SbTab Label="Available">
        This tab is available
    </SbTab>
    <SbTab Label="Premium" Disabled="@(!isPremiumUser)">
        Premium content - upgrade to access
    </SbTab>
</SbTabs>

@code {
    private bool isPremiumUser = false;
}
```

### Complex Tab Content

```razor
<SbTabs>
    <SbTab Label="Users">
        <SbStack Gap="4">
            <SbStack Direction="SbStackDirection.Row" Justify="SbJustify.SpaceBetween">
                <SbHeading Level="3">Users List</SbHeading>
                <SbButton>Add User</SbButton>
            </SbStack>
            <SbDataGrid Items="@users">
                <SbColumn Field="Name" Title="Name" />
                <SbColumn Field="Email" Title="Email" />
                <SbColumn Field="Role" Title="Role" />
            </SbDataGrid>
        </SbStack>
    </SbTab>
    <SbTab Label="Roles">
        <SbStack Gap="4">
            <SbHeading Level="3">Role Management</SbHeading>
            @foreach (var role in roles)
            {
                <SbCard>
                    <strong>@role.Name</strong>
                    <p>@role.Description</p>
                </SbCard>
            }
        </SbStack>
    </SbTab>
</SbTabs>
```
