using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfClipboard = System.Windows.Clipboard;
using WpfCursors = System.Windows.Input.Cursors;

namespace ScreenCapture
{
    public partial class SelectionOverlayWindow : Window
    {
        private const uint GaRoot = 2;
        private Point _start;
        private bool _dragging;

        [StructLayout(LayoutKind.Sequential)]
        private struct NativePoint
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct NativeRect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        [DllImport("user32.dll")]
        private static extern IntPtr WindowFromPoint(NativePoint point);

        [DllImport("user32.dll")]
        private static extern IntPtr GetAncestor(IntPtr hwnd, uint gaFlags);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetWindowRect(IntPtr hwnd, out NativeRect rect);

        public SelectionOverlayWindow()
        {
            InitializeComponent();

            // 仮想スクリーン全体（マルチモニタ対応の基礎）
            Left = SystemParameters.VirtualScreenLeft;
            Top = SystemParameters.VirtualScreenTop;
            Width = SystemParameters.VirtualScreenWidth;
            Height = SystemParameters.VirtualScreenHeight;

            Cursor = WpfCursors.Cross;

            // Escキーでキャンセル
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };
        }

        private void OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                var pos = e.GetPosition(this);
                var screenPoint = new Point(
                    pos.X + SystemParameters.VirtualScreenLeft,
                    pos.Y + SystemParameters.VirtualScreenTop);
                CaptureWindowAtPoint(screenPoint);
                e.Handled = true;
                return;
            }

            _start = e.GetPosition(this);
            _dragging = true;
            CaptureMouse();

            SelectionRect.Visibility = Visibility.Visible;
            UpdateRect(_start, _start);
        }

        private void OnMouseMove(object sender, WpfMouseEventArgs e)
        {
            if (!_dragging) return;
            var pos = e.GetPosition(this);
            UpdateRect(_start, pos);
        }

        private void OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!_dragging) return;
            _dragging = false;
            ReleaseMouseCapture();

            var end = e.GetPosition(this);
            var r = NormalizeRect(_start, end);

            // Window内座標 → 画面座標へ
            var screenRect = new System.Drawing.Rectangle(
                (int)(r.X + SystemParameters.VirtualScreenLeft),
                (int)(r.Y + SystemParameters.VirtualScreenTop),
                (int)r.Width,
                (int)r.Height);

            if (screenRect.Width < 2 || screenRect.Height < 2)
            {
                Close();
                return;
            }

            // キャプチャ前にオーバーレイを非表示にして、元の明るさでキャプチャ
            Hide();
            
            // UIが更新されるのを待ってからキャプチャ
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            System.Threading.Thread.Sleep(50); // 画面描画の完了を確実にするため
            
            var bmp = CaptureScreen(screenRect);
            var bitmapSource = ToBitmapSource(bmp);

            // キャプチャした画像をクリップボードに保存
            WpfClipboard.SetImage(bitmapSource);

            var cap = new CaptureWindow(bitmapSource, screenRect.Location);
            cap.Show();

            Close();
        }

        private void UpdateRect(Point a, Point b)
        {
            var r = NormalizeRect(a, b);
            System.Windows.Controls.Canvas.SetLeft(SelectionRect, r.X);
            System.Windows.Controls.Canvas.SetTop(SelectionRect, r.Y);
            SelectionRect.Width = r.Width;
            SelectionRect.Height = r.Height;
        }

        private static Rect NormalizeRect(Point a, Point b)
        {
            var x = Math.Min(a.X, b.X);
            var y = Math.Min(a.Y, b.Y);
            var w = Math.Abs(a.X - b.X);
            var h = Math.Abs(a.Y - b.Y);
            return new Rect(x, y, w, h);
        }

        private static System.Drawing.Bitmap CaptureScreen(System.Drawing.Rectangle rect)
        {
            var bmp = new System.Drawing.Bitmap(
                rect.Width, rect.Height,
                System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using var g = System.Drawing.Graphics.FromImage(bmp);
            g.CopyFromScreen(
                rect.Left, rect.Top, 0, 0, rect.Size,
                System.Drawing.CopyPixelOperation.SourceCopy);

            return bmp;
        }


        private static BitmapSource ToBitmapSource(Bitmap bitmap)
        {
            using var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            ms.Position = 0;

            var img = new BitmapImage();
            img.BeginInit();
            img.CacheOption = BitmapCacheOption.OnLoad;
            img.StreamSource = ms;
            img.EndInit();
            img.Freeze();
            return img;
        }

        private void CaptureWindowAtPoint(Point screenPoint)
        {
            Hide();
            Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            System.Threading.Thread.Sleep(50);

            var dpi = VisualTreeHelper.GetDpi(this);
            var hwnd = WindowFromPoint(new NativePoint
            {
                X = (int)Math.Round(screenPoint.X * dpi.DpiScaleX),
                Y = (int)Math.Round(screenPoint.Y * dpi.DpiScaleY)
            });
            if (hwnd == IntPtr.Zero)
            {
                Close();
                return;
            }

            hwnd = GetAncestor(hwnd, GaRoot);
            if (hwnd == IntPtr.Zero || !GetWindowRect(hwnd, out var rect))
            {
                Close();
                return;
            }

            var screenRect = new Rectangle(
                rect.Left,
                rect.Top,
                rect.Right - rect.Left,
                rect.Bottom - rect.Top);

            if (screenRect.Width < 2 || screenRect.Height < 2)
            {
                Close();
                return;
            }

            var bmp = CaptureScreen(screenRect);
            var bitmapSource = ToBitmapSource(bmp);

            WpfClipboard.SetImage(bitmapSource);

            var cap = new CaptureWindow(bitmapSource, screenRect.Location);
            cap.Show();

            Close();
        }
    }
}
