using Microsoft.HandsFree.Keyboard.Controls;
using Microsoft.HandsFree.Keyboard.Controls.Layout;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.MVVM;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.HandsFree.Keyboard.UserInterface
{
    /// <summary>
    /// Interaction logic for KeyboardPanel.xaml
    /// </summary>
    public partial class KeyboardPanel : UserControl, IDisposable
    {
        readonly FileSystemWatcher fileSystemWatcher;

        public KeyboardPanel()
        {
            InitializeComponent();

            var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var fileName = "KeyboardLayout.xml";

            // Update layout if file in My Documents is changed.
            fileSystemWatcher = new FileSystemWatcher(myDocuments, fileName);
            fileSystemWatcher.Changed += (s, e) => DispatchedLoadKeyboardLayout();
            fileSystemWatcher.Created += (s, e) => DispatchedLoadKeyboardLayout();
            fileSystemWatcher.Deleted += (s, e) => DispatchedLoadKeyboardLayout();
            fileSystemWatcher.Renamed += (s, e) => DispatchedLoadKeyboardLayout();
            fileSystemWatcher.EnableRaisingEvents = true;

            AppSettings.Instance.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard), () =>
            {
                AppSettings.Instance.Keyboard.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard.KeyboardLayout), DispatchedLoadKeyboardLayout);
                AppSettings.Instance.Keyboard.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard.KeyboardScale), DispatchedLoadKeyboardLayout);

                DispatchedLoadKeyboardLayout();
            });

            AppSettings.Instance.Keyboard.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard.KeyboardLayout), DispatchedLoadKeyboardLayout);
            AppSettings.Instance.Keyboard.AttachPropertyChangedAction(nameof(AppSettings.Instance.Keyboard.KeyboardScale), DispatchedLoadKeyboardLayout);
        }

        [Conditional("DEBUG")]
        static void AssertAllKeyboardsValid(IKeyboardHost host, params string[] layoutXmls)
        {
            try
            {
                foreach (var layoutXml in layoutXmls)
                {
                    var layout = KeyboardLayout.Load(layoutXml);
                    layout.AssertValid(host);
                }
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        internal static string GetKeyboardLayoutXml(KeyboardLayoutName name)
        {
            string xml;

            switch (name)
            {
                default:
                    Debug.Assert(name == KeyboardLayoutName.Default, "Default layout is default!");
                    xml = Properties.Resources.Default;
                    break;

                case KeyboardLayoutName.Tabtip:
                    xml = Properties.Resources.KeyboardLayoutTabTip;
                    break;

                case KeyboardLayoutName.Tobii:
                    xml = Properties.Resources.KeyboardLayoutTobii;
                    break;

                case KeyboardLayoutName.ThreeRowed:
                    xml = Properties.Resources.ThreeRowed;
                    break;

                case KeyboardLayoutName.Krohn:
                    xml = Properties.Resources.KrohnKeyboard;
                    break;

                case KeyboardLayoutName.NonArturo14:
                    xml = Properties.Resources.NonArturo14;
                    break;

                case KeyboardLayoutName.Supersize:
                    xml = Properties.Resources.Supersize;
                    break;

                case KeyboardLayoutName.Q15:
                    xml = Properties.Resources.Q15;
                    break;

                case KeyboardLayoutName.Q9:
                    xml = Properties.Resources.Q9;
                    break;

                case KeyboardLayoutName.Q6:
                    xml = Properties.Resources.Q6;
                    break;

                case KeyboardLayoutName.Custom:
                    // Indicate custom layout.
                    xml = null;
                    break;
            }

            return xml;
        }

        static KeyboardLayout GetKeyboardLayout(IKeyboardHost host, KeyboardLayoutName name)
        {
            var layoutXml = GetKeyboardLayoutXml(name);

            if (layoutXml == null)
            {
                try
                {
                    var myDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                    var fileName = "KeyboardLayout.xml";
                    var path = Path.Combine(myDocuments, fileName);

                    layoutXml = File.ReadAllText(path);
                }
                catch (Exception ex)
                {
                    host.ShowException("Error loading custom keyboard", ex);

                    layoutXml = GetKeyboardLayoutXml(KeyboardLayoutName.Default);
                }
            }

            KeyboardLayout layout;
            try
            {
                layout = KeyboardLayout.Load(layoutXml);
            }
            catch (Exception ex)
            {
                host.ShowException("Error parsing custom keyboard", ex);

                layoutXml = GetKeyboardLayoutXml(KeyboardLayoutName.Default);
                layout = KeyboardLayout.Load(layoutXml);
            }

            return layout;
        }

        void LoadKeyboardLayout()
        {
            var host = (IKeyboardHost)DataContext;

            AssertAllKeyboardsValid(host, Properties.Resources.KeyboardLayout, Properties.Resources.KeyboardLayoutTabTip, Properties.Resources.KeyboardLayoutTobii);

            var layout = GetKeyboardLayout(host, AppSettings.Instance.Keyboard.KeyboardLayout);
#if DEBUG
            try
            {
                layout.AssertValid(host);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
#endif

            ScaleBox.Width = AppSettings.Instance.Keyboard.KeyboardScale * ActualWidth;

            TheKeyboard.LoadLayout(layout, host);
            System.Windows.Input.Keyboard.DefaultRestoreFocusMode = RestoreFocusMode.None;
        }

        void DispatchedLoadKeyboardLayout()
        {
            Dispatcher.Invoke(() => LoadKeyboardLayout());
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == ActualWidthProperty)
            {
                LoadKeyboardLayout();
            }
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (fileSystemWatcher != null)
                {
                    fileSystemWatcher.Dispose();
                }
            }
        }

        /// <summary>
        /// Implement IDispose pattern.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}

