using System.Collections.Generic;
using TMPro;
using UnityEngine;

/// <summary>
/// ランキング表示を担当するView
/// </summary>
public class RankingView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI rankingText;

    /// <summary>
    /// ローディング状態を表示する
    /// </summary>
    public void SetLoading() => rankingText.text = "Loading...";

    /// <summary>
    /// エラーメッセージを表示する
    /// </summary>
    public void SetError(string message) => rankingText.text = $"Error: {message}";

    /// <summary>
    /// ランキング一覧を表示する
    /// </summary>
    public void SetRanking(List<RankingEntry> entries)
    {
        if (entries == null || entries.Count == 0)
        {
            rankingText.text = "No data";
            return;
        }

        var sb = new System.Text.StringBuilder();
        sb.AppendLine("<b>--- Ranking ---</b>");
        for (var i = 0; i < entries.Count; i++)
        {
            var e = entries[i];
            sb.AppendLine($"{i + 1}. {e.PlayerName}  {e.Score}pts  ({e.Accuracy:P0})");
        }
        rankingText.text = sb.ToString();
    }
}
