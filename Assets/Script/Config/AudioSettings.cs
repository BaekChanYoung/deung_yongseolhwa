using UnityEngine;

[CreateAssetMenu(menuName = "Config/AudioSettings")]

public class AudioSettings : ScriptableObject
{
    [Range(0, 1)] public float defaultMaster = 1f;
    [Range(0, 1)] public float defaultMusic = 0.75f;
    [Range(0, 1)] public float defaultSfx = 0.8f;
    [Header("덕킹 설정")]
    [Tooltip("SFX 재생 시 BGM 볼륨을 얼마나 줄일지")]
    [Range(0, 1)] public float duckTarget = 0.2f;
    [Tooltip("덕킹 전환 시간")]
    public float duckTime = 0.15f;
    [Tooltip("원래 볼륨으로 복구 시간")]
    public float restoreTime = 0.4f;
    [Header("UI 설정")]
    [Tooltip("슬라이더 최소값")]
    [Range(0, 1)] public float minSliderValue = 0.25f;
    [Tooltip("슬라이더 최대값 표시")]
    public string maxVolumeText = "최대";
}