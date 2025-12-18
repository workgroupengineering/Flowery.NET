using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Metadata;

namespace Flowery.Controls
{
    /// <summary>
    /// A selectable chip list for choosing multiple tags with an animated/organized layout.
    /// Supports automatic font scaling when contained within a FloweryScaleManager.EnableScaling="True" container.
    /// </summary>
    public class DaisyTagPicker : TemplatedControl
    {
        protected override Type StyleKeyOverride => typeof(DaisyTagPicker);

        private readonly List<string> _internalSelected = new();

        /// <summary>
        /// Defines the <see cref="Tags"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<string>?> TagsProperty =
            AvaloniaProperty.Register<DaisyTagPicker, IList<string>?>(nameof(Tags));

        /// <summary>
        /// Gets or sets the list of available tags.
        /// </summary>
        [Content]
        public IList<string>? Tags
        {
            get => GetValue(TagsProperty);
            set => SetValue(TagsProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="SelectedTags"/> property.
        /// </summary>
        public static readonly StyledProperty<IList<string>?> SelectedTagsProperty =
            AvaloniaProperty.Register<DaisyTagPicker, IList<string>?>(nameof(SelectedTags));

        /// <summary>
        /// Gets or sets the selected tags. When null, selection is managed internally.
        /// </summary>
        public IList<string>? SelectedTags
        {
            get => GetValue(SelectedTagsProperty);
            set => SetValue(SelectedTagsProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Size"/> property.
        /// </summary>
        public static readonly StyledProperty<DaisySize> SizeProperty =
            AvaloniaProperty.Register<DaisyTagPicker, DaisySize>(nameof(Size), DaisySize.Small);

        /// <summary>
        /// Gets or sets the size of the tag chips.
        /// </summary>
        public DaisySize Size
        {
            get => GetValue(SizeProperty);
            set => SetValue(SizeProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="Title"/> property.
        /// </summary>
        public static readonly StyledProperty<string> TitleProperty =
            AvaloniaProperty.Register<DaisyTagPicker, string>(nameof(Title), "Selected Tags");

        /// <summary>
        /// Gets or sets the title for the selected tags section.
        /// </summary>
        public string Title
        {
            get => GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        /// <summary>
        /// Defines the <see cref="SelectedTagsList"/> property.
        /// </summary>
        public static readonly DirectProperty<DaisyTagPicker, IEnumerable<string>> SelectedTagsListProperty =
            AvaloniaProperty.RegisterDirect<DaisyTagPicker, IEnumerable<string>>(
                nameof(SelectedTagsList),
                o => o.SelectedTagsList);

        private IEnumerable<string> _selectedTagsList = Array.Empty<string>();
        public IEnumerable<string> SelectedTagsList
        {
            get => _selectedTagsList;
            private set => SetAndRaise(SelectedTagsListProperty, ref _selectedTagsList, value);
        }

        /// <summary>
        /// Defines the <see cref="AvailableTagsList"/> property.
        /// </summary>
        public static readonly DirectProperty<DaisyTagPicker, IEnumerable<string>> AvailableTagsListProperty =
            AvaloniaProperty.RegisterDirect<DaisyTagPicker, IEnumerable<string>>(
                nameof(AvailableTagsList),
                o => o.AvailableTagsList);

        private IEnumerable<string> _availableTagsList = Array.Empty<string>();
        public IEnumerable<string> AvailableTagsList
        {
            get => _availableTagsList;
            private set => SetAndRaise(AvailableTagsListProperty, ref _availableTagsList, value);
        }

        /// <summary>
        /// Raised when the selection changes.
        /// </summary>
        public event EventHandler<IReadOnlyList<string>>? SelectionChanged;

        /// <summary>
        /// Internal command used by the template to toggle tags.
        /// </summary>
        public ICommand ToggleTagCommand { get; }

        static DaisyTagPicker()
        {
            TagsProperty.Changed.AddClassHandler<DaisyTagPicker>((s, _) => s.UpdateLists());
            SelectedTagsProperty.Changed.AddClassHandler<DaisyTagPicker>((s, _) => s.UpdateLists());
        }

        public DaisyTagPicker()
        {
            ToggleTagCommand = new ActionCommand<string>(tag => { if (tag != null) ToggleTag(tag); });
            UpdateLists();
        }

        private void UpdateLists()
        {
            var tags = Tags ?? Array.Empty<string>();
            var selected = SelectedTags ?? _internalSelected;

            SelectedTagsList = tags.Where(t => selected.Contains(t)).ToList();
            AvailableTagsList = tags.Where(t => !selected.Contains(t)).ToList();
        }

        public void ToggleTag(string tag)
        {
            var selected = SelectedTags ?? _internalSelected;

            var newSelected = selected.Contains(tag)
                ? selected.Where(t => t != tag).ToList()
                : selected.Concat(new[] { tag }).ToList();

            if (this.IsSet(SelectedTagsProperty))
            {
                SetCurrentValue(SelectedTagsProperty, newSelected);
            }
            else
            {
                _internalSelected.Clear();
                _internalSelected.AddRange(newSelected);
                UpdateLists();
            }

            SelectionChanged?.Invoke(this, newSelected);
        }

        private class ActionCommand<T> : ICommand
        {
            private readonly Action<T> _execute;
            public ActionCommand(Action<T> execute) => _execute = execute;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _execute((T)parameter!);
            public event EventHandler? CanExecuteChanged { add { } remove { } }
        }
    }
}
