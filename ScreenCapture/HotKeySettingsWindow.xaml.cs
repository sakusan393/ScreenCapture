using System.Windows;
using System.Windows.Input;
using WpfMessageBox = System.Windows.MessageBox;
using WpfKeyEventArgs = System.Windows.Input.KeyEventArgs;

namespace ScreenCapture
{
    public partial class HotKeySettingsWindow : Window
    {
        private Key _selectedKey = Key.None;

        public HotKeySettingsWindow()
        {
            InitializeComponent();
            LoadSettings();
        }

        private void LoadSettings()
        {
            EnableHotKeyCheckBox.IsChecked = HotKeySettings.IsEnabled;
            
            var modifiers = HotKeySettings.Modifiers;
            CtrlCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Control);
            ShiftCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Shift);
            AltCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Alt);
            WinCheckBox.IsChecked = modifiers.HasFlag(ModifierKeys.Windows);
            
            _selectedKey = HotKeySettings.Key;
            KeyTextBox.Text = _selectedKey.ToString();
        }

        private void KeyTextBox_PreviewKeyDown(object sender, WpfKeyEventArgs e)
        {
            e.Handled = true;
            
            var key = e.Key == Key.System ? e.SystemKey : e.Key;
            
            // 修飾キーのみは無効
            if (key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }
            
            _selectedKey = key;
            KeyTextBox.Text = key.ToString();
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            if (EnableHotKeyCheckBox.IsChecked == true)
            {
                var modifiers = ModifierKeys.None;
                if (CtrlCheckBox.IsChecked == true)
                    modifiers |= ModifierKeys.Control;
                if (ShiftCheckBox.IsChecked == true)
                    modifiers |= ModifierKeys.Shift;
                if (AltCheckBox.IsChecked == true)
                    modifiers |= ModifierKeys.Alt;
                if (WinCheckBox.IsChecked == true)
                    modifiers |= ModifierKeys.Windows;
                
                if (modifiers == ModifierKeys.None)
                {
                    WpfMessageBox.Show("少なくとも1つの修飾キー（Ctrl、Shift、Alt、Win）を選択してください。",
                        "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                if (_selectedKey == Key.None)
                {
                    WpfMessageBox.Show("キーを選択してください。",
                        "エラー", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                HotKeySettings.Modifiers = modifiers;
                HotKeySettings.Key = _selectedKey;
            }
            
            HotKeySettings.IsEnabled = EnableHotKeyCheckBox.IsChecked == true;
            
            DialogResult = true;
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
