using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media.Imaging;
using Avalonia.Platform.Storage;
using Avalonia.VisualTree;

namespace Flowery.NET.Gallery.Examples;

public partial class SectionHeader : UserControl
{
    /// <summary>
    /// Maps SectionId to Daisy control name. Used for screenshot filenames and documentation.
    /// Keep in sync with generate_docs.py _section_to_control() mapping.
    /// </summary>
    public static readonly Dictionary<string, string> SectionIdToControlName = new(StringComparer.OrdinalIgnoreCase)
    {
        // Actions
        ["button"] = "DaisyButton",
        ["dropdown"] = "DaisySelect",
        ["fab"] = "DaisyFab",
        ["modal"] = "DaisyModal",
        ["modalradii"] = "DaisyModal",
        ["swap"] = "DaisySwap",

        // Cards & Layout
        ["card"] = "DaisyCard",

        // Data Input
        ["checkbox"] = "DaisyCheckBox",
        ["fileinput"] = "DaisyFileInput",
        ["input"] = "DaisyInput",
        ["numericupdown"] = "DaisyNumericUpDown",
        ["radio"] = "DaisyRadio",
        ["range"] = "DaisyRange",
        ["rating"] = "DaisyRating",
        ["select"] = "DaisySelect",
        ["textarea"] = "DaisyTextArea",
        ["toggle"] = "DaisyToggle",

        // Feedback
        ["alert"] = "DaisyAlert",
        ["badge"] = "DaisyBadge",
        ["loading"] = "DaisyLoading",
        ["progress"] = "DaisyProgress",
        ["skeleton"] = "DaisySkeleton",
        ["toast"] = "DaisyToast",

        // Data Display
        ["accordion"] = "DaisyAccordion",
        ["avatar"] = "DaisyAvatar",
        ["carousel"] = "DaisyCarousel",
        ["chatbubble"] = "DaisyChatBubble",
        ["collapse"] = "DaisyCollapse",
        ["countdown"] = "DaisyCountdown",
        ["diff"] = "DaisyDiff",
        ["divider"] = "DaisyDivider",
        ["kbd"] = "DaisyKbd",
        ["list"] = "DaisyList",
        ["stat"] = "DaisyStat",
        ["table"] = "DaisyTable",
        ["timeline"] = "DaisyTimeline",

        // Navigation
        ["breadcrumbs"] = "DaisyBreadcrumbs",
        ["dock"] = "DaisyDock",
        ["drawer"] = "DaisyDrawer",
        ["menu"] = "DaisyMenu",
        ["navbar"] = "DaisyNavbar",
        ["pagination"] = "DaisyPagination",
        ["steps"] = "DaisySteps",
        ["tabs"] = "DaisyTabs",

        // Layout
        ["statusdot"] = "DaisyStatusIndicator",
        ["radialprogress"] = "DaisyRadialProgress",
        ["indicator"] = "DaisyIndicator",
        ["mask"] = "DaisyMask",
        ["stack"] = "DaisyStack",
        ["hero"] = "DaisyHero",
        ["join"] = "DaisyJoin",
        ["mockup"] = "DaisyMockup",
        ["hovergallery"] = "DaisyHoverGallery",
        ["glass"] = "DaisyGlass",
        ["textrotate"] = "DaisyTextRotate",

        // Color Picker
        ["colorslider"] = "DaisyColorSlider",
        ["colorwheel"] = "DaisyColorWheel",
        ["colorgrid"] = "DaisyColorGrid",
        ["coloreditor"] = "DaisyColorEditor",
        ["screenpicker"] = "DaisyScreenColorPicker",
        ["screencolorpicker"] = "DaisyScreenColorPicker",
        ["colorpickerdialog"] = "DaisyColorPickerDialog",
        ["colorpicker"] = "DaisyColorPickerDialog",

        // Custom controls
        ["modifierkeys"] = "DaisyModifierKeys",
        ["weathericon"] = "DaisyWeatherIcon",
        ["weathercard"] = "DaisyWeatherCard",
        ["currentweather"] = "DaisyWeatherCurrent",
        ["weatherforecast"] = "DaisyWeatherForecast",
        ["weathermetrics"] = "DaisyWeatherMetrics",

        // Theming
        ["themecontroller"] = "DaisyThemeController",
        ["themedropdown"] = "DaisyThemeDropdown",
        ["themeswap"] = "DaisyThemeSwap",
        ["themeradio"] = "DaisyThemeRadio",
    };

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

