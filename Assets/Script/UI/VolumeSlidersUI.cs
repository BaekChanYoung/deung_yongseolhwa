using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class VolumeSlidersUI : MonoBehaviour
{
    public Slider MasterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    public AudioSettings audioSettings;

    public GameObject Setting_Bg;
    public GameObject masterGroup;
    public GameObject musicGroup;
    public GameObject sfxGroup;
    public GameObject exitButton;

    public CanvasGroup contentCanvasGroup;
    public float fadeDuration = 0.2f;

    IAudioService audioService;

    void Awake()
    {
        audioService = ServiceLocator.Resolve<IAudioService>();
        if (audioService == null)
        {
            Debug.LogWarning("IAudioService를 찾을 수 없습니다. Bootstrapper가 먼저 로드되었는지 확인하세요.");
            return;
        }

        // 사용할 슬라이더 최소값
        float minVal = 0.25f;
        if (audioSettings != null)
        {
            minVal = audioSettings.minSliderValue; // AudioSettings에서 값 로드
        }

        // 마스터 볼륨 설정
        if (MasterSlider != null)
        {
            MasterSlider.minValue = minVal; // <--- 최소값 설정
            MasterSlider.maxValue = 1.0f;   // <--- 최대값 설정

            // PlayerPrefs 로드 및 이벤트 연결 (기존 로직 유지)
            float master = PlayerPrefs.GetFloat("MasterVol", 1.0f);
            MasterSlider.SetValueWithoutNotify(Mathf.Max(master, minVal)); // 로드된 값이 minVal보다 작으면 minVal로 설정
            MasterSlider.onValueChanged.AddListener(audioService.SetMasterVolume);
        }

        // 음악 볼륨 설정
        if (musicSlider != null)
        {
            musicSlider.minValue = minVal; // <--- 최소값 설정
            musicSlider.maxValue = 1.0f;   // <--- 최대값 설정

            float m = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            musicSlider.SetValueWithoutNotify(Mathf.Max(m, minVal));
            musicSlider.onValueChanged.AddListener(audioService.SetMusicVolume);
        }

        // SFX 볼륨 설정
        if (sfxSlider != null)
        {
            sfxSlider.minValue = minVal; // <--- 최소값 설정
            sfxSlider.maxValue = 1.0f;   // <--- 최대값 설정

            float s = PlayerPrefs.GetFloat("SfxVol", 0.8f);
            sfxSlider.SetValueWithoutNotify(Mathf.Max(s, minVal));
            sfxSlider.onValueChanged.AddListener(audioService.SetSfxVolume);
        }
    }

    void OnDestroy()
    {
        if (MasterSlider != null && audioService != null) MasterSlider.onValueChanged.RemoveListener(audioService.SetMasterVolume);
        if (musicSlider != null && audioService != null) musicSlider.onValueChanged.RemoveListener(audioService.SetMusicVolume);
        if (sfxSlider != null && audioService != null) sfxSlider.onValueChanged.RemoveListener(audioService.SetSfxVolume);
    }

    public void StartContentFadeIn()
    {
        if (contentCanvasGroup != null)
        {
            // 내용물 활성화는 족자봉이 끝날 때
            contentCanvasGroup.alpha = 0f;
            // 내용물 그룹 자체는 활성화 (자식 오브젝트의 Raycast를 위해)
            contentCanvasGroup.gameObject.SetActive(true);

            StartCoroutine(FadeContent(1f)); // 1f (Alpha 1.0)으로 페이드 인
        }
    }

    // 애니메이션 이벤트에서 호출될 함수: 페이드 아웃 시작
    public void StartContentFadeOut()
    {
        if (contentCanvasGroup != null)
        {
            StartCoroutine(FadeContent(0f)); // 0f (Alpha 0.0)으로 페이드 아웃
        }
    }

    // 코루틴: Alpha 값을 목표치까지 부드럽게 변경
    IEnumerator FadeContent(float targetAlpha)
    {
        float startAlpha = contentCanvasGroup.alpha;
        float time = 0f;

        // 마스터, BGM, SFX 그룹을 순차적으로 활성화하려면 이 로직을 여기에 통합해야 합니다.
        // 여기서는 ContentGroup 전체의 Alpha를 조절하는 일반적인 페이드만 사용합니다.

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            contentCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        contentCanvasGroup.alpha = targetAlpha;

        // 페이드 아웃(닫기)이 완료된 경우에만 비활성화 (2번 항목 해결)
        if (targetAlpha <= 0.01f)
        {
            contentCanvasGroup.gameObject.SetActive(false);
        }
    }

    //// 애니메이션 이벤트에서 호출될 함수

    //public void ShowSetting_Bg()
    //{
    //    if (Setting_Bg != null) Setting_Bg.SetActive(true);
    //    if (exitButton != null) exitButton.SetActive(true);
    //}

    //public void ShowMaster()
    //{
    //    if (masterGroup != null) masterGroup.SetActive(true);
    //}

    //public void ShowMusic()
    //{
    //    if (musicGroup != null) musicGroup.SetActive(true);
    //}

    //public void ShowSFX()
    //{
    //    if (sfxGroup != null) sfxGroup.SetActive(true);
    //}
}
