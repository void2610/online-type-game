using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;

namespace Void2610.Supabase
{
    /// <summary>
    /// Realtime代替の定期ポーリングシステム
    /// updated_at カラムを使った変更検知
    /// </summary>
    public sealed class SupabasePoller<T> : IDisposable where T : ITimestamped
    {
        /// <summary>
        /// 新しいレコードが検出されたときの通知
        /// </summary>
        public Observable<T> OnInsert => _onInsertSubject;

        /// <summary>
        /// レコードが更新されたときの通知
        /// </summary>
        public Observable<T> OnUpdate => _onUpdateSubject;

        /// <summary>
        /// レコードが削除されたときの通知 (削除検知は制限的)
        /// </summary>
        public Observable<string> OnDelete => _onDeleteSubject;

        private readonly SupabaseClient _client;
        private readonly string _table;
        private readonly float _intervalSeconds;
        private readonly Subject<T> _onInsertSubject = new Subject<T>();
        private readonly Subject<T> _onUpdateSubject = new Subject<T>();
        private readonly Subject<string> _onDeleteSubject = new Subject<string>();
        private DateTime _lastPolledAt;
        private CancellationTokenSource _cts;
        private bool _isPolling;

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="client">Supabaseクライアント</param>
        /// <param name="table">監視するテーブル名</param>
        /// <param name="intervalSeconds">ポーリング間隔(秒)</param>
        public SupabasePoller(SupabaseClient client, string table, float intervalSeconds = 1.0f)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _table = table ?? throw new ArgumentNullException(nameof(table));
            _intervalSeconds = intervalSeconds;
            _lastPolledAt = DateTime.UtcNow;
        }

        /// <summary>
        /// ポーリング開始
        /// </summary>
        public void Start()
        {
            if (_isPolling)
            {
                Debug.LogWarning("Poller is already running");
                return;
            }

            _cts = new CancellationTokenSource();
            _isPolling = true;
            _lastPolledAt = DateTime.UtcNow;

            PollAsync(_cts.Token).Forget();
        }

        /// <summary>
        /// ポーリング停止
        /// </summary>
        public void Stop()
        {
            if (!_isPolling) return;

            _cts?.Cancel();
            _cts?.Dispose();
            _cts = null;
            _isPolling = false;
        }

        /// <summary>
        /// リソース解放
        /// </summary>
        public void Dispose()
        {
            Stop();
            _onInsertSubject?.Dispose();
            _onUpdateSubject?.Dispose();
            _onDeleteSubject?.Dispose();
        }

        /// <summary>
        /// ポーリングループ
        /// </summary>
        private async UniTaskVoid PollAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    await UniTask.Delay(TimeSpan.FromSeconds(_intervalSeconds), cancellationToken: cancellationToken);

                    // 前回のポーリング以降に更新されたレコードを取得
                    var query = _client.Db.From<T>(_table)
                        .Select("*")
                        .Gte("updated_at", _lastPolledAt.ToString("o"))
                        .Order("updated_at", true);

                    var results = await query.ExecuteAsync();

                    if (results != null && results.Count > 0)
                    {
                        foreach (var record in results)
                        {
                            // 新規か更新かを判定
                            if (record.CreatedAt >= _lastPolledAt)
                            {
                                _onInsertSubject.OnNext(record);
                            }
                            else if (record.UpdatedAt >= _lastPolledAt)
                            {
                                _onUpdateSubject.OnNext(record);
                            }
                        }

                        // 最後のタイムスタンプを更新
                        _lastPolledAt = DateTime.UtcNow;
                    }
                }
                catch (OperationCanceledException)
                {
                    // キャンセル時は正常終了
                    break;
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Polling error: {ex.Message}");
                }
            }
        }
    }

    /// <summary>
    /// タイムスタンプを持つレコードのインターフェース
    /// </summary>
    public interface ITimestamped
    {
        /// <summary>
        /// 作成日時
        /// </summary>
        DateTime CreatedAt { get; set; }

        /// <summary>
        /// 更新日時
        /// </summary>
        DateTime UpdatedAt { get; set; }
    }
}
