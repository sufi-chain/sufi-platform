# SbAccordion

A collapsible accordion component for organizing content into expandable sections.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Multiple | bool | false | Whether multiple items can be expanded simultaneously |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnExpandedChanged | EventCallback\<HashSet\<SbAccordionItem\>\> | Fired when expanded items change |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Accordion items (SbAccordionItem components) |

## CSS Classes

- `sb-accordion` - Base class

When an item is expanded, its header uses a distinct background (`sb-accordion-item--expanded .sb-accordion-item__header`) so it is visually separated from the content area.

## Examples

### Basic Usage

```razor
<SbAccordion>
    <SbAccordionItem Title="Section 1">
        <p>Content for section 1</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Section 2">
        <p>Content for section 2</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Section 3">
        <p>Content for section 3</p>
    </SbAccordionItem>
</SbAccordion>
```

### Multiple Expanded

```razor
<SbAccordion Multiple="true">
    <SbAccordionItem Title="General Information" DefaultExpanded="true">
        <p>General info content...</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Technical Details" DefaultExpanded="true">
        <p>Technical details content...</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Support">
        <p>Support content...</p>
    </SbAccordionItem>
</SbAccordion>
```

### FAQ Style

```razor
<SbAccordion>
    @foreach (var faq in faqs)
    {
        <SbAccordionItem Title="@faq.Question">
            <p>@faq.Answer</p>
        </SbAccordionItem>
    }
</SbAccordion>

@code {
    private List<FAQ> faqs = new()
    {
        new("What is your return policy?", "We offer 30-day returns..."),
        new("How do I track my order?", "You can track your order..."),
        new("Do you ship internationally?", "Yes, we ship to over 100 countries...")
    };
}
```

### Default Expanded

Use `DefaultExpanded="true"` on an item to have it expanded when the page loads. When `Multiple` is false (default), only one item can be expanded at a time.

```razor
<SbAccordion>
    <SbAccordionItem Title="Introduction" DefaultExpanded="true">
        <p>This section is expanded by default when the page loads.</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Details">
        <p>This section starts collapsed.</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Conclusion">
        <p>Final section.</p>
    </SbAccordionItem>
</SbAccordion>
```

### With Subtitles

Use the `Subtitle` slot on `SbAccordionItem` to show a short description below the title. Title and subtitle are stacked vertically in the header.

```razor
<SbAccordion>
    <SbAccordionItem Title="Profile">
        <Subtitle>
            <SbText Size="SbSize.Sm" Color="SbColor.Muted">Manage your personal information</SbText>
        </Subtitle>
        <ChildContent>
            <SbText>Update your profile details.</SbText>
        </ChildContent>
    </SbAccordionItem>
    <SbAccordionItem Title="Notifications">
        <Subtitle>
            <SbText Size="SbSize.Sm" Color="SbColor.Muted">Configure notification preferences</SbText>
        </Subtitle>
        <ChildContent>
            <SbText>Choose which notifications you want to receive.</SbText>
        </ChildContent>
    </SbAccordionItem>
</SbAccordion>
```

### With Icons

```razor
<SbAccordion>
    <SbAccordionItem Title="Account Settings">
        <Icon><SbIcon Name="user" /></Icon>
        <ChildContent>
            <SbStack Gap="3">
                <SbTextField Label="Username" />
                <SbTextField Label="Email" />
            </SbStack>
        </ChildContent>
    </SbAccordionItem>
    <SbAccordionItem Title="Security">
        <Icon><SbIcon Name="shield" /></Icon>
        <ChildContent>
            <SbStack Gap="3">
                <SbTextField Label="Current Password" Type="password" />
                <SbTextField Label="New Password" Type="password" />
            </SbStack>
        </ChildContent>
    </SbAccordionItem>
    <SbAccordionItem Title="Notifications">
        <Icon><SbIcon Name="bell" /></Icon>
        <ChildContent>
            <SbSwitch Label="Email notifications" />
            <SbSwitch Label="Push notifications" />
        </ChildContent>
    </SbAccordionItem>
</SbAccordion>
```

### Product Specifications

```razor
<SbAccordion>
    <SbAccordionItem Title="Product Description" DefaultExpanded="true">
        <p>@product.Description</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Specifications">
        <SbTable>
            <thead>
                <tr>
                    <th>Property</th>
                    <th>Value</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var spec in product.Specifications)
                {
                    <tr>
                        <td>@spec.Key</td>
                        <td>@spec.Value</td>
                    </tr>
                }
            </tbody>
        </SbTable>
    </SbAccordionItem>
    <SbAccordionItem Title="Reviews (@product.ReviewCount)">
        @foreach (var review in product.Reviews)
        {
            <SbCard>
                <strong>@review.Author</strong> - @review.Rating/5
                <p>@review.Comment</p>
            </SbCard>
        }
    </SbAccordionItem>
</SbAccordion>
```

### Tracking Expanded State

```razor
<SbAccordion OnExpandedChanged="HandleExpansionChange">
    <SbAccordionItem Title="Step 1: Basic Info">
        <SbTextField Label="Name" @bind-Value="name" />
    </SbAccordionItem>
    <SbAccordionItem Title="Step 2: Details">
        <SbTextArea Label="Description" @bind-Value="description" />
    </SbAccordionItem>
</SbAccordion>

<SbText>Expanded sections: @expandedCount</SbText>

@code {
    private int expandedCount = 0;
    
    private void HandleExpansionChange(HashSet<SbAccordionItem> expanded)
    {
        expandedCount = expanded.Count;
    }
}
```
