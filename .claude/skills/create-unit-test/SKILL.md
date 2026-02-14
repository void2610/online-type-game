# Unity Test Framework ユニットテスト作成スキル

指定されたクラスまたはメソッドに対して、Unity Test Framework (NUnit) を使用したユニットテストを作成します。

## 入力

$ARGUMENTS

## テスト作成ガイドライン

### 1. ファイル配置

```
Assets/Tests/Runtime/
├── Models/
│   └── [対象クラス名]Tests.cs
└── Services/
    └── [対象クラス名]Tests.cs
```

### 2. テストクラスの構造

```csharp
using NUnit.Framework;

namespace [Namespace].Tests
{
    /// <summary>
    /// [テスト対象クラス名] のユニットテスト
    /// </summary>
    [TestFixture]
    public class [テスト対象クラス名]Tests
    {
        private [テスト対象クラス] _sut; // System Under Test

        [SetUp]
        public void SetUp()
        {
            // 各テスト前の初期化処理
            _sut = new [テスト対象クラス]();
        }

        [TearDown]
        public void TearDown()
        {
            // 各テスト後のクリーンアップ処理（必要な場合）
        }

        // テストメソッド...
    }
}
```

### 3. テストメソッドの命名規則

**日本語を活用した明確な命名**を使用：

```csharp
[Test]
public void メソッド名_条件や状況_期待される結果()
{
    // ...
}
```

**命名例：**
- `Validate_正しい入力_正解を返す()`
- `Calculate_ゼロで除算_例外をスローする()`
- `GetItem_存在しないID_nullを返す()`
- `Constructor_パラメータなし_デフォルト値が設定される()`

### 4. AAA パターン（Arrange-Act-Assert）

すべてのテストは AAA パターンに従って構造化：

```csharp
[Test]
public void Add_二つの正の数_合計を返す()
{
    // Arrange
    var calculator = new Calculator();
    int a = 5;
    int b = 10;

    // Act
    int result = calculator.Add(a, b);

    // Assert
    Assert.That(result, Is.EqualTo(15));
}
```

### 5. Assert の書き方（Constraint Model）

```csharp
// 等価性
Assert.That(result, Is.EqualTo(expected));
Assert.That(result, Is.Not.EqualTo(unexpected));

// null チェック
Assert.That(result, Is.Null);
Assert.That(result, Is.Not.Null);

// 真偽値
Assert.That(condition, Is.True);
Assert.That(condition, Is.False);

// コレクション
Assert.That(list, Is.Empty);
Assert.That(list, Has.Count.EqualTo(3));
Assert.That(list, Contains.Item(item));

// 文字列
Assert.That(str, Does.StartWith("prefix"));
Assert.That(str, Does.Contain("substring"));

// 範囲
Assert.That(value, Is.GreaterThan(0));
Assert.That(value, Is.InRange(1, 10));

// 例外
Assert.Throws<ArgumentException>(() => method());

// 型
Assert.That(obj, Is.InstanceOf<ExpectedType>());
```

### 6. パラメータ化テスト

```csharp
[Test]
[TestCase("a", "あ")]
[TestCase("ka", "か")]
[TestCase("shi", "し")]
public void TryConvert_有効なローマ字_ひらがなに変換される(string romaji, string expected)
{
    // Act
    var result = RomajiTable.TryConvert(romaji, out var hiragana);

    // Assert
    Assert.That(result, Is.True);
    Assert.That(hiragana, Is.EqualTo(expected));
}
```

### 7. テストカテゴリ（#region）

```csharp
#region 基本的な変換テスト

[Test]
public void Convert_母音_正しく変換される() { /* ... */ }

#endregion

#region エッジケース

[Test]
public void Convert_空文字列_falseを返す() { /* ... */ }

#endregion
```

### 8. テストすべき項目

1. **正常系**: 期待通りの入力での動作
2. **境界値**: 最小値、最大値、境界付近の値
3. **エッジケース**: 空文字列、null、空のコレクション
4. **異常系**: 無効な入力、例外がスローされるべきケース
5. **状態遷移**: オブジェクトの状態変化

### 9. PlayMode テスト（UnityTest）

```csharp
[UnityTest]
public IEnumerator MovePlayer_目標位置まで移動_正しい位置に到達する()
{
    // Arrange
    var player = new GameObject().AddComponent<PlayerController>();
    var targetPosition = new Vector3(10, 0, 0);

    // Act
    player.MoveTo(targetPosition);
    yield return new WaitForSeconds(1f);

    // Assert
    Assert.That(player.transform.position, Is.EqualTo(targetPosition));
}
```

### 10. モック・スタブ

```csharp
public class MockTimeProvider : ITimeProvider
{
    public float DeltaTime { get; set; } = 0.016f;
}

[Test]
public void Update_一定時間経過_位置が更新される()
{
    // Arrange
    var mockTime = new MockTimeProvider { DeltaTime = 1f };
    var mover = new Mover(mockTime);

    // Act
    mover.Update();

    // Assert
    Assert.That(mover.Position, Is.EqualTo(expectedPosition));
}
```

## 実行手順

1. テスト対象のコードを読み込んで理解する
2. テストすべき項目をリストアップする
3. テストクラスを作成する
4. AAA パターンに従ってテストメソッドを実装する
5. `#region` を使ってテストをカテゴリ分けする
