# SbNavItem

A navigation link item component with active state detection.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Href | string? | null | Navigation URL |
| Text | string? | null | Navigation text |
| Match | NavLinkMatch | Prefix | URL matching behavior (All or Prefix) |
| Disabled | bool | false | Whether the item is disabled |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Icon content displayed before text |
| Badge | RenderFragment | Badge content displayed after text |

## CSS Classes

- `sb-nav-item` - Base class
- `sb-nav-item--active` - When the current URL matches
- `sb-nav-item--disabled` - When item is disabled
- `sb-nav-item__icon` - Icon container
- `sb-nav-item__text` - Text container
- `sb-nav-item__badge` - Badge container

## URL Matching

- **Prefix** (default): Item is active if current URL starts with `Href`
- **All**: Item is active only if current URL exactly matches `Href`

## Examples

### Basic Usage

```razor
<SbNavItem Href="/dashboard" Text="Dashboard" />
```

### With Icon

```razor
<SbNavItem Href="/users" Text="Users">
    <Icon>
        <SbIcon Name="users" Size="SbSize.Sm" />
    </Icon>
</SbNavItem>
```

### With Badge

```razor
<SbNavItem Href="/inbox" Text="Inbox">
    <Icon><SbIcon Name="inbox" /></Icon>
    <Badge><SbBadge Value="5" Color="SbColor.Primary" /></Badge>
</SbNavItem>
```

### Exact URL Match

```razor
@* Only active when URL is exactly "/" *@
<SbNavItem Href="/" Text="Home" Match="NavLinkMatch.All">
    <Icon><SbIcon Name="home" /></Icon>
</SbNavItem>

@* Active when URL starts with "/users" (e.g., /users, /users/123) *@
<SbNavItem Href="/users" Text="Users" Match="NavLinkMatch.Prefix">
    <Icon><SbIcon Name="users" /></Icon>
</SbNavItem>
```

### Disabled Item

```razor
<SbNavItem Href="/premium" Text="Premium Features" Disabled="@(!isPremiumUser)">
    <Icon><SbIcon Name="star" /></Icon>
</SbNavItem>

@code {
    private bool isPremiumUser = false;
}
```

### Navigation Menu Example

```razor
<SbNavMenu>
    <SbNavItem Href="/" Text="Dashboard" Match="NavLinkMatch.All">
        <Icon><SbIcon Name="layout" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/projects" Text="Projects">
        <Icon><SbIcon Name="folder" /></Icon>
        <Badge><SbBadge Value="3" /></Badge>
    </SbNavItem>
    <SbNavItem Href="/team" Text="Team">
        <Icon><SbIcon Name="users" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/reports" Text="Reports">
        <Icon><SbIcon Name="bar-chart" /></Icon>
    </SbNavItem>
    
    <SbSpacer Grow="true" />
    
    <SbNavItem Href="/settings" Text="Settings">
        <Icon><SbIcon Name="settings" /></Icon>
    </SbNavItem>
    <SbNavItem Href="/help" Text="Help & Support">
        <Icon><SbIcon Name="help-circle" /></Icon>
    </SbNavItem>
</SbNavMenu>
```

### Icon-Only (for collapsed sidebar)

```razor
<SbNavItem Href="/home">
    <Icon>
        <SbTooltip Text="Home" Placement="right">
            <SbIcon Name="home" />
        </SbTooltip>
    </Icon>
</SbNavItem>
```

### With Custom Styling

```razor
<SbNavItem Href="/important" 
           Text="Important" 
           Class="nav-item-highlight"
           Style="font-weight: bold;">
    <Icon><SbIcon Name="alert-circle" Color="SbColor.Warning" /></Icon>
</SbNavItem>
```

### Dynamic Navigation

```razor
@foreach (var item in navigationItems)
{
    <SbNavItem Href="@item.Url" 
               Text="@item.Title"
               Disabled="@(!item.HasAccess)">
        <Icon><SbIcon Name="@item.Icon" /></Icon>
        @if (item.BadgeCount > 0)
        {
            <Badge><SbBadge Value="@item.BadgeCount" /></Badge>
        }
    </SbNavItem>
}

@code {
    private List<NavItem> navigationItems = new()
    {
        new() { Url = "/", Title = "Home", Icon = "home", HasAccess = true },
        new() { Url = "/users", Title = "Users", Icon = "users", HasAccess = true, BadgeCount = 5 },
        new() { Url = "/admin", Title = "Admin", Icon = "shield", HasAccess = isAdmin }
    };
}
```
