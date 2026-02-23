using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Hardcodet.Wpf.TaskbarNotification;
using Rug.Osc;
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
        public Settings AppSettings { get; private set; } = null!;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppSettings = Settings.Load();
            _inputSimulator = new InputSimulator();

            _trayIcon = new TaskbarIcon
            {
                IconSource = new System.Windows.Media.Imaging.BitmapImage(new Uri("pack://application:,,,/mute.ico")),
                ToolTipText = "VRCMuteSync"
            };

            // 2. 右クリックメニューの作成
            var menu = new ContextMenu();
            var settingsItem = new MenuItem { Header = "設定" };
            settingsItem.Click += (s, args) => new SettingsWindow().Show();
            var exitItem = new MenuItem { Header = "終了" };
            exitItem.Click += (s, args) => Shutdown();

            menu.Items.Add(settingsItem);
            menu.Items.Add(new Separator());
            menu.Items.Add(exitItem);
            _trayIcon.ContextMenu = menu;

            // 3. OSCサーバー起動
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
                        if (!isMuted) // ミュート「解除」時
                        {
                            Dispatcher.Invoke(() =>
                            {
                                if (AppSettings.MainKey == 0) return; // 未設定なら何もしない

                                var modifiers = AppSettings.ModifierKeys.ConvertAll(k => (VirtualKeyCode)k).ToArray();
                                var mainKey = (VirtualKeyCode)AppSettings.MainKey;

                                if (modifiers.Length > 0)
                                {
                                    // Ctrl + Shift + M のような複合キーを送信
                                    _inputSimulator!.Keyboard.ModifiedKeyStroke(modifiers, mainKey);
                                }
                                else
                                {
                                    // F15などの単体キーを送信
                                    _inputSimulator!.Keyboard.KeyPress(mainKey);
                                }
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