    /// <summary>
    /// Gets the control name for this section, derived from SectionId using the mapping.
    /// Returns the SectionId as fallback if no mapping exists.
    /// </summary>
    public string ControlName
    {
        get
        {
            var sectionId = SectionId;
            if (string.IsNullOrEmpty(sectionId))
                return string.Empty;

            // Normalize: lowercase, remove hyphens and underscores
            var normalized = sectionId.ToLowerInvariant().Replace("-", "").Replace("_", "");

            return SectionIdToControlName.TryGetValue(normalized, out var controlName)
                ? controlName
                : sectionId; // Fallback to SectionId if no mapping
        }
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

    public async Task<bool> SaveScreenshotToFileAsync(string filePath, bool hideScreenshotButton = false)
    {
        var sectionContainer = FindSectionContainer();
        if (sectionContainer == null) return false;

        // Hide screenshot button during batch capture (using Opacity to avoid layout shift)
        var screenshotButton = this.FindControl<Flowery.Controls.DaisyButton>("ScreenshotButton");
        if (hideScreenshotButton && screenshotButton != null)
            screenshotButton.Opacity = 0;

        // Find parent ScrollViewer
        var scrollViewer = sectionContainer.FindAncestorOfType<ScrollViewer>();

        // Ensure the container is visible on screen
        sectionContainer.BringIntoView();

        // Give layout system time to scroll and render (increased for reliability)
        await Task.Delay(1000);

        // Smart Chunking Logic
        try
        {
            var bounds = sectionContainer.Bounds;

            // Check if section is taller than screen/viewport (approx 800px threshold or viewport height)
            double viewportHeight = 800;
            if (scrollViewer != null)
                viewportHeight = scrollViewer.Bounds.Height; // Use actual viewport height if available

            // If it fits in one go, use standard capture
            if (bounds.Height <= viewportHeight)
            {
                return CaptureAndSave(sectionContainer, filePath, scrollViewer);
            }

            // Otherwise, we need to scroll and capture chunks
            var totalHeight = bounds.Height;
            var baseFileName = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            var directory = Path.GetDirectoryName(filePath);

            // Smart Chunking Algorithm
            var contentPanel = sectionContainer;
            // Attempt to find the specific content panel (usually the next sibling of this header or header parent)
            if (sectionContainer is StackPanel stack && stack.Children.Count > 1)
            {
                // Find the index of this header (or assume it's at the top)
                // Actually, sectionContainer IS the StackPanel containing [Header, Content]
                // So the content is typically Children[1] (WrapPanel or StackPanel)
                // We will try to iterate the children of the content panel
            }

            var chunks = new System.Collections.Generic.List<double>(); // Scroll offsets
            // If we can find the content panel and its children, we can be smart
            // Otherwise fall back to fixed 800px chunks
            bool smartChunking = false;

            if (sectionContainer is Panel panel && panel.Children.Count > 1)
            {
                // Assume Children[0] is Header, Children[1] is Content
                // Or iterate to find SectionHeader type
                var headerIndex = -1;
                for(int i=0; i<panel.Children.Count; i++)
                {
                    if (panel.Children[i] == this)
                    {
                        headerIndex = i;
                        break;
                    }
                }

                if (headerIndex != -1 && headerIndex + 1 < panel.Children.Count)
                {
                    var contentElement = panel.Children[headerIndex + 1] as Panel;
                    if (contentElement != null)
                    {
                        // We found the content container (e.g. WrapPanel)
                        smartChunking = true;

                        // Calculate absolute Y position of content start relative to section
                        var contentStartY = contentElement.Bounds.Y; // Relative to section

                        // Get actual spacing from parent panel if it's a StackPanel
                        double spacing = 0;
                        if (panel is StackPanel sp)
                            spacing = sp.Spacing;

                        // Current "Cursor" for page building
                        double pageStart = 0;
                        double currentPageHeight = 0;

                        // Always start with header included in first page
                        currentPageHeight = contentStartY;

                        foreach (var child in contentElement.Children)
                        {
                            // Get child's actual height including any margin
                            var childMargin = child is Control c ? c.Margin : default;
                            var childHeight = child.Bounds.Height + childMargin.Top + childMargin.Bottom + spacing;

                            if (currentPageHeight + childHeight > viewportHeight && currentPageHeight > 100)
                            {
                                // Finish this page
                                chunks.Add(pageStart);

                                // Start next page
                                // New page starts at the top of this child
                                // Calculate absolute offset of this child relative to section start
                                var childTop = contentStartY + child.Bounds.Y;

                                pageStart = childTop;
                                currentPageHeight = 0; // Reset for new page
                            }

                            currentPageHeight += childHeight;
                        }

                        // Add final page
                        chunks.Add(pageStart);
                    }
                }
            }

            if (!smartChunking || chunks.Count == 0)
            {
                // Fallback to fixed chunks
                var count = (int)Math.Ceiling(totalHeight / viewportHeight);
                for (int i = 0; i < count; i++) chunks.Add(i * viewportHeight);
            }

            bool success = true;

            for (int i = 0; i < chunks.Count; i++)
            {
                var targetY = chunks[i];

                if (scrollViewer != null)
                {
                    // Find section position relative to ScrollViewer content
                    // We can translate relative to the ScrollViewer itself (which represents the viewport top-left)
                    // But we want relative to the CONTENT (so we know absolute scroll position)
                    // The easiest way is to use the section's bounds relative to the ScrollViewer, then add current Offset.

                    var sectionPos = sectionContainer.TranslatePoint(new Point(0, 0), scrollViewer);
                    if (sectionPos.HasValue)
                    {
                        // Calculate the absolute Y position in the scrollable content
                        // Current Visual Y + Current Scroll Offset = Absolute Y in Content
                        var absoluteSectionY = sectionPos.Value.Y + scrollViewer.Offset.Y;

                        // Scroll to the specific calculated offset
                        scrollViewer.Offset = new Vector(0, absoluteSectionY + targetY);
                        await Task.Delay(500); // Wait for scroll to settle
                    }
                }

                var suffix = "_" + (char)('a' + i);
                // If only 1 chunk was detected (e.g. smart chunking found it fits), don't add suffix
                if (chunks.Count == 1) suffix = "";

                var chunkPath = Path.Combine(directory!, $"{baseFileName}{suffix}{extension}");

                if (!CaptureAndSave(sectionContainer, chunkPath, scrollViewer))
                    success = false;
            }

            return success;
        }
        catch
        {
            return false;
        }
        finally
        {
            // Restore screenshot button visibility
            if (hideScreenshotButton && screenshotButton != null)
                screenshotButton.Opacity = 1;
        }
    }

    private bool CaptureAndSave(Control target, string path, ScrollViewer? scrollViewer = null)
    {
        byte[] pngBytes = Array.Empty<byte>();

#if WINDOWS
        if (OperatingSystem.IsWindows())
        {
            pngBytes = CaptureControlScreenshot(target, scrollViewer);
        }
        else
#endif
        {
            // Fallback for non-Windows (simple render)
            var pixelSize = new PixelSize(
                Math.Max(1, (int)Math.Ceiling(target.Bounds.Width)),
                Math.Max(1, (int)Math.Ceiling(target.Bounds.Height)));

            using var renderBitmap = new RenderTargetBitmap(pixelSize);
            renderBitmap.Render(target);

            using var stream = new MemoryStream();
            renderBitmap.Save(stream);
            pngBytes = stream.ToArray();
        }

        if (pngBytes == null || pngBytes.Length == 0) return false;

        File.WriteAllBytes(path, pngBytes);
        return true;
    }

    private async void OnScreenshotClick(object? sender, RoutedEventArgs e)
    {
        await CaptureWithDialogAsync();
    }

    private async Task CaptureWithDialogAsync()
    {
        var sectionContainer = FindSectionContainer();
        if (sectionContainer == null) return;

        var scrollViewer = sectionContainer.FindAncestorOfType<ScrollViewer>();
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        // Calculate chunks to determine if we need single file or folder
        var bounds = sectionContainer.Bounds;
        double viewportHeight = scrollViewer?.Bounds.Height ?? 800;
        var chunks = CalculateChunks(sectionContainer, scrollViewer, viewportHeight);

        var safeTitle = SanitizeFileName(Title);

        try
        {
            var storageProvider = topLevel.StorageProvider;

            if (chunks.Count <= 1)
            {
                // Single chunk - show Save As dialog
                var file = await storageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
                {
                    Title = "Save Screenshot",
                    SuggestedFileName = $"{safeTitle}.png",
                    DefaultExtension = "png",
                    FileTypeChoices = new[]
                    {
                        new FilePickerFileType("PNG Images") { Patterns = new[] { "*.png" } }
                    }
                });

                if (file == null) return;
                var filePath = file.Path.LocalPath;

                // Capture and save single file
                sectionContainer.BringIntoView();
                await Task.Delay(500);
                CaptureAndSave(sectionContainer, filePath, scrollViewer);
            }
            else
            {
                // Multiple chunks - show Folder Picker dialog
                var folders = await storageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
                {
                    Title = "Select Folder for Screenshots",
                    AllowMultiple = false
                });

                if (folders == null || folders.Count == 0) return;
                var folderPath = folders[0].Path.LocalPath;

                // Capture all chunks
                await SaveChunksToFolderAsync(sectionContainer, scrollViewer, chunks, folderPath, safeTitle, viewportHeight);
            }

            // Visual feedback
            var button = this.FindControl<Flowery.Controls.DaisyButton>("ScreenshotButton");
            if (button != null)
            {
                var originalContent = button.Content;
                button.Content = "✓";
                await Task.Delay(1000);
                button.Content = originalContent;
            }
        }
        catch (Exception)
        {
            // Silently fail
        }
    }

    private async Task SaveChunksToFolderAsync(Control sectionContainer, ScrollViewer? scrollViewer,
        System.Collections.Generic.List<double> chunks, string folderPath, string safeTitle, double viewportHeight)
    {
        for (int i = 0; i < chunks.Count; i++)
        {
            var targetY = chunks[i];

            if (scrollViewer != null)
            {
                var sectionPos = sectionContainer.TranslatePoint(new Point(0, 0), scrollViewer);
                if (sectionPos.HasValue)
                {
                    var absoluteSectionY = sectionPos.Value.Y + scrollViewer.Offset.Y;
                    scrollViewer.Offset = new Vector(0, absoluteSectionY + targetY);
                    await Task.Delay(500);
                }
            }

            var suffix = chunks.Count > 1 ? "_" + (char)('a' + i) : "";
            var filePath = Path.Combine(folderPath, $"{safeTitle}{suffix}.png");
            CaptureAndSave(sectionContainer, filePath, scrollViewer);
        }
    }

    private System.Collections.Generic.List<double> CalculateChunks(Control sectionContainer, ScrollViewer? scrollViewer, double viewportHeight)
    {
        var chunks = new System.Collections.Generic.List<double>();
        var bounds = sectionContainer.Bounds;

        // If it fits in one go, return single chunk at 0
        if (bounds.Height <= viewportHeight)
        {
            chunks.Add(0);
            return chunks;
        }

        // Try smart chunking based on children
        if (sectionContainer is Panel panel && panel.Children.Count > 1)
        {
            var headerIndex = -1;
            for (int i = 0; i < panel.Children.Count; i++)
            {
                if (panel.Children[i] == this)
                {
                    headerIndex = i;
                    break;
                }
            }

            if (headerIndex != -1 && headerIndex + 1 < panel.Children.Count)
            {
                var contentElement = panel.Children[headerIndex + 1] as Panel;
                if (contentElement != null)
                {
                    var contentStartY = contentElement.Bounds.Y;
                    double spacing = panel is StackPanel sp ? sp.Spacing : 0;

                    double pageStart = 0;
                    double currentPageHeight = contentStartY;

                    foreach (var child in contentElement.Children)
                    {
                        var childMargin = child is Control c ? c.Margin : default;
                        var childHeight = child.Bounds.Height + childMargin.Top + childMargin.Bottom + spacing;

                        if (currentPageHeight + childHeight > viewportHeight && currentPageHeight > 100)
                        {
                            chunks.Add(pageStart);
                            var childTop = contentStartY + child.Bounds.Y;
                            pageStart = childTop;
                            currentPageHeight = 0;
                        }

                        currentPageHeight += childHeight;
                    }

                    chunks.Add(pageStart);
                    return chunks;
                }
            }
        }

        // Fallback to fixed chunks
        var count = (int)Math.Ceiling(bounds.Height / viewportHeight);
        for (int i = 0; i < count; i++)
            chunks.Add(i * viewportHeight);

        return chunks;
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Join("_", name.Split(invalid, StringSplitOptions.RemoveEmptyEntries)).Replace(" ", "_");
    }

