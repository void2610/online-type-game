using LitMotion;
using UnityEngine;
using UnityEngine.Rendering.Universal;

/// <summary>
/// タイピングエフェクト（ミス時の点滅等）を管理するView
/// </summary>
public class TypingEffectView : MonoBehaviour
{
    [SerializeField] private Light2D leftLight;
    [SerializeField] private Light2D rightLight;
    [SerializeField] private float baseIntensity = 1f;
    [SerializeField] private float flickerMinIntensity = 0.2f;
    [SerializeField] private float flickerDuration = 0.05f;
    [SerializeField] private int flickerCount = 6;
    [SerializeField] private Color lightColor = new(1f, 0.2f, 0.2f, 1f);

    private MotionHandle _leftHandle;
    private MotionHandle _rightHandle;

    /// <summary>
    /// ミスタイプ時のチラつきエフェクトを再生
    /// </summary>
    public void PlayMissFlash()
    {
        FlickerLight(leftLight, ref _leftHandle);
        FlickerLight(rightLight, ref _rightHandle);
    }

    private void Start()
    {
        // 常時点灯
        SetLightIntensity(baseIntensity);
    }

    private void FlickerLight(Light2D light, ref MotionHandle handle)
    {
        // 実行中のアニメーションをキャンセル
        handle.TryCancel();

        // ベース輝度からチラつきの最低輝度まで素早く明滅し、ベース輝度に戻る
        handle = LMotion.Create(baseIntensity, flickerMinIntensity, flickerDuration)
            .WithLoops(flickerCount, LoopType.Yoyo)
            .WithOnComplete(() => light.intensity = baseIntensity)
            .Bind(v => light.intensity = v)
            .AddTo(gameObject);
    }

    private void SetLightIntensity(float intensity)
    {
        leftLight.color = lightColor;
        leftLight.intensity = intensity;
        rightLight.color = lightColor;
        rightLight.intensity = intensity;
    }
}
