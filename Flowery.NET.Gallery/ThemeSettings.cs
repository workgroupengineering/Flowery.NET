using System;
using System.IO;

namespace Flowery.NET.Gallery;

public static class ThemeSettings
{
    private static readonly string SettingsPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "Flowery.NET.Gallery",
        "theme.txt");

    public static string? Load()
    {
        try
        {
            if (File.Exists(SettingsPath))
                return File.ReadAllText(SettingsPath).Trim();
        }
        catch { }
        return null;
    }

    public static void Save(string themeName)
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SettingsPath)!);
            File.WriteAllText(SettingsPath, themeName);
        }
        catch { /* ignore */ }
    }
}

