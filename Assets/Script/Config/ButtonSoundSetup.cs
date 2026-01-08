/*using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 씬의 모든 버튼에 ButtonSoundPlayer를 일괄 추가하는 에디터 도구
/// </summary>
public class ButtonSoundSetup : MonoBehaviour
{
#if UNITY_EDITOR
    [MenuItem("Tools/Audio/Add ButtonSoundPlayer to All Buttons")]
    static void AddButtonSoundPlayerToAllButtons()
    {
        Button[] allButtons = FindObjectsOfType<Button>(true);
        int addedCount = 0;
        int skippedCount = 0;

        foreach (Button button in allButtons)
        {
            if (button.GetComponent<ButtonSoundPlayer>() != null)
            {
                skippedCount++;
                continue;
            }

            ButtonSoundPlayer soundPlayer = button.gameObject.AddComponent<ButtonSoundPlayer>();
            soundPlayer.volume = 0.7f;
            soundPlayer.autoSetup = true;

            // 추가된 컴포넌트 또는 게임오브젝트를 더티 표시
            EditorUtility.SetDirty(soundPlayer);           // 컴포넌트를 더티 표시
            // 또는: EditorUtility.SetDirty(button.gameObject); // 게임오브젝트를 더티 표시

            addedCount++;
        }

        if (addedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
    }

    [MenuItem("Tools/Audio/Remove ButtonSoundPlayer from All Buttons")]
    static void RemoveButtonSoundPlayerFromAllButtons()
    {
        ButtonSoundPlayer[] allSoundPlayers = FindObjectsOfType<ButtonSoundPlayer>(true);

        int removedCount = 0;

        foreach (ButtonSoundPlayer soundPlayer in allSoundPlayers)
        {
            Debug.Log($"[ButtonSoundSetup] 제거됨: {soundPlayer.gameObject.name}");
            DestroyImmediate(soundPlayer);
            removedCount++;
        }

        Debug.Log("========================================");
        Debug.Log($"[ButtonSoundSetup] ButtonSoundPlayer 제거 완료!");
        Debug.Log($"  제거: {removedCount}개");
        Debug.Log("========================================");

        if (removedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
    }

    [MenuItem("Tools/Audio/Set Default Click Sound to All ButtonSoundPlayers")]
    static void SetDefaultClickSoundToAll()
    {
        // 기본 효과음 경로 (프로젝트에 맞게 수정)
        string defaultSfxPath = "Assets/Audio/SFX/UI_Click.wav";

        AudioClip defaultClip = AssetDatabase.LoadAssetAtPath<AudioClip>(defaultSfxPath);

        if (defaultClip == null)
        {
            Debug.LogError($"[ButtonSoundSetup] 기본 효과음을 찾을 수 없습니다: {defaultSfxPath}");
            Debug.LogError("defaultSfxPath를 프로젝트의 실제 경로로 수정하세요!");
            return;
        }

        ButtonSoundPlayer[] allSoundPlayers = FindObjectsOfType<ButtonSoundPlayer>(true);
        int updatedCount = 0;

        foreach (ButtonSoundPlayer soundPlayer in allSoundPlayers)
        {
            soundPlayer.clickSfx = defaultClip;
            EditorUtility.SetDirty(soundPlayer);
            Debug.Log($"[ButtonSoundSetup] 효과음 할당: {soundPlayer.gameObject.name}");
            updatedCount++;
        }

        Debug.Log("========================================");
        Debug.Log($"[ButtonSoundSetup] 기본 효과음 할당 완료!");
        Debug.Log($"  업데이트: {updatedCount}개");
        Debug.Log($"  효과음: {defaultClip.name}");
        Debug.Log("========================================");

        if (updatedCount > 0)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(
                UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene()
            );
        }
    }
#endif
}*/