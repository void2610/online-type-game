# Basic English Typing Sample

このサンプルは、ThockKitを使用した基本的な英語タイピングゲームの実装例です。

## 概要

- **言語:** 英語のみ
- **機能:** シンプルなタイピング練習
- **依存:** Unity UI (uGUI), UniTask, Unity Input System

## セットアップ手順

### 1. シーンの作成

1. 新しいシーンを作成
2. ヒエラルキーに以下の構成でUIを作成：

```
Canvas
├── TypedText (Text)
├── RemainingText (Text)
└── StatusText (Text)
```

### 2. コンポーネントの設定

#### Canvas
- Render Mode: Screen Space - Overlay

#### TypedText
- Font Size: 48
- Alignment: Center, Middle
- Color: 緑系 (例: #33CC33)
- Anchor: Center
- Position: Y = 50

#### RemainingText
- Font Size: 48
- Alignment: Center, Middle
- Color: グレー系 (例: #808080)
- Anchor: Center
- Position: Y = 50

#### StatusText
- Font Size: 24
- Alignment: Center, Top
- Anchor: Top Center
- Position: Y = -50

### 3. LifetimeScopeの設定

1. ヒエラルキーのルートに空のGameObjectを作成し、名前を「LifetimeScope」に変更
2. `BasicTypingLifetimeScope` コンポーネントをアタッチ
3. `BasicTypingView` コンポーネントを同じGameObjectにアタッチ
4. BasicTypingViewの Inspector で、各テキストフィールドを設定：
   - Typed Text: TypedText を設定
   - Remaining Text: RemainingText を設定
   - Status Text: StatusText を設定
5. BasicTypingLifetimeScopeの Inspector で：
   - View: BasicTypingView を設定

## 使い方

1. シーンを再生
2. 「Press any key to start」と表示されたら、任意のキーを押してスタート
3. 表示された単語を入力
4. 全ての問題をクリアすると「Complete!」と表示される
5. 任意のキーで再スタート

## カスタマイズ

### 問題を変更する

`BasicTypingPresenter.cs` の `CreateSampleQuestions()` メソッドを編集してください：

```csharp
private List<TypingQuestion> CreateSampleQuestions()
{
    return new List<TypingQuestion>
    {
        new("your"),
        new("custom"),
        new("words"),
        new("here"),
    };
}
```

### 設定を変更する

`BasicTypingLifetimeScope.cs` の設定を変更できます：

```csharp
// 大文字小文字を区別しない場合
var settings = new TypingSessionSettings(
    skipWhitespace: false,
    skipSymbols: false,
    caseSensitive: false  // 大文字小文字を区別しない
);

// 空白や記号をスキップする場合
var settings = TypingSessionSettings.SkipAll;
```

## ファイル構成

- `BasicTypingPresenter.cs` - プレゼンター（入力処理、状態管理）
- `BasicTypingView.cs` - ビュー（UI表示）
- `BasicTypingLifetimeScope.cs` - VContainer設定

## 次のステップ

- 日本語タイピングサンプルを試す
- カスタムUIに変更する
- スコアシステムを追加する
- 音声やエフェクトを追加する
