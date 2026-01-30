using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ScreenCapture
{
    public partial class CaptureWindow : Window
    {
        private DraggableText? _selectedText;
        private DraggableImage? _selectedImage;
        private bool _isDraggingWindow;
        private Point _dragStartPoint;

        public CaptureWindow(BitmapSource image, System.Drawing.Point screenLocation)
        {
            InitializeComponent();

            CaptureImage.Source = image;

            Left = screenLocation.X;
            Top = screenLocation.Y;
            Width = image.PixelWidth;
            Height = image.PixelHeight;

            // Escキーでウィンドウを閉じる
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                    Close();
            };

            // Ctrl+Vでクリップボードから画像を貼り付け
            KeyDown += OnKeyDown;

            // ウィンドウドラッグ機能（背景をドラッグで移動）
            CaptureImage.MouseLeftButtonDown += (s, e) =>
            {
                // テキストからフォーカスを外す
                ClearTextFocus();
                
                _isDraggingWindow = true;
                _dragStartPoint = e.GetPosition(this);
                CaptureImage.CaptureMouse();
                e.Handled = true;
            };

            CaptureImage.MouseMove += (s, e) =>
            {
                if (_isDraggingWindow)
                {
                    Point current = e.GetPosition(this);
                    Left += current.X - _dragStartPoint.X;
                    Top += current.Y - _dragStartPoint.Y;
                }
            };

            CaptureImage.MouseLeftButtonUp += (s, e) =>
            {
                if (_isDraggingWindow)
                {
                    _isDraggingWindow = false;
                    CaptureImage.ReleaseMouseCapture();
                }
            };

            // マウスが入ったら枠線を非表示
            MouseEnter += (s, e) => BorderFrame.Visibility = Visibility.Collapsed;
            
            // マウスが出たら枠線を表示（キャプチャエリアを視認できるように）
            MouseLeave += (s, e) => BorderFrame.Visibility = Visibility.Visible;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Canvas上でもウィンドウドラッグを可能にする（テキストがない場所をドラッグ）
            OverlayCanvas.MouseLeftButtonDown += (s, ev) =>
            {
                // Canvas自体をクリックした場合のみ（子要素でない場合）
                if (ev.Source == OverlayCanvas)
                {
                    // テキストからフォーカスを外す
                    ClearTextFocus();
                    
                    _isDraggingWindow = true;
                    _dragStartPoint = ev.GetPosition(this);
                    OverlayCanvas.CaptureMouse();
                    ev.Handled = true;
                }
            };

            OverlayCanvas.MouseMove += (s, ev) =>
            {
                if (_isDraggingWindow && OverlayCanvas.IsMouseCaptured)
                {
                    Point current = ev.GetPosition(this);
                    Left += current.X - _dragStartPoint.X;
                    Top += current.Y - _dragStartPoint.Y;
                }
            };

            OverlayCanvas.MouseLeftButtonUp += (s, ev) =>
            {
                if (_isDraggingWindow && OverlayCanvas.IsMouseCaptured)
                {
                    _isDraggingWindow = false;
                    OverlayCanvas.ReleaseMouseCapture();
                }
            };

            // 右クリックメニュー等の初期化
            var menu = new ContextMenu();

            var add = new MenuItem { Header = "テキスト追加" };
            add.Click += (_, __) => AddTextAt(Mouse.GetPosition(OverlayCanvas));
            menu.Items.Add(add);

            menu.Items.Add(new Separator());

            var pasteImage = new MenuItem { Header = "画像を貼り付け (Ctrl+V)" };
            pasteImage.Click += (_, __) => PasteImageFromClipboard();
            menu.Items.Add(pasteImage);

            menu.Items.Add(new Separator());

            var bigger = new MenuItem { Header = "文字を大きく (+2)" };
            bigger.Click += (_, __) =>
            {
                if (_selectedText == null) return;
                _selectedText.SetStyle(_selectedText.GetFontSize() + 2, _selectedText.GetColor());
            };
            menu.Items.Add(bigger);

            var smaller = new MenuItem { Header = "文字を小さく (-2)" };
            smaller.Click += (_, __) =>
            {
                if (_selectedText == null) return;
                _selectedText.SetStyle(Math.Max(8, _selectedText.GetFontSize() - 2), _selectedText.GetColor());
            };
            menu.Items.Add(smaller);

            menu.Items.Add(new Separator());

            var white = new MenuItem { Header = "色: 白" };
            white.Click += (_, __) =>
            {
                if (_selectedText == null) return;
                _selectedText.SetStyle(_selectedText.GetFontSize(), Colors.White);
            };
            menu.Items.Add(white);

            var red = new MenuItem { Header = "色: 赤" };
            red.Click += (_, __) =>
            {
                if (_selectedText == null) return;
                _selectedText.SetStyle(_selectedText.GetFontSize(), Colors.Red);
            };
            menu.Items.Add(red);

            var yellow = new MenuItem { Header = "色: 黄" };
            yellow.Click += (_, __) =>
            {
                if (_selectedText == null) return;
                _selectedText.SetStyle(_selectedText.GetFontSize(), Colors.Yellow);
            };
            menu.Items.Add(yellow);

            OverlayCanvas.ContextMenu = menu;
        }

        // キーダウンイベント処理
        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            // Ctrl+Vで画像を貼り付け
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteImageFromClipboard();
                e.Handled = true;
            }
        }

        // クリップボードから画像を貼り付け
        private void PasteImageFromClipboard()
        {
            if (Clipboard.ContainsImage())
            {
                var image = Clipboard.GetImage();
                if (image != null)
                {
                    AddImageAt(new Point(50, 50), image);
                }
            }
        }

        // 画像をCanvasに追加
        private void AddImageAt(Point p, BitmapSource image)
        {
            // まず既存の選択を解除
            DeselectAllImages();

            var di = new DraggableImage(image);
            Canvas.SetLeft(di, p.X);
            Canvas.SetTop(di, p.Y);

            // クリックで選択状態にする
            di.PreviewMouseLeftButtonDown += (s, e) =>
            {
                // 他の画像の選択を解除
                DeselectAllImages();
                
                // この画像を選択
                di.Select();
                _selectedImage = di;
                
                // 最前面に移動
                BringToFront(di);
                
                // イベントは DraggableImage 内で処理されるように伝播させる
            };

            // 削除イベントを処理
            di.DeleteRequested += (s, e) =>
            {
                OverlayCanvas.Children.Remove(di);
                if (_selectedImage == di)
                {
                    _selectedImage = null;
                }
            };

            OverlayCanvas.Children.Add(di);
            
            // 追加直後は選択状態にする
            _selectedImage = di;
            di.Select();
        }

        // すべての画像の選択を解除
        private void DeselectAllImages()
        {
            foreach (var child in OverlayCanvas.Children)
            {
                if (child is DraggableImage di)
                {
                    di.Deselect();
                }
            }
            _selectedImage = null;
        }

        // すべてのテキストからフォーカスを外す
        private void ClearTextFocus()
        {
            // すべてのDraggableTextからフォーカスを外す
            foreach (var child in OverlayCanvas.Children)
            {
                if (child is DraggableText dt)
                {
                    dt.EndEdit();
                }
            }

            // 画像の選択も解除
            DeselectAllImages();
            
            // テキストの選択も解除
            DeselectAllTexts();
        }

        private void AddTextAt(Point p)
        {
            // 他の選択を解除
            DeselectAllImages();
            DeselectAllTexts();

            var dt = new DraggableText();
            Canvas.SetLeft(dt, p.X);
            Canvas.SetTop(dt, p.Y);

            // 選択中のテキストのスタイルを引き継ぐ（色変更後に追加した場合）
            if (_selectedText != null)
            {
                dt.SetStyle(_selectedText.GetFontSize(), _selectedText.GetColor());
            }

            // クリックで選択状態にする
            dt.PreviewMouseLeftButtonDown += (s, e) =>
            {
                DeselectAllImages();
                DeselectAllTexts();
                dt.Select();
                _selectedText = dt;
                
                // 最前面に移動
                BringToFront(dt);
            };

            // 削除イベントを処理
            dt.DeleteRequested += (s, e) =>
            {
                OverlayCanvas.Children.Remove(dt);
                if (_selectedText == dt)
                {
                    _selectedText = null;
                }
            };

            OverlayCanvas.Children.Add(dt);
            _selectedText = dt;

            // 追加直後は編集開始（UIが完全に描画されてからフォーカスを設定）
            Dispatcher.BeginInvoke(new System.Action(() =>
            {
                dt.StartEdit();
            }), System.Windows.Threading.DispatcherPriority.Input);
        }

        // すべてのテキストの選択を解除
        private void DeselectAllTexts()
        {
            foreach (var child in OverlayCanvas.Children)
            {
                if (child is DraggableText dt)
                {
                    dt.Deselect();
                }
            }
            _selectedText = null;
        }

        // 要素を最前面に移動
        private void BringToFront(UIElement element)
        {
            // Canvasから一度削除して、最後に追加し直すことで最前面に移動
            if (OverlayCanvas.Children.Contains(element))
            {
                OverlayCanvas.Children.Remove(element);
                OverlayCanvas.Children.Add(element);
            }
        }
    }
}
