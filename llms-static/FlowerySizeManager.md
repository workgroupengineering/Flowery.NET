# FlowerySizeManager

Static service for global size management across all Daisy controls. Provides discrete size tiers (ExtraSmall to ExtraLarge) that all controls respond to simultaneously.

## Quick Start

```csharp
using Flowery.Controls;

// Apply a global size
FlowerySizeManager.ApplySize(DaisySize.Medium);

// Or use a built-in dropdown
```

```xml
<controls:DaisySizeDropdown />
```

## Size Tiers

| Size | Typical Use Case | Height | Font Size |
| ---- | ---------------- | ------ | --------- |
| `ExtraSmall` | High-density UIs, data tables | 24px | 10px |
| `Small` | **Default** - Desktop apps | 32px | 12px |
| `Medium` | Touch-friendly, accessibility | 48px | 14px |
| `Large` | Larger screens, presentations | 64px | 18px |
| `ExtraLarge` | Maximum readability, kiosk | 80px | 20px |

## API Reference

### Properties

```csharp
// Get the current global size
DaisySize currentSize = FlowerySizeManager.CurrentSize;

// Auto-apply global size to new controls (default: false)
FlowerySizeManager.UseGlobalSizeByDefault = true;
```

### Methods

```csharp
// Apply by enum
FlowerySizeManager.ApplySize(DaisySize.Large);

// Apply by name (returns true if successful)
bool success = FlowerySizeManager.ApplySize("Large");

// Reset to default (Small)
FlowerySizeManager.Reset();
```

### Events

```csharp
FlowerySizeManager.SizeChanged += (sender, size) =>
{
    Console.WriteLine($"Size changed to: {size}");
};
```

## Attached Properties

### IgnoreGlobalSize

Prevents a control (and its descendants) from responding to global size changes:

```xml
<!-- Single control -->
<controls:DaisyButton controls:FlowerySizeManager.IgnoreGlobalSize="True" 
                      Size="Large" Content="Always Large" />

<!-- Container (protects all children) -->
<StackPanel controls:FlowerySizeManager.IgnoreGlobalSize="True">
    <controls:DaisyButton Size="ExtraSmall" Content="XS" />
    <controls:DaisyButton Size="Small" Content="S" />
    <controls:DaisyButton Size="Medium" Content="M" />
    <controls:DaisyButton Size="Large" Content="L" />
    <controls:DaisyButton Size="ExtraLarge" Content="XL" />
</StackPanel>
```

#### Visual Tree Inheritance

The property uses "first explicit setting wins" semantics via visual tree traversal:

1. `ShouldIgnoreGlobalSize(control)` walks up from the control to the root
2. Stops at the first ancestor with an explicitly set value (`True` or `False`)
3. Returns that value, or `false` if no explicit setting is found

This allows **opting back in** within an opted-out container:

```xml
<StackPanel controls:FlowerySizeManager.IgnoreGlobalSize="True">
    <controls:DaisyButton Size="Small" Content="Stays Small" />
    
    <!-- This child opts BACK IN to global sizing -->
    <Border controls:FlowerySizeManager.IgnoreGlobalSize="False">
        <controls:DaisyButton Content="Responds to global size!" />
    </Border>
</StackPanel>
```

> **Implementation:** Controls should call `FlowerySizeManager.ShouldIgnoreGlobalSize(this)` in their `OnGlobalSizeChanged` handler, not `GetIgnoreGlobalSize(this)` which only checks the control itself.

### ResponsiveFont

Makes TextBlock font sizes respond to global size changes:

```xml
<TextBlock Text="Body text" 
           controls:FlowerySizeManager.ResponsiveFont="Primary" />

<TextBlock Text="Hint text" Opacity="0.7"
           controls:FlowerySizeManager.ResponsiveFont="Secondary" />

<TextBlock Text="Section Title" FontWeight="Bold"
           controls:FlowerySizeManager.ResponsiveFont="Header" />
```

#### Font Tiers

| Tier | Description | XS/S/M/L/XL Sizes |
| ---- | ----------- | ------------------- |
| `None` | No responsive sizing (default) | - |
| `Primary` | Body text, descriptions | 10/12/14/18/20 |
| `Secondary` | Hints, captions, labels | 9/10/12/14/16 |
| `Tertiary` | Very small text, counters | 8/9/11/12/14 |
| `Header` | Section titles, headings | 14/16/20/24/28 |

#### How It Works

1. When `ResponsiveFont` is set, the TextBlock subscribes to `SizeChanged`
2. Font size is immediately applied based on current global size
3. Updates automatically when global size changes
4. Subscription is cleaned up when control is unloaded

> **Why not DynamicResource?** Avalonia's nested resource dictionary scoping prevents dynamically updated resources from propagating reliably. The attached property approach (event subscription) is the recommended pattern for text that should scale with global size.

## Supported Controls

All Daisy controls with a `Size` property respond to global size changes:

- `DaisyButton`, `DaisyInput`, `DaisyTextArea`
- `DaisySelect`, `DaisyCheckBox`, `DaisyRadio`, `DaisyToggle`
- `DaisyBadge`, `DaisyProgress`, `DaisyRadialProgress`
- `DaisyTabs`, `DaisyMenu`, `DaisyKbd`
- `DaisyAvatar`, `DaisyLoading`, `DaisyFileInput`
- `DaisyNumericUpDown`, `DaisyDateTimeline`
- And more...

## Making Custom Controls Size-Aware

### Option 1: Subscribe to SizeChanged Event

