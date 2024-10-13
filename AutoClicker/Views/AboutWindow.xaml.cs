using System;
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
            AboutInformationText.Text = $"{assemblyInfo.Name} v{assemblyInfo.Version}";
            Uri uri = AssemblyUtils.GetProjectUri();
            UrlHyperlink.NavigateUri = uri;
            UrlHyperlink.Inlines.Add(uri.OriginalString);
        }

        #endregion Life Cycle

        #region Event Handlers

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = e.Uri.OriginalString,
                UseShellExecute = true
            });
            e.Handled = true;
        }

        #endregion Event Handlers
    }
}
