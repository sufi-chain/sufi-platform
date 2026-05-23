# SbStack

A flexible layout component that arranges children in a row or column with configurable spacing and alignment.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Direction | SbStackDirection | Column | Stack direction (Row or Column) |
| Gap | int | 2 | Gap between items (uses spacing scale 0-16) |
| Align | SbAlign | Stretch | Cross-axis alignment |
| Justify | SbJustify | Start | Main-axis justification |
| Wrap | bool | false | Whether items should wrap |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | The content to render inside the stack |

## SbStackDirection Enum

```csharp
public enum SbStackDirection
{
    Row,    // Horizontal layout (flex-direction: row)
    Column  // Vertical layout (flex-direction: column)
}
```

## SbAlign Enum

```csharp
public enum SbAlign
{
    Start,     // Align items to the start
    Center,    // Align items to the center
    End,       // Align items to the end
    Stretch,   // Stretch items to fill the container
    Baseline   // Align items to the baseline
}
```

## SbJustify Enum

```csharp
public enum SbJustify
{
    Start,        // Items packed toward the start
    Center,       // Items are centered
    End,          // Items packed toward the end
    SpaceBetween, // Evenly distributed with first at start, last at end
    SpaceAround,  // Evenly distributed with equal space around
    SpaceEvenly   // Evenly distributed with equal space between
}
```

## CSS Classes

- `sb-stack` - Base class
- `sb-stack--row` - Horizontal direction
- `sb-stack--column` - Vertical direction
- `sb-stack--wrap` - Enable wrapping
- `sb-stack--align-{start|center|end|stretch|baseline}` - Alignment classes
- `sb-stack--justify-{start|center|end|between|around|evenly}` - Justification classes

## Examples

### Basic Vertical Stack

```razor
<SbStack Gap="4">
    <div>Item 1</div>
    <div>Item 2</div>
    <div>Item 3</div>
</SbStack>
```

### Horizontal Stack

```razor
<SbStack Direction="SbStackDirection.Row" Gap="2">
    <SbButton>Save</SbButton>
    <SbButton Variant="SbButtonVariant.Outline">Cancel</SbButton>
</SbStack>
```

### Centered Content

```razor
<SbStack Direction="SbStackDirection.Row" 
         Justify="SbJustify.Center" 
         Align="SbAlign.Center"
         Gap="4">
    <SbIcon Name="check" />
    <span>Success!</span>
</SbStack>
```

### Space Between

```razor
<SbStack Direction="SbStackDirection.Row" 
         Justify="SbJustify.SpaceBetween"
         Align="SbAlign.Center">
    <h1>Title</h1>
    <SbButton>Action</SbButton>
</SbStack>
```

### Wrapped Items

```razor
<SbStack Direction="SbStackDirection.Row" Wrap="true" Gap="2">
    @foreach (var tag in tags)
    {
        <SbChip>@tag</SbChip>
    }
</SbStack>
```

### Form Layout

```razor
<SbStack Gap="4">
    <SbTextField Label="Name" @bind-Value="name" />
    <SbTextField Label="Email" @bind-Value="email" />
    <SbStack Direction="SbStackDirection.Row" Gap="2" Justify="SbJustify.End">
        <SbButton Variant="SbButtonVariant.Outline">Cancel</SbButton>
        <SbButton>Submit</SbButton>
    </SbStack>
</SbStack>
```
