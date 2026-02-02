using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Clipboard = System.Windows.Clipboard;
using Cursors = System.Windows.Input.Cursors;
using MessageBox = System.Windows.MessageBox;
using WpfColor = System.Windows.Media.Color;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;
using WpfMouseEventArgs = System.Windows.Input.MouseEventArgs;
using WpfPoint = System.Windows.Point;
using Forms = System.Windows.Forms;

namespace ScreenCapture
{
    public partial class CaptureWindow : Window
    {
        private DraggableText? _selectedText;
        private DraggableImage? _selectedImage;
        private bool _isDraggingWindow;
        private WpfPoint _dragStartPoint;
        private PaintToolbarWindow? _paintToolbarWindow;

        // ペイントモード関連
        private bool _isPaintMode;
        private bool _isPainting;
        private WpfPoint _lastPoint;
        private WpfColor _paintColor = Colors.Red;
        private double _paintThickness = 3;
        private bool _isHorizontalLocked;  // 水平方向にロックされているか
        private bool _isVerticalLocked;    // 垂直方向にロックされているか

        // アンドゥ・リドゥ
        private Stack<List<UIElement>> _undoStack = new Stack<List<UIElement>>();
        private Stack<List<UIElement>> _redoStack = new Stack<List<UIElement>>();
        private int _undoLimit = 50;
        private List<UIElement> _currentStroke = new List<UIElement>();

        public CaptureWindow(BitmapSource image, System.Drawing.Point screenLocation)
        {
            InitializeComponent();

            CaptureImage.Source = image;

            BorderFrame.BorderBrush = new SolidColorBrush(TextStyleSettings.CaptureFrameColor);

            Left = screenLocation.X;
            Top = screenLocation.Y;

            // ウィンドウのサイズはキャプチャ画像のサイズに固定
            Width = image.PixelWidth;
            Height = image.PixelHeight;

            // Escキーでウィンドウを閉じる
            KeyDown += (s, e) =>
            {
                if (e.Key == Key.Escape)
                {
                    Close();
                }
            };

            // Ctrl+Vでクリップボードから画像を貼り付け
            KeyDown += OnKeyDown;
            PreviewKeyDown += OnKeyDown;

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
                    WpfPoint current = e.GetPosition(this);
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

            // マウスが入ったら枠線の透明度を上げる（75%）と閉じるボタンを表示
            MouseEnter += (s, e) =>
            {
                BorderFrame.Opacity = 1;
                CloseButton.Visibility = Visibility.Visible;
                MinimizeButton.Visibility = Visibility.Visible;
            };

            // マウスが出たら枠線の透明度を下げる（25%）と閉じるボタンを非表示
            MouseLeave += (s, e) =>
            {
                BorderFrame.Opacity = 0.5;
                CloseButton.Visibility = Visibility.Collapsed;
                MinimizeButton.Visibility = Visibility.Collapsed;
            };

            // 閉じるボタンのクリックイベント
            CloseButton.Click += (s, e) => Close();

            // 最小化ボタンのクリックイベント
            MinimizeButton.Click += (s, e) => WindowState = WindowState.Minimized;

            // ペイントツールバーの初期化
            InitializePaintToolbarWindow();

            LocationChanged += (_, __) => UpdateToolbarPosition();
            SizeChanged += (_, __) => UpdateToolbarPosition();
            Closed += (_, __) =>
            {
                _paintToolbarWindow?.Close();
                _paintToolbarWindow = null;
            };
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_paintToolbarWindow != null)
            {
                _paintToolbarWindow.Owner = this;
            }

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
                    WpfPoint current = ev.GetPosition(this);
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

            var frameColor = new MenuItem { Header = "枠線の色を変更" };
            frameColor.Click += (_, __) => ChangeFrameColor();
            menu.Items.Add(frameColor);

            var copyComposite = new MenuItem { Header = "全体をコピー (Ctrl+C)" };
            copyComposite.Click += (_, __) => CopyCompositeToClipboard();
            menu.Items.Add(copyComposite);

