using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using System;
using System.Threading.Tasks;

namespace Flowery.NET.Gallery.Examples
{
    public partial class ShowcaseExamples : UserControl
    {
        private Border? _slideHandle;
        private Border? _slideTrack;
        private TextBlock? _slideLabel;
        private bool _isDragging;
        private double _startX;
        private double _maxSlide;

        public ShowcaseExamples()
        {
            InitializeComponent();

            _slideHandle = this.FindControl<Border>("SlideHandle");
            _slideTrack = this.FindControl<Border>("SlideTrack");
            _slideLabel = this.FindControl<TextBlock>("SlideLabel");

            if (_slideHandle != null)
            {
                _slideHandle.PointerPressed += OnSlidePressed;
                _slideHandle.PointerMoved += OnSlideMoved;
                _slideHandle.PointerReleased += OnSlideReleased;
            }
        }

        private void OnSlidePressed(object? sender, PointerPressedEventArgs e)
        {
            if (_slideHandle == null || _slideTrack == null) return;
            _isDragging = true;
            _startX = e.GetPosition(_slideTrack).X - Canvas.GetLeft(_slideHandle);
            _maxSlide = _slideTrack.Bounds.Width - _slideHandle.Bounds.Width - 8; // 4px padding each side
            e.Pointer.Capture(_slideHandle);
        }

        private void OnSlideMoved(object? sender, PointerEventArgs e)
        {
            if (!_isDragging || _slideHandle == null || _slideTrack == null) return;
            var currentX = e.GetPosition(_slideTrack).X;
            var newX = Math.Max(0, Math.Min(_maxSlide, currentX - _startX));
            Canvas.SetLeft(_slideHandle, newX);
            // Visual feedback: dim track as we slide
            _slideTrack.Opacity = 1.0 - (newX / _maxSlide) * 0.5;
        }

        private async void OnSlideReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!_isDragging || _slideHandle == null || _slideTrack == null) return;
            _isDragging = false;
            e.Pointer.Capture(null);

            var currentX = Canvas.GetLeft(_slideHandle);
            if (currentX > _maxSlide * 0.9)
            {
                // Success!
                Canvas.SetLeft(_slideHandle, _maxSlide);
                _slideTrack.Background = Brushes.Red;
                _slideTrack.Opacity = 1.0; // Force full opacity on success

                // Change text
                string? originalText = null;
                if (_slideLabel != null)
                {
                    originalText = _slideLabel.Text;
                    _slideLabel.Text = "Power Off";
                }

                await Task.Delay(2000);
                // Reset
                Canvas.SetLeft(_slideHandle, 0);
                _slideTrack.Background = (IBrush)this.FindResource("DaisyBase300Brush")!;
                _slideTrack.Opacity = 1.0;

                // Restore text
                if (_slideLabel != null && originalText != null)
                {
                     _slideLabel.Text = originalText;
                }
            }
            else
            {
                // Snap back
                Canvas.SetLeft(_slideHandle, 0);
                _slideTrack.Opacity = 1.0;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
