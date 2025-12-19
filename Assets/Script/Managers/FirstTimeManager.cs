using UnityEngine;

/// <summary>
/// 최초 실행 여부 관리 + 게임 데이터 초기화 유틸리티 클래스
/// PlayerPrefs를 사용하여 앱 설치 후 첫 실행 시 필요한 모든 초기화 처리
/// </summary>
public static class FirstTimeManager
{
    // ========== PlayerPrefs Keys ==========
    private const string FIRST_TIME_KEY = "IsFirstTime";

    // 재화 키 (SkinManager, IllustrationManager와 동일)
    private const string SKIN_CURRENCY_KEY = "SkinCurrency";
    private const string ILLUSTRATION_CURRENCY_KEY = "IllustrationCurrency";

    // 스킨 해금 상태 키 접두사
    private const string SKIN_LOCKED_PREFIX = "Skin_";
    private const string SKIN_LOCKED_SUFFIX = "_Locked";

    // 일러스트 해금 상태 키 접두사
    private const string ILLUSTRATION_UNLOCKED_PREFIX = "Illustration_";
    private const string ILLUSTRATION_UNLOCKED_SUFFIX = "_Unlocked";

    // 현재 선택된 스킨
    private const string CURRENT_SKIN_ID_KEY = "CurrentSkinId";

    // ========== 초기 재화 설정 ==========
    [System.Serializable]
    public class InitialCurrencySettings
    {
        public int skinCurrency = 1000;
        public int illustrationCurrency = 500;
    }

    /// <summary>
    /// 최초 실행 여부 확인
    /// </summary>
    /// <returns>true: 최초 실행, false: 재실행</returns>
    public static bool IsFirstTime()
    {
        // PlayerPrefs에 키가 없으면 true (최초 실행)
        // 키가 있으면 저장된 값 반환
        return !PlayerPrefs.HasKey(FIRST_TIME_KEY) || PlayerPrefs.GetInt(FIRST_TIME_KEY, 1) == 1;
    }

    /// <summary>
    /// 최초 실행 완료 표시 (다이얼로그 씬이 끝난 후 호출)
    /// </summary>
    public static void MarkAsCompleted()
    {
        PlayerPrefs.SetInt(FIRST_TIME_KEY, 0); // 0 = 완료됨
        PlayerPrefs.Save();

        Debug.Log("[FirstTimeManager] 최초 실행 완료 표시됨 (다음 실행부터는 다이얼로그 건너뜀)");
    }

    /// <summary>
    /// 최초 실행 시 게임 데이터 초기화
    /// - 재화 지급
    /// - 기본 스킨 해금
    /// - 기본 일러스트 해금 (선택사항)
    /// </summary>
    /// <param name="skinCurrency">초기 스킨 재화</param>
    /// <param name="illustrationCurrency">초기 일러스트 재화</param>
    /// <param name="defaultSkinId">기본 스킨 ID (해금됨)</param>
    public static void InitializeGameData(
        int skinCurrency = 1000,
        int illustrationCurrency = 500,
        string defaultSkinId = "default")
    {
        Debug.Log("[FirstTimeManager] 게임 데이터 초기화 시작...");

        // 1. 재화 초기화
        InitializeCurrency(skinCurrency, illustrationCurrency);

        // 2. 기본 스킨 설정
        InitializeDefaultSkin(defaultSkinId);

        // 3. 초기화 완료
        PlayerPrefs.Save();

        Debug.Log("[FirstTimeManager] 게임 데이터 초기화 완료!");
    }

    /// <summary>
    /// 재화 초기화
    /// </summary>
    private static void InitializeCurrency(int skinCurrency, int illustrationCurrency)
    {
        // 스킨 재화
        PlayerPrefs.SetInt(SKIN_CURRENCY_KEY, skinCurrency);
        Debug.Log($"[FirstTimeManager] 스킨 코인 초기화: {skinCurrency}");

        // 일러스트 재화
        PlayerPrefs.SetInt(ILLUSTRATION_CURRENCY_KEY, illustrationCurrency);
        Debug.Log($"[FirstTimeManager] 일러스트 재화 초기화: {illustrationCurrency}");
    }

