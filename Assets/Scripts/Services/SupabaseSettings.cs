using UnityEngine;

/// <summary>
/// Supabase接続情報を保持するScriptableObject
/// </summary>
[CreateAssetMenu(fileName = "SupabaseSettings", menuName = "Settings/Supabase")]
public class SupabaseSettings : ScriptableObject
{
    [SerializeField] private string url;
    [SerializeField] private string anonKey;

    public string Url => url;
    public string AnonKey => anonKey;
}
