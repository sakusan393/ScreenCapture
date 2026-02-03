using System;
using System.IO;
using System.Text.Json;
using System.Windows.Input;

namespace ScreenCapture
{
    internal static class HotKeySettings
    {
        private const string SettingsFileName = "hotkey-settings.json";
        private static readonly string SettingsDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "ScreenCapture");
        private static readonly string SettingsPath = Path.Combine(SettingsDirectory, SettingsFileName);
        private static readonly JsonSerializerOptions SerializerOptions = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        private sealed class Data
        {
            public int ModifierKeys { get; set; } = (int)(System.Windows.Input.ModifierKeys.Control | System.Windows.Input.ModifierKeys.Shift);
            public int Key { get; set; } = (int)System.Windows.Input.Key.P;
            public bool IsEnabled { get; set; } = false;
        }

        private static Data _data = Load();

        public static ModifierKeys Modifiers
        {
            get => (ModifierKeys)_data.ModifierKeys;
            set
            {
                _data.ModifierKeys = (int)value;
                Save();
            }
        }

        public static Key Key
        {
            get => (Key)_data.Key;
            set
            {
                _data.Key = (int)value;
                Save();
            }
        }

        public static bool IsEnabled
        {
            get => _data.IsEnabled;
            set
            {
                _data.IsEnabled = value;
                Save();
            }
        }

        private static Data Load()
        {
            try
            {
                if (File.Exists(SettingsPath))
                {
                    var json = File.ReadAllText(SettingsPath);
                    var data = JsonSerializer.Deserialize<Data>(json);
                    if (data != null)
                    {
                        return data;
                    }
                }
            }
            catch
            {
            }

            return new Data();
        }

        private static void Save()
        {
            try
            {
                Directory.CreateDirectory(SettingsDirectory);
                var json = JsonSerializer.Serialize(_data, SerializerOptions);
                File.WriteAllText(SettingsPath, json);
            }
            catch
            {
            }
        }
    }
}
