# SbAccordionItem

A single collapsible item used within SbAccordion.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Title | string | "" | Item title displayed in the header |
| Disabled | bool | false | Whether the item is disabled |
| DefaultExpanded | bool | false | Whether the item is initially expanded |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Icon | RenderFragment | Icon displayed before the title |
| Subtitle | RenderFragment | Subtitle displayed below the title (stacked vertically in the header) |
| ChildContent | RenderFragment | Collapsible content |

## Cascading Parameters

| Parameter | Type | Description |
|-----------|------|-------------|
| Parent | SbAccordion | Parent accordion container |

## CSS Classes

- `sb-accordion-item` - Base class
- `sb-accordion-item--expanded` - When item is expanded
- `sb-accordion-item--disabled` - When item is disabled
- `sb-accordion-item__header` - Header button
- `sb-accordion-item__header-start` - Header start section
- `sb-accordion-item__icon` - Icon container
- `sb-accordion-item__title` - Title text
- `sb-accordion-item__subtitle` - Subtitle text
- `sb-accordion-item__chevron` - Expand/collapse indicator
- `sb-accordion-item__content` - Content wrapper
- `sb-accordion-item__content--expanded` - Expanded content
- `sb-accordion-item__body` - Inner content container

## Accessibility

- Uses `aria-expanded` to indicate expansion state
- Uses `aria-controls` to link button to content
- Content uses `hidden` attribute when collapsed

## Examples

### Basic Usage

```razor
<SbAccordion>
    <SbAccordionItem Title="Click to expand">
        <p>This content is revealed when expanded.</p>
    </SbAccordionItem>
</SbAccordion>
```

### Initially Expanded

```razor
<SbAccordion>
    <SbAccordionItem Title="Important Information" DefaultExpanded="true">
        <p>This content is visible by default.</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Additional Details">
        <p>This is collapsed by default.</p>
    </SbAccordionItem>
</SbAccordion>
```

### With Icon

```razor
<SbAccordionItem Title="Account Settings">
    <Icon>
        <SbIcon Name="user" Size="SbSize.Sm" />
    </Icon>
    <ChildContent>
        <SbTextField Label="Display Name" />
        <SbTextField Label="Email" />
    </ChildContent>
</SbAccordionItem>
```

### With Subtitle

```razor
<SbAccordionItem Title="Payment Method">
    <Subtitle>
        <SbText Size="sm" Color="muted">Visa ending in 4242</SbText>
    </Subtitle>
    <ChildContent>
        <SbButton>Update Payment Method</SbButton>
    </ChildContent>
</SbAccordionItem>
```

### Disabled Item

```razor
<SbAccordion>
    <SbAccordionItem Title="Free Plan">
        <p>Basic features included.</p>
    </SbAccordionItem>
    <SbAccordionItem Title="Premium Features" Disabled="@(!isPremium)">
        <Subtitle>
            <SbText Size="sm" Color="muted">Requires premium subscription</SbText>
        </Subtitle>
        <ChildContent>
            <p>Advanced features content...</p>
        </ChildContent>
    </SbAccordionItem>
</SbAccordion>

@code {
    private bool isPremium = false;
}
```

### Complex Content

```razor
<SbAccordionItem Title="Order Details">
    <Icon><SbIcon Name="shopping-cart" /></Icon>
    <Subtitle>
        <SbStack Direction="SbStackDirection.Row" Gap="2">
            <SbBadge>3 items</SbBadge>
            <SbText Size="sm">$129.99</SbText>
        </SbStack>
    </Subtitle>
    <ChildContent>
        <SbTable>
            <thead>
                <tr>
                    <th>Item</th>
                    <th>Qty</th>
                    <th>Price</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var item in orderItems)
                {
                    <tr>
                        <td>@item.Name</td>
                        <td>@item.Quantity</td>
                        <td>@item.Price.ToString("C")</td>
                    </tr>
                }
            </tbody>
        </SbTable>
    </ChildContent>
</SbAccordionItem>
```

### Settings Panel

```razor
<SbAccordion Multiple="true">
    <SbAccordionItem Title="General" DefaultExpanded="true">
        <Icon><SbIcon Name="settings" /></Icon>
        <ChildContent>
            <SbStack Gap="3">
                <SbTextField Label="Site Name" @bind-Value="siteName" />
                <SbTextField Label="Site URL" @bind-Value="siteUrl" />
                <SbSelect Label="Language" @bind-Value="language">
                    <SbSelectOption Value="en">English</SbSelectOption>
                    <SbSelectOption Value="es">Spanish</SbSelectOption>
                </SbSelect>
            </SbStack>
        </ChildContent>
    </SbAccordionItem>
    
    <SbAccordionItem Title="Appearance">
        <Icon><SbIcon Name="palette" /></Icon>
        <ChildContent>
            <SbStack Gap="3">
                <SbColorPicker Label="Primary Color" @bind-Value="primaryColor" />
                <SbSwitch Label="Dark Mode" @bind-Value="darkMode" />
            </SbStack>
        </ChildContent>
    </SbAccordionItem>
    
    <SbAccordionItem Title="Advanced">
        <Icon><SbIcon Name="code" /></Icon>
        <Subtitle>
            <SbText Size="xs" Color="warning">For developers only</SbText>
        </Subtitle>
        <ChildContent>
            <SbTextArea Label="Custom CSS" @bind-Value="customCss" Rows="10" />
        </ChildContent>
    </SbAccordionItem>
</SbAccordion>
```
