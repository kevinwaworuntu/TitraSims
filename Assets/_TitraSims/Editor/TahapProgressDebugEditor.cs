using UnityEditor;
using UnityEngine;

public class TahapProgressDebugEditor : EditorWindow
{
    private GameManager _gmComponent;
    private SerializedObject _so;

    private void OnEnable()  => EditorApplication.update += OnEditorUpdate;
    private void OnDisable() => EditorApplication.update -= OnEditorUpdate;
    private void OnFocus()   => FindGameManager();

    private void OnEditorUpdate()
    {
        if (Application.isPlaying) Repaint();
    }

    [MenuItem("TitraSims/Tahap Progress Debug")]
    public static void OpenWindow()
    {
        var window = GetWindow<TahapProgressDebugEditor>("Tahap Progress Debug");
        window.minSize = new Vector2(340, 420);
        window.FindGameManager();
    }

    private void FindGameManager()
    {
        _gmComponent = FindObjectOfType<GameManager>();
        _so = _gmComponent != null ? new SerializedObject(_gmComponent) : null;
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Tahap Progress Debug", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        if (_gmComponent == null) FindGameManager();

        if (_gmComponent == null)
        {
            EditorGUILayout.HelpBox("No GameManager found in scene.", MessageType.Warning);
            if (GUILayout.Button("Refresh")) FindGameManager();
            return;
        }

        DrawDevOverrideSection();

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space(4);

        EditorGUILayout.LabelField("Runtime Unlock (Play Mode)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Choose which tahapan should be unlocked, per mode.", MessageType.Info);
        EditorGUILayout.Space(4);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use this section.", MessageType.Warning);
            return;
        }

        var gm = GameManager.Instance;
        if (gm == null)
        {
            EditorGUILayout.HelpBox("GameManager not found in scene.", MessageType.Warning);
            return;
        }

        if (!gm.UsePlayerPrefsForProgress)
        {
            EditorGUILayout.HelpBox("Use Player Prefs For Progress is disabled — unlock is controlled by the checkbox dropdowns above. Buttons below won't have any effect.", MessageType.Warning);
        }

        DrawModeSection(gm, GameMode.TBA, "TBA");
        EditorGUILayout.Space(4);
        DrawModeSection(gm, GameMode.Kompleksometri, "Kompleksometri");

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Reset ALL Progress", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Reset All",
                    "Lock every tahapan for both TBA and Kompleksometri?", "Reset", "Cancel"))
            {
                gm.ResetAllProgress();
                Debug.Log("[TahapDebug] All progress reset.");
            }
        }
    }

    // ── Development Build Override ──────────────────────────────────────────────

    private void DrawDevOverrideSection()
    {
        _so.Update();

        SerializedProperty useProp  = _so.FindProperty("usePlayerPrefsForProgress");
        SerializedProperty tbaProp  = _so.FindProperty("devUnlockedMaskTBA");
        SerializedProperty kompProp = _so.FindProperty("devUnlockedMaskKomp");

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Development Build Override", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Disable to ignore PlayerPrefs — tick exactly which tahapan are unlocked below. Use for development/test builds.", MessageType.Info);

        EditorGUILayout.PropertyField(useProp, new GUIContent("Use Player Prefs For Progress"));

        EditorGUILayout.Space(2);
        using (new EditorGUI.DisabledScope(useProp.boolValue))
        {
            DrawDevUnlockMask(GameMode.TBA, "TBA Dev Unlock", tbaProp);
            DrawDevUnlockMask(GameMode.Kompleksometri, "Komp Dev Unlock", kompProp);
        }

        EditorGUILayout.EndVertical();

        if (_so.ApplyModifiedProperties())
        {
            EditorUtility.SetDirty(_gmComponent);
        }
    }

    private void DrawDevUnlockMask(GameMode mode, string label, SerializedProperty prop)
    {
        GameObject[] mapping = _gmComponent.GetMarkerMapping(mode);
        int count = mapping?.Length ?? 0;

        if (count == 0)
        {
            EditorGUILayout.LabelField(label, "No tahapan configured.");
            return;
        }

        var options = new string[count];
        for (int i = 0; i < count; i++) options[i] = $"{i}: {DisplayName(mapping, i)}";

        prop.intValue = EditorGUILayout.MaskField(label, prop.intValue, options);
    }

    // ── Runtime Unlock ───────────────────────────────────────────────────────────

    private void DrawModeSection(GameManager gm, GameMode mode, string label)
    {
        GameObject[] mapping = gm.GetMarkerMapping(mode);
        int count = mapping?.Length ?? 0;

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);

        if (count == 0)
        {
            EditorGUILayout.LabelField("No tahapan configured for this mode.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        int lastCompleted = gm.GetLastCompletedTahapIndex(mode);
        string unlockedStatus = lastCompleted < 0
            ? "None unlocked (only tahapan 0 is accessible)"
            : $"Unlocked through tahapan {lastCompleted}: {DisplayName(mapping, lastCompleted)}";
        EditorGUILayout.LabelField("Status", unlockedStatus);

        EditorGUILayout.Space(2);

        string[] options = new string[count];
        for (int i = 0; i < count; i++) options[i] = $"{i}: {DisplayName(mapping, i)}";

        string selectedKey = SelectedIndexKey(mode);
        int selected = Mathf.Clamp(SessionState.GetInt(selectedKey, 0), 0, count - 1);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Unlock up to", GUILayout.Width(80));
        selected = EditorGUILayout.Popup(selected, options);
        SessionState.SetInt(selectedKey, selected);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);
        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("Unlock", GUILayout.Height(24)))
        {
            gm.SetLastCompletedTahapIndex(mode, selected);
            Debug.Log($"[TahapDebug] {label} unlocked through tahapan {selected}.");
        }

        GUI.enabled = lastCompleted >= 0;
        if (GUILayout.Button("Lock All", GUILayout.Height(24)))
        {
            gm.SetLastCompletedTahapIndex(mode, -1);
            Debug.Log($"[TahapDebug] {label} progress locked.");
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    private static string DisplayName(GameObject[] mapping, int index)
    {
        if (index < 0 || index >= mapping.Length || mapping[index] == null) return "(empty)";
        return mapping[index].name;
    }

    private static string SelectedIndexKey(GameMode mode) =>
        "TahapProgressDebugEditor_Selected_" + mode;
}
