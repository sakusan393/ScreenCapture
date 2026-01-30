# アプリケーションアイコンの設定方法

## ?? アイコンファイルの作成

アプリケーションアイコン（`app.ico`）を作成するには、以下のいずれかの方法を使用してください：

### 方法1: オンラインツールを使用（推奨）

1. **Canva**、**Figma**、**Photopea**などのデザインツールで256x256のアイコン画像を作成
2. オンラインのICO変換ツールを使用：
   - https://www.icoconverter.com/
   - https://convertio.co/ja/png-ico/
   - https://www.online-convert.com/
3. 生成された`.ico`ファイルを`ScreenCapture/app.ico`として保存

### 方法2: GIMP（無料ソフト）

1. GIMPで256x256の画像を作成
2. **ファイル** → **エクスポート**
3. ファイル形式を`.ico`に選択
4. `app.ico`として保存

### 方法3: Visual Studio

1. Visual Studioでプロジェクトを開く
2. プロジェクトを右クリック → **追加** → **新しい項目**
3. **アイコンファイル (.ico)** を選択
4. `app.ico`として保存
5. Visual Studioの内蔵エディタで編集

### 方法4: PowerShellスクリプト（シンプル）

プロジェクトに含まれている`create_icon.ps1`を実行：

```powershell
cd ScreenCapture
.\create_icon.ps1
```

## ?? 推奨デザイン

- **サイズ**: 256x256ピクセル（複数サイズを含む）
- **スタイル**: シンプルで認識しやすい
- **テーマ**: カメラ、スクリーン、四角い選択範囲など
- **色**: 青系、赤系がおすすめ

## ?? プロジェクトへの追加

1. `app.ico`ファイルを`ScreenCapture/app.ico`に配置
2. プロジェクトファイル（`ScreenCapture.csproj`）に既に設定済み：
```xml
<ApplicationIcon>app.ico</ApplicationIcon>
```
3. リビルド：
```sh
dotnet build
```

## ? 確認方法

ビルド後、`ScreenCapture.exe`のアイコンが変更されていることを確認：
1. エクスプローラーで`bin\Debug\net8.0-windows\ScreenCapture.exe`を表示
2. アイコンが表示されていればOK

## ?? 現在の状態

- ? プロジェクトファイルにアイコン設定追加済み
- ?? `app.ico`ファイルが必要（上記の方法で作成してください）

## ?? 一時的な解決策

`app.ico`がない場合は、Windowsの標準アイコンが使用されます。

カスタムアイコンを作成するまでは、以下のシンプルな方法もあります：

### 既存のアイコンを使用
Windowsシステムにある既存のアイコンファイル（例: `shell32.dll`内）を参考にするか、
無料のアイコンサイトからダウンロード：
- https://www.flaticon.com/
- https://icons8.com/
- https://www.iconfinder.com/

ライセンスに注意してダウンロードし、`.ico`形式に変換して使用してください。
