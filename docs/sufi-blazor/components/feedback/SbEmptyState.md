# SbEmptyState

Displays a placeholder when there is no content to show, such as empty lists, search results with no matches, or first-time user experiences. Provides a visual cue with optional icon, title, description, and action buttons.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| IconText | string? | "📭" | Icon text (emoji or symbol) shown when Icon is not provided |
| Title | string? | "No items found" | Main heading text |
| Description | string? | null | Secondary descriptive text |
| Class | string? | null | Additional CSS classes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment? | Custom icon content (takes precedence over IconText) |
| ChildContent | RenderFragment? | Additional content below the description |
| Actions | RenderFragment? | Action buttons (e.g., "Create New", "Try Again") |

### Template Usage

#### With Custom Icon

```razor
<SbEmptyState Title="No Messages">
    <Icon>
        <SbIcon Name="inbox" Size="SbSize.Xl" />
    </Icon>
</SbEmptyState>
```

#### With Actions

```razor
<SbEmptyState Title="No Projects" Description="Get started by creating your first project.">
    <Actions>
        <SbButton>Create Project</SbButton>
    </Actions>
</SbEmptyState>
```

#### With Custom Content

```razor
<SbEmptyState Title="Search Results">
    <ChildContent>
        <p>Try adjusting your search terms or filters.</p>
    </ChildContent>
</SbEmptyState>
```

## CSS Classes

- `sb-empty-state` - Base class
- `sb-empty-state__icon` - Icon container
- `sb-empty-state__icon-text` - Icon text element
- `sb-empty-state__title` - Title heading
- `sb-empty-state__description` - Description paragraph
- `sb-empty-state__content` - Custom content wrapper
- `sb-empty-state__actions` - Actions container

## Examples

### Basic Empty State

```razor
<SbEmptyState />
```

### With Custom Title and Description

```razor
<SbEmptyState 
    Title="No notifications"
    Description="You're all caught up! Check back later for new updates." />
```

### With Custom Icon Text

```razor
<SbEmptyState 
    IconText="🔍"
    Title="No results found"
    Description="Try different search terms." />
```

### With Icon Component

```razor
<SbEmptyState Title="Empty Cart" Description="Your shopping cart is empty.">
    <Icon>
        <SbIcon Name="shopping-cart" Size="SbSize.Xl" Class="text-muted" />
    </Icon>
    <Actions>
        <SbButton Variant="SbButtonVariant.Primary">Start Shopping</SbButton>
    </Actions>
</SbEmptyState>
```

### First-Time User Experience

```razor
<SbEmptyState 
    IconText="🚀"
    Title="Welcome to Your Dashboard"
    Description="This is where your projects will appear once you create them.">
    <Actions>
        <SbButton Variant="SbButtonVariant.Primary" OnClick="CreateProject">
            Create Your First Project
        </SbButton>
        <SbButton Variant="SbButtonVariant.Ghost" OnClick="LearnMore">
            Learn More
        </SbButton>
    </Actions>
</SbEmptyState>
```

### Error State

```razor
<SbEmptyState 
    IconText="⚠️"
    Title="Unable to load data"
    Description="Something went wrong while fetching your data.">
    <Actions>
        <SbButton OnClick="Retry">Try Again</SbButton>
    </Actions>
</SbEmptyState>
```

### Search No Results

```razor
<SbEmptyState 
    IconText="🔍"
    Title="No matches found"
    Description="We couldn't find anything matching your search.">
    <ChildContent>
        <p><strong>Suggestions:</strong></p>
        <ul>
            <li>Check your spelling</li>
            <li>Try more general terms</li>
            <li>Try different keywords</li>
        </ul>
    </ChildContent>
    <Actions>
        <SbButton Variant="SbButtonVariant.Ghost" OnClick="ClearSearch">
            Clear Search
        </SbButton>
    </Actions>
</SbEmptyState>
```

### In a Data Grid

```razor
<SbDataGrid Items="@filteredItems">
    <EmptyTemplate>
        <SbEmptyState 
            IconText="📋"
            Title="No data available"
            Description="Add some items to see them here." />
    </EmptyTemplate>
    <SbColumn Field="Name" Title="Name" />
    <SbColumn Field="Status" Title="Status" />
</SbDataGrid>
```
