using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Forms = System.Windows.Forms;
using MediaColor = System.Windows.Media.Color;

namespace ScreenCapture
{
    public partial class PaintToolbarWindow : Window
    {
        public event Action<MediaColor>? ColorSelected;
        public event Action<double>? ThicknessSelected;
        public event Action? UndoRequested;
        public event Action? RedoRequested;
        public event Action<int>? UndoLimitChanged;
        public event Action? ToggleRequested;

        public PaintToolbarWindow()
        {
            InitializeComponent();

            PaintColorPicker.Background = new SolidColorBrush(TextStyleSettings.PaintColor);

            MouseLeftButtonDown += (_, __) => DragMove();
            PreviewKeyDown += OnPreviewKeyDown;

            PaintColorPicker.Click += (_, __) => OpenColorDialog();

            PaintThickness1.Click += (_, __) => SelectThickness(1, PaintThickness1);
            PaintThickness3.Click += (_, __) => SelectThickness(3, PaintThickness3);
            PaintThickness5.Click += (_, __) => SelectThickness(5, PaintThickness5);
            PaintThickness10.Click += (_, __) => SelectThickness(10, PaintThickness10);

            UndoButton.Click += (_, __) => UndoRequested?.Invoke();
            RedoButton.Click += (_, __) => RedoRequested?.Invoke();

            UndoLimitComboBox.SelectionChanged += (_, __) =>
            {
                if (UndoLimitComboBox.SelectedItem is ComboBoxItem item
                    && int.TryParse(item.Content?.ToString(), out var limit))
                {
                    UndoLimitChanged?.Invoke(limit);
                }
            };

            SelectThicknessFromSettings(TextStyleSettings.PaintThickness);
        }

        private void OnPreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (IsAltKeyPressed(e) && !e.IsRepeat)
            {
                ToggleRequested?.Invoke();
                e.Handled = true;
            }
        }

        private static bool IsAltKeyPressed(System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                return true;
            }

            return e.Key == Key.System
                && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt);
        }

        private void OpenColorDialog()
        {
            using var dialog = new Forms.ColorDialog
            {
                FullOpen = true
            };

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            var color = MediaColor.FromArgb(
                dialog.Color.A,
                dialog.Color.R,
                dialog.Color.G,
                dialog.Color.B);

            ColorSelected?.Invoke(color);
            PaintColorPicker.Background = new SolidColorBrush(color);
        }

        public void ApplySettings(MediaColor color, double thickness)
        {
            PaintColorPicker.Background = new SolidColorBrush(color);
            ColorSelected?.Invoke(color);
            SelectThicknessFromSettings(thickness);
        }

        private void SelectThicknessFromSettings(double thickness)
        {
            var selected = thickness switch
            {
                1 => (1, PaintThickness1),
                3 => (3, PaintThickness3),
                5 => (5, PaintThickness5),
                10 => (10, PaintThickness10),
                _ => (3, PaintThickness3)
            };

            SelectThickness(selected.Item1, selected.Item2);
        }

        private void SelectThickness(double thickness, System.Windows.Controls.Button selectedButton)
        {
            ThicknessSelected?.Invoke(thickness);

            var buttons = new[]
            {
                PaintThickness1,
                PaintThickness3,
                PaintThickness5,
                PaintThickness10
            };

            foreach (var button in buttons)
            {
                var isSelected = button == selectedButton;
                button.BorderBrush = isSelected ? System.Windows.Media.Brushes.Gold : System.Windows.Media.Brushes.White;
                button.BorderThickness = isSelected ? new Thickness(3) : new Thickness(2);
            }
        }

        public void SetUndoRedoEnabled(bool canUndo, bool canRedo)
        {
            UndoButton.IsEnabled = canUndo;
            UndoButton.Opacity = canUndo ? 1.0 : 0.5;

            RedoButton.IsEnabled = canRedo;
            RedoButton.Opacity = canRedo ? 1.0 : 0.5;
        }
    }
}
