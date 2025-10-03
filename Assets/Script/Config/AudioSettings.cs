using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Config/AudioSettings")]
public class AudioSettings : ScriptableObject
{
    [Range(0, 1)] public float defaultMaster = 1f;
    [Range(0, 1)] public float defaultMusic = 0.4f;
    [Range(0, 1)] public float defaultSfx = 0.8f;
}