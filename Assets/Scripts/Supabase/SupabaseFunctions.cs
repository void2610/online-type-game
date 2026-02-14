using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Void2610.Supabase
{
    /// <summary>
    /// Supabase Edge Functions API
    /// </summary>
    public sealed class SupabaseFunctions
    {
        private readonly SupabaseClient _client;

        internal SupabaseFunctions(SupabaseClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Edge Functionを呼び出し (レスポンスなし)
        /// </summary>
        public async UniTask InvokeAsync(string functionName, object body = null, Dictionary<string, string> headers = null)
        {
            var endpoint = $"/functions/v1/{functionName}";
            await _client.PostAsync<object>(endpoint, body, headers);
        }

        /// <summary>
        /// Edge Functionを呼び出し (レスポンスあり)
        /// </summary>
        public async UniTask<T> InvokeAsync<T>(string functionName, object body = null, Dictionary<string, string> headers = null)
        {
            var endpoint = $"/functions/v1/{functionName}";
            return await _client.PostAsync<T>(endpoint, body, headers);
        }
    }
}
