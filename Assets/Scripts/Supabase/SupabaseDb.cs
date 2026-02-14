using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Cysharp.Threading.Tasks;
using Newtonsoft.Json;

namespace Void2610.Supabase
{
    /// <summary>
    /// Supabase Database API (PostgREST)
    /// </summary>
    public sealed class SupabaseDb
    {
        private readonly SupabaseClient _client;

        internal SupabaseDb(SupabaseClient client)
        {
            _client = client;
        }

        /// <summary>
        /// テーブルに対するクエリを開始
        /// </summary>
        public SupabaseDbQuery<T> From<T>(string table) => new SupabaseDbQuery<T>(_client, table);

        /// <summary>
        /// RPCを実行
        /// </summary>
        public async UniTask<T> RpcAsync<T>(string functionName, object parameters = null)
        {
            var endpoint = $"/rest/v1/rpc/{functionName}";
            return await _client.PostAsync<T>(endpoint, parameters);
        }
    }

    /// <summary>
    /// PostgRESTクエリビルダー
    /// </summary>
    public sealed class SupabaseDbQuery<T>
    {
        private readonly SupabaseClient _client;
        private readonly string _table;
        private readonly List<string> _selectColumns = new List<string>();
        private readonly List<string> _filters = new List<string>();
        private readonly List<string> _orders = new List<string>();
        private int? _limitValue;
        private int? _offsetValue;

        internal SupabaseDbQuery(SupabaseClient client, string table)
        {
            _client = client;
            _table = table;
        }

        /// <summary>
        /// SELECT句を指定
        /// </summary>
        public SupabaseDbQuery<T> Select(string columns = "*")
        {
            _selectColumns.Clear();
            _selectColumns.Add(columns);
            return this;
        }

        /// <summary>
        /// WHERE句 (等価)
        /// </summary>
        public SupabaseDbQuery<T> Eq(string column, object value)
        {
            _filters.Add($"{column}=eq.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (不等価)
        /// </summary>
        public SupabaseDbQuery<T> Neq(string column, object value)
        {
            _filters.Add($"{column}=neq.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (より大きい)
        /// </summary>
        public SupabaseDbQuery<T> Gt(string column, object value)
        {
            _filters.Add($"{column}=gt.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (以上)
        /// </summary>
        public SupabaseDbQuery<T> Gte(string column, object value)
        {
            _filters.Add($"{column}=gte.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (より小さい)
        /// </summary>
        public SupabaseDbQuery<T> Lt(string column, object value)
        {
            _filters.Add($"{column}=lt.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (以下)
        /// </summary>
        public SupabaseDbQuery<T> Lte(string column, object value)
        {
            _filters.Add($"{column}=lte.{EncodeValue(value)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (LIKE)
        /// </summary>
        public SupabaseDbQuery<T> Like(string column, string pattern)
        {
            _filters.Add($"{column}=like.{EncodeValue(pattern)}");
            return this;
        }

        /// <summary>
        /// WHERE句 (IN)
        /// </summary>
        public SupabaseDbQuery<T> In(string column, params object[] values)
        {
            var encoded = string.Join(",", values.Select(v => EncodeValue(v)));
            _filters.Add($"{column}=in.({encoded})");
            return this;
        }

        /// <summary>
        /// ORDER BY句
        /// </summary>
        public SupabaseDbQuery<T> Order(string column, bool ascending = true)
        {
            _orders.Add($"{column}.{(ascending ? "asc" : "desc")}");
            return this;
        }

        /// <summary>
        /// LIMIT句
        /// </summary>
        public SupabaseDbQuery<T> Limit(int count)
        {
            _limitValue = count;
            return this;
        }

        /// <summary>
        /// OFFSET句
        /// </summary>
        public SupabaseDbQuery<T> Offset(int count)
        {
            _offsetValue = count;
            return this;
        }

        /// <summary>
        /// 単一レコードを取得
        /// </summary>
        public async UniTask<T> SingleAsync()
        {
            Limit(1);
            var results = await ExecuteAsync();
            return results != null && results.Count > 0 ? results[0] : default;
        }

        /// <summary>
        /// クエリを実行してリストを取得
        /// </summary>
        public async UniTask<List<T>> ExecuteAsync()
        {
            var endpoint = BuildEndpoint();
            var result = await _client.GetAsync<List<T>>(endpoint);
            return result ?? new List<T>();
        }

        /// <summary>
        /// INSERT
        /// </summary>
        public async UniTask<T> InsertAsync(object data)
        {
            var endpoint = $"/rest/v1/{_table}";
            var headers = new Dictionary<string, string>
            {
                { "Prefer", "return=representation" }
            };

            var result = await _client.PostAsync<List<T>>(endpoint, data, headers);
            return result != null && result.Count > 0 ? result[0] : default;
        }

        /// <summary>
        /// UPDATE
        /// </summary>
        public async UniTask<List<T>> UpdateAsync(object data)
        {
            var endpoint = BuildEndpoint();
            var headers = new Dictionary<string, string>
            {
                { "Prefer", "return=representation" }
            };

            return await _client.PatchAsync<List<T>>(endpoint, data, headers);
        }

        /// <summary>
        /// DELETE
        /// </summary>
        public async UniTask<List<T>> DeleteAsync()
        {
            var endpoint = BuildEndpoint();
            var headers = new Dictionary<string, string>
            {
                { "Prefer", "return=representation" }
            };

            return await _client.DeleteAsync<List<T>>(endpoint, headers);
        }

        /// <summary>
        /// エンドポイントを構築
        /// </summary>
        private string BuildEndpoint()
        {
            var sb = new StringBuilder($"/rest/v1/{_table}");
            var queryParams = new List<string>();

            // SELECT
            if (_selectColumns.Count > 0)
            {
                queryParams.Add($"select={HttpUtility.UrlEncode(string.Join(",", _selectColumns))}");
            }

            // フィルタ
            foreach (var filter in _filters)
            {
                queryParams.Add(filter);
            }

            // ORDER BY
            if (_orders.Count > 0)
            {
                queryParams.Add($"order={string.Join(",", _orders)}");
            }

            // LIMIT
            if (_limitValue.HasValue)
            {
                queryParams.Add($"limit={_limitValue.Value}");
            }

            // OFFSET
            if (_offsetValue.HasValue)
            {
                queryParams.Add($"offset={_offsetValue.Value}");
            }

            if (queryParams.Count > 0)
            {
                sb.Append("?");
                sb.Append(string.Join("&", queryParams));
            }

            return sb.ToString();
        }

        /// <summary>
        /// 値をエンコード
        /// </summary>
        private string EncodeValue(object value)
        {
            if (value == null) return "null";
            if (value is string str) return HttpUtility.UrlEncode(str);
            return HttpUtility.UrlEncode(value.ToString());
        }
    }
}
