using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ScreenCapture
{
    public partial class PaintToolbarWindow : Window
    {
        public event Action<Color>? ColorSelected;
        public event Action<double>? ThicknessSelected;
        public event Action? UndoRequested;
        public event Action? RedoRequested;
        public event Action<int>? UndoLimitChanged;

        public PaintToolbarWindow()
        {
            InitializeComponent();

            MouseLeftButtonDown += (_, __) => DragMove();

            PaintColorWhite.Click += (_, __) => ColorSelected?.Invoke(Colors.White);
            PaintColorBlack.Click += (_, __) => ColorSelected?.Invoke(Colors.Black);
            PaintColorRed.Click += (_, __) => ColorSelected?.Invoke(Colors.Red);
            PaintColorYellow.Click += (_, __) => ColorSelected?.Invoke(Colors.Yellow);
            PaintColorGreen.Click += (_, __) => ColorSelected?.Invoke(Colors.Lime);
            PaintColorBlue.Click += (_, __) => ColorSelected?.Invoke(Colors.Blue);

            PaintThickness1.Click += (_, __) => ThicknessSelected?.Invoke(1);
            PaintThickness3.Click += (_, __) => ThicknessSelected?.Invoke(3);
            PaintThickness5.Click += (_, __) => ThicknessSelected?.Invoke(5);
            PaintThickness10.Click += (_, __) => ThicknessSelected?.Invoke(10);

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
