using System;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;

namespace Flowery.NET.Gallery.Examples;

public class ModalRadiiEventArgs : EventArgs
{
    public double TopLeft { get; set; } = 16;
    public double TopRight { get; set; } = 16;
    public double BottomLeft { get; set; } = 16;
    public double BottomRight { get; set; } = 16;
    public string Title { get; set; } = "Modal";
}

public partial class ActionsExamples : UserControl, IScrollableExample
{
    public event EventHandler? OpenModalRequested;
    public event EventHandler<ModalRadiiEventArgs>? OpenModalWithRadiiRequested;

    public ActionsExamples()
    {
        InitializeComponent();
    }

    public void OpenModalBtn_Click(object? sender, RoutedEventArgs e)
    {
        OpenModalRequested?.Invoke(this, EventArgs.Empty);
    }

    public void OpenDefaultModal_Click(object? sender, RoutedEventArgs e)
    {
        OpenModalWithRadiiRequested?.Invoke(this, new ModalRadiiEventArgs
        {
            TopLeft = 16, TopRight = 16, BottomLeft = 16, BottomRight = 16,
            Title = "Default Corners"
        });
    }

    public void OpenPillTopModal_Click(object? sender, RoutedEventArgs e)
    {
        OpenModalWithRadiiRequested?.Invoke(this, new ModalRadiiEventArgs
        {
            TopLeft = 24, TopRight = 24, BottomLeft = 8, BottomRight = 8,
            Title = "Pill Top"
        });
    }

    public void OpenSharpModal_Click(object? sender, RoutedEventArgs e)
    {
        OpenModalWithRadiiRequested?.Invoke(this, new ModalRadiiEventArgs
        {
            TopLeft = 0, TopRight = 0, BottomLeft = 0, BottomRight = 0,
            Title = "Sharp Corners"
        });
    }

    public void ScrollToSection(string sectionId)
    {
        var scrollViewer = this.FindControl<ScrollViewer>("MainScrollViewer");
        if (scrollViewer == null) return;

        var sectionHeader = this.GetVisualDescendants()
            .OfType<SectionHeader>()
            .FirstOrDefault(h => h.SectionId == sectionId);

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
}
