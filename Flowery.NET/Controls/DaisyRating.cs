using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;

namespace Flowery.Controls
{
    /// <summary>
    /// Specifies how rating values are snapped when clicking.
    /// </summary>
    public enum RatingPrecision
    {
        /// <summary>Only whole star values (1, 2, 3, 4, 5)</summary>
        Full,
        /// <summary>Half-star increments (0.5, 1, 1.5, 2, ...)</summary>
        Half,
        /// <summary>One decimal place (0.1 increments)</summary>
        Precise
    }

    public class DaisyRating : RangeBase
    {
        protected override Type StyleKeyOverride => typeof(DaisyRating);

        private Control? _foregroundPart;
        private Control? _backgroundPart;

        // Spacing between stars (must match the template's StackPanel Spacing)
        private const double StarSpacing = 4.0;

        public DaisyRating()
        {
            Minimum = 0;
            Maximum = 5;
            Value = 0;
            Cursor = Cursor.Parse("Hand");
        }

        /// <summary>
        /// Calculates the actual width occupied by the stars based on count and size.
        /// Each star's width equals the Height property, with StarSpacing between them.
        /// </summary>
        private double GetStarsWidth()
        {
            var starCount = (int)Maximum;
            if (starCount <= 0) return 0;

            var starSize = Height;
            // Total = (starCount * starSize) + ((starCount - 1) * spacing)
            return (starCount * starSize) + ((starCount - 1) * StarSpacing);
        }

        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyRating, DaisySize>(nameof(Size), DaisySize.Medium);

        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly StyledProperty<bool> IsReadOnlyProperty =
            AvaloniaProperty.Register<DaisyRating, bool>(nameof(IsReadOnly));

        public bool IsReadOnly
        {
            get => GetValue(IsReadOnlyProperty);
            set => SetValue(IsReadOnlyProperty, value);
        }

        public static readonly StyledProperty<RatingPrecision> PrecisionProperty =
            AvaloniaProperty.Register<DaisyRating, RatingPrecision>(nameof(Precision), RatingPrecision.Full);

        /// <summary>
        /// Gets or sets how rating values are snapped when clicking.
        /// Full = whole stars only, Half = 0.5 increments, Precise = 0.1 increments.
        /// </summary>
        public RatingPrecision Precision
        {
            get => GetValue(PrecisionProperty);
            set => SetValue(PrecisionProperty, value);
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
            _foregroundPart = e.NameScope.Find<Control>("PART_ForegroundStars");
            _backgroundPart = e.NameScope.Find<Control>("PART_BackgroundStars");

            UpdateVisuals();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            if (change.Property == ValueProperty ||
                change.Property == MinimumProperty ||
                change.Property == MaximumProperty)
            {
                UpdateVisuals();
            }
        }

        private void UpdateVisuals()
        {
            if (_foregroundPart == null || _backgroundPart == null) return;

            // Ideally we wait for layout to know the width, but for now we can try to rely on the container size logic.
            // If we use a Grid for layout, we can set the Width of the foreground container wrapper.
            // Actually, we need to know the 'full' width to calculate the percentage.
            // But if we put them in a grid, they have the same size.
            // We just need to clip the foreground.

            // NOTE: In Avalonia, if we change Width of a container, we trigger layout.
            // We want to Clip.

            // Let's rely on the Bounds change to update the Clip Rect.
            InvalidateArrange();
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var result = base.ArrangeOverride(finalSize);
            UpdateClip(finalSize);
            return result;
        }

        private void UpdateClip(Size bounds)
        {
            if (_foregroundPart == null) return;

            var range = Maximum - Minimum;
            if (range <= 0) return;

            var percent = (Value - Minimum) / range;
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;

            // Use the actual stars width, not the control's bounds
            var starsWidth = GetStarsWidth();
            var clipWidth = starsWidth * percent;

            _foregroundPart.Width = clipWidth;
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);
            if (IsReadOnly) return;

            UpdateValueFromPoint(e.GetPosition(this));
            e.Pointer.Capture(this);
        }

        protected override void OnPointerMoved(PointerEventArgs e)
        {
            base.OnPointerMoved(e);
            if (IsReadOnly) return;

            if (this.Equals(e.Pointer.Captured))
            {
                UpdateValueFromPoint(e.GetPosition(this));
            }
        }

        protected override void OnPointerReleased(PointerReleasedEventArgs e)
        {
            base.OnPointerReleased(e);
            if (IsReadOnly) return;

            if (this.Equals(e.Pointer.Captured))
            {
                e.Pointer.Capture(null);
            }
        }

        private void UpdateValueFromPoint(Point p)
        {
            // Use actual stars width instead of control bounds
            var starsWidth = GetStarsWidth();
            if (starsWidth <= 0) return;

            var percent = p.X / starsWidth;
            if (percent < 0) percent = 0;
            if (percent > 1) percent = 1;

            var range = Maximum - Minimum;
            var rawValue = (percent * range) + Minimum;

            // Snap value based on Precision setting
            var newValue = SnapValue(rawValue);

            SetCurrentValue(ValueProperty, newValue);
        }

        private double SnapValue(double rawValue)
        {
            switch (Precision)
            {
                case RatingPrecision.Half:
                    // Snap to nearest 0.5
                    return Math.Ceiling(rawValue * 2) / 2.0;

                case RatingPrecision.Precise:
                    // Snap to nearest 0.1
                    return Math.Ceiling(rawValue * 10) / 10.0;

                case RatingPrecision.Full:
                default:
                    // Snap to whole number
                    return Math.Ceiling(rawValue);
            }
        }
    }
}
