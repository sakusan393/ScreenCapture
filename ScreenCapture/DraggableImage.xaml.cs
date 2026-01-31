using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Forms = System.Windows.Forms;
using MediaColor = System.Windows.Media.Color;

namespace ScreenCapture
{
    public partial class DraggableImage : System.Windows.Controls.UserControl
    {
        private bool _isSelected;
        private MediaColor _borderColor = TextStyleSettings.ImageBorderColor;
        private int _borderThicknessIndex = -1;
        private readonly double[] _borderThicknesses = { 10, 20 };
        private System.Windows.Point _rotateStartPoint;
        private double _rotateStartAngle;
        private System.Windows.Point _dragStartPosition;
        private double _initialWidth;
        private double _initialHeight;
        private System.Windows.Point _initialCanvasPosition;

        // 削除イベント
        public event EventHandler? DeleteRequested;

        public DraggableImage(BitmapSource image)
        {
            InitializeComponent();

            ImageControl.Source = image;
            Width = image.PixelWidth;
            Height = image.PixelHeight;

            UpdateBorderVisibility();

            // ドラッグで移動
            DragThumb.DragDelta += (s, e) =>
            {
                var left = Canvas.GetLeft(this);
                var top = Canvas.GetTop(this);

                if (double.IsNaN(left)) left = 0;
                if (double.IsNaN(top)) top = 0;

                Canvas.SetLeft(this, left + e.HorizontalChange);
                Canvas.SetTop(this, top + e.VerticalChange);
            };

            // クリックで選択状態を切り替え（イベントは伝播させる）
            MouseLeftButtonDown += (s, e) =>
            {
                // 常に選択状態にする（CaptureWindowで他の選択を解除済み）
                Select();
            };

            // 回転ハンドル
            RotateHandle.DragStarted += OnRotateStarted;
            RotateHandle.DragDelta += OnRotateDelta;
            RotateHandle.MouseDown += (s, e) => e.Handled = true; // 親への伝播を防ぐ

            // リサイズハンドル
            TopLeftHandle.DragStarted += OnResizeStarted;
            TopLeftHandle.DragDelta += (s, e) => OnResizeDelta(e, -1, -1);
            TopLeftHandle.MouseDown += (s, e) => e.Handled = true;

            TopRightHandle.DragStarted += OnResizeStarted;
            TopRightHandle.DragDelta += (s, e) => OnResizeDelta(e, 1, -1);
            TopRightHandle.MouseDown += (s, e) => e.Handled = true;

            BottomLeftHandle.DragStarted += OnResizeStarted;
            BottomLeftHandle.DragDelta += (s, e) => OnResizeDelta(e, -1, 1);
            BottomLeftHandle.MouseDown += (s, e) => e.Handled = true;

            BottomRightHandle.DragStarted += OnResizeStarted;
            BottomRightHandle.DragDelta += (s, e) => OnResizeDelta(e, 1, 1);
            BottomRightHandle.MouseDown += (s, e) => e.Handled = true;

            BorderColorButton.Click += (_, e) =>
            {
                ChangeBorderColor();
                e.Handled = true;
            };
            BorderColorButton.MouseDown += (s, e) => e.Handled = true;

            BorderToggleButton.Click += (_, e) =>
            {
                ToggleBorder();
                e.Handled = true;
            };
            BorderToggleButton.MouseDown += (s, e) => e.Handled = true;

            // 削除ボタン
            DeleteButton.Click += (s, e) =>
            {
                DeleteRequested?.Invoke(this, EventArgs.Empty);
                e.Handled = true;
            };
            DeleteButton.MouseDown += (s, e) => e.Handled = true;

            UpdateBorderVisibility();
        }

        // 選択状態にする
        public void Select()
        {
            _isSelected = true;
            BoundingBox.Visibility = Visibility.Visible;
        }

        // 選択解除
        public void Deselect()
        {
            _isSelected = false;
            BoundingBox.Visibility = Visibility.Collapsed;
        }

        public bool IsSelected => _isSelected;

        private void ToggleBorder()
        {
            if (_borderThicknessIndex < 0)
            {
                _borderThicknessIndex = 0;
            }
            else if (_borderThicknessIndex == 0)
            {
                _borderThicknessIndex = 1;
            }
            else
            {
                _borderThicknessIndex = -1;
            }
            UpdateBorderVisibility();
        }