            OverlayCanvas.ContextMenu = menu;
        }

        // キーダウンイベント処理
        private void OnKeyDown(object sender, WpfKeyEventArgs e)
        {
            // Altキー単独でペイントモード切り替え
            if (IsAltKeyPressed(e))
            {
                if (!e.IsRepeat) // キーリピートを無視
                {
                    TogglePaintMode();
                }
                e.Handled = true;
                return;
            }

            // Ctrl+Zでアンドゥ
            if (e.Key == Key.Z && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Undo();
                e.Handled = true;
                return;
            }

            // Ctrl+Yでリドゥ
            if (e.Key == Key.Y && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                Redo();
                e.Handled = true;
                return;
            }

            // Ctrl+Vで画像を貼り付け
            if (e.Key == Key.V && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                PasteImageFromClipboard();
                e.Handled = true;
            }

            // Ctrl+Cで合成画像をクリップボードにコピー
            if (e.Key == Key.C && (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
            {
                CopyCompositeToClipboard();
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
                    AddImageAt(new WpfPoint(50, 50), image);
                }
            }
        }

        // 画像をCanvasに追加
        private void AddImageAt(WpfPoint p, BitmapSource image)
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

        private void AddTextAt(WpfPoint p)
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
                dt.SetStyle(
                    _selectedText.GetFontSize(),
                    _selectedText.GetColor(),
                    _selectedText.GetBackgroundColor());
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

        // 合成画像をクリップボードにコピー
        private void CopyCompositeToClipboard()
        {
            try
            {
                // すべての選択状態を一時的に解除（バウンディングボックスを非表示）
                var wasTextSelected = _selectedText != null;
                var wasImageSelected = _selectedImage != null;
                var selectedText = _selectedText;
                var selectedImage = _selectedImage;

                // 選択を解除してバウンディングボックスを非表示
                DeselectAllTexts();
                DeselectAllImages();

                // UI要素を一時的に非表示
                var borderFrameVisibility = BorderFrame.Visibility;
                var closeButtonVisibility = CloseButton.Visibility;
                var minimizeButtonVisibility = MinimizeButton.Visibility;

                BorderFrame.Visibility = Visibility.Collapsed;
                CloseButton.Visibility = Visibility.Collapsed;
                MinimizeButton.Visibility = Visibility.Collapsed;

                // UIの更新を待つ
                Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Render);

                // ウィンドウ全体のサイズを取得
                int width = (int)ActualWidth;
                int height = (int)ActualHeight;

                // RenderTargetBitmapを使用してウィンドウをキャプチャ
                var renderTarget = new RenderTargetBitmap(
                    width,
                    height,
                    96, // dpiX
                    96, // dpiY
                    PixelFormats.Pbgra32);

                // Gridをレンダリング（CaptureImageとOverlayCanvasを含む）
                var grid = (Grid)Content;
                renderTarget.Render(grid);

                // クリップボードにコピー
                Clipboard.SetImage(renderTarget);

                // UI要素を復元
                BorderFrame.Visibility = borderFrameVisibility;
                CloseButton.Visibility = closeButtonVisibility;
                MinimizeButton.Visibility = minimizeButtonVisibility;

                // 選択状態を復元
                if (wasTextSelected && selectedText != null)
                {
                    _selectedText = selectedText;
                    selectedText.Select();
                }
                if (wasImageSelected && selectedImage != null)
                {
                    _selectedImage = selectedImage;
                    selectedImage.Select();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピーに失敗しました: {ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ペイントツールバーの初期化
        private void InitializePaintToolbarWindow()
        {
            _paintToolbarWindow = new PaintToolbarWindow();

            _paintToolbarWindow.ColorSelected += SetPaintColor;
            _paintToolbarWindow.ThicknessSelected += SetPaintThickness;
            _paintToolbarWindow.UndoRequested += Undo;
            _paintToolbarWindow.RedoRequested += Redo;
            _paintToolbarWindow.UndoLimitChanged += limit =>
            {
                _undoLimit = limit;
                TrimUndoStack();
            };
            _paintToolbarWindow.ToggleRequested += TogglePaintMode;

            _paintToolbarWindow.ApplySettings(TextStyleSettings.PaintColor, TextStyleSettings.PaintThickness);

            // Canvasのマウスイベント（ペイント用）
            OverlayCanvas.MouseLeftButtonDown += Canvas_MouseLeftButtonDown;
            OverlayCanvas.MouseMove += Canvas_MouseMove;
            OverlayCanvas.MouseLeftButtonUp += Canvas_MouseLeftButtonUp;

            UpdateUndoRedoButtons();
        }

        // ペイントモードの切り替え
        private void TogglePaintMode()
        {
            _isPaintMode = !_isPaintMode;

            if (_isPaintMode)
            {
                _paintToolbarWindow?.Show();
                UpdateToolbarPosition();
                Activate();
                Focus();
                OverlayCanvas.Cursor = Cursors.Pen;
            }
            else
            {
                _paintToolbarWindow?.Hide();
                Activate();
                Focus();
                OverlayCanvas.Cursor = Cursors.Arrow;
                _isPainting = false;
            }
        }

        private static bool IsAltKeyPressed(WpfKeyEventArgs e)
        {
            if (e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
            {
                return true;
            }

            return e.Key == Key.System
                && (e.SystemKey == Key.LeftAlt || e.SystemKey == Key.RightAlt);
        }

        private void UpdateToolbarPosition()
        {
            if (_paintToolbarWindow == null || !_isPaintMode)
            {
                return;
            }

            _paintToolbarWindow.UpdateLayout();

            var toolbarWidth = _paintToolbarWindow.ActualWidth;
            var newLeft = Left + (Width - toolbarWidth) / 2;
            var newTop = Top + Height + 8;

            _paintToolbarWindow.Left = newLeft;
            _paintToolbarWindow.Top = newTop;
        }

        // ペイント色の設定
        private void SetPaintColor(WpfColor color)
        {
            _paintColor = color;
            TextStyleSettings.PaintColor = color;
        }

        // ペイント太さの設定
        private void SetPaintThickness(double thickness)
        {
            _paintThickness = thickness;
            TextStyleSettings.PaintThickness = thickness;
        }

        // Canvasマウスダウン
        private void Canvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (!_isPaintMode) return;

            _isPainting = true;
            _lastPoint = e.GetPosition(OverlayCanvas);
            _currentStroke = new List<UIElement>(); // 新しいストロークを開始
            _isHorizontalLocked = false;  // ロックをリセット
            _isVerticalLocked = false;    // ロックをリセット
            OverlayCanvas.CaptureMouse();
            e.Handled = true;
        }

        // Canvasマウス移動
        private void Canvas_MouseMove(object sender, WpfMouseEventArgs e)
        {
            if (!_isPaintMode || !_isPainting) return;

            var currentPoint = e.GetPosition(OverlayCanvas);

            // Shiftキーが押されている場合は水平または垂直の線に補正
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                // 最初の移動で方向を決定してロック
                if (!_isHorizontalLocked && !_isVerticalLocked)
                {
                    var deltaX = Math.Abs(currentPoint.X - _lastPoint.X);
                    var deltaY = Math.Abs(currentPoint.Y - _lastPoint.Y);

                    // 一定の距離移動するまで方向を確定しない（誤判定防止）
                    if (deltaX > 5 || deltaY > 5)
                    {
                        if (deltaX > deltaY)
                        {
                            _isHorizontalLocked = true;  // 水平方向にロック
                        }
                        else
                        {
                            _isVerticalLocked = true;    // 垂直方向にロック
                        }
                    }
                }

                // ロックされた方向に応じて座標を補正
                if (_isHorizontalLocked)
                {
                    // 水平線（Y座標を固定）
                    currentPoint.Y = _lastPoint.Y;
                }
                else if (_isVerticalLocked)
                {
                    // 垂直線（X座標を固定）
                    currentPoint.X = _lastPoint.X;
                }
            }

            // 線を描画
            var line = new Line
            {
                X1 = _lastPoint.X,
                Y1 = _lastPoint.Y,
                X2 = currentPoint.X,
                Y2 = currentPoint.Y,
                Stroke = new SolidColorBrush(_paintColor),
                StrokeThickness = _paintThickness,
                StrokeStartLineCap = PenLineCap.Round,
                StrokeEndLineCap = PenLineCap.Round
            };

            OverlayCanvas.Children.Add(line);

            // 現在のストロークに追加（アンドゥスタックにはまだ追加しない）
            _currentStroke.Add(line);

            _lastPoint = currentPoint;
        }

        // Canvasマウスアップ
        private void Canvas_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (!_isPaintMode) return;

            _isPainting = false;
            OverlayCanvas.ReleaseMouseCapture();

            // ストローク完了：アンドゥスタックに追加
            if (_currentStroke.Count > 0)
            {
                AddStrokeToUndoStack(_currentStroke);
                _currentStroke = new List<UIElement>();
            }
        }

        // ストロークをアンドゥスタックに追加
        private void AddStrokeToUndoStack(List<UIElement> stroke)
        {
            _undoStack.Push(new List<UIElement>(stroke));

            // スタックサイズを制限
            TrimUndoStack();

            // リドゥスタックをクリア（新しい操作が行われたため）
            _redoStack.Clear();

            UpdateUndoRedoButtons();
        }

        // アンドゥ
        private void Undo()
        {
            if (_undoStack.Count == 0) return;

            var stroke = _undoStack.Pop();

            // ストロークの全要素をCanvasから削除
            foreach (var element in stroke)
            {
                if (OverlayCanvas.Children.Contains(element))
                {
                    OverlayCanvas.Children.Remove(element);
                }
            }

            // リドゥスタックに追加
            _redoStack.Push(stroke);

            UpdateUndoRedoButtons();
        }

        // リドゥ
        private void Redo()
        {
            if (_redoStack.Count == 0) return;

            var stroke = _redoStack.Pop();

            // ストロークの全要素をCanvasに追加
            foreach (var element in stroke)
            {
                OverlayCanvas.Children.Add(element);
            }

            // アンドゥスタックに追加
            _undoStack.Push(stroke);

            UpdateUndoRedoButtons();
        }

        // アンドゥ・リドゥボタンの有効/無効を更新
        private void UpdateUndoRedoButtons()
        {
            _paintToolbarWindow?.SetUndoRedoEnabled(_undoStack.Count > 0, _redoStack.Count > 0);
        }

        private void TrimUndoStack()
        {
            if (_undoStack.Count <= _undoLimit)
            {
                return;
            }

            var items = _undoStack.ToList();
            items.Reverse();
            var oldestStroke = items.First();

            // 最も古いストロークの全要素をCanvasから削除
            foreach (var element in oldestStroke)
            {
                if (OverlayCanvas.Children.Contains(element))
                {
                    OverlayCanvas.Children.Remove(element);
                }
            }

            // スタックを再構築
            _undoStack.Clear();
            foreach (var item in items.Skip(1).Reverse())
            {
                _undoStack.Push(item);
            }
        }

        private void ChangeFrameColor()
        {
            var currentColor = BorderFrame.BorderBrush is SolidColorBrush b ? b.Color : Colors.Red;
            using var dialog = new Forms.ColorDialog
            {
                FullOpen = true,
                Color = System.Drawing.Color.FromArgb(
                    currentColor.A,
                    currentColor.R,
                    currentColor.G,
                    currentColor.B)
            };

            if (dialog.ShowDialog() != Forms.DialogResult.OK)
            {
                return;
            }

            var color = WpfColor.FromArgb(
                dialog.Color.A,
                dialog.Color.R,
                dialog.Color.G,
                dialog.Color.B);

            BorderFrame.BorderBrush = new SolidColorBrush(color);
            TextStyleSettings.CaptureFrameColor = color;
        }
    }
}
