# ScreenCapture Project - AI Agent Guidelines

このファイルは、AIエージェント（GitHub Copilot等）がこのプロジェクトで作業する際の指針を定義します。

## プロジェクト概要

**ScreenCapture** は、Rapture風のスクリーンキャプチャアプリケーションです。
画面の一部をキャプチャし、テキスト注釈を追加できる.NET 8 WPFアプリケーションです。

## 技術スタック

- **.NET 8.0**
- **WPF (Windows Presentation Foundation)**
- **C# 12**
- **XAML**

## プロジェクト構造

```
ScreenCapture/
├── ScreenCapture/              # メインプロジェクト
│   ├── App.xaml                # アプリケーションエントリポイント
│   ├── App.xaml.cs
│   ├── CaptureWindow.xaml      # キャプチャ画像を表示するウィンドウ
│   ├── CaptureWindow.xaml.cs
│   ├── SelectionOverlayWindow.xaml  # 範囲選択オーバーレイ
│   ├── SelectionOverlayWindow.xaml.cs
│   ├── DraggableText.xaml      # ドラッグ可能なテキストコントロール
│   └── DraggableText.xaml.cs
├── .gitignore
├── README.md
└── agents.md                   # このファイル
```

## コーディング規約

### 全般

1. **言語**: コメントとドキュメントは日本語で記述
2. **命名規則**: 
   - クラス名: PascalCase（例: `CaptureWindow`）
   - メソッド名: PascalCase（例: `AddTextAt`）
   - プライベートフィールド: camelCase with `_` prefix（例: `_selected`）
   - ローカル変数: camelCase（例: `screenRect`）
3. **インデント**: スペース4つ
4. **改行**: Windows形式（CRLF）

### XAML

1. **属性の順序**:
   - `x:Name` / `x:Class`
   - レイアウト関連（Width, Height, Margin等）
   - 表示関連（Visibility, Opacity等）
   - 動作関連（IsEnabled, IsHitTestVisible等）
   - イベントハンドラ
2. **命名**: コントロール名は用途を明確に（例: `BorderFrame`, `OverlayCanvas`）
3. **コメント**: 各セクションに日本語コメントを追加

### C#

1. **using文**: 
   - System名前空間を先頭にグループ化
   - プロジェクト固有の名前空間は最後
2. **イベントハンドラ**: 
   - ラムダ式を使用してコンパクトに記述
   - 複雑なロジックは別メソッドに分離
3. **非同期処理**: 
   - UI更新は`Dispatcher.BeginInvoke`を使用
   - 適切な`DispatcherPriority`を指定
4. **リソース管理**: 
   - `using`文を使用してリソースを確実に解放
   - Bitmapなどのアンマネージドリソースに注意

## Git ワークフロー

### ブランチ戦略

- **main**: 安定版・リリース可能なコード
- **feature/***: 新機能開発用ブランチ
- **fix/***: バグ修正用ブランチ

### ワークフロー

1. **機能追加時**:
   ```bash
   # featureブランチを作成
   git checkout -b feature/機能名
   
   # 開発・コミット
   git add .
   git commit -m "feat: 機能の説明"
   
   # mainにマージ
   git checkout main
   git merge feature/機能名
   git push origin main
   
   # featureブランチを削除
   git branch -d feature/機能名
   ```

2. **バグ修正時**:
   ```bash
   # fixブランチを作成
   git checkout -b fix/バグ名
   
   # 修正・コミット
   git add .
   git commit -m "fix: バグの説明"
   
   # mainにマージ
   git checkout main
   git merge fix/バグ名
   git push origin main
   
   # fixブランチを削除
   git branch -d fix/バグ名
   ```

### コミットメッセージ規約

**Conventional Commits**形式を使用:

- `feat:` - 新機能追加
- `fix:` - バグ修正
- `docs:` - ドキュメント変更
- `style:` - コードフォーマット（機能に影響なし）
- `refactor:` - リファクタリング
- `test:` - テスト追加・修正
- `chore:` - ビルドプロセスやツールの変更

例:
```
feat: テキストのフォントサイズ変更機能を追加
fix: キャプチャ時の明るさ問題を修正
docs: READMEに使い方を追加
refactor: DraggableTextのコードを整理
```

## 機能実装ガイドライン

### 新機能を追加する際の手順

1. **featureブランチを作成**
   ```bash
   git checkout -b feature/新機能名
   ```

2. **要件を明確化**
   - 機能の目的を明確にする
   - ユーザーストーリーを作成
   - 既存機能への影響を確認

3. **実装**
   - 既存のコーディング規約に従う
   - XAMLとC#のコードを分離
   - 適切なコメントを追加

4. **テスト**
   - 手動テストで動作確認
   - エッジケースを確認
   - 既存機能が壊れていないか確認

5. **コミット**
   ```bash
   git add .
   git commit -m "feat: 新機能の説明"
   ```

6. **mainにマージ**
   ```bash
   git checkout main
   git merge feature/新機能名
   git push origin main
   ```

7. **ブランチをクリーンアップ**
   ```bash
   git branch -d feature/新機能名
   ```

## アーキテクチャ原則

### ウィンドウの役割分担

1. **SelectionOverlayWindow**
   - 画面全体を覆うオーバーレイ
   - 範囲選択の管理
   - スクリーンキャプチャの実行

2. **CaptureWindow**
   - キャプチャ画像の表示
   - テキスト注釈の管理
   - ウィンドウのドラッグ移動

3. **DraggableText**
   - 編集可能なテキストコントロール
   - フォーカス管理
   - ドラッグ移動

### 状態管理

- フォーカス状態: TextBoxのIsHitTestVisibleで制御
- 選択状態: `_selected`フィールドで管理
- ドラッグ状態: `_isDraggingWindow`フラグで管理

### イベント処理

- マウスイベントは各コントロールレベルで処理
- `e.Handled = true`で適切にイベントを止める
- UIスレッドでの更新は`Dispatcher`を使用

## よくある問題と解決策

### 1. TextBoxの背景が白く表示される
**原因**: WPFのデフォルトテンプレートが適用されている
**解決**: カスタム`ControlTemplate`を定義して背景を透明に

### 2. キャプチャ画像が暗い
**原因**: オーバーレイウィンドウが一緒にキャプチャされている
**解決**: キャプチャ前にオーバーレイを`Hide()`する

### 3. テキストがドラッグできない
**原因**: TextBoxがマウスイベントをキャプチャしている
**解決**: `LostFocus`時に`TextBox.IsHitTestVisible = false`に設定

### 4. フォーカスが自動的に設定されない
**原因**: UIが完全にレンダリングされる前にフォーカスを設定
**解決**: `Dispatcher.BeginInvoke`で遅延実行

## パフォーマンス考慮事項

1. **ビットマップのリソース管理**
   - `using`文で確実に解放
   - メモリリークを避ける

2. **UI更新の最適化**
   - 頻繁な更新は避ける
   - 必要に応じて`Dispatcher`の優先度を調整

3. **マルチモニタ対応**
   - `SystemParameters.VirtualScreen*`を使用
   - 座標変換を適切に行う

## 今後の拡張予定

以下の機能追加を検討中:

- [ ] 図形描画（矢印、四角形、円など）
- [ ] クリップボードへのコピー
- [ ] ファイル保存機能
- [ ] ホットキー対応
- [ ] 複数のキャプチャウィンドウ管理
- [ ] 色の選択肢を増やす
- [ ] フォントの選択
- [ ] ウィンドウのリサイズ機能

## ライセンス

MIT License

## 連絡先

GitHub: https://github.com/sakusan393/ScreenCapture
