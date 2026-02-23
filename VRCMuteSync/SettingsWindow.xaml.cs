using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using WindowsInput.Native;
using Wpf.Ui.Controls;

namespace VRCMuteSync
{
    public partial class SettingsWindow : FluentWindow
    {
        private App _app;
        private List<int> _tempModifiers = new List<int>();
        private int _tempMainKey = 0;

        public SettingsWindow()
        {
            InitializeComponent();
            _app = (App)Application.Current;

            PortTextBox.Text = _app.AppSettings.OscPort.ToString();

            // 現在の設定を一時変数にコピー
            _tempModifiers = new List<int>(_app.AppSettings.ModifierKeys);
            _tempMainKey = _app.AppSettings.MainKey;

            UpdateHotkeyDisplay();
        }

        private void HotkeyTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            e.Handled = true; // デフォルトの入力をキャンセル

            // Altキーなどのシステムキーも取得できるようにする
            Key key = (e.Key == Key.System ? e.SystemKey : e.Key);

            // Escキーで設定をリセット
            if (key == Key.Escape)
            {
                _tempModifiers.Clear();
                _tempMainKey = 0;
                UpdateHotkeyDisplay();
                return;
            }

            // Ctrl, Shift, Altなどの修飾キー単体で押された時はメインキーを待つためスキップ
            if (key == Key.LeftShift || key == Key.RightShift ||
                key == Key.LeftCtrl || key == Key.RightCtrl ||
                key == Key.LeftAlt || key == Key.RightAlt ||
                key == Key.LWin || key == Key.RWin)
            {
                return;
            }

            // 修飾キーの状態を取得して保存
            _tempModifiers.Clear();
            if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                _tempModifiers.Add((int)VirtualKeyCode.CONTROL);
            if ((Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
                _tempModifiers.Add((int)VirtualKeyCode.SHIFT);
            if ((Keyboard.Modifiers & ModifierKeys.Alt) == ModifierKeys.Alt)
                _tempModifiers.Add((int)VirtualKeyCode.MENU);

            // メインのキーをInputSimulator用の仮想キーコードに変換して保存
            _tempMainKey = KeyInterop.VirtualKeyFromKey(key);

            UpdateHotkeyDisplay();
        }

        private void UpdateHotkeyDisplay()
        {
            if (_tempMainKey == 0)
            {
                HotkeyTextBox.Text = "未設定";
                return;
            }

            List<string> displayKeys = new List<string>();
            if (_tempModifiers.Contains((int)VirtualKeyCode.CONTROL)) displayKeys.Add("Ctrl");
            if (_tempModifiers.Contains((int)VirtualKeyCode.SHIFT)) displayKeys.Add("Shift");
            if (_tempModifiers.Contains((int)VirtualKeyCode.MENU)) displayKeys.Add("Alt");

            // メインキーの名前を追加
            displayKeys.Add(((Key)KeyInterop.KeyFromVirtualKey(_tempMainKey)).ToString());

            HotkeyTextBox.Text = string.Join(" + ", displayKeys);
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(PortTextBox.Text, out int port))
            {
                _app.AppSettings.OscPort = port;
                _app.AppSettings.ModifierKeys = _tempModifiers;
                _app.AppSettings.MainKey = _tempMainKey;
                _app.AppSettings.Save();

                _app.StartOscServer();
                Close();
            }
            else
            {
                System.Windows.MessageBox.Show("ポート番号は正しく入力してください。");
            }
        }
    }
}