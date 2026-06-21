using UnityEditor;
using UnityEngine;

public class TahapProgressDebugEditor : EditorWindow
{
    private const string KEY_TBA  = "LastCompletedTahapTBA";
    private const string KEY_KOMP = "LastCompletedTahapKomp";

    private int _tbaValue;
    private int _kompValue;

    [MenuItem("TitraSims/Tahap Progress Debug")]
    public static void OpenWindow()
    {
        var window = GetWindow<TahapProgressDebugEditor>("Tahap Progress Debug");
        window.minSize = new Vector2(320, 200);
        window.RefreshValues();
    }

    private void OnFocus() => RefreshValues();

    private void RefreshValues()
    {
        _tbaValue  = PlayerPrefs.GetInt(KEY_TBA,  -1);
        _kompValue = PlayerPrefs.GetInt(KEY_KOMP, -1);
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Tahap Progress (PlayerPrefs)", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("-1 = no tahap completed yet.", MessageType.Info);
        EditorGUILayout.Space(4);

        DrawKeyRow("TBA", KEY_TBA, ref _tbaValue);
        EditorGUILayout.Space(4);
        DrawKeyRow("Kompleksometri", KEY_KOMP, ref _kompValue);

        EditorGUILayout.Space(12);
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);

        if (GUILayout.Button("Delete ALL Keys", GUILayout.Height(28)))
        {
            if (EditorUtility.DisplayDialog("Delete All",
                    "Delete both LastCompletedTahapTBA and LastCompletedTahapKomp?", "Delete", "Cancel"))
            {
                PlayerPrefs.DeleteKey(KEY_TBA);
                PlayerPrefs.DeleteKey(KEY_KOMP);
                PlayerPrefs.Save();
                RefreshValues();
                Debug.Log("[TahapDebug] All progress keys deleted.");
            }
        }
    }

    private void DrawKeyRow(string label, string key, ref int value)
    {
        bool exists = PlayerPrefs.HasKey(key);

        EditorGUILayout.BeginVertical(EditorStyles.helpBox);

        EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Key", key, EditorStyles.miniLabel);

        string statusText = exists ? $"Saved: {PlayerPrefs.GetInt(key, -1)}" : "Not set";
        EditorGUILayout.LabelField("Current", statusText);

        EditorGUILayout.Space(2);

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Set Value", GUILayout.Width(70));
        value = EditorGUILayout.IntField(value, GUILayout.Width(60));

        if (GUILayout.Button("Save", GUILayout.Width(60)))
        {
            PlayerPrefs.SetInt(key, value);
            PlayerPrefs.Save();
            Debug.Log($"[TahapDebug] {key} = {value}");
            Repaint();
        }

        GUI.enabled = exists;
        if (GUILayout.Button("Delete Key", GUILayout.Width(80)))
        {
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
            value = -1;
            Debug.Log($"[TahapDebug] {key} deleted.");
            Repaint();
        }
        GUI.enabled = true;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
    }
}