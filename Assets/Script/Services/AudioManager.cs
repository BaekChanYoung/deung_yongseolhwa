using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour, IAudioService
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;
    [Header("Audio Sources")]
    public AudioSource MasterSource; // 마스터 볼륨 조절용
    public AudioSource musicSource; // BGM 전용(루프)
    public AudioSource sfxSource; // SFX 재생용(PlayOneShot 사용)
    [Header("Settings")]
    [Tooltip("오디오 기본 설정 (ScriptableObject)")]
    public AudioSettings settings;

    // Exposed parameter names (AudioMixer에서 정확히 노출한 이름 사용)
    const string MASTER_PARAM = "MasterVolume";
    const string MUSIC_PARAM = "MusicVolume";
    const string SFX_PARAM = "SFXVolume";

    private float cachedMasterVolume;
    private float cachedMusicVolume;
    private float cachedSfxVolume;

    private Coroutine musicFadeCoroutine;

    //private static bool _instanceExists = false; // AudioManager 자체의 중복 방지 플래그 (이전 대화에서 사용했다면)

    void Awake()
    {
        // 중복 인스턴스 방지 로직 (Self-registration pattern)
        if (ServiceLocator.Resolve<IAudioService>() != null) // 이미 등록된 인스턴스가 있다면
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
        {
            finalDefaultMaster = settings.defaultMaster;
            finalDefaultMusic = settings.defaultMusic;
            finalDefaultSfx = settings.defaultSfx;
        }
        else
        {
            Debug.LogWarning("AudioSettings가 연결되지 않았습니다. 기본값(1, 0.75, 0.8)을 사용합니다.");
        }

        cachedMasterVolume = PlayerPrefs.GetFloat("MasterVol", finalDefaultMaster);
        cachedMusicVolume = PlayerPrefs.GetFloat("MusicVol", finalDefaultMusic);
        cachedSfxVolume = PlayerPrefs.GetFloat("SfxVol", finalDefaultSfx);

        // PlayerPrefs에서 불러오되, 없으면 AudioSettings 기본값 사용
        SetMasterVolume_Internal(cachedMasterVolume);
        SetMusicVolume_Internal(cachedMusicVolume);
        SetSfxVolume_Internal(cachedSfxVolume);

        StartCoroutine(DelayedVolumeSet());

        Debug.Log($"[AudioManager] 볼륨 로드 완료 - Master:{cachedMasterVolume:F2}, Music:{cachedMusicVolume:F2}, SFX:{cachedSfxVolume:F2}");
    }

    // ========== AudioSource Output 자동 설정 ==========
    void SetupAudioSources()
    {
        if (mainMixer == null)
        {
            Debug.LogError("[AudioManager] AudioMixer가 없어서 AudioSource 설정을 건너뜁니다.");
            return;
        }

        // Master 그룹 찾기
        var masterGroup = mainMixer.FindMatchingGroups("Master");
        var musicGroup = mainMixer.FindMatchingGroups("Music");
        var sfxGroup = mainMixer.FindMatchingGroups("SFX");

        if (masterGroup.Length > 0 && MasterSource != null)
        {
            MasterSource.outputAudioMixerGroup = masterGroup[0];
            Debug.Log("[AudioManager] ✓ MasterSource → Master 그룹 연결");
        }

        if (musicGroup.Length > 0 && musicSource != null)
        {
            musicSource.outputAudioMixerGroup = musicGroup[0];
            musicSource.playOnAwake = false; // 자동 재생 방지
            Debug.Log("[AudioManager] ✓ musicSource → Music 그룹 연결");
        }
        else if (musicSource != null)
        {
            Debug.LogWarning("[AudioManager] Music 그룹을 찾을 수 없습니다!");
        }

        if (sfxGroup.Length > 0 && sfxSource != null)
        {
            sfxSource.outputAudioMixerGroup = sfxGroup[0];
            sfxSource.playOnAwake = false;
            Debug.Log("[AudioManager] ✓ sfxSource → SFX 그룹 연결");
        }
        else if (sfxSource != null)
        {
            Debug.LogWarning("[AudioManager] SFX 그룹을 찾을 수 없습니다!");
        }
    }
    // ================================================

    // ========== 딜레이 후 볼륨 재설정 (안전장치) ==========
    IEnumerator DelayedVolumeSet()
    {
        // 1프레임 대기 (AudioMixer 초기화 완료 보장)
        yield return null;

        Debug.Log("[AudioManager] 볼륨 재설정 중...");
        SetMasterVolume_Internal(cachedMasterVolume);
        SetMusicVolume_Internal(cachedMusicVolume);
        SetSfxVolume_Internal(cachedSfxVolume);

        Debug.Log("[AudioManager] 볼륨 재설정 완료!");
    }
    // ================================================

    void CheckExposedParameters()
    {
        float testValue;
        bool masterExists = mainMixer.GetFloat(MASTER_PARAM, out testValue);
        bool musicExists = mainMixer.GetFloat(MUSIC_PARAM, out testValue);
        bool sfxExists = mainMixer.GetFloat(SFX_PARAM, out testValue);

        if (!masterExists)
        {
            Debug.LogError($"[AudioManager] '{MASTER_PARAM}' 파라미터가 노출되지 않았습니다!");
        }

        if (!musicExists)
        {
            Debug.LogError($"[AudioManager] '{MUSIC_PARAM}' 파라미터가 노출되지 않았습니다!");
        }

        if (!sfxExists)
        {
            Debug.LogError($"[AudioManager] '{SFX_PARAM}' 파라미터가 노출되지 않았습니다!");
        }

        if (masterExists && musicExists && sfxExists)
        {
            Debug.Log("[AudioManager] ✓ 모든 Exposed Parameters가 정상입니다.");
        }
    }

    void Start()
    {
        /*if (musicSource != null && musicSource.clip != null)
        {
            // 추가 확인: 현재 Music 볼륨 dB 읽기
            float currentMusicDb;
            if (mainMixer.GetFloat(MUSIC_PARAM, out currentMusicDb))
            {
                Debug.Log($"[AudioManager] 음악 재생 시작 - 현재 Music dB: {currentMusicDb:F2}");
            }

            musicSource.Play();
            Debug.Log("[AudioManager] 배경음악 재생 시작");
        }*/
    }

    // IAudioService 구현 (public 메서드)
    public void SetMasterVolume(float sliderValue)
    {
        cachedMasterVolume = sliderValue;
        PlayerPrefs.SetFloat("MasterVol", sliderValue);
        PlayerPrefs.Save();
        SetMasterVolume_Internal(sliderValue);
    }

    void SetMasterVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MASTER_PARAM, db);
    }

    public void SetMusicVolume(float sliderValue)
    {
        cachedMusicVolume = sliderValue;
        PlayerPrefs.SetFloat("MusicVol", sliderValue);
        PlayerPrefs.Save();
        SetMusicVolume_Internal(sliderValue);
    }

    void SetMusicVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MUSIC_PARAM, db);
    }

    public void SetSfxVolume(float sliderValue)
    {
        cachedSfxVolume = sliderValue;
        PlayerPrefs.SetFloat("SfxVol", sliderValue);
        PlayerPrefs.Save();
        SetSfxVolume_Internal(sliderValue);
    }

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
        if (cachedSfxVolume <= 0.01f) return;
        if (clip == null)
        {
            Debug.LogWarning("[AudioManager] AudioClip이 null입니다!");
            return;
        }
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volume));
    }

    // Duck: SFX 우선 재생 시 BGM을 순간적으로 낮추는 방식
    public void DuckMusic(float duckTarget = 0.2f, float duckTime = 0.15f, float restoreTime = 0.4f)
    {

        if (settings != null)
        {
            duckTarget = settings.duckTarget;
            duckTime = settings.duckTime;
            restoreTime = settings.restoreTime;
        }

        StartCoroutine(DuckCoroutine(duckTarget, duckTime, restoreTime));
    }

    IEnumerator DuckCoroutine(float duckTarget, float duckTime, float restoreTime)
    {
        // 현재 볼륨 로드
        float current;
        mainMixer.GetFloat(MUSIC_PARAM, out current);
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
        float savedDb = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(cachedMusicVolume));
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

    /// <summary>
    /// 음악 재생 (즉시)
    /// </summary>
    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        if (musicSource == null || clip == null)
        {
            Debug.LogWarning("[AudioManager] musicSource 또는 clip이 null입니다!");
            return;
        }

        // 기존 페이드 중단
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();

        Debug.Log($"[AudioManager] 음악 재생: {clip.name}");
    }

    /// <summary>
    /// 음악 정지
    /// </summary>
    public void StopMusic()
    {
        if (musicSource == null) return;

        // 기존 페이드 중단
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
            musicFadeCoroutine = null;
        }

        musicSource.Stop();
        Debug.Log("[AudioManager] 음악 정지");
    }

    /// <summary>
    /// 크로스 페이드로 음악 전환
    /// </summary>
    public void CrossFadeMusic(AudioClip newClip, float fadeDuration = 1f)
    {
        if (musicSource == null || newClip == null)
        {
            Debug.LogWarning("[AudioManager] musicSource 또는 newClip이 null입니다!");
            return;
        }

        // 같은 음악이 이미 재생 중이면 무시
        if (musicSource.clip == newClip && musicSource.isPlaying)
        {
            Debug.Log($"[AudioManager] 이미 재생 중: {newClip.name}");
            return;
        }

        // 기존 페이드 중단
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }

        musicFadeCoroutine = StartCoroutine(CrossFadeMusicCoroutine(newClip, fadeDuration));
    }

    /// <summary>
    /// 특정 음악이 재생 중인지 확인
    /// </summary>
    public bool IsMusicPlaying(AudioClip clip)
    {
        if (musicSource == null || clip == null) return false;
        return musicSource.clip == clip && musicSource.isPlaying;
    }

    IEnumerator CrossFadeMusicCoroutine(AudioClip newClip, float fadeDuration)
    {
        float halfDuration = fadeDuration * 0.5f;

        // 1단계: 페이드 아웃 (AudioSource.volume 사용)
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < halfDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / halfDuration);
                yield return null;
            }

            musicSource.Stop();
        }

        // 2단계: 페이드 인
        musicSource.clip = newClip;
        musicSource.loop = true;
        musicSource.volume = 0f;
        musicSource.Play();

        float elapsed2 = 0f;

        while (elapsed2 < halfDuration)
        {
            elapsed2 += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, 1f, elapsed2 / halfDuration);
            yield return null;
        }

        musicSource.volume = 1f;

        Debug.Log($"[AudioManager] 크로스 페이드 완료: {newClip.name}");

        musicFadeCoroutine = null;
    }

    private void ResetAllSettings()
    {
        Debug.Log("========================================");
        Debug.Log("[PlayerPrefs] 초기화 시작...");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("[PlayerPrefs] 삭제 완료!");

        // 즉시 기본값으로 복원
        if (settings != null)
        {
            cachedMasterVolume = settings.defaultMaster;
            cachedMusicVolume = settings.defaultMusic;
            cachedSfxVolume = settings.defaultSfx;

            SetMasterVolume_Internal(cachedMasterVolume);
            SetMusicVolume_Internal(cachedMusicVolume);
            SetSfxVolume_Internal(cachedSfxVolume);

            Debug.Log($"[PlayerPrefs] 기본값 복원 완료!");
            Debug.Log($"Master: {cachedMasterVolume:F2}");
            Debug.Log($"Music: {cachedMusicVolume:F2}");
            Debug.Log($"SFX: {cachedSfxVolume:F2}");
        }
        Debug.Log("========================================");
    }

    private void CheckPlayerPrefs()
    {
        Debug.Log("========================================");
        Debug.Log("[PlayerPrefs] 저장된 값 확인:");

        // HasKey로 존재 여부 확인
        if (PlayerPrefs.HasKey("MasterVol"))
            Debug.Log($"MasterVol: {PlayerPrefs.GetFloat("MasterVol")}");
        else
            Debug.Log("MasterVol: 저장된 값 없음");

        if (PlayerPrefs.HasKey("MusicVol"))
            Debug.Log($"MusicVol: {PlayerPrefs.GetFloat("MusicVol")}");
        else
            Debug.Log("MusicVol: 저장된 값 없음");

        if (PlayerPrefs.HasKey("SfxVol"))
            Debug.Log($"SfxVol: {PlayerPrefs.GetFloat("SfxVol")}");
        else
            Debug.Log("SfxVol: 저장된 값 없음");

        Debug.Log("----------------------------------------");
        Debug.Log("[캐시] 현재 메모리 값:");
        Debug.Log($"cachedMasterVolume: {cachedMasterVolume:F2}");
        Debug.Log($"cachedMusicVolume: {cachedMusicVolume:F2}");
        Debug.Log($"cachedSfxVolume: {cachedSfxVolume:F2}");
        Debug.Log("========================================");
    }

    void OnDestroy()
    {
        if ((UnityEngine.Object)ServiceLocator.Resolve<IAudioService>() == this)
            ServiceLocator.Unregister<IAudioService>();
    }

    void OnApplicationQuit()
    {
        PlayerPrefs.Save();
        Debug.Log("[AudioManager] 게임 종료 - PlayerPrefs 저장 완료");
    }
}