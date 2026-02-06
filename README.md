# ScreenCapture

画面キャプチャ＋注釈ツール - Rapture風のスクリーンキャプチャアプリケーション

[![Release](https://img.shields.io/github/v/release/sakusan393/ScreenCapture)](https://github.com/sakusan393/ScreenCapture/releases/latest)
[![Downloads](https://img.shields.io/github/downloads/sakusan393/ScreenCapture/total)](https://github.com/sakusan393/ScreenCapture/releases)
[![License](https://img.shields.io/github/license/sakusan393/ScreenCapture)](LICENSE)

## 主な機能

### 画面キャプチャ
- ドラッグで範囲を選択してキャプチャ
- **Ctrl+クリック**でクリックしたウィンドウをキャプチャ
- 複数モニター対応
- **Ctrl+C**でクリップボードにコピー
- **Ctrl+S**で画像として保存（PNG/JPEG/BMP）

### テキスト注釈
- 右クリックでテキストを追加
- **カラーピッカー**で文字色を選択（透明度対応）
- **背景色**をカラーピッカーで選択（透明度対応）
- **サイズ変更**は [+][-] ボタン、または押しっぱなしで連続変更
- ダブルクリックで再編集
- ドラッグで移動
- 色設定は次回起動時も保持

### 画像の貼り付け
- **Ctrl+V**でクリップボードから画像を追加
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
- ペイント設定は次回起動時も保持

### その他
- マウスホバーで枠線・ボタンを表示
- 枠線色をカラーピッカーで変更（透明度対応）
- キャプチャウィンドウの背景色をカラーピッカーで変更
- 四辺・四隅をドラッグしてエリアを拡張（拡張分は背景色で塗りつぶし）
- 複数の要素を選択すると最前面に移動
- **Layers** パネルでペイント/画像/テキストのレイヤー順をドラッグで変更（順序は保持され、保存/コピーには含まれません）
- **Esc**キーで終了
- 最小化
- **マウスホイール**でキャプチャウィンドウの透明度を変更（ほぼ完全透明まで可能）

## 使い方

1. **ScreenCapture.exe**をダブルクリックして起動
2. マウスドラッグで範囲を選択してキャプチャ
3. 右クリックで「テキスト追加」、または **Alt** でペイントモード
4. **Ctrl+C** でクリップボードにコピー
5. Word/PowerPointなどに貼り付け

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
| **Esc** | 終了 |

## ホットキー設定

- タスクトレイ（通知領域）のアイコンを右クリック
- **Hotkey Settings...** からホットキーを有効化・変更
- ホットキー有効時は設定したキーでキャプチャを起動

## 技術スタック

- .NET 8.0
- WPF (Windows Presentation Foundation)
- C#
- Extended WPF Toolkit (カラーピッカー)

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

# 実行用EXEの作成
cd ScreenCapture
dotnet publish -c Release
# 出力: bin\Release\net8.0-windows\win-x64\publish\ScreenCapture.exe
```


## ライセンス

このソフトウェアは個人利用・商用利用ともに無料で使用できます。

## バグ報告・機能要望

[Issues](https://github.com/sakusan393/ScreenCapture/issues) までお願いします。

## 作者

GitHub: [@sakusan393](https://github.com/sakusan393)

