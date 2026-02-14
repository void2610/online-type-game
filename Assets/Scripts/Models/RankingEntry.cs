using System;
using Newtonsoft.Json;

/// <summary>
/// ランキングテーブルの1行を表すモデル
/// </summary>
[Serializable]
public class RankingEntry
{
    /// <summary>
    /// 自動生成されるID
    /// </summary>
    [JsonProperty("id")]
    public long Id { get; set; }

    /// <summary>
    /// プレイヤー名
    /// </summary>
    [JsonProperty("player_name")]
    public string PlayerName { get; set; }

    /// <summary>
    /// スコア
    /// </summary>
    [JsonProperty("score")]
    public int Score { get; set; }

    /// <summary>
    /// 精度
    /// </summary>
    [JsonProperty("accuracy")]
    public float Accuracy { get; set; }

    /// <summary>
    /// DB側で自動設定されるタイムスタンプ
    /// </summary>
    [JsonProperty("created_at")]
    public string CreatedAt { get; set; }
}
