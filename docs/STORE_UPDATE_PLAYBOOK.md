# Microsoft Store 更新プレイブック

この文書は、別の Codex スレッドや別の日の作業でも、公開済みの 393 ScreenCapture を安全に更新するための手順です。会話履歴ではなく、[`store/release-state.json`](../store/release-state.json) を現在公開中の版の基準にします。

## 1. 新しいスレッドで作業を始める

新しいスレッドでは、最初に次を読みます。

1. [`AGENTS.md`](../AGENTS.md)
2. [`README.md`](../README.md)
3. [`DEVELOPMENT.md`](../DEVELOPMENT.md)
4. [`docs/ARCHITECTURE.md`](ARCHITECTURE.md)
5. この文書
6. [`store/release-state.json`](../store/release-state.json)

作業開始時に `git status --short`、現在のブランチ、最新の `main` を確認します。バグ修正や機能追加は最新の `main` から `feature/<topic>` ブランチを作成し、依頼と無関係な既存変更を混ぜません。

新しいスレッドへは、次のように依頼できます。

```text
D:\00_visualstudio\ScreenCapture の 393 ScreenCapture を更新してください。
AGENTS.md と docs/STORE_UPDATE_PLAYBOOK.md、store/release-state.json を最初に確認し、
「ここに修正または追加したい内容を書く」を実装してください。
必要な回帰確認を行い、次の Microsoft Store 更新版を準備してください。
Partner Center への提出や公開済み状態の更新は、私が公開完了を伝えるまで行わないでください。
```

## 2. 修正または機能追加を行う

通常の開発は [`DEVELOPMENT.md`](../DEVELOPMENT.md) に従います。特に次を守ります。

- XAML と対応する code-behind、設定保存、コピー／保存結果への影響をまとめて確認する。
- `%APPDATA%\ScreenCapture` の個人設定、`bin/`、`obj/`、`artifacts/`、MSIX成果物はコミットしない。
- UIや仕様を変えた場合は、README、アーキテクチャ、Storeスクリーンショットも必要に応じて更新する。
- Store提出の前に、修正内容を feature ブランチでレビューし、`main` へ取り込む。

最低限の検証:

```powershell
dotnet restore ScreenCapture.sln
dotnet build ScreenCapture.sln --configuration Debug
dotnet publish ScreenCapture/ScreenCapture.csproj --configuration Release
git diff --check
```

自動テストだけでは画面キャプチャ、DPI、複数モニター、通知領域、グローバルホットキーを確認できないため、変更箇所に対応する手動回帰確認も行います。

## 3. 次のStore版を準備する

修正内容が固まったら、公開中の版より大きい3桁バージョンを指定します。

- バグ修正や小規模な変更: PATCHを上げる（例: `1.0.3` → `1.0.4`）
- 後方互換のある大きな機能追加: MINORを上げる（例: `1.0.3` → `1.1.0`）
- 互換性を大きく変える更新: MAJORを上げる

たとえば次のバグ修正版を準備する場合:

```powershell
.\scripts\Prepare-StoreUpdate.ps1 -Version 1.0.4
```

このスクリプトは次を行います。

- `store/release-state.json` の公開版より新しいことを検証する。
- `ScreenCapture/ScreenCapture.csproj` の `<Version>` を更新する。
- `store/releases/<version>.md` に変更内容と検証結果を記録するためのひな形を作る。

MSIXはプロジェクトの3桁版に末尾 `.0` を付けます。プロジェクト `1.0.4` からStoreパッケージ `1.0.4.0` が作られます。同じ版や古い版を新しい提出物として使いません。

作成されたリリースノートの「変更内容」「手動確認」「このバージョンの最新情報」を埋め、バージョン変更を修正コードと同じPRに含めます。

## 4. Store提出パッケージを作る

`main` に取り込まれた提出対象コミットから作成します。

```powershell
.\scripts\Build-StoreMsix.ps1
```

またはGitHub Actionsの **Store package** を対象ブランチ／コミットで手動実行します。出力先は `artifacts/store/` です。Partner Centerへアップロードするファイルは次です。

```text
artifacts/store/ScreenCapture_<4桁version>_x64.msixupload
```

提出前に次を確認します。

- ファイル名とPartner Center解析結果の版が予定した4桁版である。
- Package Identity、Publisher、x64、Windows Desktopの条件が以前の公開版と一致する。
- SHA-256ファイルを保管し、提出した成果物を取り違えていない。
- ReleaseビルドとMSIXをWindows 10/11 x64で起動し、変更箇所と主要機能を手動確認した。

## 5. Partner Centerから更新する

Partner Centerの製品概要で **更新を開始** を選ぶと、以前の申請内容を引き継いだ新しい申請が作られます。

1. **パッケージ**で新しい `.msixupload` をアップロードする。
2. 古いパッケージが重複として表示された場合は、より高い版が同じ対象デバイスをカバーしていることを確認して古いパッケージを提出物から外す。
3. **Store 登録情報**の「このバージョンの最新情報」に `store/releases/<version>.md` の要約を記入する。
4. UIが変わった場合だけ、実際の最新UIでStoreスクリーンショットを差し替える。
5. `runFullTrust` の用途やテスト手順に変更があれば説明も更新する。
6. 段階的ロールアウトを使う場合は割合を設定する。アプリ内の更新確認APIを実装していない現状では「必須の更新」は設定しない。
7. 認定へ送信する。

認定中は、緊急性がない限り同じ提出物を取り消して作り直しません。別の修正が必要になった場合はコードを別ブランチで準備し、現在の認定結果を待って次の版に含めます。

## 6. 公開完了を記録する

Partner CenterとMicrosoft Storeで新しい版の公開を確認してから実行します。認定へ送信した時点では実行しません。

```powershell
.\scripts\Complete-StoreRelease.ps1 -Version 1.0.4 -PublishedDate 2026-08-01
```

このスクリプトは次を検証・更新します。

- `ScreenCapture.csproj` が公開した版と一致している。
- 以前の公開版より新しい。
- `store/release-state.json` の最終公開版と公開日。
- 対応する `store/releases/<version>.md` の状態。

この記録変更をコミットして `main` へ取り込みます。必要に応じて同じコミットへ `v<version>` タグを付けます。これにより、次のスレッドは正しい公開版から更新を開始できます。

## 7. 秘密情報と成果物

コミットしてよいもの:

- Store公開Identity（Name、Publisher、表示名）
- MSIXマニフェストのテンプレート
- 更新スクリプト、リリースノート、実画面のStoreスクリーンショット

コミットしないもの:

- Partner Centerのパスワード、APIキー、Cookie、証明書秘密鍵
- `.msix`、`.msixupload`、`.appxsym`、PDBなどの生成物
- `%APPDATA%\ScreenCapture` の設定JSON

## 公式資料

- [Publish update to your MSIX app on the Store](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/publish-update-to-your-app-on-store)
- [Upload MSIX app packages](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/upload-app-packages)
- [Manage and update your app](https://learn.microsoft.com/windows/apps/publish/faq/manage-and-update-your-app)
