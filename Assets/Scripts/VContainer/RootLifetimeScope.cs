using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// アプリケーション全体で共有するDIコンテナ。
/// DontDestroyOnLoadで永続化し、シーン遷移を跨いで生存する。
/// </summary>
public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private SupabaseSettings supabaseSettings;

    protected override void Configure(IContainerBuilder builder)
    {
        // Supabaseサービスをシングルトンで登録
        builder.Register<SupabaseService>(Lifetime.Singleton)
            .WithParameter("url", supabaseSettings.Url)
            .WithParameter("key", supabaseSettings.AnonKey);
    }
}