    /// <summary>
    /// 기본 스킨 설정 (해금 + 선택)
    /// </summary>
    private static void InitializeDefaultSkin(string defaultSkinId)
    {
        // 기본 스킨 해금
        string skinKey = $"{SKIN_LOCKED_PREFIX}{defaultSkinId}{SKIN_LOCKED_SUFFIX}";
        PlayerPrefs.SetInt(skinKey, 0); // 0 = 해금됨
        Debug.Log($"[FirstTimeManager] 기본 스킨 해금: {defaultSkinId}");

        // 기본 스킨 선택
        PlayerPrefs.SetString(CURRENT_SKIN_ID_KEY, defaultSkinId);
        Debug.Log($"[FirstTimeManager] 기본 스킨 선택: {defaultSkinId}");
    }

    /// <summary>
    /// 특정 스킨 해금 (초기화용)
    /// </summary>
    /// <param name="skinId">해금할 스킨 ID</param>
    public static void UnlockSkinForInit(string skinId)
    {
        string skinKey = $"{SKIN_LOCKED_PREFIX}{skinId}{SKIN_LOCKED_SUFFIX}";
        PlayerPrefs.SetInt(skinKey, 0); // 0 = 해금됨
        Debug.Log($"[FirstTimeManager] 스킨 해금 (초기화): {skinId}");
    }

    /// <summary>
    /// 특정 일러스트 해금 (초기화용)
    /// </summary>
    /// <param name="illustrationId">해금할 일러스트 ID</param>
    public static void UnlockIllustrationForInit(string illustrationId)
    {
        string illustKey = $"{ILLUSTRATION_UNLOCKED_PREFIX}{illustrationId}{ILLUSTRATION_UNLOCKED_SUFFIX}";
        PlayerPrefs.SetInt(illustKey, 1); // 1 = 해금됨
        Debug.Log($"[FirstTimeManager] 일러스트 해금 (초기화): {illustrationId}");
    }

    /// <summary>
    /// 재화 추가 (테스트/보상용)
    /// </summary>
    /// <param name="skinAmount">스킨 재화 추가량</param>
    /// <param name="illustrationAmount">일러스트 재화 추가량</param>
    public static void AddCurrency(int skinAmount = 0, int illustrationAmount = 0)
    {
        if (skinAmount > 0)
        {
            int current = PlayerPrefs.GetInt(SKIN_CURRENCY_KEY, 0);
            PlayerPrefs.SetInt(SKIN_CURRENCY_KEY, current + skinAmount);
            Debug.Log($"[FirstTimeManager] 스킨 코인 추가: +{skinAmount} → 총 {current + skinAmount}");
        }

        if (illustrationAmount > 0)
        {
            int current = PlayerPrefs.GetInt(ILLUSTRATION_CURRENCY_KEY, 0);
            PlayerPrefs.SetInt(ILLUSTRATION_CURRENCY_KEY, current + illustrationAmount);
            Debug.Log($"[FirstTimeManager] 일러스트 재화 추가: +{illustrationAmount} → 총 {current + illustrationAmount}");
        }

        PlayerPrefs.Save();
    }

