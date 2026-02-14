using Void2610.ThockKit.Core.Models;
using Void2610.ThockKit.Extensions;
using UnityEngine;
using VContainer;
using VContainer.Unity;

/// <summary>
/// MainScene用のDIコンテナ。
/// タイピングゲームとランキング表示の依存を登録する。
/// </summary>
public class MainLifetimeScope : LifetimeScope
{
    [SerializeField] private TypingGameView typingGameView;
    [SerializeField] private RankingView rankingView;

    protected override void Configure(IContainerBuilder builder)
    {
        // ThockKit（英語タイピング）
        builder.RegisterThockKit(new TypingSessionSettings(false, false, false));

        // View
        builder.RegisterComponent(typingGameView);
        builder.RegisterComponent(rankingView);

        // Presenter
        builder.RegisterEntryPoint<TypingGamePresenter>();
    }
}
