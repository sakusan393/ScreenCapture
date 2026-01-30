# GitHub Releaseの作成手順

## ?? 準備完了

? タグ `v1.0.0` を作成してプッシュ済み  
? 配布用EXEファイル: `D:\00_visualstudio\ScreenCapture\ScreenCapture\bin\Release\net8.0-windows\win-x64\publish\ScreenCapture.exe`  
? リリースノート: `GITHUB_RELEASE.md`

## ?? GitHubでReleaseを作成する手順

### 1. GitHubのリポジトリページを開く
https://github.com/sakusan393/ScreenCapture

### 2. Releasesページに移動
- 右側のサイドバーの **「Releases」** をクリック
- または、直接アクセス: https://github.com/sakusan393/ScreenCapture/releases

### 3. 新しいReleaseを作成
- **「Draft a new release」** ボタンをクリック

### 4. Releaseの設定

#### Tag version
- ドロップダウンから **`v1.0.0`** を選択
  （すでにプッシュ済みのタグが表示されます）

#### Release title
```
ScreenCapture v1.0.0 - 初回リリース
```

#### Description
- `GITHUB_RELEASE.md` の内容をコピー＆ペースト
- または、以下の内容をコピー：

```markdown
画面キャプチャ＋注釈ツール - 初回リリース

## ?? ダウンロード

**`ScreenCapture.exe`** (約155MB) をダウンロードして、ダブルクリックで起動できます。

## ? 主な機能

- ?? 画面範囲選択キャプチャ
- ?? テキスト注釈（8色・サイズ変更可能）
- ??? 画像貼り付け・リサイズ・回転
- ?? ペイント機能（6色・4段階の太さ・Shiftで直線）
- ?? アンドゥ・リドゥ（履歴10-100回）
- ?? Ctrl+Cでクリップボードにコピー

詳細は下記のリリースノートをご覧ください。

---

?? **動作環境**: Windows 10/11 (64bit)  
?? **インストール**: 不要（.NET Runtime内蔵）  
?? **ライセンス**: 無料（個人・商用利用可）
```

### 5. ファイルをアップロード

#### 方法A: ドラッグ&ドロップ
1. Descriptionエリアの下にある **「Attach binaries by dropping them here or selecting them.」** をクリック
2. または、直接ファイルをドラッグ&ドロップ

#### 方法B: ファイル選択
1. ファイル選択ダイアログを開く
2. 以下のパスのファイルを選択：
```
D:\00_visualstudio\ScreenCapture\ScreenCapture\bin\Release\net8.0-windows\win-x64\publish\ScreenCapture.exe
```

### 6. オプション設定（推奨）

- ? **Set as the latest release** にチェック
- ? **Set as a pre-release** はチェックしない（正式版なので）

### 7. 公開
- 緑色の **「Publish release」** ボタンをクリック

## ? 完了！

Releaseが公開されました！以下のURLでアクセスできます：

```
https://github.com/sakusan393/ScreenCapture/releases/tag/v1.0.0
```

## ?? 共有

他の人に以下のリンクを送れば、ダウンロードできます：

- **リリースページ**: https://github.com/sakusan393/ScreenCapture/releases/latest
- **直接ダウンロード**: https://github.com/sakusan393/ScreenCapture/releases/download/v1.0.0/ScreenCapture.exe

## ?? 次回のリリース時

新しいバージョンをリリースする場合：

1. コードを更新
2. ビルド: `dotnet publish -c Release`
3. タグ作成: `git tag -a v1.1.0 -m "Version 1.1.0"`
4. プッシュ: `git push origin v1.1.0`
5. GitHub Releasesで新しいリリースを作成

---

**質問・サポート**: GitHubのIssuesをご利用ください。
