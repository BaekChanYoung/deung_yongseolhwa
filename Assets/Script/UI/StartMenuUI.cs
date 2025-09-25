using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StartMenuUI : MonoBehaviour
{
    public Slider MasterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;

    IAudioService audio;

    void Awake()
    {
        audio = ServiceLocator.Resolve<IAudioService>();
        if (audio == null) Debug.LogWarning("IAudioService not found");

        if (MasterSlider != null && audio != null)
        {
            float master = PlayerPrefs.GetFloat("MasterVol", 1.0f);
            MasterSlider.SetValueWithoutNotify(master);
            MasterSlider.onValueChanged.AddListener(audio.SetMasterVolume);
        }

        if (musicSlider != null)
        {
            float m = PlayerPrefs.GetFloat("MusicVol", 0.5f);
            musicSlider.SetValueWithoutNotify(m);
            musicSlider.onValueChanged.AddListener(audio.SetMusicVolume);
        }
        if (sfxSlider != null)
        {
            float s = PlayerPrefs.GetFloat("SfxVol", 0.7f);
            sfxSlider.SetValueWithoutNotify(s);
            sfxSlider.onValueChanged.AddListener(audio.SetSfxVolume);
        }
    }

    void OnDestroy()
    {
        if (musicSlider != null && audio != null) musicSlider.onValueChanged.RemoveListener(audio.SetMusicVolume);
        if (sfxSlider != null && audio != null) sfxSlider.onValueChanged.RemoveListener(audio.SetSfxVolume);
    }
}
