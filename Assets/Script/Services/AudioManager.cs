using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IAudioService
{
    //public static AudioManager Instance { get; private set; }

    [Header("Audio Mixer")]
    public AudioMixer mainMixer;
    [Header("Audio Sources")]
    public AudioSource MasterSource; // 마스터 볼륨 조절용
    public AudioSource musicSource; // BGM 전용(루프)
    public AudioSource sfxSource; // SFX 재생용(PlayOneShot 사용)

    // Exposed parameter names (AudioMixer에서 정확히 노출한 이름 사용)
    const string MASTER_PARAM = "MasterVolume";
    const string MUSIC_PARAM = "MusicVolume";
    const string SFX_PARAM = "SFXVolume";

<<<<<<< Updated upstream
    void Awake()
    {
        //if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        //else { Destroy(gameObject); return; }
=======
    private float cachedMasterVolume;
    private float cachedMusicVolume;
    private float cachedSfxVolume;

    //private static bool _instanceExists = false; // AudioManager 자체의 중복 방지 플래그 (이전 대화에서 사용했다면)

    void Awake()
    {
        // 중복 인스턴스 방지 로직 (Self-registration pattern)
        if (ServiceLocator.Resolve<IAudioService>() != null) // 이미 등록된 인스턴스가 있다면
<<<<<<< Updated upstream
=======
        {
            Destroy(gameObject); // 이 인스턴스는 파괴합니다.
            return;
        }

        // ServiceLocator에 자기 자신을 등록
        ServiceLocator.Register<IAudioService>(this);
        Debug.Log("[AudioManager] ServiceLocator에 등록되었습니다.");

        SetupAudioSources();

        // AudioSettings 기본값 사용
        float finalDefaultMaster = 1f;
        float finalDefaultMusic = 0.75f;
        float finalDefaultSfx = 0.8f;

        // settings가 연결되어 있으면 그 값을 사용
        if (settings != null)
>>>>>>> Stashed changes
        {
            Destroy(gameObject); // 이 인스턴스는 파괴합니다.
            return;
        }

        // ServiceLocator에 자기 자신을 등록
        ServiceLocator.Register<IAudioService>(this);
        Debug.Log("[AudioManager] ServiceLocator에 등록되었습니다.");

        SetupAudioSources();
>>>>>>> Stashed changes

        // 초기 설정 불러오기
        SetMasterVolume_Internal(PlayerPrefs.GetFloat("MasterVol", 1f));
        SetMusicVolume_Internal(PlayerPrefs.GetFloat("MusicVol", 0.5f));
        SetSfxVolume_Internal(PlayerPrefs.GetFloat("SfxVol", 0.7f));
    }

    void Start()
    {
        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }
    }


    // IAudioService 구현 (public 메서드)
    public void SetMasterVolume(float sliderValue) { PlayerPrefs.SetFloat("MasterVol", sliderValue); SetMasterVolume_Internal(sliderValue); }
    void SetMasterVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MASTER_PARAM, db);
    }

    public void SetMusicVolume(float sliderValue) { PlayerPrefs.SetFloat("MusicVol", sliderValue); SetMusicVolume_Internal(sliderValue); }
    void SetMusicVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MUSIC_PARAM, db);
    }

    public void SetSfxVolume(float sliderValue) { PlayerPrefs.SetFloat("SfxVol", sliderValue); SetSfxVolume_Internal(sliderValue); }
    void SetSfxVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(SFX_PARAM, db);
    }

    public void SetMusicVolumeInternal(float sliderValue)
    {
        // PlayerPrefs 저장 없이 볼륨만 변경!
        cachedMusicVolume = sliderValue; // 캐시는 업데이트
        SetMusicVolume_Internal(sliderValue);
    }

    // SFX 재생 (PlayOneShot 사용)
    public void PlaySfx(AudioClip clip, float volume = 1f)
    {
        if (PlayerPrefs.GetFloat("SfxVol", 0.7f) <= 0.01f) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // Duck: SFX 우선 재생 시 BGM을 순간적으로 낮추는 방식
    public void DuckMusic(float duckTarget = 0.2f, float duckTime = 0.15f, float restoreTime = 0.4f)
    {
        StartCoroutine(DuckCoroutine(duckTarget, duckTime, restoreTime));
    }

    IEnumerator DuckCoroutine(float duckTarget, float duckTime, float restoreTime)
    {
        // 현재 볼륨 로드
        float current; mainMixer.GetFloat(MUSIC_PARAM, out current);
        // current는 dB. 변환 필요 if you want percentage, but we'll work in dB:
        float targetDb = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(duckTarget)); // duckTarget in 0..1
        // fade to duck
        float t = 0f;
        while (t < duckTime)
        { 
            mainMixer.SetFloat(MUSIC_PARAM, Mathf.Lerp(current, targetDb, t / duckTime)); 
            t += Time.unscaledDeltaTime; 
            yield return null; 
        }
        mainMixer.SetFloat(MUSIC_PARAM, targetDb);

        // wait a short while (depending on gameplay) — here we just restore after restoreTime
        yield return new WaitForSecondsRealtime(restoreTime);

        // restore to previously saved slider value
        float saved = PlayerPrefs.GetFloat("MusicVol", 0.5f);
        float savedDb = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(saved));
        t = 0f;
        while (t < restoreTime)
        { 
            mainMixer.SetFloat(MUSIC_PARAM, Mathf.Lerp(targetDb, savedDb, t / restoreTime));
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        mainMixer.SetFloat(MUSIC_PARAM, savedDb);
    }

    // Snapshot 전환 예시 (에디터에서 스냅샷 생성 필요)
    public void TransitionToSnapshot(string snapshotName, float time)
    {
        var snap = mainMixer.FindSnapshot(snapshotName);
        if (snap != null) snap.TransitionTo(time);
        else Debug.LogWarning("Snapshot not found: " + snapshotName);
    }

    void OnDestroy()
    {
        if (ServiceLocator.Resolve<IAudioService>() == this) ServiceLocator.Unregister<IAudioService>();
    }
}