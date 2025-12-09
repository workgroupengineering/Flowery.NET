using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Flowery.Controls;

namespace Flowery.NET.Gallery;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Restore saved theme or use Dark as default
        var savedTheme = ThemeSettings.Load() ?? "Dark";
        DaisyThemeManager.ApplyTheme(savedTheme);

        // Save theme whenever it changes
        DaisyThemeManager.ThemeChanged += (_, name) => ThemeSettings.Save(name);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow();
        }

        base.OnFrameworkInitializationCompleted();
    }
}