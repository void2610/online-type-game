using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

/// <summary>
/// ランキングテーブルの1行を表すモデル
/// </summary>
[Table("rankings")]
public class RankingEntry : BaseModel
{
    /// <summary>
    /// 自動生成されるID（INSERT時は送信しない）
    /// </summary>
    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("player_name")]
    public string PlayerName { get; set; }

    [Column("score")]
    public int Score { get; set; }

    [Column("accuracy")]
    public float Accuracy { get; set; }

    /// <summary>
    /// DB側で自動設定されるタイムスタンプ（INSERT時は送信しない）
    /// </summary>
    [Column("created_at", ignoreOnInsert: true, ignoreOnUpdate: true)]
    public string CreatedAt { get; set; }
}