        private void ChangeBorderColor()
        {
            using var dialog = new Forms.ColorDialog
            {
                FullOpen = true,
                Color = System.Drawing.Color.FromArgb(
                    _borderColor.A,
                    _borderColor.R,
                    _borderColor.G,
                    _borderColor.B)
            };

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            _borderColor = MediaColor.FromArgb(
                dialog.Color.A,
                dialog.Color.R,
                dialog.Color.G,
                dialog.Color.B);
            TextStyleSettings.ImageBorderColor = _borderColor;
            UpdateBorderVisibility();
        }

        private void UpdateBorderVisibility()
        {
            var isVisible = _borderThicknessIndex >= 0;
            ImageBorder.Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            ImageBorder.BorderBrush = new SolidColorBrush(_borderColor);
            ImageBorder.BorderThickness = new Thickness(
                isVisible ? _borderThicknesses[_borderThicknessIndex] : _borderThicknesses[0]);
            BorderToggleButton.Opacity = isVisible ? 1.0 : 0.6;
        }

        // 回転開始
        private void OnRotateStarted(object sender, DragStartedEventArgs e)
        {
            _rotateStartPoint = Mouse.GetPosition(this);
            _rotateStartAngle = RotateTransform.Angle;
        }

        // 回転中
        private void OnRotateDelta(object sender, DragDeltaEventArgs e)
        {
            var currentPoint = Mouse.GetPosition(this);
            var center = new System.Windows.Point(ActualWidth / 2, ActualHeight / 2);

            // 開始点と現在点の角度を計算
            var startVector = _rotateStartPoint - center;
            var currentVector = currentPoint - center;

            var startAngle = Math.Atan2(startVector.Y, startVector.X);
            var currentAngle = Math.Atan2(currentVector.Y, currentVector.X);

            var deltaAngle = (currentAngle - startAngle) * 180 / Math.PI;

            RotateTransform.Angle = _rotateStartAngle + deltaAngle;
        }

        // リサイズ開始
        private void OnResizeStarted(object sender, DragStartedEventArgs e)
        {
            _dragStartPosition = Mouse.GetPosition(Parent as UIElement);
            _initialWidth = ActualWidth;
            _initialHeight = ActualHeight;
            
            var left = Canvas.GetLeft(this);
            var top = Canvas.GetTop(this);
            if (double.IsNaN(left)) left = 0;
            if (double.IsNaN(top)) top = 0;
            _initialCanvasPosition = new System.Windows.Point(left, top);
        }

        // リサイズ中
        private void OnResizeDelta(DragDeltaEventArgs e, int xDirection, int yDirection)
        {
            var currentPosition = Mouse.GetPosition(Parent as UIElement);
            var delta = currentPosition - _dragStartPosition;

            // 新しいサイズを計算
            var newWidth = _initialWidth + delta.X * xDirection;
            var newHeight = _initialHeight + delta.Y * yDirection;

            // 最小サイズを設定
            newWidth = Math.Max(50, newWidth);
            newHeight = Math.Max(50, newHeight);

            // アスペクト比を維持する場合
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var aspectRatio = _initialWidth / _initialHeight;
                var widthByHeight = newHeight * aspectRatio;
                var heightByWidth = newWidth / aspectRatio;
                
                if (Math.Abs(newWidth - _initialWidth) > Math.Abs(newHeight - _initialHeight))
                {
                    newHeight = heightByWidth;
                }
                else
                {
                    newWidth = widthByHeight;
                }
            }

            // サイズを更新
            Width = newWidth;
            Height = newHeight;

            // 位置を調整（左上や上側のハンドルをドラッグする場合）
            var newLeft = _initialCanvasPosition.X;
            var newTop = _initialCanvasPosition.Y;

            if (xDirection < 0)
            {
                newLeft = _initialCanvasPosition.X - (newWidth - _initialWidth);
            }
            if (yDirection < 0)
            {
                newTop = _initialCanvasPosition.Y - (newHeight - _initialHeight);
            }

            Canvas.SetLeft(this, newLeft);
            Canvas.SetTop(this, newTop);
        }
    }
}
