using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;

namespace ScreenCapture
{
    public partial class ThirdPartyLicensesWindow : Window
    {
        private static readonly (string Title, string ResourceName)[] LicenseResources =
        {
            (
                "PixiEditor.ColorPicker 3.4.2.3 — MIT License",
                "ScreenCapture.Licenses.MIT-PixiEditor.ColorPicker.txt"),
            (
                "Noto Sans JP — SIL Open Font License 1.1",
                "ScreenCapture.Fonts.OFL-NotoSansJP.txt")
        };

        public ThirdPartyLicensesWindow()
        {
            InitializeComponent();
            LicenseTextBox.Text = LoadLicenseTexts();
        }

        private static string LoadLicenseTexts()
        {
            var text = new StringBuilder();

            foreach (var license in LicenseResources)
            {
                if (text.Length > 0)
                {
                    text.AppendLine();
                    text.AppendLine(new string('=', 80));
                    text.AppendLine();
                }

                text.AppendLine(license.Title);
                text.AppendLine(new string('-', license.Title.Length));
                text.Append(LoadLicenseText(license.ResourceName, license.Title));
            }

            return text.ToString();
        }

        private static string LoadLicenseText(string resourceName, string title)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            if (stream == null)
            {
                return $"The embedded license for {title} could not be loaded.";
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
