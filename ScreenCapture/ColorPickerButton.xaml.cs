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
        private Window? _popupHostWindow;
        public ColorPickerButton()
        {
            InitializeComponent();
            ApplySelectedColor(SelectedColor);
            IsVisibleChanged += OnColorPickerButtonIsVisibleChanged;
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

            SelectedColor = Picker.SelectedColor;
        }

        private void SwatchButton_Click(object sender, RoutedEventArgs e)
        {
            PickerPopup.IsOpen = !PickerPopup.IsOpen;
            e.Handled = true;
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
            SwatchButton.IsChecked = true;
            _popupHostWindow = Window.GetWindow(this);
            if (_popupHostWindow != null)
            {
                _popupHostWindow.PreviewMouseDown += PopupHostWindow_PreviewMouseDown;
                _popupHostWindow.Deactivated += PopupHostWindow_Deactivated;
            }
            Picker.Focus();
        }

        private void PickerPopup_Closed(object? sender, EventArgs e)
        {
            if (_popupHostWindow != null)
            {
                _popupHostWindow.PreviewMouseDown -= PopupHostWindow_PreviewMouseDown;
                _popupHostWindow.Deactivated -= PopupHostWindow_Deactivated;
                _popupHostWindow = null;
            }

            SwatchButton.IsChecked = false;
            PopupClosed?.Invoke(this, EventArgs.Empty);
        }

        private void PopupHostWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (!IsInsideColorPicker(e.OriginalSource as DependencyObject))
            {
                PickerPopup.IsOpen = false;
            }
        }

        private void PickerPopupContent_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PickerPopupContent_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void PopupHostWindow_Deactivated(object? sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (PickerPopup.IsOpen
                    && !PickerPopupContent.IsMouseOver
                    && !SwatchButton.IsMouseOver)
                {
                    PickerPopup.IsOpen = false;
                }
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        private bool IsInsideColorPicker(DependencyObject? source)
        {
            for (var current = source; current != null; current = VisualTreeHelper.GetParent(current))
            {
                if (current == this || current == PickerPopupContent)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
