using System.Collections;
using System.Collections.Generic;
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

    void Awake()
    {
        // AudioSettings 기본값 사용
        float defaultMaster = 1f;
        float defaultMusic = 0.5f;
        float defaultSfx = 0.7f;

        // settings가 연결되어 있으면 그 값을 사용
        if (settings != null)
        {
            defaultMaster = settings.defaultMaster;
            defaultMusic = settings.defaultMusic;
            defaultSfx = settings.defaultSfx;
        }
        else
        {
            Debug.LogWarning("AudioSettings가 연결되지 않았습니다. 기본값(1, 0.5, 0.7)을 사용합니다.");
        }

        // PlayerPrefs에서 불러오되, 없으면 AudioSettings 기본값 사용
        SetMasterVolume_Internal(PlayerPrefs.GetFloat("MasterVol", defaultMaster));
        SetMusicVolume_Internal(PlayerPrefs.GetFloat("MusicVol", defaultMusic));
        SetSfxVolume_Internal(PlayerPrefs.GetFloat("SfxVol", defaultSfx));
    }

    void Start()
    {
        if (Input.GetKey(KeyCode.LeftShift))
        {
            PlayerPrefs.DeleteAll();
            Debug.Log("PlayerPrefs 초기화됨!");
        }

        if (musicSource != null && musicSource.clip != null)
        {
            musicSource.Play();
        }
    }


    // IAudioService 구현 (public 메서드)
    public void SetMasterVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MasterVol", sliderValue);
        SetMasterVolume_Internal(sliderValue);
    }

    void SetMasterVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MASTER_PARAM, db);
    }

    public void SetMusicVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("MusicVol", sliderValue);
        SetMusicVolume_Internal(sliderValue);
    }

    void SetMusicVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(MUSIC_PARAM, db);
    }

    public void SetSfxVolume(float sliderValue)
    {
        PlayerPrefs.SetFloat("SfxVol", sliderValue);
        SetSfxVolume_Internal(sliderValue);
    }

    void SetSfxVolume_Internal(float sliderValue)
    {
        float db = Mathf.Lerp(-80f, 0f, Mathf.Clamp01(sliderValue));
        mainMixer.SetFloat(SFX_PARAM, db);
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
        if (ServiceLocator.Resolve<IAudioService>() == this)
            ServiceLocator.Unregister<IAudioService>();
    }
}