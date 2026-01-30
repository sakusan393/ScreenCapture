using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenCapture
{
    public partial class DraggableImage : UserControl
    {
        private bool _isSelected;
        private Point _rotateStartPoint;
        private double _rotateStartAngle;
        private Point _resizeStartPoint;
        private Size _resizeStartSize;

        public DraggableImage(BitmapSource image)
        {
            InitializeComponent();

            ImageControl.Source = image;
            Width = image.PixelWidth;
            Height = image.PixelHeight;

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

            // クリックで選択状態を切り替え
            DragThumb.MouseDown += (s, e) =>
            {
                Select();
                e.Handled = true;
            };

            // 回転ハンドル
            RotateHandle.DragStarted += OnRotateStarted;
            RotateHandle.DragDelta += OnRotateDelta;

            // リサイズハンドル
            TopLeftHandle.DragStarted += OnResizeStarted;
            TopLeftHandle.DragDelta += (s, e) => OnResizeDelta(e, -1, -1);

            TopRightHandle.DragStarted += OnResizeStarted;
            TopRightHandle.DragDelta += (s, e) => OnResizeDelta(e, 1, -1);

            BottomLeftHandle.DragStarted += OnResizeStarted;
            BottomLeftHandle.DragDelta += (s, e) => OnResizeDelta(e, -1, 1);

            BottomRightHandle.DragStarted += OnResizeStarted;
            BottomRightHandle.DragDelta += (s, e) => OnResizeDelta(e, 1, 1);
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
            var center = new Point(ActualWidth / 2, ActualHeight / 2);

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
            _resizeStartPoint = Mouse.GetPosition(Parent as UIElement);
            _resizeStartSize = new Size(ActualWidth, ActualHeight);
        }

        // リサイズ中
        private void OnResizeDelta(DragDeltaEventArgs e, int xDirection, int yDirection)
        {
            var newWidth = _resizeStartSize.Width + e.HorizontalChange * xDirection * 2;
            var newHeight = _resizeStartSize.Height + e.VerticalChange * yDirection * 2;

            // 最小サイズを設定
            newWidth = Math.Max(50, newWidth);
            newHeight = Math.Max(50, newHeight);

            // アスペクト比を維持する場合
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                var aspectRatio = _resizeStartSize.Width / _resizeStartSize.Height;
                newHeight = newWidth / aspectRatio;
            }

            Width = newWidth;
            Height = newHeight;

            // スケールを更新
            ScaleTransform.ScaleX = newWidth / (ImageControl.Source as BitmapSource).PixelWidth;
            ScaleTransform.ScaleY = newHeight / (ImageControl.Source as BitmapSource).PixelHeight;
        }
    }
}
