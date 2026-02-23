using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WindowsInput.Native;
using Wpf.Ui.Controls;

namespace VRCMuteSync
{
    public partial class SettingsWindow
    {
        private readonly App _app;
        private List<int> _tempKeys = [];
        private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";

        public SettingsWindow()
        {
            InitializeComponent();
            _app = (App)Application.Current;

            PortTextBox.Text = _app.AppSettings.OscPort.ToString();

            _tempKeys = [.. _app.AppSettings.Hotkeys];
            UpdateHotkeyDisplay();

            UpdateHotkeyDisplay();

            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, false);
            StartupCheckBox.IsChecked = key?.GetValue("VRCMuteSync") != null;
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true;
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            if (key == Key.Escape)
            {
                _tempKeys.Clear();
                UpdateHotkeyDisplay();
                return;
            }

            if (key == Key.ImeProcessed || key == Key.DeadCharProcessed || key == Key.None)
            {
                return;
            }

            _tempKeys.RemoveAll(vk => !Keyboard.IsKeyDown(KeyInterop.KeyFromVirtualKey(vk)));

            int newVk = KeyInterop.VirtualKeyFromKey(key);
            if (!_tempKeys.Contains(newVk))
            {
                _tempKeys.Add(newVk);
            }

            UpdateHotkeyDisplay();
        }
        private void UpdateHotkeyDisplay()
        {
            if (_tempKeys.Count == 0)
            {
                HotkeyTextBox.Text = VRCMuteSync.Properties.Resources.Msg_NotSet;
                return;
            }

            List<string> displayKeys = [];
            foreach (var vk in _tempKeys)
            {
                displayKeys.Add(KeyInterop.KeyFromVirtualKey(vk).ToString());
            }

            HotkeyTextBox.Text = string.Join(" + ", displayKeys);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortTextBox.Text, out int port))
            {
                _app.AppSettings.OscPort = port;
                _app.AppSettings.Hotkeys = _tempKeys;
                _app.AppSettings.Save();

                using RegistryKey? key = Registry.CurrentUser.OpenSubKey(RunKey, true);
                if (key != null)
                {
                    if (StartupCheckBox.IsChecked == true)
                    {
                        string exePath = System.Diagnostics.Process.GetCurrentProcess().MainModule!.FileName;
                        key.SetValue("VRCMuteSync", $"\"{exePath}\"");
                    }
                    else
                    {
                        key.DeleteValue("VRCMuteSync", false);
                    }
                }

                _app.StartOscServer();
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show(VRCMuteSync.Properties.Resources.Msg_PortError);
            }
        }
    }
}