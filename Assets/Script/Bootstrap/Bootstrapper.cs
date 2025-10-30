using UnityEngine;

public class Bootstrapper : MonoBehaviour
{
    [Header("Service Prefabs")]
    public AudioManager audioManagerPrefab; // Inspector에서 AudioManager 프리팹을 연결합니다.
    public SceneService sceneServicePrefab;
    public SceneTransitionUI transitionUIPrefab;

    // 이 플래그는 게임 전체에서 AudioManager가 한 번만 초기화되도록 보장합니다.
    private static bool _isAudioManagerInitialized = false;
    private static bool _isSceneServiceInitialized = false;

    void Awake()
    {
        // 1. Bootstrapper_GO 자체는 DontDestroyOnLoad로 씬 전환에도 유지됩니다.
        // 하지만 Bootstrapper 스크립트가 여러 번 생성되는 것을 막기 위한 별도 로직은 필요 없습니다.
        // 왜냐하면 우리는 AudioManager 자체에 _isAudioManagerInitialized 플래그를 두어
        // AudioManager가 중복 생성되지 않도록 할 것이기 때문입니다.
        DontDestroyOnLoad(this.gameObject);



        // 2. AudioManager 인스턴스 생성 및 DontDestroyOnLoad 적용
        // AudioManager는 자기 자신의 Awake에서 ServiceLocator에 등록하고,
        // _isInitialized 플래그로 중복 생성을 방지합니다.
        if (!_isAudioManagerInitialized) // AudioManager가 아직 초기화되지 않았다면
        {
            if (audioManagerPrefab != null)
            {
                AudioManager audioManagerInstance = Instantiate(audioManagerPrefab);
                DontDestroyOnLoad(audioManagerInstance.gameObject); // 생성된 AudioManager도 유지
                _isAudioManagerInitialized = true; // AudioManager 초기화 완료 플래그 설정
                Debug.Log("[Bootstrapper] AudioManager 인스턴스 생성 및 DontDestroyOnLoad 적용 완료.");
            }
            else
            {
                Debug.LogError("[Bootstrapper] AudioManager Prefab이 연결되지 않았습니다! 오디오 시스템 초기화 실패.");
            }
        }

        else
        {
            // 이미 AudioManager가 초기화되어 있다면, 이 Bootstrapper는 할 일이 없으므로 스스로를 파괴합니다.
            // (만약 다른 초기화 로직이 있다면 Destroy를 하지 않고 유지할 수 있습니다.)
            Destroy(gameObject);
        }

        // ========== SceneTransitionUI 초기화 (새로 추가!) ==========
        var existingTransitionUI = FindObjectOfType<SceneTransitionUI>();
        if (existingTransitionUI == null && transitionUIPrefab != null)
        {
            SceneTransitionUI transitionUIInstance = Instantiate(transitionUIPrefab);
            // SceneTransitionUI는 자체적으로 DontDestroyOnLoad 처리
            Debug.Log("[Bootstrapper] SceneTransitionUI 생성 완료");
        }

        // ========== SceneService 초기화 (새로 추가!) ==========
        if (!_isSceneServiceInitialized)
        {
            if (sceneServicePrefab != null)
            {
                SceneService sceneServiceInstance = Instantiate(sceneServicePrefab);
                DontDestroyOnLoad(sceneServiceInstance.gameObject);
                _isSceneServiceInitialized = true;
                Debug.Log("[Bootstrapper] SceneService 인스턴스 생성 및 DontDestroyOnLoad 적용 완료.");

                // TransitionUI 자동 연결
                var transitionUI = FindObjectOfType<SceneTransitionUI>();
                if (sceneServiceInstance.transitionUI == null && transitionUI != null)
                {
                    sceneServiceInstance.transitionUI = transitionUI;
                    Debug.Log("[Bootstrapper] SceneService에 TransitionUI 자동 연결 완료");
                }
            }
            else
            {
                Debug.LogError("[Bootstrapper] SceneService Prefab이 연결되지 않았습니다!");
            }
        }

        Debug.Log("[Bootstrapper] ========================================");
        Debug.Log("[Bootstrapper] 게임 초기화 완료!");
        Debug.Log("[Bootstrapper] ========================================");
    }
}
