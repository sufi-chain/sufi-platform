# SbTabs

A tabbed navigation component for switching between different content panels.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| ActiveIndex | int | 0 | The active tab index (0-based) |
| ActiveTab | string? | null | The active tab ID (string-based selection) |
| LazyLoad | bool | false | Whether to lazy-load tab content |
| Vertical | bool | false | When true, tabs are vertical (side-by-side with content) |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| ActiveIndexChanged | EventCallback\<int\> | Fired when active tab index changes |
| ActiveTabChanged | EventCallback\<string?\> | Fired when active tab ID changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Tab content (SbTab components) |

## CSS Classes

- `sb-tabs` - Base class
- `sb-tabs--vertical` - Vertical layout (tabs on side, content adjacent). Applied when `Vertical="true"`.
- `sb-tabs__list` - Tab list container
- `sb-tabs__tab` - Individual tab button
- `sb-tabs__tab--active` - Active tab
- `sb-tabs__tab--disabled` - Disabled tab
- `sb-tabs__tab-icon` - Tab icon
- `sb-tabs__tab-label` - Tab label text
- `sb-tabs__tab-badge` - Tab badge
- `sb-tabs__content` - Content container

## Accessibility

- Uses `role="tablist"` and `role="tab"` for tab buttons
- Uses `role="tabpanel"` for content panels
- Supports `aria-selected`, `aria-controls`, and `hidden` attributes

## Examples

### Basic Usage

```razor
<SbTabs>
    <SbTab Label="General">
        <p>General settings content</p>
    </SbTab>
    <SbTab Label="Security">
        <p>Security settings content</p>
    </SbTab>
    <SbTab Label="Notifications">
        <p>Notification settings content</p>
    </SbTab>
</SbTabs>
```

### With Icons

```razor
<SbTabs>
    <SbTab Label="Home">
        <Icon><SbIcon Name="home" /></Icon>
        <ChildContent>Home content</ChildContent>
    </SbTab>
    <SbTab Label="Profile">
        <Icon><SbIcon Name="user" /></Icon>
        <ChildContent>Profile content</ChildContent>
    </SbTab>
    <SbTab Label="Settings">
        <Icon><SbIcon Name="settings" /></Icon>
        <ChildContent>Settings content</ChildContent>
    </SbTab>
</SbTabs>
```

### With Badges

```razor
<SbTabs>
    <SbTab Label="Inbox">
        <Badge><SbBadge Value="5" /></Badge>
        <ChildContent>Inbox messages</ChildContent>
    </SbTab>
    <SbTab Label="Sent">
        <ChildContent>Sent messages</ChildContent>
    </SbTab>
    <SbTab Label="Drafts">
        <Badge><SbBadge Value="2" /></Badge>
        <ChildContent>Draft messages</ChildContent>
    </SbTab>
</SbTabs>
```

### Controlled Tabs (Index-based)

```razor
<SbTabs @bind-ActiveIndex="activeTab">
    <SbTab Label="Tab 1">Content 1</SbTab>
    <SbTab Label="Tab 2">Content 2</SbTab>
    <SbTab Label="Tab 3">Content 3</SbTab>
</SbTabs>

@code {
    private int activeTab = 0;
}
```

### Controlled Tabs (ID-based)

```razor
<SbTabs @bind-ActiveTab="activeTabId">
    <SbTab Id="general" Label="General">General content</SbTab>
    <SbTab Id="advanced" Label="Advanced">Advanced content</SbTab>
    <SbTab Id="about" Label="About">About content</SbTab>
</SbTabs>

@code {
    private string? activeTabId = "general";
}
```

### Vertical Tabs (sidebar layout)

```razor
<SbTabs Vertical="true" @bind-ActiveTab="activeTabId">
    <SbTab Id="general" Label="General">General settings</SbTab>
    <SbTab Id="security" Label="Security">Security settings</SbTab>
</SbTabs>
```

### With Disabled Tab

```razor
<SbTabs>
    <SbTab Label="Active">Active tab content</SbTab>
    <SbTab Label="Another">Another tab content</SbTab>
    <SbTab Label="Disabled" Disabled="true">
        This content is inaccessible
    </SbTab>
</SbTabs>
```

### Lazy Loading

```razor
<SbTabs LazyLoad="true">
    <SbTab Label="Basic">
        <p>Basic info (loaded immediately)</p>
    </SbTab>
    <SbTab Label="Heavy Content">
        @* This content is only rendered when tab is first activated *@
        <ExpensiveComponent />
    </SbTab>
</SbTabs>
```

### Settings Page Layout

```razor
<SbCard>
    <SbTabs>
        <SbTab Label="Profile">
            <SbStack Gap="4" Class="p-4">
                <SbTextField Label="Name" @bind-Value="name" />
                <SbTextField Label="Email" @bind-Value="email" />
                <SbButton>Save Profile</SbButton>
            </SbStack>
        </SbTab>
        <SbTab Label="Password">
            <SbStack Gap="4" Class="p-4">
                <SbTextField Label="Current Password" Type="password" />
                <SbTextField Label="New Password" Type="password" />
                <SbTextField Label="Confirm Password" Type="password" />
                <SbButton>Update Password</SbButton>
            </SbStack>
        </SbTab>
        <SbTab Label="Preferences">
            <SbStack Gap="4" Class="p-4">
                <SbSwitch Label="Dark Mode" @bind-Value="darkMode" />
                <SbSwitch Label="Email Notifications" @bind-Value="emailNotifications" />
                <SbButton>Save Preferences</SbButton>
            </SbStack>
        </SbTab>
    </SbTabs>
</SbCard>
```
