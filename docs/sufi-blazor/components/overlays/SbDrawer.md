# SbDrawer

A slide-out panel component for displaying content from the edge of the screen.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Open | bool | false | Whether the drawer is open |
| Placement | SbDrawerPlacement | Start | Drawer position (Start, End, Top, Bottom) |
| Modal | bool | true | Whether drawer has a backdrop |
| Title | string? | null | Drawer title |
| Width | string | "320px" | Width for Start/End placement |
| Height | string | "320px" | Height for Top/Bottom placement |
| CloseOnEscape | bool | true | Whether to close on Escape key |
| CloseOnBackdropClick | bool | true | Whether to close on backdrop click |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| OpenChanged | EventCallback\<bool\> | Fired when open state changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| Header | RenderFragment | Custom header content |
| ChildContent | RenderFragment | Drawer body content |
| Footer | RenderFragment | Footer content |

## SbDrawerPlacement Enum

```csharp
public enum SbDrawerPlacement
{
    Start,   // Left in LTR, Right in RTL
    End,     // Right in LTR, Left in RTL
    Top,     // Top of screen
    Bottom   // Bottom of screen
}
```

## CSS Classes

- `sb-drawer` - Base class
- `sb-drawer--start|end|top|bottom` - Placement variants
- `sb-drawer--modal` - When modal is true
- `sb-drawer__container` - Inner container
- `sb-drawer__header` - Header section
- `sb-drawer__title` - Title text
- `sb-drawer__close-btn` - Close button
- `sb-drawer__body` - Body content
- `sb-drawer__footer` - Footer section

## CSS Variables

- `--sb-drawer-width` - Width for horizontal drawers
- `--sb-drawer-height` - Height for vertical drawers

## Examples

### Basic Usage

```razor
<SbButton OnClick="() => isOpen = true">Open Drawer</SbButton>

<SbDrawer @bind-Open="isOpen" Title="Menu">
    <p>Drawer content</p>
</SbDrawer>

@code {
    private bool isOpen = false;
}
```

### Different Placements

```razor
<SbDrawer @bind-Open="leftOpen" Placement="SbDrawerPlacement.Start" Title="Left Drawer">
    Left side content
</SbDrawer>

<SbDrawer @bind-Open="rightOpen" Placement="SbDrawerPlacement.End" Title="Right Drawer">
    Right side content
</SbDrawer>

<SbDrawer @bind-Open="topOpen" Placement="SbDrawerPlacement.Top" Title="Top Drawer">
    Top content
</SbDrawer>

<SbDrawer @bind-Open="bottomOpen" Placement="SbDrawerPlacement.Bottom" Title="Bottom Drawer">
    Bottom content
</SbDrawer>
```

### Custom Width

```razor
<SbDrawer @bind-Open="isOpen" 
          Title="Wide Drawer" 
          Width="500px">
    <p>This drawer is 500px wide.</p>
</SbDrawer>
```

### Navigation Drawer

```razor
<SbDrawer @bind-Open="navOpen" Title="Navigation">
    <SbNavMenu>
        <SbNavItem Href="/" Text="Home" OnClick="() => navOpen = false">
            <Icon><SbIcon Name="home" /></Icon>
        </SbNavItem>
        <SbNavItem Href="/products" Text="Products" OnClick="() => navOpen = false">
            <Icon><SbIcon Name="box" /></Icon>
        </SbNavItem>
        <SbNavItem Href="/about" Text="About" OnClick="() => navOpen = false">
            <Icon><SbIcon Name="info" /></Icon>
        </SbNavItem>
    </SbNavMenu>
</SbDrawer>
```

### Filter Drawer

```razor
<SbDrawer @bind-Open="filterOpen" 
          Placement="SbDrawerPlacement.End" 
          Title="Filters"
          Width="400px">
    <ChildContent>
        <SbStack Gap="4">
            <SbSelect Label="Category" @bind-Value="filter.Category">
                <SbSelectOption Value="">All</SbSelectOption>
                <SbSelectOption Value="electronics">Electronics</SbSelectOption>
                <SbSelectOption Value="clothing">Clothing</SbSelectOption>
            </SbSelect>
            
            <SbSlider Label="Price Range" 
                      Min="0" Max="1000" 
                      @bind-Value="filter.MaxPrice" />
            
            <SbCheckbox Label="In Stock Only" @bind-Value="filter.InStock" />
        </SbStack>
    </ChildContent>
    <Footer>
        <SbStack Direction="SbStackDirection.Row" Gap="2">
            <SbButton Variant="SbButtonVariant.Outline" OnClick="ClearFilters">
                Clear
            </SbButton>
            <SbButton OnClick="ApplyFilters">Apply Filters</SbButton>
        </SbStack>
    </Footer>
</SbDrawer>
```

### Cart Drawer

```razor
<SbDrawer @bind-Open="cartOpen" 
          Placement="SbDrawerPlacement.End" 
          Title="Shopping Cart"
          Width="420px">
    <ChildContent>
        @if (cartItems.Any())
        {
            <SbStack Gap="3">
                @foreach (var item in cartItems)
                {
                    <SbCard>
                        <SbStack Direction="SbStackDirection.Row" Gap="3">
                            <img src="@item.Image" alt="@item.Name" width="60" />
                            <SbStack Gap="1">
                                <SbText Weight="semibold">@item.Name</SbText>
                                <SbText>@item.Price.ToString("C")</SbText>
                            </SbStack>
                        </SbStack>
                    </SbCard>
                }
            </SbStack>
        }
        else
        {
            <SbEmptyState Title="Cart Empty" Description="Add items to your cart" />
        }
    </ChildContent>
    <Footer>
        <SbStack Gap="3">
            <SbStack Direction="SbStackDirection.Row" Justify="SbJustify.SpaceBetween">
                <SbText Weight="semibold">Total:</SbText>
                <SbText Weight="semibold">@cartTotal.ToString("C")</SbText>
            </SbStack>
            <SbButton FullWidth="true">Checkout</SbButton>
        </SbStack>
    </Footer>
</SbDrawer>
```

### Non-Modal Drawer

```razor
<SbDrawer @bind-Open="isOpen" 
          Title="Side Panel" 
          Modal="false">
    <p>This drawer doesn't have a backdrop overlay.</p>
</SbDrawer>
```
