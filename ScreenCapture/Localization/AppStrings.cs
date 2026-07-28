using System.Globalization;
using System.Resources;

namespace ScreenCapture.Localization
{
    internal static class AppStrings
    {
        private static readonly ResourceManager ResourceManager =
            new("ScreenCapture.Resources.AppStrings", typeof(AppStrings).Assembly);

        public static string ContextMenuAddText => GetString(nameof(ContextMenuAddText));
        public static string ContextMenuPasteImage => GetString(nameof(ContextMenuPasteImage));
        public static string ContextMenuTogglePaintMode => GetString(nameof(ContextMenuTogglePaintMode));
        public static string ContextMenuCopyAll => GetString(nameof(ContextMenuCopyAll));
        public static string ContextMenuSaveImage => GetString(nameof(ContextMenuSaveImage));
        public static string CopyFailed => GetString(nameof(CopyFailed));
        public static string ErrorTitle => GetString(nameof(ErrorTitle));
        public static string SaveDialogFilter => GetString(nameof(SaveDialogFilter));
        public static string ImageSaved => GetString(nameof(ImageSaved));
        public static string SaveCompleteTitle => GetString(nameof(SaveCompleteTitle));
        public static string ImageSaveFailed => GetString(nameof(ImageSaveFailed));
        public static string ChangeFrameColorToolTip => GetString(nameof(ChangeFrameColorToolTip));
        public static string ChangeBackgroundColorToolTip => GetString(nameof(ChangeBackgroundColorToolTip));
        public static string MinimizeToolTip => GetString(nameof(MinimizeToolTip));
        public static string CloseWindowToolTip => GetString(nameof(CloseWindowToolTip));
        public static string IncreaseTextSizeToolTip => GetString(nameof(IncreaseTextSizeToolTip));
        public static string DecreaseTextSizeToolTip => GetString(nameof(DecreaseTextSizeToolTip));
        public static string TextColorToolTip => GetString(nameof(TextColorToolTip));
        public static string TextBackgroundColorToolTip => GetString(nameof(TextBackgroundColorToolTip));
        public static string TrayToolTip => GetString(nameof(TrayToolTip));
        public static string TrayHotkeyStatus => GetString(nameof(TrayHotkeyStatus));
        public static string TrayScreenCapture => GetString(nameof(TrayScreenCapture));
        public static string TrayHotkeySettings => GetString(nameof(TrayHotkeySettings));
        public static string TrayStartWithWindows => GetString(nameof(TrayStartWithWindows));
        public static string TrayStartupChecking => GetString(nameof(TrayStartupChecking));
        public static string TrayThirdPartyLicenses => GetString(nameof(TrayThirdPartyLicenses));
        public static string TrayExit => GetString(nameof(TrayExit));
        public static string StartupDisabledByUser => GetString(nameof(StartupDisabledByUser));
        public static string StartupDisabledByPolicy => GetString(nameof(StartupDisabledByPolicy));
        public static string StartupSettingsTitle => GetString(nameof(StartupSettingsTitle));
        public static string StartupChangeFailed => GetString(nameof(StartupChangeFailed));

        public static string Format(string format, params object[] args)
            => string.Format(CultureInfo.CurrentCulture, format, args);

        private static string GetString(string name)
            => ResourceManager.GetString(name, CultureInfo.CurrentUICulture) ?? name;
    }
}
