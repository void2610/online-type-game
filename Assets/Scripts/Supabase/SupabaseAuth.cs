using System;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Void2610.Supabase
{
    /// <summary>
    /// Supabase Auth API (GoTrue)
    /// </summary>
    public sealed class SupabaseAuth
    {
        private const string SessionKey = "SupabaseSession";

        /// <summary>
        /// 現在のセッション
        /// </summary>
        public Session CurrentSession => _currentSession;

        /// <summary>
        /// ログイン中かどうか
        /// </summary>
        public bool IsSignedIn => _currentSession != null && !string.IsNullOrEmpty(_currentSession.AccessToken);

        private readonly SupabaseClient _client;
        private Session _currentSession;

        internal SupabaseAuth(SupabaseClient client)
        {
            _client = client;
        }

        /// <summary>
        /// メールアドレスとパスワードでサインアップ
        /// </summary>
        public async UniTask<Session> SignUpAsync(string email, string password)
        {
            var body = new { email, password };
            var response = await _client.PostAsync<AuthResponse>("/auth/v1/signup", body);
            return HandleAuthResponse(response);
        }

        /// <summary>
        /// メールアドレスとパスワードでサインイン
        /// </summary>
        public async UniTask<Session> SignInAsync(string email, string password)
        {
            var body = new { email, password };
            var response = await _client.PostAsync<AuthResponse>("/auth/v1/token?grant_type=password", body);
            return HandleAuthResponse(response);
        }

        /// <summary>
        /// 匿名サインイン
        /// </summary>
        public async UniTask<Session> SignInAnonymouslyAsync()
        {
            var response = await _client.PostAsync<AuthResponse>("/auth/v1/signup", new { });
            return HandleAuthResponse(response);
        }

        /// <summary>
        /// サインアウト
        /// </summary>
        public async UniTask SignOutAsync()
        {
            if (IsSignedIn)
            {
                try
                {
                    await _client.PostAsync<object>("/auth/v1/logout", null);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Sign out API error (continuing): {ex.Message}");
                }
            }

            ClearSession();
        }

        /// <summary>
        /// セッションをリフレッシュ
        /// </summary>
        public async UniTask<Session> RefreshSessionAsync()
        {
            if (_currentSession == null || string.IsNullOrEmpty(_currentSession.RefreshToken))
            {
                throw new InvalidOperationException("No session to refresh");
            }

            var body = new { refresh_token = _currentSession.RefreshToken };
            var response = await _client.PostAsync<AuthResponse>("/auth/v1/token?grant_type=refresh_token", body);
            return HandleAuthResponse(response);
        }

        /// <summary>
        /// 保存されたセッションを復元
        /// </summary>
        public void RestoreSession()
        {
            if (PlayerPrefs.HasKey(SessionKey))
            {
                try
                {
                    var json = PlayerPrefs.GetString(SessionKey);
                    var session = JsonConvert.DeserializeObject<Session>(json);

                    if (session != null && !string.IsNullOrEmpty(session.AccessToken))
                    {
                        _currentSession = session;
                        _client.SetAccessToken(session.AccessToken);
                        Debug.Log("Session restored from PlayerPrefs");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to restore session: {ex.Message}");
                    ClearSession();
                }
            }
        }

        /// <summary>
        /// 認証レスポンスを処理
        /// </summary>
        private Session HandleAuthResponse(AuthResponse response)
        {
            if (response == null || string.IsNullOrEmpty(response.AccessToken))
            {
                throw new Exception("Invalid auth response");
            }

            _currentSession = new Session
            {
                AccessToken = response.AccessToken,
                RefreshToken = response.RefreshToken,
                ExpiresIn = response.ExpiresIn,
                TokenType = response.TokenType,
                User = response.User
            };

            _client.SetAccessToken(_currentSession.AccessToken);
            SaveSession();

            return _currentSession;
        }

        /// <summary>
        /// セッションをPlayerPrefsに保存
        /// </summary>
        private void SaveSession()
        {
            if (_currentSession != null)
            {
                var json = JsonConvert.SerializeObject(_currentSession);
                PlayerPrefs.SetString(SessionKey, json);
                PlayerPrefs.Save();
            }
        }

        /// <summary>
        /// セッションをクリア
        /// </summary>
        private void ClearSession()
        {
            _currentSession = null;
            _client.ClearAccessToken();
            PlayerPrefs.DeleteKey(SessionKey);
            PlayerPrefs.Save();
        }
    }

    /// <summary>
    /// 認証レスポンス
    /// </summary>
    [Serializable]
    public class AuthResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    /// <summary>
    /// セッション情報
    /// </summary>
    [Serializable]
    public class Session
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("refresh_token")]
        public string RefreshToken { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("user")]
        public User User { get; set; }
    }

    /// <summary>
    /// ユーザー情報
    /// </summary>
    [Serializable]
    public class User
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("role")]
        public string Role { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }
    }
}
