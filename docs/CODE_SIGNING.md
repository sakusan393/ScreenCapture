# コード署名とRelease運用

この文書は、SignPath Foundationへの申請前後でScreenCaptureのReleaseを作成する手順をまとめたものです。公開ポリシーは [`CODE_SIGNING_POLICY.md`](../CODE_SIGNING_POLICY.md)、利用者向けのプライバシー説明は [`PRIVACY.md`](../PRIVACY.md) を参照してください。

## 現在の段階

SignPath Foundationは、署名対象と同じ形式のアプリがすでに公開されていることを申請条件にしています。そのため、最初にGitHub Actionsから未署名の単一EXEをReleaseとして公開します。Releaseノートには未署名であることを明記します。

初回Release後にSignPath Foundationへ申請し、承認時に発行される組織ID、プロジェクトslug、署名ポリシーslug、APIトークンを使って署名ステップを追加します。値が確定する前にダミー値をワークフローへ追加しないでください。

## 初回の未署名Release

1. `ScreenCapture/ScreenCapture.csproj` の `Version` が公開するバージョンであることを確認する。
2. Release準備の変更を `main` へマージする。
3. バージョンと一致するタグを作成してpushする。バージョン `1.0.0` ならタグは `v1.0.0` とする。

```powershell
git switch main
git pull --ff-only
git tag -a v1.0.0 -m "ScreenCapture 1.0.0"
git push origin v1.0.0
```

`.github/workflows/release.yml` はタグが `main` の履歴上にあり、タグとプロジェクトのバージョンが一致することを検証します。成功すると次を生成します。

- GitHub Actions Artifact内の未署名 `ScreenCapture.exe`
- `ScreenCapture.exe` のSHA-256チェックサム
- 同じ2ファイルを添付したGitHub Release

## SignPath Foundationへの申請

申請前に次を確認します。

- プロジェクト本体と全コンポーネントがOSI承認済みライセンスで公開されている。
- GitHubアカウントで多要素認証を有効化している。
- READMEに **Code signing policy** とPrivacy policyへのリンクがある。
- 機能説明と、実行可能な単一EXEを含むGitHub Releaseがある。
- GitHub Actionsの履歴からReleaseのビルドを確認できる。

申請先と公式条件:

- [SignPath Foundation application](https://signpath.org/apply)
- [SignPath Foundation conditions](https://signpath.org/terms.html)
- [SignPath GitHub integration](https://docs.signpath.io/trusted-build-systems/github)

### OSS依存関係の確認

`dotnet list ScreenCapture/ScreenCapture.csproj package --include-transitive` で、署名対象に含まれるNuGet依存を確認します。現在確認済みの構成は次のとおりです。

| コンポーネント | 種別 | ライセンス |
|---|---|---|
| ScreenCapture | プロジェクト本体 | MIT |
| PixiEditor.ColorPicker / PixiEditor.ColorPicker.Models | 直接／推移的NuGet依存 | MIT |
| Microsoft.Xaml.Behaviors.Wpf | 推移的NuGet依存 | MIT |
| System.Drawing.Common / Microsoft.Win32.SystemEvents | 直接／推移的NuGet依存 | MIT |
| .NET / WPF self-contained runtime | 発行ランタイム | MITおよび各Third-party notices |
| Noto Sans JP | 埋め込みフォント | SIL Open Font License 1.1 |

パッケージ更新時は、解決された推移的依存とライセンスを再確認し、必要な通知を [`THIRD_PARTY_NOTICES.md`](../THIRD_PARTY_NOTICES.md) へ反映します。

## 承認後

承認後は、Releaseワークフローで未署名Artifactをアップロードした直後に `signpath/github-action-submit-signing-request` を実行します。必要な値はGitHub Actions SecretsまたはSignPathから指定された安全な設定へ保存し、リポジトリへ直接書き込みません。

署名済みArtifactを受け取ってからSHA-256を計算し、その署名済みEXEだけをGitHub Releaseへ添付します。署名要求は毎回Approverが手動で承認します。
