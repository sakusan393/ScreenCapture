# アプリケーションアイコンの更新

## 現在使われているファイル

- `icon.png`: 1024 x 1024 pxのマスター画像。Microsoft Store用PNGの生成元でもあります。
- `app.ico`: EXEと通知領域で使うマルチサイズICOです。
- `create_icon.ps1`: `icon.png` から `app.ico` を再生成します。

`ScreenCapture.csproj` は `app.ico` をアプリケーションアイコンおよび埋め込みリソースとして参照します。`App.xaml.cs` は同じ埋め込みリソースを通知領域アイコンに使用します。

## 更新手順

1. 正方形のマスター画像で `icon.png` を置き換えます。1024 x 1024 pxを推奨します。
2. ImageMagickをインストールし、`magick` コマンドをPATHから実行可能にします。
3. 次のコマンドでICOを再生成します。

```powershell
Set-Location ScreenCapture
.\create_icon.ps1
```

生成される `app.ico` には、16、20、24、32、40、48、64、128、256 pxの画像が含まれます。

Microsoft Store用の `StoreLogo.png`、`Square44x44Logo.png`、`Square150x150Logo.png` は、Storeパッケージ生成時に `icon.png` から自動生成されます。

## 確認

リポジトリルートでDebugビルドし、EXEと通知領域の両方で新しいアイコンを確認します。

```powershell
dotnet build ScreenCapture.sln --configuration Debug
```

Debug出力先は通常、次のディレクトリです。

```text
ScreenCapture\bin\Debug\net8.0-windows\win-x64\
```

Windowsのアイコンキャッシュにより、エクスプローラー上の表示がすぐ更新されない場合があります。その場合は、EXEのプロパティまたは通知領域でも確認してください。
