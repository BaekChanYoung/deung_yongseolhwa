using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public AudioManager audioManagerPrefab; // 에디터에서 프리팹 연결 가능

    void Awake()
    {
        var am = FindObjectOfType<AudioManager>();
        if (am == null && audioManagerPrefab != null)
        {
            am = Instantiate(audioManagerPrefab);
        }

        if (am != null && ServiceLocator.Resolve<IAudioService>() == null)
        {
            ServiceLocator.Register<IAudioService>(am);
        }

        DontDestroyOnLoad(gameObject); // bootstrap 유지(선택)
    }
}

