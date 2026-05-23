# SbDivider

A visual separator for content sections. Supports horizontal and vertical orientations, and can include an optional label with customizable alignment.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Orientation | SbOrientation | Horizontal | Divider direction (Horizontal, Vertical) |
| LabelAlign | SbAlign | Center | Label position when content is provided (Start, Center, End) |
| Class | string? | null | Additional CSS classes |
| Style | string? | null | Inline styles |
| AdditionalAttributes | Dictionary<string, object>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment? | Optional label content displayed within the divider |

### Template Usage

#### Simple Divider

```razor
<SbDivider />
```

#### With Label

```razor
<SbDivider>OR</SbDivider>
```

## CSS Classes

- `sb-divider` - Base class
- `sb-divider--horizontal` - Horizontal orientation
- `sb-divider--vertical` - Vertical orientation
- `sb-divider--with-label` - Has label content
- `sb-divider--label-start` - Label aligned to start
- `sb-divider--label-center` - Label aligned to center
- `sb-divider--label-end` - Label aligned to end
- `sb-divider__content` - Label container

## Rendered Element

- `<hr>` - When no content is provided
- `<div>` - When label content is provided

## Accessibility

- Uses `role="separator"` for semantic separation

## Examples

### Basic Horizontal Divider

```razor
<p>Section 1 content</p>
<SbDivider />
<p>Section 2 content</p>
```

### Vertical Divider

```razor
<div style="display: flex; height: 100px; align-items: center;">
    <span>Left</span>
    <SbDivider Orientation="SbOrientation.Vertical" />
    <span>Right</span>
</div>
```

### With Label

```razor
<p>First part</p>
<SbDivider>OR</SbDivider>
<p>Second part</p>
```

### Label Alignment

```razor
<SbDivider LabelAlign="SbAlign.Start">START</SbDivider>
<SbDivider LabelAlign="SbAlign.Center">CENTER</SbDivider>
<SbDivider LabelAlign="SbAlign.End">END</SbDivider>
```

### Login Form Example

```razor
<SbForm>
    <SbTextField Label="Email" @bind-Value="email" />
    <SbTextField Label="Password" Type="password" @bind-Value="password" />
    <SbButton FullWidth="true" Type="submit">Sign In</SbButton>
</SbForm>

<SbDivider>OR</SbDivider>

<SbButton FullWidth="true" Variant="SbButtonVariant.Outlined">
    <SbIcon Name="github" /> Continue with GitHub
</SbButton>
```

### Section Separator

```razor
<section>
    <SbHeading Level="2">About Us</SbHeading>
    <p>Company description...</p>
</section>

<SbDivider />

<section>
    <SbHeading Level="2">Our Services</SbHeading>
    <p>Services description...</p>
</section>
```

### In a Card

```razor
<SbCard>
    <Header>
        <SbHeading Level="4">Order Summary</SbHeading>
    </Header>
    <ChildContent>
        <div class="order-items">
            <p>Item 1 - $10.00</p>
            <p>Item 2 - $15.00</p>
        </div>
        
        <SbDivider />
        
        <div class="order-total">
            <strong>Total: $25.00</strong>
        </div>
    </ChildContent>
</SbCard>
```

### Toolbar with Vertical Dividers

```razor
<div class="toolbar" style="display: flex; gap: 8px; align-items: center;">
    <SbIconButton Icon="bold" />
    <SbIconButton Icon="italic" />
    <SbIconButton Icon="underline" />
    
    <SbDivider Orientation="SbOrientation.Vertical" />
    
    <SbIconButton Icon="align-left" />
    <SbIconButton Icon="align-center" />
    <SbIconButton Icon="align-right" />
    
    <SbDivider Orientation="SbOrientation.Vertical" />
    
    <SbIconButton Icon="list" />
    <SbIconButton Icon="list-ordered" />
</div>
```
