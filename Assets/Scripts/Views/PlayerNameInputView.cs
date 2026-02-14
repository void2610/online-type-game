using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// プレイヤー名入力UI
/// </summary>
public class PlayerNameInputView : MonoBehaviour
{
    [SerializeField] private TMP_InputField _nameInputField;
    [SerializeField] private Button _confirmButton;

    /// <summary>
    /// 入力フィールド
    /// </summary>
    public TMP_InputField NameInputField => _nameInputField;

    /// <summary>
    /// 確定ボタンのクリックイベント
    /// </summary>
    public Observable<Unit> OnConfirmClicked => _confirmButton.OnClickAsObservable();

    /// <summary>
    /// 表示/非表示を切り替え
    /// </summary>
    public void SetActive(bool active)
    {
        gameObject.SetActive(active);
    }
}
