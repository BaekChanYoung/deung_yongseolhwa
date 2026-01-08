using System.Collections.Generic;
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

    // 캐싱을 위한 Dictionary
    private Dictionary<string, SceneMusic> musicCache;

    // 초기화 (첫 접근 시 자동 실행)
    void OnEnable()
    {
        BuildCache();
    }

    void BuildCache()
    {
        musicCache = new Dictionary<string, SceneMusic>();

        foreach (var sceneMusic in sceneMusicList)
        {
            if (string.IsNullOrEmpty(sceneMusic.sceneName))
            {
                Debug.LogWarning("[SceneMusicData] Scene name is empty!");
                continue;
            }

            if (musicCache.ContainsKey(sceneMusic.sceneName))
            {
                Debug.LogWarning($"[SceneMusicData] Duplicate scene name: {sceneMusic.sceneName}");
                continue;
            }

            musicCache.Add(sceneMusic.sceneName, sceneMusic);
        }

        Debug.Log($"[SceneMusicData] Loaded {musicCache.Count} scene music entries");
    }

    /// <summary>
    /// 씬 이름으로 음악 찾기
    /// </summary>
    public SceneMusic GetMusicForScene(string sceneName)
    {
        if (musicCache == null)
            BuildCache();

        if (musicCache.TryGetValue(sceneName, out var sceneMusic))
            return sceneMusic;

        Debug.LogWarning($"[SceneMusicData] No music found for scene: {sceneName}");
        return null;
    }

    /// <summary>
    /// 등록된 모든 씬 이름 가져오기
    /// </summary>
    /*public string[] GetRegisteredSceneNames()
    {
        if (musicCache == null)
            BuildCache();

        return new List<string>(musicCache.Keys).ToArray();
    }*/
}