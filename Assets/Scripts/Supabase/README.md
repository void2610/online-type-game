# Unity × Supabase 軽量RESTクライアント

WebGL対応の軽量Supabase RESTクライアント実装。`UnityWebRequest` + `UniTask` で完全実装。

## 特徴

- **WebGL完全対応**: `UnityWebRequest` のみ使用、WebSocket依存なし
- **軽量**: 必要な機能のみを実装、公式SDKの巨大な依存なし
- **UniTask統合**: すべての非同期処理でUniTaskを使用
- **R3統合**: ポーリングイベントはR3のObservableで提供
- **Assembly Definition**: `Void2610.Supabase` として独立したアセンブリ
- **コーディング規約準拠**: プロジェクトのコーディング規約に完全準拠

## セットアップ

### 1. SupabaseSettings作成

```
1. Project ウィンドウで右クリック
2. Create > Supabase > Settings
3. URLとAnonymous Keyを設定
```

### 2. 初期化

```csharp
using Void2610.Supabase;
using VContainer;

public class SupabaseInstaller : LifetimeScope
{
    [SerializeField] private SupabaseSettings settings;

    protected override void Configure(IContainerBuilder builder)
    {
        var client = new SupabaseClient(settings);
        builder.RegisterInstance(client);

        // セッション復元
        client.Auth.RestoreSession();
    }
}
```

## 使用例

### 認証 (Auth)

```csharp
// メールアドレスでサインアップ
var session = await client.Auth.SignUpAsync("test@example.com", "password123");

// サインイン
var session = await client.Auth.SignInAsync("test@example.com", "password123");

// 匿名サインイン
var session = await client.Auth.SignInAnonymouslyAsync();

// セッションリフレッシュ
var newSession = await client.Auth.RefreshSessionAsync();

// サインアウト
await client.Auth.SignOutAsync();

// セッション情報
if (client.Auth.IsSignedIn)
{
    var userId = client.Auth.CurrentSession.User.Id;
}
```

### データベース (Database)

```csharp
// テーブル定義
[Serializable]
public class Player
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("score")]
    public int Score { get; set; }
}

// SELECT (全件取得)
var players = await client.Db.From<Player>("players")
    .Select("*")
    .ExecuteAsync();

// SELECT (条件付き)
var topPlayers = await client.Db.From<Player>("players")
    .Select("*")
    .Gt("score", 100)
    .Order("score", false)
    .Limit(10)
    .ExecuteAsync();

// INSERT
var newPlayer = new { name = "Alice", score = 0 };
var inserted = await client.Db.From<Player>("players")
    .InsertAsync(newPlayer);

// UPDATE
var updated = await client.Db.From<Player>("players")
    .Eq("id", playerId)
    .UpdateAsync(new { score = 200 });

// DELETE
var deleted = await client.Db.From<Player>("players")
    .Eq("id", playerId)
    .DeleteAsync();

// RPC (PostgreSQL関数呼び出し)
var result = await client.Db.RpcAsync<int>("increment_score", new { player_id = playerId, amount = 10 });
```

### ストレージ (Storage)

```csharp
// アップロード
var imageData = File.ReadAllBytes("image.png");
var path = await client.Storage.From("avatars")
    .UploadAsync("public/avatar.png", imageData, "image/png");

// ダウンロード
var data = await client.Storage.From("avatars")
    .DownloadAsync("public/avatar.png");

// 公開URL取得
var url = client.Storage.From("avatars")
    .GetPublicUrl("public/avatar.png");

// 削除
await client.Storage.From("avatars")
    .DeleteAsync("public/avatar.png");
```

### Edge Functions

```csharp
// レスポンスなし
await client.Functions.InvokeAsync("hello-world", new { name = "Alice" });

// レスポンスあり
var result = await client.Functions.InvokeAsync<MyResponse>("process-data", new { data = "test" });
```

### ポーリング (Realtime代替)

```csharp
// ITimestampedを実装したモデル
[Serializable]
public class GameState : ITimestamped
{
    [JsonProperty("id")]
    public string Id { get; set; }

    [JsonProperty("created_at")]
    public DateTime CreatedAt { get; set; }

    [JsonProperty("updated_at")]
    public DateTime UpdatedAt { get; set; }

    [JsonProperty("status")]
    public string Status { get; set; }
}

// ポーラー作成
var poller = new SupabasePoller<GameState>(client, "game_states", intervalSeconds: 1.0f);

// イベント購読 (R3)
poller.OnInsert.Subscribe(state => Debug.Log($"新規: {state.Id}")).AddTo(this);
poller.OnUpdate.Subscribe(state => Debug.Log($"更新: {state.Id}")).AddTo(this);

// 開始
poller.Start();

// 停止
poller.Stop();

// 破棄
poller.Dispose();
```

## クエリビルダーAPI

### フィルタ

- `.Eq(column, value)` - 等しい
- `.Neq(column, value)` - 等しくない
- `.Gt(column, value)` - より大きい
- `.Gte(column, value)` - 以上
- `.Lt(column, value)` - より小さい
- `.Lte(column, value)` - 以下
- `.Like(column, pattern)` - LIKE検索
- `.In(column, ...values)` - IN句

### ソート・ページング

- `.Order(column, ascending)` - ソート
- `.Limit(count)` - LIMIT
- `.Offset(count)` - OFFSET

### 実行

- `.ExecuteAsync()` - リスト取得
- `.SingleAsync()` - 単一レコード取得
- `.InsertAsync(data)` - INSERT
- `.UpdateAsync(data)` - UPDATE
- `.DeleteAsync()` - DELETE

## 制約事項

- Realtime非対応 (ポーリングで代替)
- Auth: OAuth, Magic Link非対応
- Storage: 署名付きURL非対応

## ファイル構成

```
Assets/Scripts/Supabase/
├── SupabaseSettings.cs         # ScriptableObject設定
├── SupabaseClient.cs            # メインクライアント
├── SupabaseAuth.cs              # Auth API
├── SupabaseDb.cs                # Database API (PostgREST)
├── SupabaseFunctions.cs         # Edge Functions
├── SupabaseStorage.cs           # Storage API
├── SupabasePoller.cs            # Realtime代替ポーリング
└── README.md                    # このファイル
```

## 依存関係

- UniTask (Cysharp.Threading.Tasks)
- R3 (Observable)
- Newtonsoft.Json (com.unity.nuget.newtonsoft-json)

## ライセンス

このコードはプロジェクト内で自由に使用できます。
