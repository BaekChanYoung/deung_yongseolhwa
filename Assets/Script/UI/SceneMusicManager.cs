using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환 시 자동으로 배경음악 변경
/// </summary>
public class SceneMusicManager : MonoBehaviour
{
    [Header("References")]
    [Tooltip("씬별 음악 데이터 (ScriptableObject)")]
    public SceneMusicData sceneMusicData;

    [Header("Settings")]
    [Tooltip("크로스 페이드 시간 (초)")]
    [Range(0f, 5f)]
    public float crossFadeDuration = 1.5f;

    [Tooltip("음악이 없는 씬에서 페이드 아웃 시간")]
    [Range(0f, 3f)]
    public float fadeOutDuration = 1f;

    private IAudioService audioService;
    private string currentSceneName;

    void Awake()
    {
        // 중복 방지
        var existing = FindObjectsOfType<SceneMusicManager>();
        if (existing.Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);

        Debug.Log("[SceneMusicManager] 초기화 완료");
    }

    void Start()
    {
        // AudioService 가져오기
        audioService = ServiceLocator.Resolve<IAudioService>();

        if (audioService == null)
        {
            Debug.LogError("[SceneMusicManager] IAudioService를 찾을 수 없습니다! AudioManager가 초기화되었는지 확인하세요.");
            enabled = false; // 스크립트 비활성화
            return;
        }

        if (sceneMusicData == null)
        {
            Debug.LogError("[SceneMusicManager] SceneMusicData가 연결되지 않았습니다!");
            enabled = false;
            return;
        }

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 현재 씬 음악 재생
        PlayMusicForCurrentScene();
    }

    void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[SceneMusicManager] 씬 로드됨: {scene.name}");
        PlayMusicForCurrentScene();
    }

    void PlayMusicForCurrentScene()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        // 같은 씬이면 무시 (중복 재생 방지)
        if (sceneName == currentSceneName)
        {
            Debug.Log($"[SceneMusicManager] 같은 씬이므로 음악 유지: {sceneName}");
            return;
        }

        if (sceneName == SceneNames.LOADING)
        {
            Debug.Log($"[SceneMusicManager] LoadingScene은 음악 유지");
            // currentSceneName 업데이트 안 함 (이전 씬 유지)
            return; // 음악 그대로 유지!
        }

        currentSceneName = sceneName;

        // 씬 음악 데이터 찾기
        var sceneMusic = sceneMusicData.GetMusicForScene(sceneName);

        if (sceneMusic == null || sceneMusic.musicClip == null)
        {
            // 음악이 없는 씬 (예: LoadingScene)
            Debug.Log($"[SceneMusicManager] '{sceneName}' 씬은 배경음악이 없습니다. 음악 정지.");
            StopCurrentMusic();
            return;
        }

        // 같은 음악이 이미 재생 중이면 무시
        if (audioService.IsMusicPlaying(sceneMusic.musicClip))
        {
            Debug.Log($"[SceneMusicManager] 이미 재생 중: {sceneMusic.musicClip.name}");
            return;
        }

        // 크로스 페이드로 음악 전환
        Debug.Log($"[SceneMusicManager] 음악 전환: {sceneName} → {sceneMusic.musicClip.name}");
        audioService.CrossFadeMusic(sceneMusic.musicClip, crossFadeDuration);
    }

    void StopCurrentMusic()
    {
        if (audioService != null)
        {
            audioService.StopMusic();
        }
    }
}