using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;
using System.Windows.Threading;

namespace ScreenCapture
{
    public partial class DraggableText : System.Windows.Controls.UserControl
    {
        private bool _isSelected;
        private bool _isEditing;
        private DispatcherTimer? _fontSizeTimer;
        private int _fontSizeDirection; // 1: 拡大, -1: 縮小

        // XAMLの TextBox へのアクセス用プロパティ
        public System.Windows.Controls.TextBox TextBoxControl => TextBox;

        // 削除イベント
        public event EventHandler? DeleteRequested;

        public DraggableText()
        {
            InitializeComponent();

            // デバッグ: 背景を強制的に透明に設定
            TextBox.Background = System.Windows.Media.Brushes.Transparent;
            TextBox.BorderBrush = System.Windows.Media.Brushes.Transparent;
            
            // 編集中はドラッグ無効（入力を優先）
            TextBox.GotFocus += (_, __) =>
            {
                _isEditing = true;
                DragThumb.IsHitTestVisible = false;
                TextBox.IsHitTestVisible = true;
                // 編集中もバウンディングボックスとカラーパレットを表示
                if (_isSelected)
                {
                    BoundingBox.Visibility = Visibility.Visible;
                    ColorPalette.Visibility = Visibility.Visible;
                    FontSizeButtons.Visibility = Visibility.Visible;
                }
            };

            // 編集が終わったらドラッグ有効
            TextBox.LostFocus += (_, __) =>
            {
                _isEditing = false;
                DragThumb.IsHitTestVisible = true;
                TextBox.IsHitTestVisible = false;
                
                if (_isSelected)
                {
                    BoundingBox.Visibility = Visibility.Visible;
                    ColorPalette.Visibility = Visibility.Visible;
                    FontSizeButtons.Visibility = Visibility.Visible;
                }
            };

            // ドラッグ
            DragThumb.DragDelta += (s, e) =>
            {
                var left = Canvas.GetLeft(this);
                var top = Canvas.GetTop(this);

                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                Canvas.SetLeft(this, left + e.HorizontalChange);
                Canvas.SetTop(this, top + e.VerticalChange);
            };

            // ダブルクリックで編集モードに入る
            MouseDoubleClick += (_, __) =>
            {
                StartEdit();
            };

            // 色変更ボタン
            WhiteColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.White);
            WhiteColorButton.MouseDown += (s, e) => e.Handled = true;

            BlackColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Black);
            BlackColorButton.MouseDown += (s, e) => e.Handled = true;

            RedColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Red);
            RedColorButton.MouseDown += (s, e) => e.Handled = true;

            YellowColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Yellow);
            YellowColorButton.MouseDown += (s, e) => e.Handled = true;

            GreenColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Lime);
            GreenColorButton.MouseDown += (s, e) => e.Handled = true;

            BlueColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Blue);
            BlueColorButton.MouseDown += (s, e) => e.Handled = true;

            CyanColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Cyan);
            CyanColorButton.MouseDown += (s, e) => e.Handled = true;

            MagentaColorButton.Click += (_, __) => SetStyle(GetFontSize(), Colors.Magenta);
            MagentaColorButton.MouseDown += (s, e) => e.Handled = true;

            // 文字サイズ変更ボタン（押しっぱなし対応）
            FontSizeUpButton.PreviewMouseLeftButtonDown += (_, __) => StartFontSizeChange(1);
            FontSizeUpButton.PreviewMouseLeftButtonUp += (_, __) => StopFontSizeChange();
            FontSizeUpButton.MouseLeave += (_, __) => StopFontSizeChange();
            FontSizeUpButton.MouseDown += (s, e) => e.Handled = true;

            FontSizeDownButton.PreviewMouseLeftButtonDown += (_, __) => StartFontSizeChange(-1);
            FontSizeDownButton.PreviewMouseLeftButtonUp += (_, __) => StopFontSizeChange();
            FontSizeDownButton.MouseLeave += (_, __) => StopFontSizeChange();
            FontSizeDownButton.MouseDown += (s, e) => e.Handled = true;

            // 削除ボタン
            DeleteButton.Click += (s, e) =>
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            };
            DeleteButton.MouseDown += (s, e) => e.Handled = true;
        }

        // 編集開始
        public void StartEdit()
        {
            _isEditing = true;
            _isSelected = true;  // 選択状態にする
            TextBox.IsHitTestVisible = true;
            BoundingBox.Visibility = Visibility.Visible;  // バウンディングボックスを表示
            ColorPalette.Visibility = Visibility.Visible;  // カラーパレットを表示
            FontSizeButtons.Visibility = Visibility.Visible;  // フォントサイズボタンを表示
            TextBox.Focus();
            TextBox.SelectAll();
        }

        // 編集終了
        public void EndEdit()
        {
            if (_isEditing)
            {
                DragThumb.Focus();
            }
        }

        // 選択状態にする
        public void Select()
        {
            _isSelected = true;
            BoundingBox.Visibility = Visibility.Visible;
            ColorPalette.Visibility = Visibility.Visible;
            FontSizeButtons.Visibility = Visibility.Visible;
        }

        // 選択解除
        public void Deselect()
        {
            _isSelected = false;
            BoundingBox.Visibility = Visibility.Collapsed;
            ColorPalette.Visibility = Visibility.Collapsed;
            FontSizeButtons.Visibility = Visibility.Collapsed;
            StopFontSizeChange();
        }

        public bool IsSelected => _isSelected;

        public void SetStyle(double fontSize, MediaColor color)
        {
            TextBox.FontSize = fontSize;
            TextBox.Foreground = new SolidColorBrush(color);
            TextBox.CaretBrush = new SolidColorBrush(color);
        }

        public double GetFontSize() => TextBox.FontSize;

        public MediaColor GetColor()
            => TextBox.Foreground is SolidColorBrush b ? b.Color : Colors.Red;

        // 文字サイズ変更の開始（押しっぱなし対応）
        private void StartFontSizeChange(int direction)
        {
            _fontSizeDirection = direction;
            
            // 最初の1回を即座に実行
            ChangeFontSize();
            
            // タイマーを開始（100msごとに実行）
            if (_fontSizeTimer == null)
            {
                _fontSizeTimer = new DispatcherTimer
                {
                    Interval = TimeSpan.FromMilliseconds(100)
                };
                _fontSizeTimer.Tick += (s, e) => ChangeFontSize();
            }
            
            _fontSizeTimer.Start();
        }

        // 文字サイズ変更の停止
        private void StopFontSizeChange()
        {
            _fontSizeTimer?.Stop();
        }

        // 文字サイズを変更
        private void ChangeFontSize()
        {
            var newSize = GetFontSize() + (_fontSizeDirection * 2);
            newSize = Math.Max(8, Math.Min(200, newSize)); // 8～200の範囲
            SetStyle(newSize, GetColor());
        }
    }
}

