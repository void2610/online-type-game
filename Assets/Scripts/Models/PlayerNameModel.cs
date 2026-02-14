using R3;
using UnityEngine;

/// <summary>
/// プレイヤー名を保持するModel
/// </summary>
public class PlayerNameModel
{
    private const string PlayerNameKey = "PlayerName";
    private const string DefaultPlayerName = "Player";

    private readonly ReactiveProperty<string> _playerName = new();

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public ReadOnlyReactiveProperty<string> PlayerName => _playerName;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    public PlayerNameModel()
    {
        // PlayerPrefsから復元
        var savedName = PlayerPrefs.GetString(PlayerNameKey, DefaultPlayerName);
        _playerName.Value = savedName;
    }

    /// <summary>
    /// プレイヤー名を設定
    /// </summary>
    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            name = DefaultPlayerName;
        }

        _playerName.Value = name;

        // PlayerPrefsに保存
        PlayerPrefs.SetString(PlayerNameKey, name);
        PlayerPrefs.Save();
    }
}
