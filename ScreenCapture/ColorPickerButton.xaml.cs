using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ScreenCapture
{
    public partial class ColorPickerButton : System.Windows.Controls.UserControl
    {
        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor),
            typeof(MediaColor),
            typeof(ColorPickerButton),
            new FrameworkPropertyMetadata(
                System.Windows.Media.Colors.Transparent,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                OnSelectedColorPropertyChanged));

        private bool _isSynchronizingPicker;
        private bool _isPointerInteraction;
        private bool _colorChangedDuringPointerInteraction;

        public ColorPickerButton()
        {
            InitializeComponent();
            ApplySelectedColor(SelectedColor);
            IsVisibleChanged += OnColorPickerButtonIsVisibleChanged;
            Picker.AddHandler(
                Mouse.MouseUpEvent,
                new MouseButtonEventHandler(Picker_MouseButtonUp),
                true);
        }

        public MediaColor SelectedColor
        {
            get => (MediaColor)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public bool IsPopupOpen => PickerPopup.IsOpen;

        public event EventHandler? ColorChanged;

        public event EventHandler? PopupClosed;

        private void OnColorPickerButtonIsVisibleChanged(
            object sender,
            DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is false)
            {
                PickerPopup.IsOpen = false;
            }
        }

        private static void OnSelectedColorPropertyChanged(
            DependencyObject dependencyObject,
            DependencyPropertyChangedEventArgs e)
        {
            var control = (ColorPickerButton)dependencyObject;
            control.ApplySelectedColor((MediaColor)e.NewValue);
            control.ColorChanged?.Invoke(control, EventArgs.Empty);
        }

        private void ApplySelectedColor(MediaColor color)
        {
            SwatchColor.Background = new SolidColorBrush(color);

            if (Picker.SelectedColor == color)
            {
                return;
            }

            _isSynchronizingPicker = true;
            Picker.SelectedColor = color;
            _isSynchronizingPicker = false;
        }

        private void Picker_ColorChanged(object sender, RoutedEventArgs e)
        {
            if (_isSynchronizingPicker)
            {
                return;
            }

            if (_isPointerInteraction)
            {
                _colorChangedDuringPointerInteraction = true;
            }

            SelectedColor = Picker.SelectedColor;
        }

        private void Picker_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isPointerInteraction = true;
            _colorChangedDuringPointerInteraction = false;
        }

        private void Picker_MouseButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isPointerInteraction = false;

            if (_colorChangedDuringPointerInteraction)
            {
                PickerPopup.IsOpen = false;
            }
        }

        private void Picker_PreviewKeyDown(object sender, WpfKeyEventArgs e)
        {
            if (e.Key is Key.Enter or Key.Escape)
            {
                PickerPopup.IsOpen = false;
                e.Handled = true;
            }
        }

        private void PickerPopup_Opened(object? sender, EventArgs e)
        {
            ApplySelectedColor(SelectedColor);
            _isPointerInteraction = false;
            _colorChangedDuringPointerInteraction = false;
            Picker.Focus();
        }

        private void PickerPopup_Closed(object? sender, EventArgs e)
        {
            _isPointerInteraction = false;
            _colorChangedDuringPointerInteraction = false;
            PopupClosed?.Invoke(this, EventArgs.Empty);
        }
    }
}
