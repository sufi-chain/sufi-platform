# SbNavMenu

A navigation menu container component for organizing navigation items.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Collapsed | bool | false | Whether the menu is collapsed |
| AriaLabel | string | "Navigation" | Accessible label for the navigation |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| CollapsedChanged | EventCallback\<bool\> | Fired when collapsed state changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Navigation content. Use SbNavItem components, or any markup (e.g. ul/li with links). |

## Methods

| Method | Return Type | Description |
|--------|-------------|-------------|
| ToggleAsync() | Task | Toggle the collapsed state |

## CSS Classes

- `sb-nav-menu` - Base class
- `sb-nav-menu--collapsed` - When menu is collapsed

## Examples

### Basic Usage

```razor
<SbNavMenu>
    <SbNavItem Href="/" Text="Home">
        <Icon><SbIcon Name="home" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/about" Text="About">
        <Icon><SbIcon Name="info" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/contact" Text="Contact">
        <Icon><SbIcon Name="mail" /></Icon>
    </SbNavItem>
</SbNavMenu>
```

### Inside a layout sidebar

Use `SbNavMenu` inside the KomTheme's sidebar (e.g. `KomSidebar` or `KomExpandPanel`):

```razor
<SbNavMenu>
    <SbNavItem Href="/" Text="Dashboard">
        <Icon><SbIcon Name="layout" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/users" Text="Users">
        <Icon><SbIcon Name="users" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/settings" Text="Settings">
        <Icon><SbIcon Name="settings" /></Icon>
    </SbNavItem>
</SbNavMenu>
```

### Collapsible Menu

```razor
<SbNavMenu @bind-Collapsed="isCollapsed">
    <SbNavItem Href="/" Text="Home">
        <Icon><SbIcon Name="home" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/projects" Text="Projects">
        <Icon><SbIcon Name="folder" /></Icon>
    </SbNavItem>
</SbNavMenu>

<SbButton OnClick="() => isCollapsed = !isCollapsed">
    Toggle Menu
</SbButton>

@code {
    private bool isCollapsed = false;
}
```

### With Grouped Sections

```razor
<SbNavMenu AriaLabel="Main Navigation">
    <SbText Size="xs" Color="muted" Class="nav-section-label">Main</SbText>
    <SbNavItem Href="/" Text="Dashboard">
        <Icon><SbIcon Name="layout" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/analytics" Text="Analytics">
        <Icon><SbIcon Name="bar-chart" /></Icon>
    </SbNavItem>
    
    <SbSpacer Size="SbSpacerSize.Small" />
    
    <SbText Size="xs" Color="muted" Class="nav-section-label">Content</SbText>
    <SbNavItem Href="/posts" Text="Posts">
        <Icon><SbIcon Name="file-text" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/media" Text="Media">
        <Icon><SbIcon Name="image" /></Icon>
    </SbNavItem>
    
    <SbSpacer Size="SbSpacerSize.Small" />
    
    <SbText Size="xs" Color="muted" Class="nav-section-label">System</SbText>
    <SbNavItem Href="/users" Text="Users">
        <Icon><SbIcon Name="users" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/settings" Text="Settings">
        <Icon><SbIcon Name="settings" /></Icon>
    </SbNavItem>
</SbNavMenu>
```

### With Badges

```razor
<SbNavMenu>
    <SbNavItem Href="/inbox" Text="Inbox">
        <Icon><SbIcon Name="inbox" /></Icon>
        <Badge><SbBadge Value="5" Color="SbColor.Primary" /></Badge>
    </SbNavItem>
    <SbNavItem Href="/notifications" Text="Notifications">
        <Icon><SbIcon Name="bell" /></Icon>
        <Badge><SbBadge Value="12" Color="SbColor.Danger" /></Badge>
    </SbNavItem>
    <SbNavItem Href="/tasks" Text="Tasks">
        <Icon><SbIcon Name="check-square" /></Icon>
        <Badge><SbBadge Value="3" Color="SbColor.Warning" /></Badge>
    </SbNavItem>
</SbNavMenu>
```

### Admin Panel Navigation

```razor
<SbNavMenu AriaLabel="Admin Navigation">
    @* Main navigation items *@
    <SbNavItem Href="/admin" Text="Dashboard" Match="NavLinkMatch.All">
        <Icon><SbIcon Name="layout" /></Icon>
    </SbNavItem>
    
    @* Content management *@
    <SbAccordion>
        <SbAccordionItem Title="Content">
            <Icon><SbIcon Name="file-text" /></Icon>
            <ChildContent>
                <SbNavItem Href="/admin/posts" Text="Posts" />
                <SbNavItem Href="/admin/pages" Text="Pages" />
                <SbNavItem Href="/admin/media" Text="Media" />
            </ChildContent>
        </SbAccordionItem>
        <SbAccordionItem Title="Users">
            <Icon><SbIcon Name="users" /></Icon>
            <ChildContent>
                <SbNavItem Href="/admin/users" Text="All Users" />
                <SbNavItem Href="/admin/roles" Text="Roles" />
                <SbNavItem Href="/admin/permissions" Text="Permissions" />
            </ChildContent>
        </SbAccordionItem>
    </SbAccordion>
    
    <SbSpacer Grow="true" />
    
    @* Bottom items *@
    <SbNavItem Href="/admin/settings" Text="Settings">
        <Icon><SbIcon Name="settings" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/admin/help" Text="Help">
        <Icon><SbIcon Name="help-circle" /></Icon>
    </SbNavItem>
</SbNavMenu>
```
