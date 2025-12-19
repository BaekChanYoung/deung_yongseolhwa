using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 버튼 클릭 시 효과음을 재생하는 컴포넌트
/// 어떤 버튼에든 추가 가능 (재사용 가능)
/// </summary>
[RequireComponent(typeof(Button))]
public class ButtonSoundPlayer : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("클릭 효과음 (없으면 기본 효과음 사용)")]
    public AudioClip clickSfx;

    [Tooltip("효과음 볼륨")]
    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Tooltip("버튼 활성화 시 자동으로 이벤트 연결")]
    public bool autoSetup = true;

    private Button button;
    private IAudioService audioService;

    void Awake()
    {
        button = GetComponent<Button>();
        audioService = ServiceLocator.Resolve<IAudioService>();

        if (autoSetup && button != null)
        {
            button.onClick.AddListener(PlayClickSound);
        }
    }

    void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(PlayClickSound);
        }
    }

    /// <summary>
    /// 클릭 효과음 재생
    /// </summary>
    public void PlayClickSound()
    {
        if (audioService == null)
        {
            Debug.LogWarning($"[ButtonSoundPlayer] IAudioService를 찾을 수 없습니다! ({gameObject.name})");
            return;
        }

        if (clickSfx != null)
        {
            audioService.PlaySfx(clickSfx, volume);
        }
        else
        {
            Debug.LogWarning($"[ButtonSoundPlayer] Click SFX가 설정되지 않았습니다! ({gameObject.name})");
        }
    }

    /// <summary>
    /// 특정 효과음 재생 (외부에서 호출 가능)
    /// </summary>
    public void PlayCustomSound(AudioClip sfx)
    {
        if (audioService != null && sfx != null)
        {
            audioService.PlaySfx(sfx, volume);
        }
    }
}