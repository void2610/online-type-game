using UnityEngine;
using UnityEngine.UI;

namespace Void2610.ThockKit.Samples.BasicEnglishTyping
{
    /// <summary>
    /// 基本的なタイピングゲームのUI表示
    /// </summary>
    public class BasicTypingView : MonoBehaviour
    {
        [SerializeField] private Text typedText;
        [SerializeField] private Text remainingText;
        [SerializeField] private Text statusText;
        [SerializeField] private Color correctColor = new Color(0.2f, 0.8f, 0.2f);
        [SerializeField] private Color remainingColor = new Color(0.5f, 0.5f, 0.5f);

        /// <summary>
        /// 入力済みテキストを設定
        /// </summary>
        public void SetTypedText(string text)
        {
            if (typedText != null)
            {
                typedText.text = text;
                typedText.color = correctColor;
            }
        }

        /// <summary>
        /// 残りテキストを設定
        /// </summary>
        public void SetRemainingText(string text)
        {
            if (remainingText != null)
            {
                remainingText.text = text;
                remainingText.color = remainingColor;
            }
        }

        /// <summary>
        /// ステータステキストを設定
        /// </summary>
        public void SetStatus(string text)
        {
            if (statusText != null)
            {
                statusText.text = text;
            }
        }
    }
}
