# Agent Rules

- **Terminal Commands**: Always use `cmd /c` prefix for terminal commands to ensuring proper execution in the Windows environment.

## Avalonia Clipboard Usage

### Image Clipboard (Windows-only)

Avalonia's built-in clipboard API (`DataObject`, `SetDataObjectAsync`) is deprecated and doesn't reliably copy images. For Windows, use **WinForms interop**:

```csharp
using System.Runtime.Versioning;

[SupportedOSPlatform("windows")]
private static void SetBitmapClipboardData(byte[] pngBytes)
{
    if (pngBytes == null || pngBytes.Length == 0) return;
    using var stream = new MemoryStream(pngBytes);
    using var image = System.Drawing.Image.FromStream(stream);
    System.Windows.Forms.Clipboard.SetImage(image);
}
```

### Text Clipboard (Cross-platform)

For text, use Avalonia's built-in clipboard:

```csharp
var clipboard = TopLevel.GetTopLevel(this)?.Clipboard;
if (clipboard != null)
    await clipboard.SetTextAsync(textContent);
```

### Cross-Platform Project Setup

When a project needs WinForms clipboard on Windows but must remain buildable on other platforms:

1. **Conditional TFM** in `.csproj`:
   ```xml
   <TargetFramework Condition="$([MSBuild]::IsOSPlatform('Windows'))">net8.0-windows</TargetFramework>
   <TargetFramework Condition="!$([MSBuild]::IsOSPlatform('Windows'))">net8.0</TargetFramework>
   ```

2. **Conditional WinForms** in `.csproj`:
   ```xml
   <PropertyGroup Condition="$([MSBuild]::IsOSPlatform('Windows'))">
     <UseWindowsForms>true</UseWindowsForms>
     <DefineConstants>$(DefineConstants);WINDOWS</DefineConstants>
   </PropertyGroup>
   ```

3. **Conditional compilation** in code:
   ```csharp
   #if WINDOWS
   if (OperatingSystem.IsWindows())
   {
       SetBitmapClipboardData(pngBytes);
   }
   else
   #endif
   {
       // Fallback: save to temp file, copy path to clipboard
       var tempPath = Path.Combine(Path.GetTempPath(), "screenshot.png");
       await File.WriteAllBytesAsync(tempPath, pngBytes);
       await clipboard.SetTextAsync(tempPath);
   }
   ```

**Key points:**
- `UseWindowsForms` requires `net8.0-windows` TFM (SDK enforced)
- Use `#if WINDOWS` preprocessor directives to guard WinForms code
- Mark Windows-specific methods with `[SupportedOSPlatform("windows")]`
- Always provide a fallback for non-Windows platforms
