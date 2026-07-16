# 開発ガイド

この文書は、人間の開発者と Codex が同じ手順で ScreenCapture を変更・検証するための作業基準です。実装の構造や不変条件は [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) を参照してください。

## 前提環境

- Windows 10 または Windows 11
- .NET 8 SDK（または .NET 8 をターゲット可能な新しい SDK）
- 任意: Visual Studio 2022 の「.NET デスクトップ開発」ワークロード

WPF、Windows Forms、`user32.dll`、画面キャプチャ API を利用するため、ビルド後の動作確認は Windows 上で行います。

## セットアップとビルド

リポジトリルートで次を実行します。

```powershell
dotnet restore ScreenCapture.sln
dotnet build ScreenCapture.sln --configuration Debug
```

起動:

```powershell
dotnet run --project ScreenCapture/ScreenCapture.csproj --configuration Debug
```

Release 発行:

```powershell
dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release
```

プロジェクトには `win-x64`、自己完結、単一ファイル発行が設定されています。発行先は通常 `ScreenCapture/bin/Release/net8.0-windows/win-x64/publish/` です。

Microsoft Store提出用MSIXの作成:

```powershell
.\scripts\Build-StoreMsix.ps1
```

Store用のIdentityと提出手順は [`docs/MICROSOFT_STORE.md`](docs/MICROSOFT_STORE.md) を参照してください。成果物は `artifacts/store/` に生成され、Gitへコミットしません。

## GitHub ActionsとRelease

`.github/workflows/build.yml` はPull Request、`main`へのpush、手動実行でDebugビルドとRelease発行を行い、未署名EXEをGitHub Actions Artifactへ保存します。外部Actionはメジャータグではなく検証済みのコミットSHAへ固定します。

`.github/workflows/release.yml` は `vMAJOR.MINOR.PATCH` タグで起動します。タグが `main` の履歴上にあり、`ScreenCapture.csproj` の `Version` と一致する場合だけ、未署名EXE、SHA-256チェックサム、GitHub Releaseを生成します。初回ReleaseとSignPath承認後の運用は [`docs/CODE_SIGNING.md`](docs/CODE_SIGNING.md) を参照してください。

`.github/workflows/store-package.yml` は手動実行専用です。リポジトリに登録したPartner Centerの公開Identityを使用し、Store提出用の `.msixupload`、MSIX本体、シンボル、SHA-256をArtifactへ保存します。Store用パッケージをGitHub Releaseの署名済みEXEとして扱わないでください。

SignPath承認前は、APIトークンやダミーの組織IDをワークフローへ追加しません。承認後もAPIトークンはGitHub Actions Secretsへ保存し、ログやリポジトリへ出力しないでください。

## ブランチ運用

1. 新しい機能追加や修正は、最新の `main` から `feature/<topic>` ブランチを作成する。
2. 1 ブランチには関連する変更だけを含める。生成物や `%APPDATA%` の個人設定をコミットしない。
3. 作業完了後、ビルド結果と手動確認結果を添えて確認を依頼する。
4. OK が出てから `main` にマージし、その後に push する。

Codex は、ユーザーから明示されない限りコミット、マージ、push を行いません。

## 自動検証

現在は自動テストプロジェクトがありません。すべての変更で次を最低限実行します。

```powershell
dotnet build ScreenCapture.sln --configuration Debug
git diff --check
git status --short
```

パッケージ更新や発行設定の変更では、追加でRelease発行を実行し、生成されたEXEが起動することを確認します。Store/MSIX関連の変更では、登録済みIdentityで `Build-StoreMsix.ps1` を実行し、`.msixupload` が生成されることも確認します。

## 手動回帰チェックリスト

変更箇所に関連する項目に加え、影響範囲が広い場合は一覧を通して確認します。

### 起動と常駐

- 初回起動で全仮想スクリーンを覆う範囲選択画面が表示される。
- 2 回目の起動では別プロセスが常駐せず、既存プロセスへキャプチャ開始が通知される。
- タスクトレイのダブルクリックと「Screen Capture」で範囲選択を開始できる。
- ホットキーを有効化・変更・無効化でき、アプリ再起動後も設定が残る。
- 「Exit」で常駐プロセスが終了する。

