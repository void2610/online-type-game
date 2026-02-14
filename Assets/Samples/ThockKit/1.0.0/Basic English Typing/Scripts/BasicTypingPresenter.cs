using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer.Unity;
using Void2610.ThockKit.Core.Interfaces;
using Void2610.ThockKit.Core.Models;

namespace Void2610.ThockKit.Samples.BasicEnglishTyping
{
    /// <summary>
    /// 基本的な英語タイピングのサンプル実装
    /// </summary>
    public class BasicTypingPresenter : ITickable, IStartable, IDisposable
    {
        private readonly ITypingSession _session;
        private readonly BasicTypingView _view;
        private readonly CompositeDisposable _disposables = new();

        private int _correctCount;
        private int _missCount;
        private int _questionCount;
        private int _currentQuestionIndex;

        // テキスト入力バッファ
        private readonly Queue<char> _inputBuffer = new();

        public BasicTypingPresenter(ITypingSession session, BasicTypingView view)
        {
            _session = session;
            _view = view;

            // セッションイベントの購読
            _session.CurrentQuestion.Subscribe(OnQuestionChanged).AddTo(_disposables);
            _session.CurrentPosition.Subscribe(_ => UpdateDisplay()).AddTo(_disposables);
            _session.OnInput.Subscribe(OnInput).AddTo(_disposables);
            _session.OnQuestionCompleted.Subscribe(_ => OnQuestionCompleted()).AddTo(_disposables);
            _session.OnSessionCompleted.Subscribe(_ => OnSessionCompleted()).AddTo(_disposables);

            // キーボード入力の購読
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                keyboard.onTextInput += OnTextInput;
            }
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

        public void Start()
        {
            _view.SetTypedText("");
            _view.SetRemainingText("Press any key to start");
            _view.SetStatus("");

            WaitAndStartSession().Forget();
        }

        public void Tick()
        {
            // 入力バッファの処理
            while (_inputBuffer.Count > 0)
            {
                var c = _inputBuffer.Dequeue();
                _session.ProcessInput(c);
            }
        }

        public void Dispose()
        {
            // キーボード入力の購読解除
            var keyboard = Keyboard.current;
            if (keyboard != null)
            {
                keyboard.onTextInput -= OnTextInput;
            }

            _disposables.Dispose();
            _session.Dispose();
        }

        private async UniTask WaitAndStartSession()
        {
            await UniTask.WaitUntil(() => Input.anyKeyDown);

            var questions = CreateSampleQuestions();
            _questionCount = questions.Count;
            _currentQuestionIndex = 0;
            _correctCount = 0;
            _missCount = 0;

            _session.StartSession(questions);
        }

        private void OnQuestionChanged(TypingQuestion question)
        {
            if (question == null) return;
            UpdateDisplay();
        }

        private void OnInput(InputResult result)
        {
            if (result.IsIgnored) return;

            if (result.IsCorrect)
            {
                _correctCount++;
            }
            else
            {
                _missCount++;
            }
            UpdateDisplay();
        }

        private void OnQuestionCompleted()
        {
            _currentQuestionIndex++;
        }

        private void OnSessionCompleted()
        {
            var accuracy = (_correctCount + _missCount) > 0
                ? (float)_correctCount / (_correctCount + _missCount)
                : 1f;

            _view.SetTypedText("Complete!");
            _view.SetRemainingText("Press any key to retry");
            _view.SetStatus($"Correct: {_correctCount}  Miss: {_missCount}  Accuracy: {accuracy:P1}");

            // リトライ待機
            WaitAndStartSession().Forget();
        }

        private void UpdateDisplay()
        {
            var question = _session.CurrentQuestion.CurrentValue;
            if (question == null) return;

            var pos = _session.CurrentPosition.CurrentValue;
            var inputText = question.InputText;

            // 入力済みテキスト
            var typedText = inputText.Substring(0, pos);
            _view.SetTypedText(typedText);

            // 残りテキスト
            _view.SetRemainingText(pos < inputText.Length ? inputText.Substring(pos) : string.Empty);
            UpdateStatus();
        }

        private void UpdateStatus()
        {
            var accuracy = (_correctCount + _missCount) > 0
                ? (float)_correctCount / (_correctCount + _missCount)
                : 1f;

            _view.SetStatus($"{_currentQuestionIndex + 1}/{_questionCount}  Accuracy: {accuracy:P0}");
        }

        private List<TypingQuestion> CreateSampleQuestions()
        {
            return new List<TypingQuestion>
            {
                new("hello"),
                new("world"),
                new("typing"),
                new("library"),
                new("unity"),
                new("sample"),
                new("game"),
                new("complete"),
            };
        }
    }
}
