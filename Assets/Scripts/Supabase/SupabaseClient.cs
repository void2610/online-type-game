using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

namespace Void2610.Supabase
{
    /// <summary>
    /// Supabaseクライアントのメインクラス
    /// HTTP通信の基盤とセッション管理を提供
    /// </summary>
    public sealed class SupabaseClient
    {
        /// <summary>
        /// Auth API
        /// </summary>
        public SupabaseAuth Auth { get; }

        /// <summary>
        /// Database API
        /// </summary>
        public SupabaseDb Db { get; }

        /// <summary>
        /// Functions API
        /// </summary>
        public SupabaseFunctions Functions { get; }

        /// <summary>
        /// Storage API
        /// </summary>
        public SupabaseStorage Storage { get; }

        /// <summary>
        /// 現在のアクセストークン
        /// </summary>
        public string AccessToken => _accessToken;

        private readonly SupabaseSettings _settings;
        private string _accessToken;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="settings">Supabase設定</param>
        public SupabaseClient(SupabaseSettings settings)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));

            if (!_settings.IsValid)
            {
                throw new ArgumentException("Invalid Supabase settings");
            }

            Auth = new SupabaseAuth(this);
            Db = new SupabaseDb(this);
            Functions = new SupabaseFunctions(this);
            Storage = new SupabaseStorage(this);
        }

        /// <summary>
        /// アクセストークンを設定
        /// </summary>
        internal void SetAccessToken(string token)
        {
            _accessToken = token;
        }

        /// <summary>
        /// アクセストークンをクリア
        /// </summary>
        internal void ClearAccessToken()
        {
            _accessToken = null;
        }

        /// <summary>
        /// GET リクエスト
        /// </summary>
        internal async UniTask<T> GetAsync<T>(string endpoint, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint);
            using var request = UnityWebRequest.Get(url);
            ApplyHeaders(request, headers);

            await request.SendWebRequest();
            return HandleResponse<T>(request);
        }

        /// <summary>
        /// POST リクエスト
        /// </summary>
        internal async UniTask<T> PostAsync<T>(string endpoint, object body = null, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint);
            var json = body != null ? JsonConvert.SerializeObject(body) : "{}";
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer()
            };

            ApplyHeaders(request, headers);
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            return HandleResponse<T>(request);
        }

        /// <summary>
        /// PATCH リクエスト
        /// </summary>
        internal async UniTask<T> PatchAsync<T>(string endpoint, object body = null, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint);
            var json = body != null ? JsonConvert.SerializeObject(body) : "{}";
            var bodyRaw = Encoding.UTF8.GetBytes(json);

            using var request = new UnityWebRequest(url, "PATCH")
            {
                uploadHandler = new UploadHandlerRaw(bodyRaw),
                downloadHandler = new DownloadHandlerBuffer()
            };

            ApplyHeaders(request, headers);
            request.SetRequestHeader("Content-Type", "application/json");

            await request.SendWebRequest();
            return HandleResponse<T>(request);
        }

        /// <summary>
        /// DELETE リクエスト
        /// </summary>
        internal async UniTask<T> DeleteAsync<T>(string endpoint, Dictionary<string, string> headers = null)
        {
            var url = BuildUrl(endpoint);
            using var request = UnityWebRequest.Delete(url);
            request.downloadHandler = new DownloadHandlerBuffer();

            ApplyHeaders(request, headers);

            await request.SendWebRequest();
            return HandleResponse<T>(request);
        }

        /// <summary>
        /// URLを構築
        /// </summary>
        private string BuildUrl(string endpoint)
        {
            var baseUrl = _settings.Url.TrimEnd('/');
            var path = endpoint.TrimStart('/');
            return $"{baseUrl}/{path}";
        }

        /// <summary>
        /// 共通ヘッダーを適用
        /// </summary>
        private void ApplyHeaders(UnityWebRequest request, Dictionary<string, string> additionalHeaders)
        {
            // 必須ヘッダー
            request.SetRequestHeader("apikey", _settings.AnonKey);

            // アクセストークンがあれば追加
            if (!string.IsNullOrEmpty(_accessToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_accessToken}");
            }

            // 追加ヘッダー
            if (additionalHeaders != null)
            {
                foreach (var kvp in additionalHeaders)
                {
                    request.SetRequestHeader(kvp.Key, kvp.Value);
                }
            }
        }

        /// <summary>
        /// レスポンスを処理
        /// </summary>
        private T HandleResponse<T>(UnityWebRequest request)
        {
            if (request.result == UnityWebRequest.Result.Success)
            {
                var text = request.downloadHandler.text;

                // 空レスポンスの場合
                if (string.IsNullOrWhiteSpace(text))
                {
                    return default;
                }

                // JSONデシリアライズ
                return JsonConvert.DeserializeObject<T>(text);
            }
            else
            {
                var errorMessage = $"Supabase API Error: {request.error}\n{request.downloadHandler?.text}";
                Debug.LogError(errorMessage);
                throw new Exception(errorMessage);
            }
        }
    }
}
