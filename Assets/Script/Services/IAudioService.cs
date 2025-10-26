using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IAudioService
{
    void SetMasterVolume(float v);
    void SetMusicVolume(float v);
    void SetSfxVolume(float v);
    void PlaySfx(AudioClip clip, float vol = 1f);
    void DuckMusic(float duckTarget = 0.2f, float duckTime = 0.15f, float restoreTime = 0.4f);
    void TransitionToSnapshot(string snapshotName, float time);
}

