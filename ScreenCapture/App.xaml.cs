using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Forms;
using ScreenCapture.Localization;
using Application = System.Windows.Application;

namespace ScreenCapture
{
    public partial class App : Application
    {
        private const string SingleInstanceMutexName = "ScreenCapture.SingleInstance";
        private const string SingleInstancePipeName = "ScreenCapture.SingleInstancePipe";
        private static System.Threading.Mutex? _singleInstanceMutex;
        private System.Threading.CancellationTokenSource? _singleInstanceCts;

        private HotKeyManager? _hotKeyManager;
        private NotifyIcon? _notifyIcon;
        private ToolStripMenuItem? _startupMenuItem;
        private Window? _hiddenWindow;

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            var launchedAtStartup = e.Args.Any(
                argument => string.Equals(argument, "--startup", StringComparison.OrdinalIgnoreCase));

            _singleInstanceMutex = new System.Threading.Mutex(true, SingleInstanceMutexName, out var createdNew);
            if (!createdNew)
            {
                if (!launchedAtStartup)
                {
                    SignalExistingInstance();
                }
                Shutdown();
                return;
            }

            StartSingleInstanceListener();

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

            // 通常起動では範囲選択を開始し、Windowsサインイン時は通知領域にだけ常駐する
            if (!launchedAtStartup)
            {
                ShowSelectionOverlay();
            }
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
                        HotKeySettings.IsEnabled = false;
                        _hotKeyManager.UnregisterAll();
                        UpdateContextMenu();
                    }
                }
                catch
                {
                    HotKeySettings.IsEnabled = false;
                    _hotKeyManager.UnregisterAll();
                    UpdateContextMenu();
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

        private void SignalExistingInstance()
        {
            try
            {
                using var client = new System.IO.Pipes.NamedPipeClientStream(
                    ".",
                    SingleInstancePipeName,
                    System.IO.Pipes.PipeDirection.Out);
                client.Connect(500);
                using var writer = new System.IO.StreamWriter(client) { AutoFlush = true };
                writer.WriteLine("capture");
            }
            catch
            {
            }
        }

        private void StartSingleInstanceListener()
        {
            _singleInstanceCts = new System.Threading.CancellationTokenSource();
            var token = _singleInstanceCts.Token;

            System.Threading.Tasks.Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        using var server = new System.IO.Pipes.NamedPipeServerStream(
                            SingleInstancePipeName,
                            System.IO.Pipes.PipeDirection.In,
                            1,
                            System.IO.Pipes.PipeTransmissionMode.Message,
                            System.IO.Pipes.PipeOptions.Asynchronous);

                        await server.WaitForConnectionAsync(token);
                        using var reader = new System.IO.StreamReader(server);
                        var message = await reader.ReadLineAsync();
                        if (string.Equals(message, "capture", StringComparison.OrdinalIgnoreCase))
                        {
                            Dispatcher.Invoke(ShowSelectionOverlay);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                    catch
                    {
                    }
                }
            }, token);
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
                var iconResourceName = resourceNames.FirstOrDefault(r => r.EndsWith("app.ico"));
                
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
                    var iconPath = System.IO.Path.Combine(AppContext.BaseDirectory, "app.ico");
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
                Text = AppStrings.TrayToolTip,
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
                var hotkeyInfo = new ToolStripMenuItem(
                    AppStrings.Format(
                        AppStrings.TrayHotkeyStatus,
                        HotKeySettings.Modifiers,
                        HotKeySettings.Key))
                {
                    Enabled = false
                };
                contextMenu.Items.Add(hotkeyInfo);
                contextMenu.Items.Add(new ToolStripSeparator());
            }
            
            var captureItem = new ToolStripMenuItem(AppStrings.TrayScreenCapture);
            captureItem.Click += (s, e) => ShowSelectionOverlay();
            contextMenu.Items.Add(captureItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var settingsItem = new ToolStripMenuItem(AppStrings.TrayHotkeySettings);
            settingsItem.Click += (s, e) => ShowHotKeySettings();
            contextMenu.Items.Add(settingsItem);

            if (StartupTaskService.IsAvailable)
            {
                _startupMenuItem = new ToolStripMenuItem(AppStrings.TrayStartupChecking)
                {
                    Enabled = false
                };
                _startupMenuItem.Click += OnStartupMenuItemClick;
                contextMenu.Items.Add(_startupMenuItem);
            }
            else
            {
                _startupMenuItem = null;
            }

            contextMenu.Items.Add(new ToolStripSeparator());

            var licensesItem = new ToolStripMenuItem(AppStrings.TrayThirdPartyLicenses);
            licensesItem.Click += (s, e) => ShowThirdPartyLicenses();
            contextMenu.Items.Add(licensesItem);

            contextMenu.Items.Add(new ToolStripSeparator());

            var exitItem = new ToolStripMenuItem(AppStrings.TrayExit);
            exitItem.Click += (s, e) => Shutdown();
            contextMenu.Items.Add(exitItem);

            if (_notifyIcon != null)
            {
                _notifyIcon.ContextMenuStrip = contextMenu;
            }

            if (_startupMenuItem != null)
            {
                RefreshStartupMenuItemAsync(_startupMenuItem);
            }
        }

        private async void RefreshStartupMenuItemAsync(ToolStripMenuItem startupMenuItem)
        {
            try
            {
                var state = await StartupTaskService.GetStateAsync();
                if (!ReferenceEquals(_startupMenuItem, startupMenuItem))
                {
                    return;
                }

                startupMenuItem.Text = AppStrings.TrayStartWithWindows;
                startupMenuItem.Checked =
                    state == global::Windows.ApplicationModel.StartupTaskState.Enabled ||
                    state == global::Windows.ApplicationModel.StartupTaskState.EnabledByPolicy;
                startupMenuItem.Enabled = true;
            }
            catch
            {
                if (ReferenceEquals(_startupMenuItem, startupMenuItem))
                {
                    startupMenuItem.Text = AppStrings.TrayStartWithWindows;
                    startupMenuItem.Enabled = false;
                }
            }
        }

        private async void OnStartupMenuItemClick(object? sender, EventArgs e)
        {
            if (sender is not ToolStripMenuItem startupMenuItem)
            {
                return;
            }

            startupMenuItem.Enabled = false;

            try
            {
                var state = await StartupTaskService.GetStateAsync();
                switch (state)
                {
                    case global::Windows.ApplicationModel.StartupTaskState.Enabled:
                        await StartupTaskService.SetEnabledAsync(false);
                        break;

                    case global::Windows.ApplicationModel.StartupTaskState.Disabled:
                        await StartupTaskService.SetEnabledAsync(true);
                        break;

                    case global::Windows.ApplicationModel.StartupTaskState.DisabledByUser:
                        var result = System.Windows.MessageBox.Show(
                            AppStrings.StartupDisabledByUser,
                            AppStrings.StartupSettingsTitle,
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Information);
                        if (result == MessageBoxResult.Yes)
                        {
                            Process.Start(new ProcessStartInfo("ms-settings:startupapps")
                            {
                                UseShellExecute = true
                            });
                        }
                        break;

                    case global::Windows.ApplicationModel.StartupTaskState.DisabledByPolicy:
                    case global::Windows.ApplicationModel.StartupTaskState.EnabledByPolicy:
                        System.Windows.MessageBox.Show(
                            AppStrings.StartupDisabledByPolicy,
                            AppStrings.StartupSettingsTitle,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                        break;
                }
            }
            catch
            {
                System.Windows.MessageBox.Show(
                    AppStrings.StartupChangeFailed,
                    AppStrings.StartupSettingsTitle,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            finally
            {
                RefreshStartupMenuItemAsync(startupMenuItem);
            }
        }

        private void ShowHotKeySettings()
        {
            var settingsWindow = new HotKeySettingsWindow
            {
                Owner = _hiddenWindow,
                Topmost = true
            };
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

        private void ShowThirdPartyLicenses()
        {
            var licensesWindow = new ThirdPartyLicensesWindow
            {
                Owner = _hiddenWindow,
                Topmost = true
            };
            licensesWindow.ShowDialog();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _singleInstanceCts?.Cancel();
            _singleInstanceCts?.Dispose();
            _singleInstanceMutex?.ReleaseMutex();
            _singleInstanceMutex?.Dispose();
            base.OnExit(e);
        }
    }
}
