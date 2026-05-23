# SbTooltip

A lightweight tooltip component that appears on hover or focus to provide additional information.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Text | string? | null | Simple text tooltip content |
| Placement | SbPlacement | Top | Tooltip placement |
| Delay | int | 200 | Delay before showing (ms) |
| HideDelay | int | 0 | Delay before hiding (ms) |
| MaxWidth | string? | null | Maximum width of tooltip |
| AdditionalAttributes | Dictionary\<string, object\>? | null | Additional HTML attributes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| ChildContent | RenderFragment | Content to wrap with tooltip |
| Content | RenderFragment | Rich tooltip content (replaces Text) |

## CSS Classes

- `sb-tooltip-wrapper` - Wrapper element
- `sb-tooltip` - Tooltip container
- `sb-tooltip--{placement}` - Placement variants

## Accessibility

- Uses `role="tooltip"` on the tooltip element
- `aria-hidden` toggles with visibility

## Examples

### Basic Usage

```razor
<SbTooltip Text="Click to save your changes">
    <SbButton>Save</SbButton>
</SbTooltip>
```

### Different Placements

```razor
<SbTooltip Text="Top tooltip" Placement="SbPlacement.Top">
    <SbButton>Top</SbButton>
</SbTooltip>

<SbTooltip Text="Right tooltip" Placement="SbPlacement.End">
    <SbButton>Right</SbButton>
</SbTooltip>

<SbTooltip Text="Bottom tooltip" Placement="SbPlacement.Bottom">
    <SbButton>Bottom</SbButton>
</SbTooltip>

<SbTooltip Text="Left tooltip" Placement="SbPlacement.Start">
    <SbButton>Left</SbButton>
</SbTooltip>
```

### With Delay

```razor
<SbTooltip Text="Slow tooltip" Delay="500">
    <SbButton>Hover me</SbButton>
</SbTooltip>

<SbTooltip Text="Instant tooltip" Delay="0">
    <SbButton>Instant</SbButton>
</SbTooltip>
```

### Rich Content

```razor
<SbTooltip>
    <Content>
        <SbStack Gap="2">
            <SbText Weight="semibold">Keyboard Shortcut</SbText>
            <SbText Size="sm">Press <kbd>Ctrl</kbd> + <kbd>S</kbd> to save</SbText>
        </SbStack>
    </Content>
    <ChildContent>
        <SbButton>Save</SbButton>
    </ChildContent>
</SbTooltip>
```

### Icon Button Tooltips

```razor
<SbStack Direction="SbStackDirection.Row" Gap="1">
    <SbTooltip Text="Edit">
        <SbIconButton Icon="edit" OnClick="Edit" />
    </SbTooltip>
    
    <SbTooltip Text="Delete">
        <SbIconButton Icon="trash" Color="SbColor.Danger" OnClick="Delete" />
    </SbTooltip>
    
    <SbTooltip Text="Share">
        <SbIconButton Icon="share" OnClick="Share" />
    </SbTooltip>
</SbStack>
```

### With Max Width

```razor
<SbTooltip MaxWidth="200px">
    <Content>
        This is a longer tooltip that will wrap when it exceeds the maximum width setting.
    </Content>
    <ChildContent>
        <SbIcon Name="info" />
    </ChildContent>
</SbTooltip>
```

### Icon rail tooltips

In an icon rail (e.g. KomTheme's `KomIconRail`), wrap nav items with `SbTooltip` and `Placement="SbPlacement.End"` so the tooltip appears to the right:

```razor
<SbStack Direction="SbStackDirection.Column" Gap="1">
    <SbTooltip Text="Home" Placement="SbPlacement.End">
        <SbNavItem Href="/" Icon="home" />
    </SbTooltip>
    <SbTooltip Text="Dashboard" Placement="SbPlacement.End">
        <SbNavItem Href="/dashboard" Icon="layout" />
    </SbTooltip>
    <SbTooltip Text="Settings" Placement="SbPlacement.End">
        <SbNavItem Href="/settings" Icon="settings" />
    </SbTooltip>
</SbStack>
```

### Disabled Element Tooltip

```razor
<SbTooltip Text="You don't have permission to delete">
    <span>
        <SbButton Disabled="true">Delete</SbButton>
    </span>
</SbTooltip>
```

### With Hide Delay

```razor
<SbTooltip Text="I stay visible longer" HideDelay="500">
    <SbButton>Hover me</SbButton>
</SbTooltip>
```

### Form Field Help

```razor
<SbStack Gap="4">
    <SbStack Direction="SbStackDirection.Row" Gap="2" Align="SbAlign.End">
        <SbTextField Label="API Key" @bind-Value="apiKey" />
        <SbTooltip Placement="SbPlacement.End">
            <Content>
                <SbStack Gap="1" Style="max-width: 250px">
                    <SbText Weight="semibold">API Key</SbText>
                    <SbText Size="sm">
                        Your API key is used to authenticate requests.
                        Keep it secure and never share it publicly.
                    </SbText>
                    <SbLink Href="/docs/api-keys" Size="sm">Learn more</SbLink>
                </SbStack>
            </Content>
            <ChildContent>
                <SbIcon Name="help-circle" Size="SbSize.Sm" Color="SbColor.Muted" />
            </ChildContent>
        </SbTooltip>
    </SbStack>
</SbStack>
```
