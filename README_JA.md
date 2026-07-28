# ScreenCapture

[English](README.md) | **日本語**

画面キャプチャ＋テキストによる注釈＋別画像追加＋ペイント機能を備えたWindows用のキャプチャツールです。

[![Release](https://img.shields.io/github/v/release/sakusan393/ScreenCapture)](https://github.com/sakusan393/ScreenCapture/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/sakusan393/ScreenCapture/total)](https://github.com/sakusan393/ScreenCapture/releases)
[![License](https://img.shields.io/github/license/sakusan393/ScreenCapture)](LICENSE)

## 主な機能

### 画面キャプチャ
- ドラッグで範囲を選択してキャプチャ
- **Ctrl+クリック**でクリックしたウィンドウをキャプチャ
- 複数モニター対応
- **Ctrl+C**でクリップボードにコピー
- **Ctrl+S**で画像として保存（現在の実装はPNGエンコード）

### テキスト注釈
- 右クリックでテキストを追加
- 埋め込みの **Noto Sans JP SemiBold** を使用（PCへのフォントインストール不要）
- **カラーピッカー**で文字色を選択（透明度対応）
- **背景色**をカラーピッカーで選択（透明度対応）
- **サイズ変更**は [+][-] ボタン、または押しっぱなしで連続変更
- ダブルクリックで再編集
- ドラッグで移動
- 文字色・背景色・文字サイズの設定は次回起動時も保持

### 画像の貼り付け
- **Ctrl+V**でクリップボードから画像を追加
- エクスプローラーでコピーした JPG / JPEG / PNG ファイルの貼り付けに対応（複数ファイル可）
- 透過 PNG の透明度を維持し、大きな画像はキャプチャ領域内へ縦横比を保って自動縮小
- リサイズは四隅のハンドルをドラッグ（Shiftでアスペクト比固定）
- 回転は上部中央のハンドルをドラッグ
- ドラッグで移動
- 枠線表示のON/OFF切り替え（10px/20px）
- 枠線色をカラーピッカーで変更（透明度対応）

### ペイント機能
- **Alt**キーでペイントモード ON/OFF
- 右クリックメニューからもペイントモード切り替え
- **カラーピッカー**で色を選択（透明度対応）
- **4段階の太さ**（1, 3, 5, 10）を選択
- **Shift+ドラッグ**で水平・垂直の直線
- **Ctrl+ドラッグ**で矢印モード（カーソルが上向き矢印に変化）
- **Ctrl+Z**でアンドゥ
- **Ctrl+Y**でリドゥ
- **履歴回数**は 10/20/50/100 回から選択
- ペイントの色と太さは次回起動時も保持

### その他
- マウスホバーで枠線・ボタンを表示
- 枠線色をカラーピッカーで変更（透明度対応）
- キャプチャウィンドウの背景色をカラーピッカーで変更
- 背景色の透明度はドラッグ操作を維持するため Alpha 1 が最小（0 を選択した場合も 1 へ補正）
- 四辺・四隅をドラッグしてエリアを拡張（拡張分は背景色で塗りつぶし）
- 画像またはテキストを選択すると、同じ種類のレイヤー内で最前面に移動
- **Layers** パネルでペイント/画像/テキストのレイヤー順をドラッグで変更（並べ替えた順序は次回起動時も保持され、Layers パネル自体は保存/コピーに含まれません）
- ペイントモードでペイントレイヤーが画像/テキストより上の場合、描画が優先されます
- **Esc**キーで現在の編集画面を閉じる、または範囲選択をキャンセル（アプリは通知領域で動作を継続）
- 最小化
- **マウスホイール**でキャプチャウィンドウの透明度を変更（ほぼ完全透明まで可能）
- Microsoft Store版では、通知領域メニューの **Windowsへのサインイン時に起動** から自動起動を明示的にON/OFF（サインイン時は範囲選択画面を出さず、通知領域にだけ常駐）

## 使い方

1. **ScreenCapture.exe**をダブルクリックして起動
2. マウスドラッグで範囲を選択してキャプチャ
3. 右クリックで「テキスト追加」、または **Alt** でペイントモード
4. **Ctrl+C** でクリップボードにコピー
5. Word/PowerPointなどに貼り付け

## 表示言語

- キャプチャ後の右クリックメニュー、保存ダイアログ、結果メッセージ、編集ツールのツールチップは、Windows の表示言語に合わせて日本語または英語で表示されます。
- 日本語以外の表示言語では英語へフォールバックします。
- 日本語版と英語版は同じアプリパッケージに含まれるため、言語別にインストールし直す必要はありません。

## 配布

- GitHub ReleaseのEXEは、現在は未署名の直接配布版です。
- Microsoft Store向けにはMSIX提出パッケージを生成できます。Storeの審査を通過したパッケージはMicrosoftによって署名され、Storeから安全にインストール・更新できます。
- Store提出手順: [Microsoft Store 配布](docs/MICROSOFT_STORE.md)

## ショートカットキー

| キー | 機能 |
|------|------|
| **Ctrl+C** | クリップボードにコピー |
| **Ctrl+S** | 画像として保存 |
| **Ctrl+V** | 画像を貼り付け |
| **Ctrl+クリック（キャプチャ範囲選択中）** | クリックしたウィンドウをキャプチャ |
| **Alt** | ペイントモード ON/OFF |
| **Ctrl（ペイント中）** | 矢印モード ON/OFF |
| **Shift+ドラッグ（ペイント中）** | 水平・垂直線 |
| **Shift+ドラッグ（画像リサイズ中）** | アスペクト比固定 |
| **Ctrl+Z** | アンドゥ |
| **Ctrl+Y** | リドゥ |
| **マウスホイール** | 透明度変更 |
| **Esc** | 現在の編集画面を閉じる／範囲選択をキャンセル |

## ホットキー設定

- タスクトレイ（通知領域）のアイコンを右クリック
- **Hotkey Settings...** からホットキーを有効化・変更
- Microsoft Store版では **Windowsへのサインイン時に起動** から自動起動をON/OFF（Windowsの設定とタスクマネージャーからも変更できます）
- **Third-party Licenses...** から埋め込みフォントとカラーピッカーのライセンスを表示
- ホットキー有効時は設定したキーでキャプチャを起動

## 技術スタック

- .NET 8.0
- WPF (Windows Presentation Foundation)
- C#
- PixiEditor.ColorPicker (透明度対応カラーピッカー)

## 開発環境

- Visual Studio 2022
- Windows 10/11

## 開発者向けドキュメント

- [Codex 作業ガイド](AGENTS.md): Codex が変更時に守るルールと確認事項
- [開発ガイド](DEVELOPMENT.md): セットアップ、ブランチ運用、ビルド、手動回帰項目
- [アーキテクチャ](docs/ARCHITECTURE.md): 実行フロー、座標、レイヤー、設定、既知の制約
- [コード署名とRelease運用](docs/CODE_SIGNING.md): 初回Release、SignPath申請、承認後の署名フロー
- [Microsoft Store 配布](docs/MICROSOFT_STORE.md): MSIX作成、Partner Center、Store提出手順
- [Microsoft Store 更新プレイブック](docs/STORE_UPDATE_PLAYBOOK.md): 別スレッドから修正、次版作成、Store更新を行う手順
- [アイコン更新手順](ScreenCapture/ICON_SETUP.md): EXE と通知領域アイコンの更新方法

## ビルド方法

```sh
# リポジトリをクローン
git clone https://github.com/sakusan393/ScreenCapture.git
cd ScreenCapture

# ビルド
dotnet build ScreenCapture.sln --configuration Debug

# 実行
dotnet run --project ScreenCapture/ScreenCapture.csproj

# 実行用EXEの作成
dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release
# 出力: ScreenCapture\bin\Release\net8.0-windows10.0.19041.0\win-x64\publish\ScreenCapture.exe

# Microsoft Store提出用MSIXの作成
.\scripts\Build-StoreMsix.ps1
```

## ライセンス

ScreenCaptureは [MIT License](LICENSE) で公開しています。個人利用・商用利用、変更、再配布が可能です。詳細な条件はライセンス本文を確認してください。

埋め込みフォントの Noto Sans JP は SIL Open Font License 1.1、PixiEditor.ColorPicker は MIT License で提供されています。ライセンス本文は [`THIRD_PARTY_NOTICES.md`](THIRD_PARTY_NOTICES.md) と、タスクトレイメニューの **Third-party Licenses...** から確認できます。

## Code signing policy

GitHub Releaseの直接配布版は、署名サービスが利用可能になるまで未署名であることをReleaseノートに明記します。Microsoft Store版は、Store認定後にMicrosoftによって署名されます。将来SignPath Foundationの承認を得た場合、GitHub Releaseの署名済み成果物には [SignPath.io](https://signpath.io/) と [SignPath Foundation](https://signpath.org/) の証明書を使用します。

- Committer and reviewer: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))
- Approver: [393](https://393.bz/) ([sakusan393](https://github.com/sakusan393))
- 詳細: [Code signing policy](CODE_SIGNING_POLICY.md)
- [Privacy policy](PRIVACY.md): ScreenCaptureは情報をネットワーク上のシステムへ送信しません。

## バグ報告・機能要望

[Issues](https://github.com/sakusan393/ScreenCapture/issues) までお願いします。

## 作者

Website: [393](https://393.bz/)

GitHub: [@sakusan393](https://github.com/sakusan393)
