<!-- Supplementary documentation for DaisyInput -->
<!-- This content is merged into auto-generated docs by generate_docs.py -->

# Overview

DaisyInput is a styled TextBox with **9 variants** and **4 size presets**. It supports bordered, ghost, and semantic colored borders, plus inner content slots for icons or buttons. Defaults to a padded, rounded text field that matches DaisyUI forms.

## Variant Options

| Variant | Description |
|---------|-------------|
| Bordered (default) | Subtle 30% opacity border; brightens on focus. |
| Ghost | No border and transparent background; adds light fill on focus. |
| Primary / Secondary / Accent | Colored borders with focus states. |
| Info / Success / Warning / Error | Semantic border colors. |

## Size Options

| Size | Height | Font Size | Use Case |
|------|--------|-----------|----------|
| ExtraSmall | 24 | 10 | Dense tables/toolbars. |
| Small | 32 | 12 | Compact forms. |
| Medium (default) | 48 | 14 | General usage. |
| Large | 64 | 18 | Prominent inputs/hero sections. |

> [!NOTE]
> DaisyInput uses **fixed heights** for each size to match DaisyUI's design. Single-line inputs will not grow vertically. For multi-line text entry, use `DaisyTextArea` instead.

## Properties

DaisyInput extends `TextBox` — all standard TextBox properties (`Text`, `Watermark`, `Padding`, `TextAlignment`, `TextWrapping`, `VerticalContentAlignment`, etc.) are supported.

**DaisyInput-specific properties:**

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Variant` | `DaisyInputVariant` | `Bordered` | Visual style variant (see table above). |
| `Size` | `DaisySize` | `Medium` | Size preset (see table above). |
| `InnerLeftContent` | `object` | `null` | Content slot inside left edge (e.g., search icon). |
| `InnerRightContent` | `object` | `null` | Content slot inside right edge (e.g., clear button). |

## Content Alignment

The `VerticalContentAlignment` property controls vertical positioning of both the watermark and text content:
- **Center** (default): Text vertically centered — standard for single-line inputs.
- **Top**: Text aligned to top — used by `DaisyTextArea` for multi-line editing.

## Quick Examples

```xml
<!-- Basic -->
<controls:DaisyInput Watermark="Bordered (Default)" />

<!-- Ghost -->
<controls:DaisyInput Variant="Ghost" Watermark="Ghost" />

<!-- Semantic -->
<controls:DaisyInput Variant="Primary" Watermark="Primary" />
<controls:DaisyInput Variant="Error" Watermark="Error state" />

<!-- Sizes -->
<controls:DaisyInput Size="Small" Watermark="Small Input" />
<controls:DaisyInput Size="Large" Watermark="Large Input" />

<!-- With icons -->
<controls:DaisyInput Watermark="Search..." Size="Small">
    <controls:DaisyInput.InnerLeftContent>
        <PathIcon Data="{StaticResource DaisyIconSearch}" Width="14" Height="14" Opacity="0.7" />
    </controls:DaisyInput.InnerLeftContent>
</controls:DaisyInput>
```

## Tips & Best Practices

- Use Ghost for inputs on colored surfaces; use Bordered/Primary for standard light backgrounds.
- Pair semantic variants with validation states (Error/Success) to reinforce feedback.
- Keep `Padding` consistent across form fields; sizes already tune height and font size.
- For search bars, add a left icon; for clear actions, add a right button via `InnerRightContent`.
