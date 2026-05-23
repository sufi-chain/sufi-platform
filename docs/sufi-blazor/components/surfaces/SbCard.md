# SbCard

A versatile container component for grouping related content with elevation, borders, and optional header/footer sections. Cards can be static, clickable, or linkable.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Elevation | int | 1 | Shadow depth level (0-5) |
| Outlined | bool | false | Shows border instead of shadow |
| Href | string? | null | Makes card a link to this URL |
| NoPadding | bool | false | Removes default padding |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OnClick | EventCallback<MouseEventArgs> | Fired when card is clicked (makes card a button) |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Header | RenderFragment? | Content for the card header section |
| ChildContent | RenderFragment? | Main body content of the card |
| Footer | RenderFragment? | Content for the card footer section |

### Template Usage

#### Basic

```razor
<SbCard>
    <p>Card body content</p>
</SbCard>
```

#### With Header and Footer

```razor
<SbCard>
    <Header>
        <SbHeading Level="3">Card Title</SbHeading>
    </Header>
    <ChildContent>
        <p>Main content goes here.</p>
    </ChildContent>
    <Footer>
        <SbButton>Action</SbButton>
    </Footer>
</SbCard>
```

## CSS Classes

- `sb-card` - Base class
- `sb-card--elevation-0` through `sb-card--elevation-5` - Shadow levels
- `sb-card--outlined` - Bordered variant
- `sb-card--clickable` - Applied when card is interactive
- `sb-card--no-padding` - Removes padding
- `sb-card__header` - Header section
- `sb-card__body` - Body section
- `sb-card__footer` - Footer section

## Rendered Element

- `<div>` - Default static card
- `<a>` - When `Href` is provided
- `<button>` - When `OnClick` is provided

## Examples

### Basic Card

```razor
<SbCard>
    <p>This is a simple card with default settings.</p>
</SbCard>
```

### With Header and Footer

```razor
<SbCard>
    <Header>
        <SbHeading Level="4">User Profile</SbHeading>
    </Header>
    <ChildContent>
        <p>Name: John Doe</p>
        <p>Email: john@example.com</p>
    </ChildContent>
    <Footer>
        <SbButton Variant="SbButtonVariant.Ghost">Edit</SbButton>
        <SbButton>Save</SbButton>
    </Footer>
</SbCard>
```

### Elevation Levels

```razor
<SbCard Elevation="0">No shadow</SbCard>
<SbCard Elevation="1">Subtle shadow</SbCard>
<SbCard Elevation="2">Light shadow</SbCard>
<SbCard Elevation="3">Medium shadow</SbCard>
<SbCard Elevation="4">Strong shadow</SbCard>
<SbCard Elevation="5">Heavy shadow</SbCard>
```

### Outlined Card

```razor
<SbCard Outlined="true">
    <p>This card has a border instead of shadow.</p>
</SbCard>
```

### Clickable Card

```razor
<SbCard OnClick="HandleCardClick">
    <Header>
        <SbHeading Level="4">Click Me</SbHeading>
    </Header>
    <p>This entire card is clickable.</p>
</SbCard>

@code {
    private void HandleCardClick(MouseEventArgs args)
    {
        Console.WriteLine("Card clicked!");
    }
}
```

### Link Card

```razor
<SbCard Href="/products/123">
    <Header>
        <SbHeading Level="4">Product Name</SbHeading>
    </Header>
    <p>Click to view product details.</p>
</SbCard>
```

### No Padding

```razor
<SbCard NoPadding="true">
    <img src="image.jpg" alt="Full bleed image" style="width: 100%;" />
    <div style="padding: 16px;">
        <SbHeading Level="4">Image Caption</SbHeading>
        <p>Description text with custom padding.</p>
    </div>
</SbCard>
```

### Product Card Example

```razor
<SbCard Class="product-card">
    <Header>
        <img src="@product.Image" alt="@product.Name" />
    </Header>
    <ChildContent>
        <SbHeading Level="4">@product.Name</SbHeading>
        <SbText Color="SbColor.Primary">$@product.Price</SbText>
        <SbText Variant="SbTextVariant.BodySmall">@product.Description</SbText>
    </ChildContent>
    <Footer>
        <SbButton FullWidth="true" OnClick="() => AddToCart(product)">
            Add to Cart
        </SbButton>
    </Footer>
</SbCard>
```

### Dashboard Stat Card

```razor
<SbCard Elevation="2">
    <Header>
        <SbText Variant="SbTextVariant.Caption" Color="SbColor.Secondary">
            Total Revenue
        </SbText>
    </Header>
    <ChildContent>
        <SbHeading Level="2">$45,231</SbHeading>
        <SbChip Color="SbColor.Success" Size="SbSize.Sm">+12.5%</SbChip>
    </ChildContent>
</SbCard>
```
