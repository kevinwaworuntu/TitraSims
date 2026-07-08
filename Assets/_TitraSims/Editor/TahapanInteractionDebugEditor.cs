using Gameplay;
using UnityEditor;
using UnityEngine;

public class TahapanInteractionDebugEditor : EditorWindow
{
    private TahapanInteractionController _controller;
    private GameMode _spawnMode = GameMode.TBA;
    private int _spawnIndex;

    [MenuItem("TitraSims/Interaction Debug")]
    public static void OpenWindow()
    {
        var window = GetWindow<TahapanInteractionDebugEditor>("Interaction Debug");
        window.minSize = new Vector2(320, 340);
    }

    private void OnEnable()  => EditorApplication.update += OnEditorUpdate;
    private void OnDisable() => EditorApplication.update -= OnEditorUpdate;
    private void OnFocus()   => FindController();

    private void OnEditorUpdate()
    {
        if (Application.isPlaying) Repaint();
    }

    private void FindController()
    {
        _controller = FindObjectOfType<TahapanInteractionController>();
    }

    private void OnGUI()
    {
        EditorGUILayout.Space(6);
        EditorGUILayout.LabelField("Tahapan Interaction Debug", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("Spawn and step through tahapan prefabs without AR camera.", MessageType.Info);
        EditorGUILayout.Space(4);

        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to use this window.", MessageType.Warning);
            return;
        }

        DrawSpawnSection();

        EditorGUILayout.Space(8);

        if (_controller != null)
        {
            DrawStatusSection();
            EditorGUILayout.Space(12);
            EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
            EditorGUILayout.Space(4);
            DrawActionButtons();
        }
        else
        {
            EditorGUILayout.HelpBox("No TahapanInteractionController in scene.\nSpawn a prefab above, or refresh.", MessageType.Warning);
            if (GUILayout.Button("Refresh Controller Reference", GUILayout.Height(22)))
                FindController();
        }
    }

    // ── Spawn ─────────────────────────────────────────────────────────────────

    private void DrawSpawnSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField("Spawn Prefab (No AR)", EditorStyles.boldLabel);

        var gm = GameManager.Instance;
        if (gm == null)
        {
            EditorGUILayout.LabelField("GameManager not found in scene.", EditorStyles.miniLabel);
            EditorGUILayout.EndVertical();
            return;
        }

        // Mode selector
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Mode", GUILayout.Width(70));
        _spawnMode = (GameMode)EditorGUILayout.EnumPopup(_spawnMode);
        EditorGUILayout.EndHorizontal();

        int maxIndex = (_spawnMode == GameMode.TBA ? gm.MarkerTBACount : gm.MarkerKompCount) - 1;
        string rangeHint = maxIndex >= 0 ? $"0 – {maxIndex}" : "no prefabs";

        // Index field
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField($"Tahap Index  ({rangeHint})", GUILayout.Width(160));
        _spawnIndex = EditorGUILayout.IntField(_spawnIndex, GUILayout.Width(50));
        _spawnIndex = Mathf.Clamp(_spawnIndex, 0, Mathf.Max(0, maxIndex));
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(2);

        // Currently spawned
        string spawnedLabel = gm.CurrentActiveMarkerObject != null
            ? gm.CurrentActiveMarkerObject.name
            : "None";
        EditorGUILayout.LabelField("Spawned", spawnedLabel, EditorStyles.miniLabel);

        EditorGUILayout.Space(4);
        EditorGUILayout.BeginHorizontal();

        GUI.enabled = maxIndex >= 0;
        if (GUILayout.Button("Spawn & Start ▶", GUILayout.Height(26)))
        {
            if (gm.DebugSpawnTahap(_spawnIndex, _spawnMode))
            {
                FindController();
                _controller?.StartInteraction();
                Debug.Log($"[InteractionDebug] Spawned tahap {_spawnIndex} ({_spawnMode}) and started.");
            }
            else
            {
                Debug.LogWarning($"[InteractionDebug] Failed to spawn tahap {_spawnIndex} ({_spawnMode}). Check GameManager mappings.");
            }
        }
        GUI.enabled = true;

        GUI.enabled = gm.CurrentActiveMarkerObject != null;
        if (GUILayout.Button("Destroy ✕", GUILayout.Height(26)))
        {
            gm.DestroyMarkerObject();
            _controller = null;
            Debug.Log("[InteractionDebug] Destroyed spawned marker object.");
        }
        GUI.enabled = true;

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }

    // ── Status ────────────────────────────────────────────────────────────────

    private void DrawStatusSection()
    {
        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        EditorGUILayout.LabelField(_controller.gameObject.name, EditorStyles.boldLabel);

        int  current = _controller.CurrentInteractionIndex;
        int  total   = _controller.TotalInteractionCount;
        bool isDone  = current >= total;

        EditorGUILayout.LabelField("Index", $"{current} / {total}");

        Color prev = GUI.contentColor;
        GUI.contentColor = _controller.IsCurrentlyPlaying ? Color.green : Color.yellow;
        EditorGUILayout.LabelField("Is Playing", _controller.IsCurrentlyPlaying.ToString());
        GUI.contentColor = prev;

        EditorGUILayout.Space(2);

        if (isDone)
        {
            GUI.contentColor = new Color(1f, 0.6f, 0f);
            EditorGUILayout.LabelField("State", "All interactions complete");
            GUI.contentColor = prev;
        }
        else
        {
            var data = _controller.CurrentInteractionData;
            EditorGUILayout.LabelField("Title", data != null ? data.Title       : "(none)", EditorStyles.miniLabel);
            EditorGUILayout.LabelField("Desc",  data != null ? data.Description : "(none)", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();
    }

    // ── Actions ───────────────────────────────────────────────────────────────

    private void DrawActionButtons()
    {
        bool isDone = _controller.CurrentInteractionIndex >= _controller.TotalInteractionCount;

        GUI.enabled = !isDone;
        if (GUILayout.Button("Force Next ▶", GUILayout.Height(28)))
        {
            _controller.DebugForceNext();
            Debug.Log($"[InteractionDebug] Forced next → index {_controller.CurrentInteractionIndex}");
        }
        GUI.enabled = true;

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Restart Current ↺", GUILayout.Height(28)))
        {
            _controller.RestartInteraction();
            Debug.Log("[InteractionDebug] Restarted current interaction.");
        }

        EditorGUILayout.Space(4);

        if (GUILayout.Button("Refresh Controller Reference", GUILayout.Height(22)))
            FindController();
    }
}