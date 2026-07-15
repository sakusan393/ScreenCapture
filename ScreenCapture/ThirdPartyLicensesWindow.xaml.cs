using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace ScreenCapture
{
    public partial class ThirdPartyLicensesWindow : Window
    {
        private const string LicenseResourceName = "ScreenCapture.Fonts.OFL-NotoSansJP.txt";

        public ThirdPartyLicensesWindow()
        {
            InitializeComponent();
            LicenseTextBox.Text = LoadLicenseText();
        }

        private static string LoadLicenseText()
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(LicenseResourceName);
            if (stream == null)
            {
                return "The embedded Noto Sans JP license could not be loaded.";
            }

            using var reader = new StreamReader(stream, Encoding.UTF8);
            return reader.ReadToEnd();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
