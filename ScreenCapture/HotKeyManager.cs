using System;
using System.Runtime.InteropServices;
using System.Windows.Input;
using System.Windows.Interop;

namespace ScreenCapture
{
    public class HotKeyManager : IDisposable
    {
        private const int WM_HOTKEY = 0x0312;
        private readonly IntPtr _windowHandle;
        private int _hotKeyId = 0;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public event EventHandler<HotKeyEventArgs>? HotKeyPressed;

        public HotKeyManager(IntPtr windowHandle)
        {
            _windowHandle = windowHandle;
        }

        public bool RegisterHotKey(ModifierKeys modifiers, Key key)
        {
            _hotKeyId++;
            var virtualKey = KeyInterop.VirtualKeyFromKey(key);
            var modifierFlags = GetModifierFlags(modifiers);
            return RegisterHotKey(_windowHandle, _hotKeyId, modifierFlags, (uint)virtualKey);
        }

        public void UnregisterAll()
        {
            for (int i = 1; i <= _hotKeyId; i++)
            {
                UnregisterHotKey(_windowHandle, i);
            }
            _hotKeyId = 0;
        }

        public bool ProcessHotKey(IntPtr wParam)
        {
            var id = wParam.ToInt32();
            if (id > 0 && id <= _hotKeyId)
            {
                HotKeyPressed?.Invoke(this, new HotKeyEventArgs());
                return true;
            }
            return false;
        }

        private static uint GetModifierFlags(ModifierKeys modifiers)
        {
            uint flags = 0;
            if (modifiers.HasFlag(ModifierKeys.Alt))
                flags |= 0x0001;
            if (modifiers.HasFlag(ModifierKeys.Control))
                flags |= 0x0002;
            if (modifiers.HasFlag(ModifierKeys.Shift))
                flags |= 0x0004;
            if (modifiers.HasFlag(ModifierKeys.Windows))
                flags |= 0x0008;
            return flags;
        }

        public void Dispose()
        {
            UnregisterAll();
        }
    }

    public class HotKeyEventArgs : EventArgs
    {
    }
}
