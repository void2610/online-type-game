using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Void2610.ThockKit.Core.Interfaces;
using Void2610.ThockKit.Core.Models;

/// <summary>
/// タイピングゲーム全体を制御するPresenter
/// </summary>
public class TypingGamePresenter : ITickable, IStartable, IDisposable
{
    private readonly ITypingSession _session;
    private readonly TypingGameView _typingView;
    private readonly RankingView _rankingView;
    private readonly SupabaseService _supabase;
    private readonly PlayerNameModel _playerNameModel;
    private readonly PlayerNameInputView _playerNameInputView;
    private readonly CompositeDisposable _disposables = new();
    private readonly Queue<char> _inputBuffer = new();

    private int _correctCount;
    private int _missCount;
    private int _currentQuestionIndex;
    private int _questionCount;

    public TypingGamePresenter(
        ITypingSession session,
        TypingGameView typingView,
        RankingView rankingView,
        SupabaseService supabase,
        PlayerNameModel playerNameModel,
        PlayerNameInputView playerNameInputView)
    {
        _session = session;
        _typingView = typingView;
        _rankingView = rankingView;
        _supabase = supabase;
        _playerNameModel = playerNameModel;
        _playerNameInputView = playerNameInputView;

        // セッションイベントの購読
        _session.CurrentQuestion.Subscribe(OnQuestionChanged).AddTo(_disposables);
        _session.CurrentPosition.Subscribe(_ => UpdateDisplay()).AddTo(_disposables);
        _session.OnInput.Subscribe(OnInput).AddTo(_disposables);
        _session.OnQuestionCompleted.Subscribe(_ => _currentQuestionIndex++).AddTo(_disposables);
        _session.OnSessionCompleted.Subscribe(_ => OnSessionCompleted().Forget()).AddTo(_disposables);

        // キーボード入力の購読
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            keyboard.onTextInput += OnTextInput;
        }
    }

    public void Start() => InitializeAsync().Forget();

    public void Tick()
    {
        // バッファからの入力処理
        while (_inputBuffer.Count > 0)
        {
            _session.ProcessInput(_inputBuffer.Dequeue());
        }
    }

    private async UniTaskVoid InitializeAsync()
    {
        // ランキング表示
        _rankingView.SetLoading();
        try
        {
            var ranking = await _supabase.FetchRankingAsync();
            _rankingView.SetRanking(ranking);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"ランキング取得失敗: {e.Message}");
            _rankingView.SetError("Failed to load");
        }

        // ゲーム開始待機
        await WaitForAnyKeyAsync();
        StartGame();
    }

    private void StartGame()
    {
        _correctCount = 0;
        _missCount = 0;
        _currentQuestionIndex = 0;
        _inputBuffer.Clear();

        var questions = CreateQuestions();
        _questionCount = questions.Count;
        _typingView.SetMessage("");
        _session.StartSession(questions);
    }

    /// <summary>
    /// キーボード入力イベントハンドラ
    /// </summary>
    private void OnTextInput(char c)
    {
        // 制御文字は無視
        if (!char.IsControl(c))
        {
            _inputBuffer.Enqueue(c);
        }
    }

    private void OnQuestionChanged(TypingQuestion question)
    {
        if (question == null) return;
        UpdateDisplay();
        UpdateStatus();
    }

    private void OnInput(InputResult result)
    {
        if (result.IsIgnored) return;
        if (result.IsCorrect)
            _correctCount++;
        else
            _missCount++;

        UpdateDisplay();
        UpdateStatus();
    }

    private async UniTaskVoid OnSessionCompleted()
    {
        var total = _correctCount + _missCount;
        var accuracy = total > 0 ? (float)_correctCount / total : 0f;
        var score = _correctCount * 100 - _missCount * 50;

        _typingView.SetMessage(
            $"Complete!\nScore: {score}  Accuracy: {accuracy:P0}\nSubmitting...");

        // スコア送信
        try
        {
            var playerName = _playerNameModel.PlayerName.CurrentValue;
            await _supabase.SubmitScoreAsync(playerName, score, accuracy);
            var ranking = await _supabase.FetchRankingAsync();
            _rankingView.SetRanking(ranking);
            _typingView.SetMessage(
                $"Complete!\nScore: {score}  Accuracy: {accuracy:P0}\nPress any key to retry");
        }
        catch (Exception e)
        {
            Debug.LogWarning($"スコア送信失敗: {e.Message}");
            _typingView.SetMessage(
                $"Complete!\nScore: {score}  Accuracy: {accuracy:P0}\n(Submit failed) Press any key to retry");
        }

        // リトライ待機
        await WaitForAnyKeyAsync();
        _inputBuffer.Clear();
        StartGame();
    }

    private void UpdateDisplay()
    {
        var question = _session.CurrentQuestion.CurrentValue;
        if (question == null) return;

        var pos = _session.CurrentPosition.CurrentValue;
        var inputText = question.InputText;

        // 入力済み部分と未入力部分をcolorタグで色分け表示
        var typed = inputText.Substring(0, pos);
        var remaining = pos < inputText.Length ? inputText.Substring(pos) : string.Empty;
        _typingView.SetQuestionText($"<color=#888888>{typed}</color>{remaining}");
    }

    private void UpdateStatus()
    {
        var total = _correctCount + _missCount;
        var accuracy = total > 0 ? (float)_correctCount / total : 1f;
        var displayIndex = Mathf.Min(_currentQuestionIndex + 1, _questionCount);
        _typingView.SetStatus(
            $"Q{displayIndex}/{_questionCount}  Accuracy: {accuracy:P0}");
    }

    /// <summary>
    /// 任意キー入力を待機する
    /// </summary>
    private async UniTask WaitForAnyKeyAsync()
    {
        _typingView.SetMessage("Press any key to start");

        // プレイヤー名入力パネルが閉じるまで待機
        await UniTask.WaitUntil(() => !_playerNameInputView.gameObject.activeSelf);

        // キー入力を待機
        await UniTask.WaitUntil(() => Input.anyKeyDown);
    }

    private List<TypingQuestion> CreateQuestions()
    {
        return new List<TypingQuestion>
        {
            new("hello"),
            new("world"),
            new("typing"),
            new("game"),
            new("unity"),
            new("online"),
            new("ranking"),
            new("complete"),
        };
    }

    public void Dispose()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null)
        {
            keyboard.onTextInput -= OnTextInput;
        }

        _disposables.Dispose();
    }
}
