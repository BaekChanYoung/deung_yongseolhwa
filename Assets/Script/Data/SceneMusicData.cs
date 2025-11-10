using UnityEngine;

/// <summary>
/// 씬별 배경음악 데이터
/// </summary>
[CreateAssetMenu(fileName = "SceneMusicData", menuName = "Audio/Scene Music Data")]
public class SceneMusicData : ScriptableObject
{
    [System.Serializable]
    public class SceneMusic
    {
        [Tooltip("씬 이름")]
        public string sceneName;

        [Tooltip("배경음악")]
        public AudioClip musicClip;

        [Tooltip("볼륨 (0.0 ~ 1.0)")]
        [Range(0f, 1f)]
        public float volume = 0.5f;

        [Tooltip("페이드 인 시간")]
        public float fadeInDuration = 1f;

        [Tooltip("루프 재생")]
        public bool loop = true;
    }

    [Header("Scene Music List")]
    public SceneMusic[] sceneMusicList;

    /// <summary>
    /// 씬 이름으로 음악 찾기
    /// </summary>
    public SceneMusic GetMusicForScene(string sceneName)
    {
        foreach (var sceneMusic in sceneMusicList)
        {
            if (sceneMusic.sceneName == sceneName)
            {
                return sceneMusic;
            }
        }

        Debug.LogWarning($"[SceneMusicData] '{sceneName}' 씬의 배경음악을 찾을 수 없습니다.");
        return null;
    }
}