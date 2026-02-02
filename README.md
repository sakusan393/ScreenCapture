# ScreenCapture

画面キャプチャ＋注釈ツール - Rapture風のスクリーンキャプチャアプリケーション

[![Release](https://img.shields.io/github/v/release/sakusan393/ScreenCapture)](https://github.com/sakusan393/ScreenCapture/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/sakusan393/ScreenCapture/total)](https://github.com/sakusan393/ScreenCapture/releases)
[![License](https://img.shields.io/github/license/sakusan393/ScreenCapture)](LICENSE)

## ダウンロード

**[最新版をダウンロード (ScreenCapture.exe)](https://github.com/sakusan393/ScreenCapture/releases/latest)**

- **サイズ**: 約155MB
- **動作環境**: Windows 10/11 (64bit)
- **インストール不要**: ダブルクリックで即起動

## 主な機能

### 画面キャプチャ
- ドラッグで範囲を選択してキャプチャ
- 複数モニター対応
- Ctrl+Cでクリップボードにコピー

### テキスト注釈
- 右クリックでテキスト追加
- **カラーピッカー**で文字色を選択
- **背景色**をカラーピッカーで選択（透明に戻すボタンあり）
- **サイズ変更**: [+][-]ボタンまたは押しっぱなしで連続変更
- ダブルクリックで再編集
- ドラッグで移動
- 色設定は次回起動時も保持

### 画像の貼り付け
- **Ctrl+V**でクリップボードから画像を追加
- リサイズ：四隅のハンドルをドラッグ
- 回転：上部中央のハンドルをドラッグ
- ドラッグで移動
- 枠線表示のON/OFF（10px/20pxで切り替え）
- 枠線色をカラーピッカーで変更

### ペイント機能
- **Alt**キーでペイントモード ON/OFF
- 右クリックメニューからペイントモード切り替え
- **カラーピッカー**で色を選択
- **4段階の太さ**（1, 3, 5, 10）
- **Shift+ドラッグ**で水平・垂直の直線
- **Ctrl+Z**でアンドゥ（元に戻す）
- **Ctrl+Y**でリドゥ（やり直す）
- **履歴回数設定**：10/20/50/100回から選択
- ペイント設定は次回起動時も保持

### その他
- マウスホバーで枠線表示・×ボタン表示
- 複数の要素を選択で最前面に移動
- Escキーで終了
- 最小化機能
- 右クリックメニューから枠線色を変更
- マウスホイールでキャプチャウィンドウの透明度を変更（枠線は影響を受けません）

## 使い方

1. **ScreenCapture.exe**をダブルクリックして起動
2. マウスドラッグで範囲を選択してキャプチャ
3. 右クリックで「テキスト追加」、またはCtrlでペイントモード
4. Ctrl+Cでクリップボードにコピー
5. Word/PowerPointなどに貼り付け

詳細な使い方は [RELEASE.md](RELEASE.md) をご覧ください。

## ショートカットキー

| キー | 機能 |
|------|------|
| **Ctrl+C** | クリップボードにコピー |
| **Ctrl+V** | 画像を貼り付け |
| **Alt** | ペイントモード ON/OFF |
| **Shift+ドラッグ** | 水平・垂直線 |
| **Ctrl+Z** | アンドゥ |
| **Ctrl+Y** | リドゥ |
| **Esc** | 終了 |

## 技術スタック

- .NET 8.0
- WPF (Windows Presentation Foundation)
- C#

## 開発環境

- Visual Studio 2022
- Windows 10/11

## ビルド方法

```sh
# リポジトリをクローン
git clone https://github.com/sakusan393/ScreenCapture.git
cd ScreenCapture

# ビルド
dotnet build

# 実行
dotnet run --project ScreenCapture/ScreenCapture.csproj

# 配布用EXEの作成
cd ScreenCapture
dotnet publish -c Release
# 出力: bin\Release\net8.0-windows\win-x64\publish\ScreenCapture.exe
```

## 更新履歴

詳細は [Releases](https://github.com/sakusan393/ScreenCapture/releases) をご覧ください。

## ライセンス

このソフトウェアは個人利用・商用利用ともに無料で使用できます。

## バグ報告・機能要望

[Issues](https://github.com/sakusan393/ScreenCapture/issues) までお願いします。

## 作者

GitHub: [@sakusan393](https://github.com/sakusan393)

