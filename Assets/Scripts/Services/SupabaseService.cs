using System.Collections.Generic;
using Cysharp.Threading.Tasks;

/// <summary>
/// Supabaseとの通信を担当するサービス
/// </summary>
public class SupabaseService
{
    private readonly Supabase.SupabaseClient _client;

    public SupabaseService(Supabase.SupabaseSettings settings)
    {
        _client = new Supabase.SupabaseClient(settings);
    }

    /// <summary>
    /// スコアを送信する
    /// </summary>
    public async UniTask SubmitScoreAsync(string playerName, int score, float accuracy)
    {
        var entry = new
        {
            player_name = playerName,
            score = score,
            accuracy = accuracy
        };

        await _client.Db.From<RankingEntry>("rankings").InsertAsync(entry);
    }

    /// <summary>
    /// ランキング上位を取得する
    /// </summary>
    public async UniTask<List<RankingEntry>> FetchRankingAsync(int limit = 10)
    {
        return await _client.Db.From<RankingEntry>("rankings")
            .Select("*")
            .Order("score", false)
            .Limit(limit)
            .ExecuteAsync();
    }
}
