using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    public AudioManager audioManagerPrefab; // �����Ϳ��� ������ ���� ����

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

        DontDestroyOnLoad(gameObject); // bootstrap ����(����)
    }
}

