<!-- Supplementary documentation for DaisyExpandableCard -->
<!-- This content is merged into auto-generated docs by generate_docs.py -->

# Overview

DaisyExpandableCard is a versatile card component that can reveal a secondary content area when triggered. It is ideal for "Showcase" or "Detail" views where initial high-level information is presented, and additional details are revealed on demand (e.g., clicking a "Learn More" or "Play" button).

Key features:

- **Smooth Animation**: Uses a width-based easing animation to push neighboring content aside while revealing the expanded area.
- **Opacity Transition**: Content in the expanded area fades in/out simultaneously with the width change.
- **Toggle Command**: Includes a built-in `ToggleCommand` for easy binding from child buttons.
- **Responsive**: Works well in horizontal scrolling containers like `ScrollViewer`.

## Key Properties

| Property | Description |
|----------|-------------|
| **IsExpanded** | Controls whether the card is currently showing its expanded content. |
| **ExpandedContent** | The UI content to display in the revealed area. |
| **ToggleCommand** | A command that toggles `IsExpanded`. Typically bound to a button inside the card. |

## Quick Examples

```xml
<controls:DaisyExpandableCard x:Name="MyCard">
    <!-- Main Content (Default) -->
    <Grid Width="150" Height="225">
        <controls:DaisyButton Content="Expand" 
                              Command="{Binding #MyCard.ToggleCommand}" />
    </Grid>

    <!-- Expanded Content -->
    <controls:DaisyExpandableCard.ExpandedContent>
        <Border Width="150" Background="#111827">
            <TextBlock Text="Detailed Info revealed!" />
        </Border>
    </controls:DaisyExpandableCard.ExpandedContent>
</controls:DaisyExpandableCard>
```

## Tips & Best Practices

- **Fixed Dimensions**: For the most reliable expansion animation, define a fixed `Width` and `Height` on both the main content and the `ExpandedContent` container (e.g., a `Border`).
- **Horizontal Layout**: Expandable cards are best used in a horizontal `StackPanel` inside a `ScrollViewer` with `HorizontalScrollBarVisibility="Auto"`. This ensures neighbors are pushed correctly.
- **Glass Effect**: Supports the same `IsGlass="True"` mode as `DaisyCard` for a modern frosted look.
