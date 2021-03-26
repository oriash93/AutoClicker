using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Navigation;
using AutoClicker.Utils;

namespace AutoClicker.Views
{
    public partial class AboutWindow : Window
    {
        #region Life Cycle

        public AboutWindow()
        {
            DataContext = this;

            Title = Constants.ABOUT_WINDOW_TITLE;
            InitializeComponent();

            AssemblyName assemblyInfo = AssemblyUtils.GetAssemblyInfo();
            AboutInformationText.Text = $"{assemblyInfo.Name} v{assemblyInfo.Version.Major}.{assemblyInfo.Version.Minor}.{assemblyInfo.Version.Build}";
            UrlHyperlink.NavigateUri = AssemblyUtils.GetProjectUri();
            UrlHyperlink.Inlines.Add(AssemblyUtils.GetProjectURL());
        }

        #endregion Life Cycle

        #region Event Handlers

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
            e.Handled = true;
        }

        #endregion Event Handlers
    }
}
