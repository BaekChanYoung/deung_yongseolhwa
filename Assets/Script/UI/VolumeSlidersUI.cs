using System.Collections;
using System.Collections.Generic;
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
            Debug.LogWarning("IAudioService를 찾을 수 없습니다. Bootstrapper가 정상적으로 로드되었는지 확인하세요.");
            return;
        }

        // 슬라이더 최소값
        float minVal = 0.25f;
        if (audioSettings != null)
        {
            minVal = audioSettings.minSliderValue; // AudioSettings에서 값 로드
        }

        // 마스터 볼륨 슬라이더 설정
        if (MasterSlider != null)
        {
            MasterSlider.minValue = minVal; // <--- 최소값 설정
            MasterSlider.maxValue = 1.0f;   // <--- 최대값 설정

            // PlayerPrefs 로드 후 이벤트 등록 (등록 순서 중요)
            float master = PlayerPrefs.GetFloat("MasterVol", 1.0f);
            MasterSlider.SetValueWithoutNotify(Mathf.Max(master, minVal)); // 로드 값이 minVal보다 작으면 minVal로 보정
            MasterSlider.onValueChanged.AddListener(audioService.SetMasterVolume);
        }

        // 뮤직 볼륨 슬라이더 설정
        if (musicSlider != null)
        {
            musicSlider.minValue = minVal; // <--- 최소값 설정
            musicSlider.maxValue = 1.0f;   // <--- 최대값 설정

            float m = PlayerPrefs.GetFloat("MusicVol", 0.75f);
            musicSlider.SetValueWithoutNotify(Mathf.Max(m, minVal)); // 보정
            musicSlider.onValueChanged.AddListener(audioService.SetMusicVolume);
        }

        // SFX 볼륨 슬라이더 설정
        if (sfxSlider != null)
        {
            sfxSlider.minValue = minVal; // <--- 최소값 설정
            sfxSlider.maxValue = 1.0f;   // <--- 최대값 설정

            float s = PlayerPrefs.GetFloat("SfxVol", 0.8f);
            sfxSlider.SetValueWithoutNotify(Mathf.Max(s, minVal)); // 보정
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
            // 활성화와 동시에 투명도 0에서 시작
            contentCanvasGroup.alpha = 0f;
            // CanvasGroup 오브젝트 활성화 (자식 오브젝트 Raycast 포함)
            contentCanvasGroup.gameObject.SetActive(true);

            StartCoroutine(FadeContent(1f)); // Alpha 1.0까지 페이드 인
        }
    }

    // 애니메이션 이벤트 등에서 호출: 페이드 아웃 시작
    public void StartContentFadeOut()
    {
        if (contentCanvasGroup != null)
        {
            StartCoroutine(FadeContent(0f)); // Alpha 0.0까지 페이드 아웃
        }
    }

    // 코루틴: Alpha를 목표값으로 보간하며 페이드 처리
    IEnumerator FadeContent(float targetAlpha)
    {
        float startAlpha = contentCanvasGroup.alpha;
        float time = 0f;

        // 마스터/BGM/SFX 그룹을 개별적으로 켜고 끄려면 이 부분에서 추가로 처리하세요.
        // 여기서는 ContentGroup 전체의 Alpha만 조절하는 일반적인 페이드를 구현합니다.

        while (time < fadeDuration)
        {
            time += Time.unscaledDeltaTime;
            contentCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            yield return null;
        }

        contentCanvasGroup.alpha = targetAlpha;

        // 페이드 아웃(닫힘) 완료 시 비활성화 (이중 입력 방지)
        if (targetAlpha <= 0.01f)
        {
            contentCanvasGroup.gameObject.SetActive(false);
        }
    }

    //// 애니메이션 이벤트에서 호출할 수 있는 UI 표시 함수 (필요 시 사용)

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
