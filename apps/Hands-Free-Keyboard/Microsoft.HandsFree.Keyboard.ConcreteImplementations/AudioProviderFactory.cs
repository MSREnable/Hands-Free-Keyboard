using Microsoft.HandsFree.Keyboard.Model;
using Microsoft.HandsFree.Settings;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Interop;
using System;
using Microsoft.HandsFree.Keyboard.Settings;
using Microsoft.HandsFree.Settings.Nudgers;

namespace Microsoft.HandsFree.Keyboard.ConcreteImplementations
{
    class AudioProviderFactory : IAudioProviderFactory
    {
        const int WM_DEVICECHANGE = 0x0219;

        class DeviceDynamicValueSetting : DynamicValueSetting
        {
            internal string Name { get; set; }
        }

        readonly Window _deviceChangeWindow = new Window
        {
            Width = 0,
            Height = 0,
            WindowStyle = WindowStyle.None,
            ResizeMode = ResizeMode.NoResize,
            ShowInTaskbar = false,
            ShowActivated = false,
            Visibility = Visibility.Hidden
        };

        readonly HwndSource _hwndSourceDeviceChangeWindow;

        public static readonly IAudioProviderFactory Instance = new AudioProviderFactory();

        /// <summary>
        /// Private constructor.
        /// </summary>
        AudioProviderFactory()
        {
            _deviceChangeWindow.Show();

            var hwnd = new WindowInteropHelper(_deviceChangeWindow).Handle;
            _hwndSourceDeviceChangeWindow = HwndSource.FromHwnd(hwnd);
            _hwndSourceDeviceChangeWindow.AddHook(WndProc);
        }

        IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_DEVICECHANGE:
                    _deviceChangeWindow.Dispatcher.BeginInvoke((Action)UpdateSettings);
                    break;
            }

            return IntPtr.Zero;
        }

        /// <summary>
        /// Create a speech provider.
        /// </summary>
        /// <param name="settings">Settings to use.</param>
        /// <returns></returns>
        public IAudioProvider Create(INarrationSettings settings)
        {
            var provider = new AudioProvider(settings);
            return provider;
        }

        internal static int GetMysteryIndex(INarrationSettings settings)
        {
            int mysteryIndex;

            switch (settings.Device)
            {
                case "":
                    mysteryIndex = AudioProvider.Default;
                    break;

                case "None":
                    mysteryIndex = AudioProvider.Null;
                    break;

                default:
                    var index = AudioDeviceEnumerator.GetDeviceIndex(settings.Device);
                    mysteryIndex = 0 <= index ? index : AudioProvider.Default;
                    break;
            }

            return mysteryIndex;
        }

        internal static void UpdateSettings()
        {
            var dynamicDevices = new List<DeviceDynamicValueSetting>
            {
                new DeviceDynamicValueSetting {Key = string.Empty, ValueString = "Default Device", Name = string.Empty},

                // Removed while we have a simple one-channel output scheme.
                //new DeviceDynamicValueSetting
                //{
                //    Key = NarrationSettings.None,
                //    ValueString = "None",
                //    Name = NarrationSettings.None
                //}
            };
            foreach (var device in AudioDeviceEnumerator.GetDevices())
            {
                dynamicDevices.Add(new DeviceDynamicValueSetting { Key = device, ValueString = device, Name = device });
            }
            AppSettings.Instance.PublicNarration.DeviceNudger.Values = dynamicDevices.ToArray();
            AppSettings.Instance.PrivateNarration.DeviceNudger.Values = dynamicDevices.ToArray();
        }
    }
}
