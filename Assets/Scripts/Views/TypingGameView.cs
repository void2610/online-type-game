using TMPro;
using UnityEngine;

/// <summary>
/// タイピングゲームの表示を担当するView
/// </summary>
public class TypingGameView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private TextMeshProUGUI messageText;

    /// <summary>
    /// お題テキストをリッチテキストタグ付きで設定する
    /// </summary>
    public void SetQuestionText(string richText) => questionText.text = richText;

    public void SetStatus(string text) => statusText.text = text;

    public void SetMessage(string text) => messageText.text = text;
}
