using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using Avalonia.VisualTree;

namespace Flowery.Controls
{
    public enum DockSize
    {
        Medium,
        ExtraSmall,
        Small,
        Large,
        ExtraLarge
    }

    public class DockItemSelectedEventArgs : RoutedEventArgs
    {
        public Control Item { get; }

        public DockItemSelectedEventArgs(RoutedEvent routedEvent, Control item)
            : base(routedEvent)
        {
            Item = item;
        }
    }

    public class DaisyDock : ItemsControl
    {
        public static readonly StyledProperty<DockSize> SizeProperty =
            AvaloniaProperty.Register<DaisyDock, DockSize>(nameof(Size), DockSize.Medium);

        public DockSize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        public static readonly StyledProperty<bool> AutoSelectProperty =
            AvaloniaProperty.Register<DaisyDock, bool>(nameof(AutoSelect), true);

        /// <summary>
        /// Gets or sets a value indicating whether clicking an item automatically applies the 'dock-active' class
        /// and removes it from other items. Defaults to true.
        /// </summary>
        public bool AutoSelect
        {
            get => GetValue(AutoSelectProperty);
            set => SetValue(AutoSelectProperty, value);
        }

        public static readonly RoutedEvent<DockItemSelectedEventArgs> ItemSelectedEvent =
            RoutedEvent.Register<DaisyDock, DockItemSelectedEventArgs>(nameof(ItemSelected), RoutingStrategies.Bubble);

        public event EventHandler<DockItemSelectedEventArgs> ItemSelected
        {
            add => AddHandler(ItemSelectedEvent, value);
            remove => RemoveHandler(ItemSelectedEvent, value);
        }

        protected override Type StyleKeyOverride => typeof(DaisyDock);

        public DaisyDock()
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

                RaiseEvent(new DockItemSelectedEventArgs(ItemSelectedEvent, button));
            }
        }

        private void UpdateSelection(Button selectedButton)
        {
            foreach (var child in this.GetLogicalChildren())
            {
                if (child is Button btn)
                {
                    btn.Classes.Set("dock-active", btn == selectedButton);
                }
            }
        }
    }
}
