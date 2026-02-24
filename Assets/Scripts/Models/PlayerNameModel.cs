using R3;
using UnityEngine;

/// <summary>
/// プレイヤー名を保持するModel
/// </summary>
public class PlayerNameModel
{
    private const string PlayerNameKey = "PlayerName";
    private const string HasSetNameKey = "HasSetPlayerName";
    private const string DefaultPlayerName = "Player";

    private readonly ReactiveProperty<string> _playerName = new();

    /// <summary>
    /// プレイヤー名
    /// </summary>
    public ReadOnlyReactiveProperty<string> PlayerName => _playerName;

    /// <summary>
    /// プレイヤー名が一度でも設定されたことがあるか
    /// </summary>
    public bool HasSetName => PlayerPrefs.GetInt(HasSetNameKey, 0) == 1;

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
        PlayerPrefs.SetInt(HasSetNameKey, 1);
        PlayerPrefs.Save();
    }
}
