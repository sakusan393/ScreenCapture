using System.Windows;

namespace ScreenCapture
{
    public partial class App : Application
    {
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            // 起動したら即、範囲選択オーバーレイを出す
            var overlay = new SelectionOverlayWindow();
            overlay.Show();
        }
    }
}
