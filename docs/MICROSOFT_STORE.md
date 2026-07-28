# Microsoft Store 配布

ScreenCapture を Microsoft Store へ MSIX として提出する手順をまとめます。Store の審査を通過した MSIX は Microsoft によって署名されるため、利用者は GitHub から未署名 EXE を直接ダウンロードする場合の SmartScreen 警告を避けられます。

アプリはすでにMicrosoft Storeで公開されています。現在公開中の版とIdentityは [`store/release-state.json`](../store/release-state.json)、バグ修正や機能追加から次回提出までの手順は [`STORE_UPDATE_PLAYBOOK.md`](STORE_UPDATE_PLAYBOOK.md) を基準にします。

## 1. 開発者登録とアプリ名予約

1. [Microsoft Store developer registration](https://storedeveloper.microsoft.com/) から個人開発者として登録する。
2. Partner Center の **Apps and games** で **New product > MSIX or PWA app** を選ぶ。
3. `ScreenCapture` を検索し、利用可能ならアプリ名を予約する。
4. **Product management > Product identity** を開く。

ScreenCaptureでは次の公開Identityを使用します。APIキーやパスワードではありません。

- 予約した製品名: `393 ScreenCapture`
- `Package/Identity/Name`: `393.393ScreenCapture`
- `Package/Identity/Publisher`: `CN=6F9032AC-F4A1-4304-8FB0-9E12219A5335`
- `Package/Properties/PublisherDisplayName`: `393`

パッケージは英語 (`en-us`) を既定言語、日本語 (`ja-jp`) を追加言語として宣言します。Partner Centerでも日本語と英語のStore登録情報を用意し、アプリUIと掲載情報の対応言語を一致させます。

アプリ名を予約する前に、推測した値や仮の値でStore提出用パッケージを作らないでください。MSIXのIdentityはPartner Centerの値と完全に一致する必要があります。

## 2. Store提出パッケージを作る

リポジトリルートで次を実行します。

```powershell
.\scripts\Build-StoreMsix.ps1
```

出力先は `artifacts/store/` です。

```text
ScreenCapture_<version>_x64.msix
ScreenCapture_<version>_x64.appxsym
ScreenCapture_<version>_x64.msixupload
ScreenCapture_<version>_x64.msixupload.sha256
```

Partner Centerには `.msixupload` をアップロードします。`.msix` はパッケージ本体、`.appxsym` はクラッシュ解析用のシンボルです。Store提出前のパッケージは一般配布せず、審査通過後にMicrosoft Storeから配布してください。

GitHub Actionsの **Store package** ワークフローを手動実行しても、登録済みのIdentityで同じ成果物を作れます。

## 3. `runFullTrust` の説明

ScreenCaptureは既存のWPFデスクトップアプリをMSIXへ格納するため、パッケージマニフェストで `runFullTrust` を宣言します。Partner Centerで用途の説明を求められた場合は、次を使用します。

> ScreenCapture is a WPF desktop application that requires full trust to use Win32 APIs for screen capture, global hotkeys, notification-area integration, clipboard operations, and user-selected file saving. It runs at medium integrity and does not request administrator elevation.

この宣言は管理者権限を要求するものではありません。アプリは通常ユーザーの `mediumIL` で動作します。

## 4. Store掲載情報

最低限、次を用意します。

- 日本語と英語の製品名、短い説明、詳細説明
- アプリアイコン（Store掲載用は300×300推奨）
- 実際の画面を示すスクリーンショット
- カテゴリと年齢区分
- サポートURL: `https://github.com/sakusan393/ScreenCapture/issues`
- プライバシーポリシーURL: `https://github.com/sakusan393/ScreenCapture/blob/main/PRIVACY.md`
- ライセンス情報: `https://github.com/sakusan393/ScreenCapture/blob/main/LICENSE`

## 5. 提出前の確認

- `ScreenCapture.csproj` のバージョンとMSIXの4桁バージョンが対応している。
- `Package/Identity/Name`、`Publisher`、予約した製品名がPartner Centerの表示と完全一致している。
- x64版のWindows 10およびWindows 11で起動できる。
- 範囲選択、ウィンドウキャプチャ、コピー、保存、通知領域、グローバルホットキーを確認する。
- アプリが管理者権限を要求しない。
- Store提出フォームのrestricted capability欄で `runFullTrust` の用途を説明する。

## 6. 公開後の更新

Partner Centerの製品概要で **更新を開始** を選ぶと、前回の申請情報を引き継いだ更新申請が作成されます。更新版では公開中より大きいパッケージバージョンを使用し、認定後は既存ユーザーへMicrosoft Store経由で配信されます。

版の準備、MSIX作成、更新申請、公開完了の記録は [`Microsoft Store 更新プレイブック`](STORE_UPDATE_PLAYBOOK.md) に従ってください。

## 公式資料

- [Publish your first Windows app](https://learn.microsoft.com/windows/apps/package-and-deploy/publish-first-app)
- [Upload MSIX app packages](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/upload-app-packages)
- [Publish update to your MSIX app on the Store](https://learn.microsoft.com/windows/apps/publish/publish-your-app/msix/publish-update-to-your-app-on-store)
- [Generating MSIX package components](https://learn.microsoft.com/windows/msix/desktop/desktop-to-uwp-manual-conversion)
- [App capability declarations](https://learn.microsoft.com/windows/apps/package-and-deploy/app-capability-declarations)