### キャプチャ

- ドラッグした範囲が暗幕を含まずに取得される。
- `Ctrl`+クリックで対象ウィンドウ全体を取得できる。
- 負の座標を持つ左／上側モニターを含む複数モニター構成で位置がずれない。
- 100% 以外の表示スケールでも範囲と取得画像が一致する。
- 小さすぎる選択と `Esc` が安全にキャンセルされる。

### 編集とレイヤー

- 右クリックからテキストを追加し、編集、移動、色、背景色、サイズ、削除を操作できる。
- 最後に変更した文字色、背景色、文字サイズが、再起動後に追加したテキストへ反映される。
- `Ctrl+V` で画像を追加し、移動、四隅リサイズ、Shift 比率固定、回転、枠、削除を操作できる。
- Layers パネルの並べ替えが画面の前後関係へ反映され、再起動後も残る。
- 前面へ移動した画像／テキストが同じグループ内の順序だけを変更する。
- 左／上／右／下へ領域を広げても、元画像と注釈の見た目の相対位置が保たれる。

### ペイント

- `Alt` と右クリックメニューの両方でペイントモードを切り替えられる。
- フリーハンド、Shift の水平／垂直線、Ctrl または Arrow ボタンの矢印を描画できる。
- 色と 4 種類の太さが反映され、再起動後も残る。
- `Ctrl+Z` / `Ctrl+Y` とツールバーの Undo / Redo がストローク単位で動作する。
- 履歴上限を変更したとき、古い描画だけが削除される。
- Paint が前面の場合は描画が優先され、Paint が背面の場合は画像／テキストを操作できる。

### 出力と表示

- `Ctrl+C` で元画像、背景の拡張部分、ペイント、画像、テキストが合成される。
- `Ctrl+S` で画像を保存できる。現実装のエンコーダーは PNG 固定である。
- コピー／保存結果に枠、ボタン、Layers パネル、カラーピッカー、選択ハンドルが入らない。
- コピー／保存の完了またはキャンセル後に、選択状態と補助 UI が元に戻る。
- マウスホイールで透明度が 1% から 100% の範囲に収まる。

## 設定とテストデータ

設定は次の場所へ保存されます。

```text
%APPDATA%\ScreenCapture\hotkey-settings.json
%APPDATA%\ScreenCapture\text-style.json
```

手動テストは普段使いの設定を書き換えます。初期値の確認が必要な場合はアプリを終了し、ファイルを退避してから起動してください。確認後は退避したファイルを戻します。設定 JSON はリポジトリへコピーしません。

## 実装時の注意

- XAML の名前を変更したら `FindName` と code-behind の直接参照を検索する。
- UI イベントと `Dispatcher` は WPF UI スレッド上で扱う。
- `System.Drawing` と WPF に同名型があるため、既存の `WpfPoint`、`WpfColor` などの alias 方針を維持する。
- `CaptureWindow` のコピーと保存は画面を直接レンダリングする。補助 UI を追加した場合は出力除外の処理も更新する。
- 設定クラスはプロパティ代入ごとに JSON を保存する。複数項目の追加では途中状態と旧ファイルの読み込みを考慮する。
- ネイティブ API の変更では、ハンドルの所有者、登録解除、アプリ終了時の後始末を確認する。

## トラブルシューティング

- 起動しても新しいプロセスがすぐ終了する: 既存の ScreenCapture がタスクトレイに常駐していないか確認する。
- ホットキーが動かない: 他アプリとの競合時は登録に失敗し、自動的に無効化される。別の組み合わせを設定する。
- 設定を初期化したい: アプリ終了後に `%APPDATA%\ScreenCapture` の JSON を退避する。
- ビルドで Windows Desktop / targeting pack が見つからない: .NET 8 SDK または Visual Studio の「.NET デスクトップ開発」を導入する。
