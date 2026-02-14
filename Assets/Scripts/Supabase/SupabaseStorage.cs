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
    /// Supabase Storage API
    /// </summary>
    public sealed class SupabaseStorage
    {
        private readonly SupabaseClient _client;

        internal SupabaseStorage(SupabaseClient client)
        {
            _client = client;
        }

        /// <summary>
        /// バケットを取得
        /// </summary>
        public SupabaseStorageBucket From(string _bucketName) => new SupabaseStorageBucket(_client, _bucketName);
    }

    /// <summary>
    /// Storageバケット操作
    /// </summary>
    public sealed class SupabaseStorageBucket
    {
        private readonly SupabaseClient _client;
        private readonly string _bucketName;

        internal SupabaseStorageBucket(SupabaseClient client, string bucketName)
        {
            _client = client;
            _bucketName = bucketName;
        }

        /// <summary>
        /// ファイルをアップロード
        /// </summary>
        public async UniTask<string> UploadAsync(string path, byte[] data, string contentType = "application/octet-stream")
        {
            var url = BuildUrl($"/object/{_bucketName}/{path}");

            using var request = new UnityWebRequest(url, "POST")
            {
                uploadHandler = new UploadHandlerRaw(data),
                downloadHandler = new DownloadHandlerBuffer()
            };

            ApplyHeaders(request);
            request.SetRequestHeader("Content-Type", contentType);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                var response = JsonConvert.DeserializeObject<UploadResponse>(request.downloadHandler.text);
                return response?.Key;
            }
            else
            {
                throw new Exception($"Upload failed: {request.error}\n{request.downloadHandler?.text}");
            }
        }

        /// <summary>
        /// ファイルをダウンロード
        /// </summary>
        public async UniTask<byte[]> DownloadAsync(string path)
        {
            var url = BuildUrl($"/object/{_bucketName}/{path}");

            using var request = UnityWebRequest.Get(url);
            ApplyHeaders(request);

            await request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.data;
            }
            else
            {
                throw new Exception($"Download failed: {request.error}");
            }
        }

        /// <summary>
        /// 公開URLを取得
        /// </summary>
        public string GetPublicUrl(string path) => BuildUrl($"/object/public/{_bucketName}/{path}");

        /// <summary>
        /// ファイルを削除
        /// </summary>
        public async UniTask DeleteAsync(string path)
        {
            var endpoint = $"/storage/v1/object/{_bucketName}/{path}";
            await _client.DeleteAsync<object>(endpoint);
        }

        /// <summary>
        /// 複数ファイルを削除
        /// </summary>
        public async UniTask DeleteAsync(string[] paths)
        {
            var endpoint = $"/storage/v1/object/{_bucketName}";
            var body = new { prefixes = paths };
            await _client.DeleteAsync<object>(endpoint);
        }

        /// <summary>
        /// URLを構築
        /// </summary>
        private string BuildUrl(string path)
        {
            // SupabaseSettingsから直接URLを取得
            var settings = GetSettings();
            var baseUrl = settings.Url.TrimEnd('/');
            var trimmedPath = path.TrimStart('/');
            return $"{baseUrl}/storage/v1/{trimmedPath}";
        }

        /// <summary>
        /// ヘッダーを適用
        /// </summary>
        private void ApplyHeaders(UnityWebRequest request)
        {
            var settings = GetSettings();
            request.SetRequestHeader("apikey", settings.AnonKey);

            if (!string.IsNullOrEmpty(_client.AccessToken))
            {
                request.SetRequestHeader("Authorization", $"Bearer {_client.AccessToken}");
            }
        }

        /// <summary>
        /// Settingsを取得 (リフレクション経由)
        /// </summary>
        private SupabaseSettings GetSettings()
        {
            var settingsField = typeof(SupabaseClient).GetField("_settings",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return (SupabaseSettings)settingsField?.GetValue(_client);
        }

        [Serializable]
        private class UploadResponse
        {
            [JsonProperty("Key")]
            public string Key { get; set; }
        }
    }
}
