using Microsoft.HandsFree.MVVM;
using Microsoft.HandsFree.Settings;
using System;
using System.IO;
using System.Windows;
using Microsoft.HandsFree.Keyboard.Settings;

namespace Microsoft.HandsFree.Keyboard.UserInterface
{
    /// <summary>
    /// Class for installing and updating application theme.
    /// </summary>
    public static class ThemeManager
    {
        static FileSystemWatcher _fileSystemWatcher;

        static void LoadThemeResources()
        {
            var theme = AppSettings.Instance.Keyboard.DisplayTheme;
            if (theme != DisplayTheme.Custom)
            {
                var uriString = $"/Microsoft.HandsFree.Keyboard.UserInterface;component/Themes/{theme}.xaml";
                var uri = new Uri(uriString, UriKind.RelativeOrAbsolute);
                var resourceDictionary = new ResourceDictionary();
                resourceDictionary.Source = uri;
                Application.Current.Resources = resourceDictionary;
            }
            else
            {
                var documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                var filePath = Path.Combine(documentsPath, "KeyboardTheme.xaml");
                var uri = new Uri(filePath);
                var resourceDictionary = new ResourceDictionary();
                try
                {
                    resourceDictionary.Source = uri;
                }
                catch
                {
                    uri = new Uri($"/Microsoft.HandsFree.Keyboard.UserInterface;component/Themes/{DisplayTheme.Default}.xaml", UriKind.RelativeOrAbsolute);
                    resourceDictionary.Source = uri;
                }
                Application.Current.Resources = resourceDictionary;
            }
        }

        static void DispatchedLoadThemeResources()
        {
            Application.Current.Dispatcher.Invoke(() => LoadThemeResources());
        }

        /// <summary>
        /// Attach manager to application.
        /// </summary>
        public static void ThemeApplication()
        {
            LoadThemeResources();
            AppSettings.Instance.Keyboard.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard.DisplayTheme), () => LoadThemeResources());

            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = "KeyboardTheme.xaml";

            // Update layout if file in My Documents is changed.
            _fileSystemWatcher = new FileSystemWatcher(myDocuments, fileName);
            _fileSystemWatcher.Changed += (s, e) => DispatchedLoadThemeResources();
            _fileSystemWatcher.Created += (s, e) => DispatchedLoadThemeResources();
            _fileSystemWatcher.Deleted += (s, e) => DispatchedLoadThemeResources();
            _fileSystemWatcher.Renamed += (s, e) => DispatchedLoadThemeResources();
            _fileSystemWatcher.EnableRaisingEvents = true;
        }
    }
}
