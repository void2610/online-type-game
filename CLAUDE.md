# CLAUDE.md

オンラインタイピングゲーム。Unity 6 (6000.0.58f2)。

## アーキテクチャ

MVPパターン + R3 / VContainer / UniTask。

- **Model**: 純粋なC#クラス。ReactivePropertyで状態公開
- **View**: MonoBehaviour。表示のみ、ロジックなし
- **Presenter**: IStartable / IDisposable。ModelとViewを購読で接続

コルーチンではなくUniTask、DOTweenではなくLitMotionを使う。

## コーディング規約

- コメント・XMLドキュメントは日本語
- `[SerializeField] private` でインスペクター公開（publicフィールド禁止）
- コードスタイルの詳細はRoslynアナライザー（`Assets/Plugins/Analyzers/`）と`.editorconfig`に従う

## uLoop MCP

Unityエディタ操作にはuLoop MCPを積極的に使うこと。

- コード変更後は`compile` → `get-logs`でエラー確認
- シーン確認は`get-hierarchy`、アセット検索は`unity-search`
- コンポーネント追加・参照設定は`execute-dynamic-code`
- 動作確認は`control-play-mode` + `capture-window`
- テスト実行は`run-tests`
- `execute-dynamic-code`でファイルI/Oは使わない

## テスト

- 配置: `Assets/Tests/Runtime/[Category]/[ClassName]Tests.cs`
- メソッド名は日本語: `メソッド名_条件_期待結果()`
- AAAパターン、Constraint Modelアサーション

## サブモジュール

- `my-unity-utils/` → `Assets/Scripts/Utils/`（symlink）: 共通ユーティリティ。重複実装を避けること
- `my-unity-settings/`: 設定システム
- `unity-analyzers/`: Roslynアナライザー。更新時は`dotnet build -c Release`してDLLをコピー