#if WINDOWS
    [SupportedOSPlatform("windows")]
    private static void SetBitmapClipboardData(byte[] pngBytes)
    {
        if (pngBytes == null || pngBytes.Length == 0) return;

        // WinForms clipboard requires STA thread
        var thread = new System.Threading.Thread(() =>
        {
            using var stream = new MemoryStream(pngBytes);
            using var image = System.Drawing.Image.FromStream(stream);
            System.Windows.Forms.Clipboard.SetImage(image);
        });
        thread.SetApartmentState(System.Threading.ApartmentState.STA);
        thread.Start();
        thread.Join();
    }

    [SupportedOSPlatform("windows")]
    private byte[] CaptureControlScreenshot(Control control, ScrollViewer? scrollViewer = null)
    {
        try
        {
            var topLevel = TopLevel.GetTopLevel(control);
            if (topLevel == null) return Array.Empty<byte>();

            // Get absolute screen coordinates of the control
            var controlTopLeft = control.PointToScreen(new Point(-8, 0)); // -8 for left padding
            var controlBottomRight = control.PointToScreen(new Point(control.Bounds.Width, control.Bounds.Height));

            int captureX = controlTopLeft.X;
            int captureY = controlTopLeft.Y;
            int captureWidth = Math.Abs(controlBottomRight.X - controlTopLeft.X);
            int captureHeight = Math.Abs(controlBottomRight.Y - controlTopLeft.Y);

            // CRITICAL: Clip to ScrollViewer viewport bounds
            // This prevents capturing content outside the visible scrollable area
            if (scrollViewer != null)
            {
                var viewportTopLeft = scrollViewer.PointToScreen(new Point(0, 0));
                var viewportBottomRight = scrollViewer.PointToScreen(new Point(scrollViewer.Bounds.Width, scrollViewer.Bounds.Height));

                int viewportX = viewportTopLeft.X;
                int viewportY = viewportTopLeft.Y;
                int viewportRight = viewportBottomRight.X;
                int viewportBottom = viewportBottomRight.Y;

                // Clip capture region to viewport
                if (captureX < viewportX)
                {
                    captureWidth -= (viewportX - captureX);
                    captureX = viewportX;
                }
                if (captureY < viewportY)
                {
                    captureHeight -= (viewportY - captureY);
                    captureY = viewportY;
                }
                if (captureX + captureWidth > viewportRight)
                    captureWidth = viewportRight - captureX;
                if (captureY + captureHeight > viewportBottom)
                    captureHeight = viewportBottom - captureY;
            }

            // Get screen bounds to prevent out-of-bounds capture (which results in black/error)
            var currentScreen = (topLevel as Window)?.Screens.ScreenFromVisual(control);

            if (currentScreen != null)
            {
                // Clip to screen working area (excludes taskbar)
                var screenBounds = currentScreen.WorkingArea;

                // If completely off-screen, return empty
                if (captureX >= screenBounds.Right || captureY >= screenBounds.Bottom ||
                    captureX + captureWidth <= screenBounds.X || captureY + captureHeight <= screenBounds.Y)
                {
                    return Array.Empty<byte>();
                }

                // Clip left/top
                if (captureX < screenBounds.X)
                {
                    captureWidth -= (screenBounds.X - captureX);
                    captureX = screenBounds.X;
                }
                if (captureY < screenBounds.Y)
                {
                    captureHeight -= (screenBounds.Y - captureY);
                    captureY = screenBounds.Y;
                }

                // Clip right/bottom
                if (captureX + captureWidth > screenBounds.Right)
                    captureWidth = screenBounds.Right - captureX;
                if (captureY + captureHeight > screenBounds.Bottom)
                    captureHeight = screenBounds.Bottom - captureY;
            }

            if (captureWidth <= 0 || captureHeight <= 0) return Array.Empty<byte>();

            // Use System.Drawing to capture the screen
            using var bitmap = new System.Drawing.Bitmap(captureWidth, captureHeight);
            using (var g = System.Drawing.Graphics.FromImage(bitmap))
            {
                // Capture from screen
                g.CopyFromScreen(captureX, captureY, 0, 0, new System.Drawing.Size(captureWidth, captureHeight));
            }

            using var stream = new MemoryStream();
            bitmap.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }
        catch (Exception)
        {
            return Array.Empty<byte>();
        }
    }
#endif

    private Control? FindSectionContainer()
    {
        // Walk up the visual tree to find the parent StackPanel that contains this header
        // Typically the structure is: StackPanel > [SectionHeader, WrapPanel/StackPanel with examples]
        var parent = this.Parent as Control;

        // The immediate parent should be the section StackPanel
        if (parent is StackPanel sectionPanel)
            return sectionPanel;

        // Fallback: if nested differently, return immediate parent
        return parent;
    }
}
