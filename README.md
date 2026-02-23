# VRCMuteSync - VRChat & Discord ミュート連動ツール

VRChatのミュート状態をOSCで検知し、Discordのミュートと自動的に連動させる軽量なWindows向け常駐アプリケーションです。
VRゴーグルを被ったまま、デスクトップ画面に戻ることなくシームレスに音声状態を管理できます。

## 📥 インストールと使い方 (Installation & Usage)

一般ユーザー向けには、[BOOTHページ](https://null-base.booth.pm/items/8019656) にてコンパイル済みの実行ファイル(`.exe`)を配布しています。

### 初期設定

1. **VRChatの設定**: リングメニューを開き、「Options」＞「OSC」＞「Enabled」をオンにします。
2. **Discordの設定**:
    * 「ユーザー設定」＞「キー割り当て」を開き、アクションを **「ミュート切り替え (Toggle Mute)」** にして、任意のキーを登録します。
    * 「ユーザー設定」＞「音声・ビデオ」の下部にある **「最新の技術を使用してキー入力のキャプチャを行う」を必ず【オフ】** にしてください。
3. **VRCMuteSyncの設定**:
    * アプリを起動し、タスクトレイのアイコンを右クリックして「設定」を開きます。
    * OSC受信ポート（デフォルト: 9001）を入力します。
    * ホットキー入力欄をクリックし、Discordで設定したのと同じキーを押して登録し、保存します。

## 🛠 ソースコードからのビルド (Build from Source)

本アプリは C# / WPF (.NET 10) で開発されています。ご自身でビルドする場合は以下の手順で行ってください。

### 必須環境
* Visual Studio 2022 (または最新の .NET SDK が利用可能なIDE)
* .NET 10.0 SDK 以降

### 手順
1. 本リポジトリをクローンします。
   ```bash
   git clone https://github.com/null-base/VRCMuteSync.git
   ```
2. Visual Studioで VRCMuteSync.sln を開きます。
3. NuGetパッケージを復元します。
4. ソリューションをビルド（F5 または Ctrl+Shift+B）して実行します。

### 使用ライブラリ
[WPF-UI](https://github.com/lepoco/wpfui) - モダンなUIコンポーネント
[Rug.Osc](https://bitbucket.org/rugcode/rug.osc) - OSC通信処理
[InputSimulatorPlus](https://github.com/TChatzigiannakis/InputSimulatorPlus) - キーボードエミュレーション

⚠️ 注意事項
Discordを「管理者として実行」している場合、Windowsのセキュリティ仕様（UIPI）により本アプリからの仮想キー送信が弾かれます。その場合は本アプリも「管理者として実行」してください。
本アプリはローカル環境で完結し、入力情報を収集・外部送信することは一切ありません。
