using UnityEngine;
using UnityEditor;
using System.Reflection;

[CustomEditor(typeof(AudioManager))]
public class AudioManagerEditor : Editor
{
    // 폴드아웃(접기/펼치기) 상태 저장
    private bool showTestTools = true;
    private bool showDebugInfo = false;

    public override void OnInspectorGUI()
    {
        // 기본 Inspector
        DrawDefaultInspector();

        AudioManager manager = (AudioManager)target;

        // ========== 구분선 ==========
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();

        // ========== 테스트 도구 섹션 ==========
        showTestTools = EditorGUILayout.Foldout(showTestTools, "🛠️ 테스트 도구", true);

        if (showTestTools)
        {
            EditorGUI.indentLevel++;

            // 현재 상태 표시
            EditorGUILayout.HelpBox(
                Application.isPlaying ? "▶️ Play 모드 실행 중" : "⏸️ Edit 모드",
                Application.isPlaying ? MessageType.Info : MessageType.None
            );

            EditorGUILayout.Space(5);

            // 버튼 1: PlayerPrefs 값 확인
            if (GUILayout.Button("📋 PlayerPrefs 값 확인", GUILayout.Height(25)))
            {
                CheckPlayerPrefs();
            }

            EditorGUILayout.Space(3);

            // 버튼 2: PlayerPrefs 초기화
            GUI.backgroundColor = new Color(1f, 0.5f, 0.5f); // 연한 빨간색
            if (GUILayout.Button("🗑️ PlayerPrefs 초기화 (주의!)", GUILayout.Height(30)))
            {
                if (EditorUtility.DisplayDialog(
                    "⚠️ 경고",
                    "모든 저장된 오디오 설정을 삭제하시겠습니까?\n\n" +
                    "• MasterVol\n• MusicVol\n• SfxVol\n\n" +
                    "이 작업은 되돌릴 수 없습니다!",
                    "삭제",
                    "취소"))
                {
                    ResetPlayerPrefs(manager);
                }
            }
            GUI.backgroundColor = Color.white;

            EditorGUILayout.Space(3);

            // 버튼 3: 테스트 사운드 재생 (Play 모드에서만)
            GUI.enabled = Application.isPlaying;
            if (GUILayout.Button("🔊 테스트 사운드 재생", GUILayout.Height(25)))
            {
                PlayTestSound(manager);
            }
            GUI.enabled = true;

            if (!Application.isPlaying)
            {
                EditorGUILayout.HelpBox("테스트 사운드는 Play 모드에서만 재생됩니다.", MessageType.Info);
            }

            EditorGUI.indentLevel--;
        }

        // ========== 디버그 정보 섹션 ==========
        EditorGUILayout.Space();
        showDebugInfo = EditorGUILayout.Foldout(showDebugInfo, "🐛 디버그 정보", true);

        if (showDebugInfo)
        {
            EditorGUI.indentLevel++;

            // Settings 연결 상태
            if (manager.settings != null)
            {
                EditorGUILayout.LabelField("Settings", "✓ 연결됨", EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField("Default Master", manager.settings.defaultMaster.ToString("F2"));
                EditorGUILayout.LabelField("Default Music", manager.settings.defaultMusic.ToString("F2"));
                EditorGUILayout.LabelField("Default SFX", manager.settings.defaultSfx.ToString("F2"));
                EditorGUI.indentLevel--;
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ AudioSettings가 연결되지 않았습니다!", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // AudioMixer 연결 상태
            if (manager.mainMixer != null)
            {
                EditorGUILayout.LabelField("Audio Mixer", "✓ 연결됨", EditorStyles.boldLabel);
            }
            else
            {
                EditorGUILayout.HelpBox("⚠️ AudioMixer가 연결되지 않았습니다!", MessageType.Warning);
            }

            EditorGUILayout.Space(5);

            // AudioSource 연결 상태
            int connectedSources = 0;
            if (manager.MasterSource != null) connectedSources++;
            if (manager.musicSource != null) connectedSources++;
            if (manager.sfxSource != null) connectedSources++;

            EditorGUILayout.LabelField("Audio Sources", $"{connectedSources}/3 연결됨", EditorStyles.boldLabel);

            if (connectedSources < 3)
            {
                EditorGUILayout.HelpBox("일부 AudioSource가 연결되지 않았습니다!", MessageType.Warning);
            }

            EditorGUI.indentLevel--;
        }

        // ========== 하단 정보 ==========
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.LabelField("AudioManager Custom Editor v1.0", EditorStyles.miniLabel);
    }

    // PlayerPrefs 확인
    void CheckPlayerPrefs()
    {
        Debug.Log("========================================");
        Debug.Log("📋 [PlayerPrefs] 저장된 값 확인");
        Debug.Log("========================================");

        bool hasAnyData = false;

        if (PlayerPrefs.HasKey("MasterVol"))
        {
            float val = PlayerPrefs.GetFloat("MasterVol");
            Debug.Log($"✓ MasterVol: {val:F3} ({val * 100:F1}%)");
            hasAnyData = true;
        }
        else
        {
            Debug.Log("✗ MasterVol: 저장된 값 없음");
        }

        if (PlayerPrefs.HasKey("MusicVol"))
        {
            float val = PlayerPrefs.GetFloat("MusicVol");
            Debug.Log($"✓ MusicVol: {val:F3} ({val * 100:F1}%)");
            hasAnyData = true;
        }
        else
        {
            Debug.Log("✗ MusicVol: 저장된 값 없음");
        }

        if (PlayerPrefs.HasKey("SfxVol"))
        {
            float val = PlayerPrefs.GetFloat("SfxVol");
            Debug.Log($"✓ SfxVol: {val:F3} ({val * 100:F1}%)");
            hasAnyData = true;
        }
        else
        {
            Debug.Log("✗ SfxVol: 저장된 값 없음");
        }

        if (!hasAnyData)
        {
            Debug.Log("ℹ️ 저장된 데이터가 없습니다. 기본값이 사용됩니다.");
        }

        Debug.Log("========================================");
    }

    // PlayerPrefs 초기화
    void ResetPlayerPrefs(AudioManager manager)
    {
        Debug.Log("========================================");
        Debug.Log("🗑️ [PlayerPrefs] 초기화 시작...");

        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        Debug.Log("✓ [PlayerPrefs] 모든 데이터 삭제 완료!");

        if (Application.isPlaying)
        {
            Debug.Log("🔄 [AudioManager] 기본값으로 리셋 중...");

            if (manager.settings != null)
            {
                // Reflection으로 private 메서드 호출
                InvokePrivateMethod(manager, "SetMasterVolume_Internal", manager.settings.defaultMaster);
                InvokePrivateMethod(manager, "SetMusicVolume_Internal", manager.settings.defaultMusic);
                InvokePrivateMethod(manager, "SetSfxVolume_Internal", manager.settings.defaultSfx);

                Debug.Log($"✓ Master: {manager.settings.defaultMaster:F2}");
                Debug.Log($"✓ Music: {manager.settings.defaultMusic:F2}");
                Debug.Log($"✓ SFX: {manager.settings.defaultSfx:F2}");
            }
        }
        else
        {
            Debug.Log("ℹ️ Play 모드에서 다시 시작하면 기본값이 적용됩니다.");
        }

        Debug.Log("========================================");
    }

    // 테스트 사운드 재생
    void PlayTestSound(AudioManager manager)
    {
        if (manager.sfxSource != null && manager.sfxSource.clip != null)
        {
            manager.PlaySfx(manager.sfxSource.clip, 1f);
            Debug.Log("🔊 테스트 사운드 재생!");
        }
        else
        {
            Debug.LogWarning("⚠️ SFX AudioSource에 AudioClip이 없습니다!");
        }
    }

    // Reflection을 사용해 private 메서드 호출
    void InvokePrivateMethod(object obj, string methodName, params object[] parameters)
    {
        var method = obj.GetType().GetMethod(
            methodName,
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        if (method != null)
        {
            method.Invoke(obj, parameters);
        }
        else
        {
            Debug.LogWarning($"메서드를 찾을 수 없음: {methodName}");
        }
    }
}