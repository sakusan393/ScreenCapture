# アプリケーションアイコンの更新

## 現在使われているファイル

- `app202620231858.ico`: EXE の `ApplicationIcon` と埋め込みリソース
- `icon.png`: 元画像として利用できる PNG
- `create_icon.ps1`: 簡易なカメラアイコンを `app.ico` として生成する補助スクリプト

`ScreenCapture.csproj` は `app202620231858.ico` を参照しています。また、`App.xaml.cs` は通知領域アイコンを埋め込みリソース名の末尾 `app202620231858.ico` で検索します。アイコン名を変更する場合は、この 2 ファイルを同時に更新してください。

## 推奨する更新手順

1. 256 x 256 px を含む複数解像度の ICO を用意する。
2. 既存の `app202620231858.ico` を置き換える。
3. リポジトリルートで Debug ビルドする。
4. EXE と通知領域の両方で新しいアイコンを確認する。

```powershell
dotnet build ScreenCapture.sln --configuration Debug
```

Debug の出力先は通常、次のディレクトリです。

```text
ScreenCapture\bin\Debug\net8.0-windows\win-x64\
```

Windows のアイコンキャッシュにより、エクスプローラー上の表示がすぐ更新されない場合があります。EXE のプロパティまたは通知領域でも確認してください。

## 補助スクリプトを使う場合

`create_icon.ps1` は現状 `ScreenCapture/app.ico` を生成し、プロジェクトが参照する名前とは異なります。

```powershell
Set-Location ScreenCapture
.\create_icon.ps1
```

生成内容を確認した後、採用する場合は `app.ico` を `app202620231858.ico` として置き換えるか、次の参照をすべて新しい名前へ変更します。

- `ScreenCapture.csproj` の `<ApplicationIcon>`
- `ScreenCapture.csproj` の `<EmbeddedResource>`
- `App.xaml.cs` の埋め込みリソース検索とフォールバックファイル名

`create_icon.ps1` は簡易生成用です。Windows の複数サイズを確実に含む配布用 ICO は、専用の画像編集・変換ツールで作成することを推奨します。外部素材を使う場合はライセンスも確認してください。
