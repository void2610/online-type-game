using UnityEngine;

namespace Void2610.Supabase
{
    /// <summary>
    /// Supabase接続設定を保持するScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SupabaseSettings", menuName = "Supabase/Settings")]
    public sealed class SupabaseSettings : ScriptableObject
    {
        /// <summary>
        /// SupabaseプロジェクトのURL (例: https://xxxxx.supabase.co)
        /// </summary>
        [SerializeField] private string url = "";

        /// <summary>
        /// Supabase Anonymous Key (公開鍵)
        /// </summary>
        [SerializeField] private string anonKey = "";

        /// <summary>
        /// SupabaseプロジェクトのURL
        /// </summary>
        public string Url => url;

        /// <summary>
        /// Supabase Anonymous Key
        /// </summary>
        public string AnonKey => anonKey;

        /// <summary>
        /// 設定が有効かどうか
        /// </summary>
        public bool IsValid => !string.IsNullOrWhiteSpace(url) && !string.IsNullOrWhiteSpace(anonKey);
    }
}
