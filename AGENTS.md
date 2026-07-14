# Codex 作業ガイド

このファイルはリポジトリ全体に適用します。ScreenCapture は Windows 専用の .NET 8 / WPF アプリケーションです。変更前に、ユーザー向け仕様は `README.md`、開発と検証の手順は `DEVELOPMENT.md`、実装上の設計は `docs/ARCHITECTURE.md` を確認してください。

## 作業開始時

1. `git status --short` で既存変更を確認し、依頼と無関係な変更を上書きしない。
2. `DEVELOPMENT.md` の運用ルールに従い、機能追加・修正は `feature/...` ブランチで行う。
3. 変更対象の XAML と code-behind をセットで読み、イベント配線と `x:Name` の参照関係を確認する。
4. 挙動を変更する場合は、必要に応じて `README.md` と `docs/ARCHITECTURE.md` も更新する。

## リポジトリの要点

- ソリューション: `ScreenCapture.sln`
- アプリプロジェクト: `ScreenCapture/ScreenCapture.csproj`
- エントリーポイント: `ScreenCapture/App.xaml` と `ScreenCapture/App.xaml.cs`
- キャプチャ範囲選択: `SelectionOverlayWindow`
- キャプチャ後の編集画面: `CaptureWindow`
- テキスト・画像オーバーレイ: `DraggableText` / `DraggableImage`
- ペイントツールバー: `PaintToolbarWindow`
- 設定: `TextStyleSettings` / `HotKeySettings`
- `MainWindow` は現在の起動経路では使われていない雛形。起動画面だと判断して実装を追加しない。
- 自動テストプロジェクトはまだない。ビルドと Windows 上での手動確認が必須。

## 標準コマンド

リポジトリルートで実行します。

```powershell
dotnet restore ScreenCapture.sln
dotnet build ScreenCapture.sln --configuration Debug
dotnet run --project ScreenCapture/ScreenCapture.csproj --configuration Debug
dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release
```

ドキュメントだけの変更でも、原則として `dotnet build ScreenCapture.sln --configuration Debug` が成功することを確認してください。実行や UI 確認は Windows セッションが必要です。

## 実装時に守る不変条件

- WPF の論理座標、スクリーンの物理ピクセル、DPI スケールを混同しない。範囲選択、ウィンドウ単位キャプチャ、複数モニターは必ず別々に確認する。
- キャプチャ前は選択オーバーレイを隠し、描画完了を待ってから `CopyFromScreen` を呼ぶ。待機を削る場合は暗幕が画像へ混入しないことを検証する。
- `CaptureWindow` の `OverlayCanvas` はペイント線、画像、テキストで共有される。新しい要素種別を追加した場合は、Z-order、ヒットテスト、左上方向の領域拡張、コピー／保存、選択 UI の除外をまとめて対応する。
- レイヤーグループ間の Z-index は 1000 刻みで管理する。Layers パネルの上の項目ほど前面で、同一グループ内では選択された画像／テキストが前面へ移動する。
- 左または上へキャプチャ領域を広げる処理では、元画像の `TranslateTransform` と全オーバーレイ要素を同じ見た目の位置へ補正する。
- ペイントレイヤーが画像やテキストより上にあるときは、ペイント中だけ下層要素のヒットテストを無効化し、描画入力を優先する。
- コピー／保存に、枠、閉じる／最小化ボタン、カラーピッカー、Layers パネル、選択ハンドルを含めない。処理後は成功・キャンセル・例外のいずれでも UI 状態を復元する。
- 設定は `%APPDATA%\ScreenCapture` に即時保存される。既存 JSON の欠損項目を既定値で扱える互換性を維持する。
- Win32 API、通知領域、グローバルホットキー、単一起動制御は `App` のライフサイクルに属する。ウィンドウを閉じても常駐動作を継続する設計を崩さない。

## 変更後の確認

最低限、次を実施して結果を報告してください。

1. Debug ビルドが警告・エラーなしで完了する。
2. 変更した経路を Windows 上で手動確認する。詳細な回帰項目は `DEVELOPMENT.md` を参照する。
3. `git diff --check` で空白エラーがない。
4. `git diff` で生成物、個人設定、無関係な変更が混入していない。

## ドキュメントの役割

- `README.md`: 利用者向けの機能、操作、導入。
- `DEVELOPMENT.md`: 開発環境、コマンド、ブランチ運用、検証チェックリスト。
- `docs/ARCHITECTURE.md`: 実行フロー、責務、座標・レイヤー・設定の設計、既知の制約。
- `ScreenCapture/ICON_SETUP.md`: 現在のアイコン資産と更新方法。

