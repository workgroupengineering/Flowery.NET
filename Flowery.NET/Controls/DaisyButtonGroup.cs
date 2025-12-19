using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;
using Flowery.Services;

namespace Flowery.Controls
{
    public enum DaisyButtonGroupShape
    {
        Default,
        Square,
        Rounded,
        Pill
    }

    public class ButtonGroupItemSelectedEventArgs : RoutedEventArgs
    {
        public Control Item { get; }

        public ButtonGroupItemSelectedEventArgs(RoutedEvent routedEvent, Control item)
            : base(routedEvent)
        {
            Item = item;
        }
    }

    /// <summary>
    /// A segmented button container styled after the "Button Group" pattern.
    /// Supports optional auto-selection and consistent styling for mixed segments
    /// (e.g., buttons with non-clickable text/count parts).
    /// </summary>
    public class DaisyButtonGroup : ItemsControl, IScalableControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyButtonGroup);

        private const double BaseTextFontSize = 14.0;

        /// <inheritdoc/>
        public void ApplyScaleFactor(double scaleFactor)
        {
            FontSize = FloweryScaleManager.ApplyScale(BaseTextFontSize, 11.0, scaleFactor);
        }

        /// <summary>
        /// Defines the <see cref="Variant"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisyButtonVariant> VariantProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, DaisyButtonVariant>(nameof(Variant), DaisyButtonVariant.Default);

        /// <summary>
        /// Gets or sets the visual variant applied to all segments (Default, Primary, Secondary, etc.).
        /// </summary>
        public DaisyButtonVariant Variant
        {
            get => GetValue(VariantProperty);
            set => SetValue(VariantProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Size"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, DaisySize>(nameof(Size), DaisySize.Medium);

        /// <summary>
        /// Gets or sets the size applied to all segments.
        /// </summary>
        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="ButtonStyle"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisyButtonStyle> ButtonStyleProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, DaisyButtonStyle>(nameof(ButtonStyle), DaisyButtonStyle.Default);

        /// <summary>
        /// Gets or sets the segment style (Default, Outline, Dash, Soft).
        /// </summary>
        public DaisyButtonStyle ButtonStyle
        {
            get => GetValue(ButtonStyleProperty);
            set => SetValue(ButtonStyleProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Shape"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisyButtonGroupShape> ShapeProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, DaisyButtonGroupShape>(nameof(Shape), DaisyButtonGroupShape.Default);

        /// <summary>
        /// Gets or sets the overall group shape. This influences corner radii for the container
        /// and the first/last segments.
        /// </summary>
        public DaisyButtonGroupShape Shape
        {
            get => GetValue(ShapeProperty);
            set => SetValue(ShapeProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly StyledProperty<Orientation> OrientationProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, Orientation>(nameof(Orientation), Orientation.Horizontal);

        /// <summary>
        /// Gets or sets whether segments are laid out horizontally or vertically.
        /// </summary>
        public Orientation Orientation
        {
            get => GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="AutoSelect"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> AutoSelectProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, bool>(nameof(AutoSelect), false);

        /// <summary>
        /// Gets or sets a value indicating whether clicking a segment automatically applies the
        /// 'button-group-active' class and removes it from other button segments.
        /// </summary>
        public bool AutoSelect
        {
            get => GetValue(AutoSelectProperty);
            set => SetValue(AutoSelectProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="ShowShadow"/> property.
        /// </summary>
        public static readonly StyledProperty<bool> ShowShadowProperty =
            AvaloniaProperty.Register<DaisyButtonGroup, bool>(nameof(ShowShadow), false);

        /// <summary>
        /// Gets or sets whether a subtle shadow is rendered around the group container.
        /// </summary>
        public bool ShowShadow
        {
            get => GetValue(ShowShadowProperty);
            set => SetValue(ShowShadowProperty, value);
        }

        public static readonly RoutedEvent<ButtonGroupItemSelectedEventArgs> ItemSelectedEvent =
            RoutedEvent.Register<DaisyButtonGroup, ButtonGroupItemSelectedEventArgs>(
                nameof(ItemSelected), RoutingStrategies.Bubble);

        /// <summary>
        /// Raised when a button segment is clicked.
        /// </summary>
        public event EventHandler<ButtonGroupItemSelectedEventArgs> ItemSelected
        {
            add => AddHandler(ItemSelectedEvent, value);
            remove => RemoveHandler(ItemSelectedEvent, value);
        }

        public DaisyButtonGroup()
        {
            AddHandler(Button.ClickEvent, OnButtonClick);
        }

        private void OnButtonClick(object? sender, RoutedEventArgs e)
        {
            var button = e.Source as Button ?? (e.Source as Control)?.FindAncestorOfType<Button>();
            if (button != null && this.IsLogicalAncestorOf(button))
            {
                if (AutoSelect)
                    UpdateSelection(button);

                RaiseEvent(new ButtonGroupItemSelectedEventArgs(ItemSelectedEvent, button));
            }
        }

        private void UpdateSelection(Button selectedButton)
        {
            foreach (var child in this.GetLogicalChildren())
            {
                if (child is Button btn)
                    btn.Classes.Set("button-group-active", btn == selectedButton);
            }
        }
    }
}
