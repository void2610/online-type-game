using System;
using R3;
using VContainer.Unity;

/// <summary>
/// プレイヤー名入力のPresenter
/// </summary>
public class PlayerNameInputPresenter : IStartable, IDisposable
{
    private readonly PlayerNameInputView _view;
    private readonly PlayerNameModel _model;
    private readonly CompositeDisposable _disposables = new();

    public PlayerNameInputPresenter(PlayerNameInputView view, PlayerNameModel model)
    {
        _view = view;
        _model = model;
    }

    public void Start()
    {
        // 現在のプレイヤー名を入力フィールドに設定
        _model.PlayerName
            .Subscribe(name => _view.NameInputField.text = name)
            .AddTo(_disposables);

        // 確定ボタンクリック時の処理
        _view.OnConfirmClicked
            .Subscribe(_ =>
            {
                var inputName = _view.NameInputField.text;
                _model.SetPlayerName(inputName);
                _view.SetActive(false);
            })
            .AddTo(_disposables);

        // プレイヤー名が未設定の場合のみ表示
        _view.SetActive(!_model.HasSetName);
    }

    public void Dispose()
    {
        _disposables?.Dispose();
    }
}
