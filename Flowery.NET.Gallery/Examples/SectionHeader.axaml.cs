using Avalonia;
using Avalonia.Controls;

namespace Flowery.NET.Gallery.Examples;

public partial class SectionHeader : UserControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<SectionHeader, string>(nameof(Title), string.Empty);

    public static readonly StyledProperty<string> SectionIdProperty =
        AvaloniaProperty.Register<SectionHeader, string>(nameof(SectionId), string.Empty);

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string SectionId
    {
        get => GetValue(SectionIdProperty);
        set => SetValue(SectionIdProperty, value);
    }

    public SectionHeader()
    {
        InitializeComponent();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == TitleProperty)
        {
            var textBlock = this.FindControl<TextBlock>("TitleText");
            if (textBlock != null)
                textBlock.Text = Title;
        }
    }
}
