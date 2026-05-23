# SbSplitPane

A resizable split container that divides space between two panes. The divider can be dragged to adjust the split ratio.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Orientation | SbSplitOrientation | Horizontal | Split direction |
| SplitPosition | double | 50 | Initial split position (percentage) |
| MinFirstPane | double | 10 | Minimum first pane size (percentage) |
| MinSecondPane | double | 10 | Minimum second pane size (percentage) |
| Collapsible | bool | false | Whether panes can be collapsed (reserved) |
| Class | string? | null | Additional CSS classes |

## Events

| Event | Type | Description |
|-------|------|-------------|
| SplitPositionChanged | EventCallback<double> | Fired when split position changes |

## Templates / Slots

| Slot | Type | Description |
|------|------|-------------|
| FirstPane | RenderFragment? | Content for the first (left/top) pane |
| SecondPane | RenderFragment? | Content for the second (right/bottom) pane |

## Orientation Options

| Value | Description |
|-------|-------------|
| Horizontal | Left/right split (vertical divider) |
| Vertical | Top/bottom split (horizontal divider) |

## CSS Classes

- `sb-split-pane` - Base class
- `sb-split-pane--horizontal` - Horizontal orientation
- `sb-split-pane--vertical` - Vertical orientation
- `sb-split-pane__panel` - Panel container
- `sb-split-pane__panel--first` - First panel
- `sb-split-pane__panel--second` - Second panel
- `sb-split-pane__divider` - Divider element
- `sb-split-pane__divider--dragging` - During resize
- `sb-split-pane__divider-handle` - Visual handle

## Examples

### Basic Horizontal Split

```razor
<SbSplitPane>
    <FirstPane>
        <div class="panel">Left Panel</div>
    </FirstPane>
    <SecondPane>
        <div class="panel">Right Panel</div>
    </SecondPane>
</SbSplitPane>
```

### Vertical Split

```razor
<SbSplitPane Orientation="SbSplitOrientation.Vertical">
    <FirstPane>
        <div class="panel">Top Panel</div>
    </FirstPane>
    <SecondPane>
        <div class="panel">Bottom Panel</div>
    </SecondPane>
</SbSplitPane>
```

### Custom Split Position

```razor
<SbSplitPane SplitPosition="30">
    <FirstPane>
        <div class="sidebar">Sidebar (30%)</div>
    </FirstPane>
    <SecondPane>
        <div class="main">Main Content (70%)</div>
    </SecondPane>
</SbSplitPane>
```

### With Minimum Sizes

```razor
<SbSplitPane MinFirstPane="20" MinSecondPane="30">
    <FirstPane>
        <div class="panel">Min 20%</div>
    </FirstPane>
    <SecondPane>
        <div class="panel">Min 30%</div>
    </SecondPane>
</SbSplitPane>
```

### Controlled Split Position

```razor
<SbSplitPane SplitPosition="@splitPosition"
             SplitPositionChanged="@(v => splitPosition = v)">
    <FirstPane>
        <div class="panel">First (@splitPosition.ToString("F0")%)</div>
    </FirstPane>
    <SecondPane>
        <div class="panel">Second (@((100 - splitPosition).ToString("F0"))%)</div>
    </SecondPane>
</SbSplitPane>

<SbButton OnClick="() => splitPosition = 50">Reset to 50/50</SbButton>

@code {
    private double splitPosition = 50;
}
```

### IDE-Style Layout

```razor
<div class="ide-layout">
    <SbSplitPane SplitPosition="20" MinFirstPane="15" MinSecondPane="50">
        <FirstPane>
            <SbInspectorPanel Title="Explorer">
                <SbTreeView Items="@files" />
            </SbInspectorPanel>
        </FirstPane>
        <SecondPane>
            <SbSplitPane Orientation="SbSplitOrientation.Vertical" 
                         SplitPosition="70">
                <FirstPane>
                    <div class="editor">
                        Code Editor Area
                    </div>
                </FirstPane>
                <SecondPane>
                    <SbTabs>
                        <SbTab Title="Terminal">Terminal output...</SbTab>
                        <SbTab Title="Problems">No problems detected</SbTab>
                        <SbTab Title="Output">Build output...</SbTab>
                    </SbTabs>
                </SecondPane>
            </SbSplitPane>
        </SecondPane>
    </SbSplitPane>
</div>
```

### Nested Splits

```razor
<SbSplitPane SplitPosition="25">
    <FirstPane>
        <nav class="left-sidebar">Navigation</nav>
    </FirstPane>
    <SecondPane>
        <SbSplitPane SplitPosition="75">
            <FirstPane>
                <main class="content">Main Content</main>
            </FirstPane>
            <SecondPane>
                <aside class="right-sidebar">Details Panel</aside>
            </SecondPane>
        </SbSplitPane>
    </SecondPane>
</SbSplitPane>
```

### Email Client Layout

```razor
<SbSplitPane SplitPosition="30">
    <FirstPane>
        <div class="email-list">
            @foreach (var email in emails)
            {
                <div class="email-item" @onclick="() => SelectEmail(email)">
                    <strong>@email.Subject</strong>
                    <span>@email.From</span>
                </div>
            }
        </div>
    </FirstPane>
    <SecondPane>
        @if (selectedEmail != null)
        {
            <div class="email-content">
                <h2>@selectedEmail.Subject</h2>
                <p>From: @selectedEmail.From</p>
                <SbDivider />
                <div>@selectedEmail.Body</div>
            </div>
        }
        else
        {
            <SbEmptyState Title="No email selected" />
        }
    </SecondPane>
</SbSplitPane>
```