```csharp
public class MyControl : UserControl
{
    public MyControl()
    {
        InitializeComponent();
        FlowerySizeManager.SizeChanged += OnSizeChanged;
        ApplySize(FlowerySizeManager.CurrentSize);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        FlowerySizeManager.SizeChanged -= OnSizeChanged;
    }

    private void OnSizeChanged(object? sender, DaisySize size) => ApplySize(size);

    private void ApplySize(DaisySize size)
    {
        MyTextBlock.FontSize = size switch
        {
            DaisySize.ExtraSmall => 10,
            DaisySize.Small => 12,
            DaisySize.Medium => 14,
            DaisySize.Large => 18,
            DaisySize.ExtraLarge => 20,
            _ => 12
        };
    }
}
```

### Option 2: Add a Size StyledProperty

For DaisyUI-style controls:

```csharp
public class MyDaisyControl : TemplatedControl
{
    public static readonly StyledProperty<DaisySize> SizeProperty =
        AvaloniaProperty.Register<MyDaisyControl, DaisySize>(nameof(Size), DaisySize.Small);

    public DaisySize Size
    {
        get => GetValue(SizeProperty);
        set => SetValue(SizeProperty, value);
    }
}
```

Then use design tokens in your theme:

```xml
<Style Selector="local|MyDaisyControl[Size=Small]">
    <Setter Property="FontSize" Value="{DynamicResource DaisySizeSmallFontSize}" />
    <Setter Property="Height" Value="{DynamicResource DaisySizeSmallHeight}" />
</Style>

<Style Selector="local|MyDaisyControl[Size=Large]">
    <Setter Property="FontSize" Value="{DynamicResource DaisySizeLargeFontSize}" />
    <Setter Property="Height" Value="{DynamicResource DaisySizeLargeHeight}" />
</Style>
```

## Built-in UI Control

### DaisySizeDropdown

Ready-to-use dropdown for size selection:

```xml
<controls:DaisySizeDropdown />
```

Features:

- Shows current size with abbreviation (XS, S, M, L, XL)
- Localized size names (11 languages)
- Automatically updates all controls when selection changes

**Advanced:** Customize visible sizes and display names via `SizeOptions` property. See [DaisySizeDropdown](DaisySizeDropdown.md).

## Localization

Size names are localized. Add translations to your language files:

```json
{
  "Size_ExtraSmall": "Extra Small",
  "Size_Small": "Small",
  "Size_Medium": "Medium",
  "Size_Large": "Large",
  "Size_ExtraLarge": "Extra Large"
}
```

Supported: English, German, French, Spanish, Italian, Japanese, Korean, Arabic, Turkish, Ukrainian, Chinese (Simplified).

## Design Tokens Integration

Each size tier maps to specific [Design Tokens](DesignTokens.md):

| Size | Height Token | Font Size Token |
| ---- | ------------ | --------------- |
| ExtraSmall | `DaisySizeExtraSmallHeight` (24) | `DaisySizeExtraSmallFontSize` (10) |
| Small | `DaisySizeSmallHeight` (32) | `DaisySizeSmallFontSize` (12) |
| Medium | `DaisySizeMediumHeight` (48) | `DaisySizeMediumFontSize` (14) |
| Large | `DaisySizeLargeHeight` (64) | `DaisySizeLargeFontSize` (18) |
| ExtraLarge | `DaisySizeExtraLargeHeight` (80) | `DaisySizeExtraLargeFontSize` (20) |

## Comparison: FlowerySizeManager vs FloweryScaleManager

| Feature | FlowerySizeManager | FloweryScaleManager |
| ------- | ------------------ | ------------------- |
| **Purpose** | User preference / accessibility | Responsive window sizing |
| **Scope** | Entire app (global) | Only `EnableScaling="True"` containers |
| **Scaling** | Discrete tiers (XS, S, M, L, XL) | Continuous (0.5× to 1.0×) |
| **Trigger** | User selection | Automatic (window resize) |
| **Best For** | Desktop apps, accessibility | Data forms, dashboards |

> **Most apps should use FlowerySizeManager only.** FloweryScaleManager is an advanced feature for specific responsive scenarios.

## Platform DPI Notes

> **Windows vs Skia Desktop**: At the same `DaisySize` setting, Windows (WinUI) and Skia Desktop may render text at different physical sizes.

| Platform | DPI Behavior |
| -------- | ------------ |
| **Windows (WinUI)** | Automatically respects system DPI scaling (e.g., 125%, 150%). Text appears larger on high-DPI displays. |
| **Skia Desktop** | May render at 1:1 pixel ratio regardless of system DPI settings. Text appears smaller on high-DPI displays. |

**Example**: On a 125% scaled display, a control set to `DaisySize.Small` (12px font) will appear:

- **Windows**: ~15px physical (12 × 1.25)
- **Skia Desktop**: ~12px physical (no scaling applied)

This is a platform rendering characteristic, not a bug. If visual consistency is critical across platforms, consider:

1. Accepting the difference as platform-native behavior
2. Investigating Skia DPI awareness settings in your Uno Platform configuration
3. Using `FloweryScaleManager` to apply manual DPI compensation on Desktop builds

## Best Practices

1. **Start with Small** - Default size works well for most desktop apps
2. **Provide a size picker** - Use `DaisySizeDropdown` for user control
3. **Test all sizes** - Ensure layouts work at ExtraSmall and ExtraLarge
4. **Use design tokens** - Not hardcoded values
5. **Clean up subscriptions** - Always unsubscribe from `SizeChanged` in `OnUnloaded`
6. **Use ResponsiveFont for TextBlocks** - Not DynamicResource
