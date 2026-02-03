using System;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using Application = System.Windows.Application;

namespace ScreenCapture
{
    public partial class App : Application
    {
        private HotKeyManager? _hotKeyManager;
        private NotifyIcon? _notifyIcon;
        private Window? _hiddenWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 非表示ウィンドウを作成（ホットキー受信用）
            _hiddenWindow = new Window
            {
                Width = 0,
                Height = 0,
                WindowStyle = WindowStyle.None,
                ShowInTaskbar = false,
                ShowActivated = false
            };
            _hiddenWindow.Show();
            _hiddenWindow.Hide();

            // タスクトレイアイコンを常に表示
            CreateNotifyIcon();

            // ホットキーが有効な場合はホットキーマネージャーを初期化
            if (HotKeySettings.IsEnabled)
            {
                InitializeHotKey();
            }

            // 起動したら即、範囲選択オーバーレイを出す
            ShowSelectionOverlay();
        }

        private void InitializeHotKey()
        {
            var windowHandle = new WindowInteropHelper(_hiddenWindow!).Handle;
            _hotKeyManager = new HotKeyManager(windowHandle);
            _hotKeyManager.HotKeyPressed += OnHotKeyPressed;

            // ホットキーを登録
            RegisterHotKey();

            // ウィンドウメッセージのフック
            var source = HwndSource.FromHwnd(windowHandle);
            source?.AddHook(WndProc);
        }

        private void ShowHotKeySettingsAndStartup()
        {
            var settingsWindow = new HotKeySettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                // ホットキーが有効になった場合、ホットキーマネージャーを初期化
                if (HotKeySettings.IsEnabled)
                {
                    InitializeHotKey();
                }
                ShowSelectionOverlay();
            }
            else
            {
                // キャンセルされた場合は通常起動
                ShowSelectionOverlay();
            }
        }

        private void RegisterHotKey()
        {
            if (HotKeySettings.IsEnabled && _hotKeyManager != null)
            {
                try
                {
                    var success = _hotKeyManager.RegisterHotKey(HotKeySettings.Modifiers, HotKeySettings.Key);
                    if (!success)
                    {
                        System.Windows.MessageBox.Show(
                            $"Failed to register hotkey: {HotKeySettings.Modifiers}+{HotKeySettings.Key}\n" +
                            "The hotkey may already be in use by another application.",
                            "Hotkey Registration Failed",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.MessageBox.Show(
                        $"Error registering hotkey: {ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            
            if (msg == WM_HOTKEY && _hotKeyManager != null)
            {
                handled = _hotKeyManager.ProcessHotKey(wParam);
            }
            
            return IntPtr.Zero;
        }

        private void OnHotKeyPressed(object? sender, HotKeyEventArgs e)
        {
            Dispatcher.Invoke(() => ShowSelectionOverlay());
        }

        private void ShowSelectionOverlay()
        {
            var overlay = new SelectionOverlayWindow();
            overlay.Show();
        }

        private void CreateNotifyIcon()
        {
            // アイコンを読み込む
            System.Drawing.Icon? icon = null;
            try
            {
                // まず埋め込みリソースから読み込みを試行
                var assembly = System.Reflection.Assembly.GetExecutingAssembly();
                var resourceNames = assembly.GetManifestResourceNames();
                var iconResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("app202620231858.ico"));
                
                if (iconResourceName != null)
                {
                    using (var stream = assembly.GetManifestResourceStream(iconResourceName))
                    {
                        if (stream != null)
                        {
                            icon = new System.Drawing.Icon(stream);
                        }
                    }
                }
                
                // 埋め込みリソースから読み込めなかった場合、ファイルから読み込みを試行
                if (icon == null)
                {
                    var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "app202620231858.ico");
                    if (System.IO.File.Exists(iconPath))
                    {
                        icon = new System.Drawing.Icon(iconPath);
                    }
                }
            }
            catch
            {
                // エラー時はデフォルトアイコンを使用
            }

            _notifyIcon = new NotifyIcon
            {
                Icon = icon ?? System.Drawing.SystemIcons.Application,
                Text = "ScreenCapture - Right-click for settings",
                Visible = true
            };

            UpdateContextMenu();

            _notifyIcon.DoubleClick += (s, e) => ShowSelectionOverlay();
        }

        private void UpdateContextMenu()
        {
            var contextMenu = new ContextMenuStrip();
            
            // ホットキーの状態を表示
            if (HotKeySettings.IsEnabled)
            {
                var hotkeyInfo = new ToolStripMenuItem($"Hotkey: {HotKeySettings.Modifiers}+{HotKeySettings.Key}")
                {
                    Enabled = false
                };
                contextMenu.Items.Add(hotkeyInfo);
                contextMenu.Items.Add(new ToolStripSeparator());
            }
            
            var captureItem = new ToolStripMenuItem("Screen Capture");
            captureItem.Click += (s, e) => ShowSelectionOverlay();
            contextMenu.Items.Add(captureItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem("Hotkey Settings...");
            settingsItem.Click += (s, e) => ShowHotKeySettings();
            contextMenu.Items.Add(settingsItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem("Exit");
            exitItem.Click += (s, e) => Shutdown();
            contextMenu.Items.Add(exitItem);

            if (_notifyIcon != null)
            {
                _notifyIcon.ContextMenuStrip = contextMenu;
            }
        }

        private void ShowHotKeySettings()
        {
            var settingsWindow = new HotKeySettingsWindow();
            if (settingsWindow.ShowDialog() == true)
            {
                // ホットキーを再初期化
                if (HotKeySettings.IsEnabled)
                {
                    if (_hotKeyManager == null)
                    {
                        InitializeHotKey();
                    }
                    else
                    {
                        _hotKeyManager.UnregisterAll();
                        RegisterHotKey();
                    }
                }
                else
                {
                    // ホットキーを無効化
                    _hotKeyManager?.UnregisterAll();
                    _hotKeyManager?.Dispose();
                    _hotKeyManager = null;
                }
                
                // メニューを更新
                UpdateContextMenu();
            }
        }
    }
}
