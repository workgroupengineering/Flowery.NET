using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Flowery.Controls;

namespace Flowery.NET.Gallery.Examples;

public partial class NavigationExamples : UserControl, IScrollableExample
{
    public NavigationExamples()
    {
        InitializeComponent();
    }

    public void ScrollToSection(string sectionName)
    {
        var scrollViewer = this.FindControl<ScrollViewer>("MainScrollViewer");
        if (scrollViewer == null) return;

        var sectionHeader = this.GetVisualDescendants()
            .OfType<SectionHeader>()
            .FirstOrDefault(h => h.SectionId == sectionName);

        if (sectionHeader?.Parent is Visual parent)
        {
            var transform = parent.TransformToVisual(scrollViewer);
            if (transform.HasValue)
            {
                var point = transform.Value.Transform(new Point(0, 0));
                // Add current scroll offset to get absolute position in content
                scrollViewer.Offset = new Vector(0, point.Y + scrollViewer.Offset.Y);
            }
        }
    }

    private void OnDockItemSelected(object sender, DockItemSelectedEventArgs e)
    {
        if (e.Item is Button btn && btn.Tag is string tag)
        {
            ShowToast($"Dock item clicked: {tag}");
        }
    }

    private void ShowToast(string message)
    {
        var toast = this.FindControl<DaisyToast>("NavigationToast");
        if (toast != null)
        {
            var alert = new DaisyAlert
            {
                Content = message,
                Variant = DaisyAlertVariant.Info,
                Margin = new Thickness(0, 4)
            };

            toast.Items.Add(alert);

            // Auto remove after 3 seconds
            DispatcherTimer.RunOnce(() =>
            {
                toast.Items.Remove(alert);
            }, TimeSpan.FromSeconds(3));
        }
    }
}
