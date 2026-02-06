using System;
using System.IO;
using System.Text.Json;
using System.Windows.Media;
using MediaColor = System.Windows.Media.Color;

namespace ScreenCapture
{
    internal static class TextStyleSettings
    {
        private const string SettingsFileName = "text-style.json";
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
            public uint TextColorArgb { get; set; } = 0xFFFF0000;
            public uint BackgroundColorArgb { get; set; } = 0x00000000;
            public uint PaintColorArgb { get; set; } = 0xFFFF0000;
            public double PaintThickness { get; set; } = 3;
            public uint ImageBorderColorArgb { get; set; } = 0xFFFFFFFF;
            public uint CaptureFrameColorArgb { get; set; } = 0xFFFF0000;
            public uint CaptureBackgroundColorArgb { get; set; } = 0xFF000000;
            public int[] LayerOrder { get; set; } = { 0, 1, 2 };
        }

        private static Data _data = Load();

        public static MediaColor TextColor
        {
            get => FromArgb(_data.TextColorArgb, Colors.Red);
            set
            {
                _data.TextColorArgb = ToArgb(value);
                Save();
            }
        }

        public static MediaColor BackgroundColor
        {
            get => FromArgb(_data.BackgroundColorArgb, Colors.Transparent);
            set
            {
                _data.BackgroundColorArgb = ToArgb(value);
                Save();
            }
        }

        public static MediaColor PaintColor
        {
            get => FromArgb(_data.PaintColorArgb, Colors.Red);
            set
            {
                _data.PaintColorArgb = ToArgb(value);
                Save();
            }
        }

        public static double PaintThickness
        {
            get => _data.PaintThickness;
            set
            {
                _data.PaintThickness = value;
                Save();
            }
        }

        public static MediaColor ImageBorderColor
        {
            get => FromArgb(_data.ImageBorderColorArgb, Colors.White);
            set
            {
                _data.ImageBorderColorArgb = ToArgb(value);
                Save();
            }
        }

        public static MediaColor CaptureFrameColor
        {
            get => FromArgb(_data.CaptureFrameColorArgb, Colors.Red);
            set
            {
                _data.CaptureFrameColorArgb = ToArgb(value);
                Save();
            }
        }

        public static MediaColor CaptureBackgroundColor
        {
            get => FromArgb(_data.CaptureBackgroundColorArgb, Colors.Black);
            set
            {
                _data.CaptureBackgroundColorArgb = ToArgb(value);
                Save();
            }
        }

        public static int[] LayerOrder
        {
            get => _data.LayerOrder ?? new[] { 0, 1, 2 };
            set
            {
                _data.LayerOrder = value ?? new[] { 0, 1, 2 };
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

        private static uint ToArgb(MediaColor color)
            => ((uint)color.A << 24)
                | ((uint)color.R << 16)
                | ((uint)color.G << 8)
                | color.B;

        private static MediaColor FromArgb(uint argb, MediaColor fallback)
        {
            try
            {
                return MediaColor.FromArgb(
                    (byte)((argb >> 24) & 0xFF),
                    (byte)((argb >> 16) & 0xFF),
                    (byte)((argb >> 8) & 0xFF),
                    (byte)(argb & 0xFF));
            }
            catch
            {
                return fallback;
            }
        }
    }
}
