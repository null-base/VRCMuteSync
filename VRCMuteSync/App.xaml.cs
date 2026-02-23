using Hardcodet.Wpf.TaskbarNotification;
using Rug.Osc;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using WindowsInput;
using WindowsInput.Native;

namespace VRCMuteSync
{
    public partial class App : Application
    {
        private TaskbarIcon? _trayIcon;
        private OscReceiver? _oscReceiver;
        private CancellationTokenSource? _cts;
        private InputSimulator? _inputSimulator;
        private bool? _lastMuteState = null;
        public Settings AppSettings { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Application.Current.ShutdownMode = ShutdownMode.OnExplicitShutdown;

            AppSettings = Settings.Load();
            _inputSimulator = new InputSimulator();

            _trayIcon = new TaskbarIcon
            {
                IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/mute.ico")),
                ToolTipText = "VRCMuteSync"
            };

            var menu = new ContextMenu
            {
                Style = new Style(typeof(ContextMenu))
            };

            var settingsItem = new MenuItem
            {
                Header = VRCMuteSync.Properties.Resources.Tray_Settings,
                Style = new Style(typeof(MenuItem))
            };

            settingsItem.Click += (s, args) =>
            {
                if (App.Current.MainWindow is not SettingsWindow window || !window.IsLoaded)
                {
                    window = new SettingsWindow();
                    App.Current.MainWindow = window;
                    window.Show();
                }
                else
                {
                    window.Activate();
                }
            };

            var exitItem = new MenuItem
            {
                Header = VRCMuteSync.Properties.Resources.Tray_Exit,
                Style = new Style(typeof(MenuItem))
            };
            exitItem.Click += (s, args) => Shutdown();

            menu.Items.Add(settingsItem);
            menu.Items.Add(new Separator { Style = new Style(typeof(Separator)) });
            menu.Items.Add(exitItem);

            _trayIcon.ContextMenu = menu;

            StartOscServer();
        }

        public void StartOscServer()
        {
            _cts?.Cancel();
            _oscReceiver?.Close();

            _cts = new CancellationTokenSource();
            _oscReceiver = new OscReceiver(AppSettings.OscPort);

            try
            {
                _oscReceiver.Connect();
                Task.Run(() => ListenOsc(_cts.Token));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"OSCポート {AppSettings.OscPort} の待機に失敗しました。\n{ex.Message}", "エラー");
            }
        }

        private void ListenOsc(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && _oscReceiver!.State == OscSocketState.Connected)
                {
                    OscPacket packet = _oscReceiver.Receive();
                    if (packet is OscMessage message && message.Address == "/avatar/parameters/MuteSelf")
                    {
                        bool isMuted = (bool)message[0];

                        if (_lastMuteState != isMuted)
                        {
                            _lastMuteState = isMuted;

                            Dispatcher.Invoke(() =>
                            {
                                Task.Run(() =>
                                {
                                    var keysToPress = AppSettings.Hotkeys.ConvertAll(k => (VirtualKeyCode)k);

                                    if (keysToPress.Count > 0)
                                    {
                                        foreach (var k in keysToPress)
                                        {
                                            _inputSimulator!.Keyboard.KeyDown(k);
                                        } 
                                        Thread.Sleep(30);

                                        for (int i = keysToPress.Count - 1; i >= 0; i--)
                                        {
                                            _inputSimulator!.Keyboard.KeyUp(keysToPress[i]);
                                        }
                                    }
                                });
                            });
                        }
                    }
                }
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            _cts?.Cancel();
            _oscReceiver?.Close();
            _trayIcon?.Dispose();
            base.OnExit(e);
        }
    }
}