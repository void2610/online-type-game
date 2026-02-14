using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Supabase;

/// <summary>
/// Supabaseとの通信を担当するサービス
/// </summary>
public class SupabaseService
{
    private readonly Client _client;
    private bool _initialized;

    public SupabaseService(string url, string key)
    {
        var options = new SupabaseOptions { AutoConnectRealtime = false };
        _client = new Client(url, key, options);
    }

    /// <summary>
    /// クライアントを初期化する
    /// </summary>
    public async UniTask InitializeAsync()
    {
        if (_initialized) return;
        await _client.InitializeAsync();
        _initialized = true;
    }

    /// <summary>
    /// スコアを送信する
    /// </summary>
    public async UniTask SubmitScoreAsync(string playerName, int score, float accuracy)
    {
        await InitializeAsync();
        var entry = new RankingEntry
        {
            PlayerName = playerName,
            Score = score,
            Accuracy = accuracy,
        };
        await _client.From<RankingEntry>().Insert(entry);
    }

    /// <summary>
    /// ランキング上位を取得する
    /// </summary>
    public async UniTask<List<RankingEntry>> FetchRankingAsync(int limit = 10)
    {
        await InitializeAsync();
        var response = await _client.From<RankingEntry>()
            .Order("score", Supabase.Postgrest.Constants.Ordering.Descending)
            .Limit(limit)
            .Get();
        return response.Models;
    }
}
