using UnityEngine;

public interface IAudioService
{
    void SetMasterVolume(float v);
    void SetMusicVolume(float v);
    void SetSfxVolume(float v);
    void PlaySfx(AudioClip clip, float vol = 1f);
    void DuckMusic(float duckTarget = 0.2f, float duckTime = 0.15f, float restoreTime = 0.4f);
    void TransitionToSnapshot(string snapshotName, float time);
    void SetMusicVolumeInternal(float v);
    void PlayMusic(AudioClip clip, bool loop = true);
    void StopMusic();
    void CrossFadeMusic(AudioClip newClip, float fadeDuration = 1f);
    bool IsMusicPlaying(AudioClip clip); // 중복 재생 방지용
}