    /// <summary>
    /// 모든 게임 데이터 초기화 (리셋)
    /// </summary>
    public static void ResetAllGameData()
    {
        Debug.Log("[FirstTimeManager] 모든 게임 데이터 초기화 중...");

        // 1. 최초 실행 플래그 초기화
        PlayerPrefs.DeleteKey(FIRST_TIME_KEY);

        // 2. 재화 초기화
        PlayerPrefs.DeleteKey(SKIN_CURRENCY_KEY);
        PlayerPrefs.DeleteKey(ILLUSTRATION_CURRENCY_KEY);

        // 3. 현재 스킨 초기화
        PlayerPrefs.DeleteKey(CURRENT_SKIN_ID_KEY);

        // 4. 모든 스킨/일러스트 해금 상태 초기화
        // (개별 키는 Manager에서 관리하므로 여기서는 생략)
        // 완전한 초기화를 원하면 PlayerPrefs.DeleteAll() 사용

        PlayerPrefs.Save();

        Debug.Log("[FirstTimeManager] 모든 게임 데이터 초기화 완료! (다음 실행 시 최초 실행으로 인식됨)");
    }

    /// <summary>
    /// 최초 실행 플래그만 초기화 (테스트용)
    /// 재화와 해금 상태는 유지
    /// </summary>
    public static void ResetFirstTimeFlag()
    {
        PlayerPrefs.DeleteKey(FIRST_TIME_KEY);
        PlayerPrefs.Save();

        Debug.Log("[FirstTimeManager] 최초 실행 플래그 초기화됨 (재화/해금 상태는 유지)");
    }

    /// <summary>
    /// PlayerPrefs 완전 초기화 (주의!)
    /// 모든 저장 데이터 삭제
    /// </summary>
    public static void DeleteAllPlayerPrefs()
    {
        Debug.LogWarning("[FirstTimeManager] ⚠️ 모든 PlayerPrefs 삭제! (복구 불가능)");
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 현재 상태 확인 (디버깅용)
    /// </summary>
    public static void CheckStatus()
    {
        Debug.Log("========================================");
        Debug.Log("[FirstTimeManager] 현재 상태:");
        Debug.Log($"  - 최초 실행 여부: {IsFirstTime()}");
        Debug.Log($"  - PlayerPrefs 키 존재: {PlayerPrefs.HasKey(FIRST_TIME_KEY)}");

        if (PlayerPrefs.HasKey(FIRST_TIME_KEY))
        {
            Debug.Log($"  - 저장된 값: {PlayerPrefs.GetInt(FIRST_TIME_KEY)}");
        }

        // 재화 상태
        Debug.Log("----------------------------------------");
        Debug.Log("[재화 상태]");
        Debug.Log($"  - 스킨 코인: {PlayerPrefs.GetInt(SKIN_CURRENCY_KEY, 0)}");
        Debug.Log($"  - 일러스트 재화: {PlayerPrefs.GetInt(ILLUSTRATION_CURRENCY_KEY, 0)}");

        // 현재 스킨
        Debug.Log("----------------------------------------");
        Debug.Log("[현재 스킨]");
        if (PlayerPrefs.HasKey(CURRENT_SKIN_ID_KEY))
        {
            Debug.Log($"  - 선택된 스킨 ID: {PlayerPrefs.GetString(CURRENT_SKIN_ID_KEY)}");
        }
        else
        {
            Debug.Log($"  - 선택된 스킨 없음");
        }

        Debug.Log("========================================");
    }

    /// <summary>
    /// 게임 시작 시 자동 초기화 (Title Scene에서 호출)
    /// </summary>
    public static void AutoInitializeOnStart()
    {
        if (IsFirstTime())
        {
            Debug.Log("[FirstTimeManager] 최초 실행 감지! 게임 데이터 초기화 시작...");

            // 기본 값으로 초기화
            InitializeGameData(
                skinCurrency: 1000,
                illustrationCurrency: 500,
                defaultSkinId: "default"
            );

            // 추가 스킨 해금 (원하는 경우)
            // UnlockSkinForInit("skin_starter");

            // 추가 일러스트 해금 (원하는 경우)
            // UnlockIllustrationForInit("illust_welcome");
        }
        else
        {
            Debug.Log("[FirstTimeManager] 재실행 감지됨. 저장된 데이터 사용.");
        }
    }
}