# SbPopover

A floating content panel that appears next to an anchor element with configurable placement.

## Parameters

| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| Open | bool | false | Whether the popover is open |
| Placement | SbPlacement | Bottom | Popover placement relative to anchor |
| Offset | int | 8 | Offset from anchor in pixels |
| FlipBehavior | bool | true | Whether to flip when near viewport edge |
| FlipPreferredHeight | int | 300 | Estimated height for flip calculation |
| CloseOnClickAway | bool | true | Close when clicking outside |
| CloseOnEscape | bool | true | Close on Escape key |
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
| AnchorContent | RenderFragment | Content that triggers the popover |
| ChildContent | RenderFragment | Popover content |

## SbPlacement Enum

```csharp
public enum SbPlacement
{
    TopStart, Top, TopEnd,
    EndStart, End, EndEnd,
    BottomStart, Bottom, BottomEnd,
    StartStart, Start, StartEnd
}
```

## CSS Classes

- `sb-popover-anchor` - Anchor wrapper
- `sb-popover` - Popover container
- `sb-popover--{placement}` - Placement variants

## Examples

### Basic Usage

```razor
<SbPopover @bind-Open="isOpen">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Toggle Popover</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbCard>
            <p>Popover content here</p>
        </SbCard>
    </ChildContent>
</SbPopover>

@code {
    private bool isOpen = false;
}
```

### Different Placements

```razor
<SbPopover Placement="SbPlacement.Top" @bind-Open="topOpen">
    <AnchorContent>
        <SbButton OnClick="() => topOpen = !topOpen">Top</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbCard>Above the button</SbCard>
    </ChildContent>
</SbPopover>

<SbPopover Placement="SbPlacement.End" @bind-Open="endOpen">
    <AnchorContent>
        <SbButton OnClick="() => endOpen = !endOpen">Right</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbCard>To the right</SbCard>
    </ChildContent>
</SbPopover>
```

### User Profile Popover

```razor
<SbPopover @bind-Open="profileOpen" Placement="SbPlacement.BottomEnd">
    <AnchorContent>
        <button class="avatar-btn" @onclick="() => profileOpen = !profileOpen">
            <img src="/avatar.jpg" alt="User" />
        </button>
    </AnchorContent>
    <ChildContent>
        <SbCard Class="profile-popover">
            <SbStack Gap="3">
                <SbStack Direction="SbStackDirection.Row" Gap="3" Align="SbAlign.Center">
                    <img src="/avatar.jpg" alt="User" class="avatar-lg" />
                    <SbStack Gap="0">
                        <SbText Weight="semibold">John Doe</SbText>
                        <SbText Size="sm" Color="muted">john@example.com</SbText>
                    </SbStack>
                </SbStack>
                <SbDivider />
                <SbNavMenu>
                    <SbNavItem Href="/profile" Text="Profile" />
                    <SbNavItem Href="/settings" Text="Settings" />
                </SbNavMenu>
                <SbDivider />
                <SbButton Variant="SbButtonVariant.Ghost" FullWidth="true">
                    Sign Out
                </SbButton>
            </SbStack>
        </SbCard>
    </ChildContent>
</SbPopover>
```

### Info Popover

```razor
<SbPopover @bind-Open="infoOpen" Placement="SbPlacement.Top">
    <AnchorContent>
        <SbIconButton Icon="info" OnClick="() => infoOpen = !infoOpen" />
    </AnchorContent>
    <ChildContent>
        <SbCard Style="max-width: 300px">
            <SbStack Gap="2">
                <SbHeading Level="4">What is this?</SbHeading>
                <SbText Size="sm">
                    This setting controls how notifications are sent to your email.
                    Enable it to receive daily digest emails.
                </SbText>
            </SbStack>
        </SbCard>
    </ChildContent>
</SbPopover>
```

### Color Picker Popover

```razor
<SbPopover @bind-Open="colorOpen" Placement="SbPlacement.BottomStart">
    <AnchorContent>
        <button class="color-swatch" 
                style="background-color: @selectedColor"
                @onclick="() => colorOpen = !colorOpen">
        </button>
    </AnchorContent>
    <ChildContent>
        <SbCard>
            <SbColorPicker @bind-Value="selectedColor" />
        </SbCard>
    </ChildContent>
</SbPopover>
```

### Without Click Away

```razor
<SbPopover @bind-Open="isOpen" CloseOnClickAway="false">
    <AnchorContent>
        <SbButton OnClick="() => isOpen = !isOpen">Sticky Popover</SbButton>
    </AnchorContent>
    <ChildContent>
        <SbCard>
            <p>This stays open when clicking outside.</p>
            <SbButton OnClick="() => isOpen = false">Close</SbButton>
        </SbCard>
    </ChildContent>
</SbPopover>
```

### Notification Popover

```razor
<SbPopover @bind-Open="notifOpen" Placement="SbPlacement.BottomEnd">
    <AnchorContent>
        <SbIconButton Icon="bell" OnClick="() => notifOpen = !notifOpen">
            <SbBadge Value="@notifications.Count" Color="SbColor.Danger" />
        </SbIconButton>
    </AnchorContent>
    <ChildContent>
        <SbCard Style="width: 320px">
            <SbStack Gap="0">
                <SbStack Direction="SbStackDirection.Row" 
                         Justify="SbJustify.SpaceBetween" 
                         Class="p-3 border-bottom">
                    <SbText Weight="semibold">Notifications</SbText>
                    <SbLink Href="/notifications">View All</SbLink>
                </SbStack>
                @foreach (var notif in notifications.Take(5))
                {
                    <div class="notification-item p-3">
                        <SbText Size="sm">@notif.Message</SbText>
                        <SbText Size="xs" Color="muted">@notif.TimeAgo</SbText>
                    </div>
                }
            </SbStack>
        </SbCard>
    </ChildContent>
</SbPopover>
```
