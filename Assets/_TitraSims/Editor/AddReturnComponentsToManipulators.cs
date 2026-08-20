using System;
using System.Collections.Generic;
using System.Linq;
using InteractionLogic;
using UnityEditor;
using UnityEngine;

namespace TitraSims.EditorTools
{
    /// <summary>
    /// One-shot maintenance tool: ensures every GameObject in the project that has an
    /// <see cref="ObjectManipulator"/> also has the return-to-origin components listed in
    /// <see cref="ReturnComponents"/>.
    ///
    /// Done through PrefabUtility rather than by editing the .prefab YAML directly, because the
    /// manipulators live in three structurally different places — plain components, components
    /// added to nested prefab instances (which need an m_AddedComponents override), and stripped
    /// references into nested prefabs (which must NOT be touched). Unity resolves all three.
    ///
    /// Base prefabs are processed before Stage_* prefabs so nested instances inherit the component
    /// from their source instead of each Stage recording a redundant added-component override.
    ///
    /// Idempotent: a component is only added where it is missing, so re-running is harmless.
    /// </summary>
    public static class AddReturnComponentsToManipulators
    {
        private const string MenuPath = "TitraSims/Interaction/Add Return Components To All Manipulators";

        /// <summary>Components to guarantee alongside every ObjectManipulator.</summary>
        private static readonly Type[] ReturnComponents =
        {
            typeof(RotationReturnInteractable),
            typeof(ScaleReturnInteractable),
        };

        [MenuItem(MenuPath)]
        private static void Run() => Execute(dryRun: false);

        [MenuItem(MenuPath + " (Preview Only)")]
        private static void Preview() => Execute(dryRun: true);

        private static void Execute(bool dryRun)
        {
            List<string> paths = AssetDatabase.FindAssets("t:Prefab")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(p => p.StartsWith("Assets/"))
                .Where(p => !p.Contains("/_Recovery/"))
                // Sources before consumers: anything outside Stage/ first.
                .OrderBy(p => p.Contains("/Stage/") ? 1 : 0)
                .ThenBy(p => p)
                .ToList();

            var addedPerType = ReturnComponents.ToDictionary(t => t, t => 0);
            int prefabsChanged = 0, alreadyPresent = 0, failed = 0;
            var log = new List<string>();

            try
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    string path = paths[i];
                    if (EditorUtility.DisplayCancelableProgressBar(
                            dryRun ? "Preview Return Components" : "Add Return Components",
                            path, (float)i / Mathf.Max(paths.Count, 1)))
                        break;

                    // Cheap read-only pre-check, so the expensive LoadPrefabContents only runs
                    // on prefabs that actually need an edit.
                    var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                    if (asset == null) continue;

                    var manipulators = asset.GetComponentsInChildren<ObjectManipulator>(true);
                    if (manipulators.Length == 0) continue;

                    var missing = new Dictionary<Type, int>();
                    foreach (var t in ReturnComponents)
                    {
                        int n = manipulators.Count(m => m.GetComponent(t) == null);
                        alreadyPresent += manipulators.Length - n;
                        if (n > 0) missing[t] = n;
                    }
                    if (missing.Count == 0) continue;

                    log.Add($"  {path}  ->  " +
                            string.Join(", ", missing.Select(kv => $"+{kv.Value} {kv.Key.Name}")));

                    foreach (var kv in missing) addedPerType[kv.Key] += kv.Value;
                    prefabsChanged++;

                    if (dryRun) continue;

                    GameObject root;
                    try   { root = PrefabUtility.LoadPrefabContents(path); }
                    catch { failed++; continue; }
                    if (root == null) { failed++; continue; }

                    try
                    {
                        foreach (var m in root.GetComponentsInChildren<ObjectManipulator>(true))
                        foreach (var t in ReturnComponents)
                        {
                            if (m.GetComponent(t) == null) m.gameObject.AddComponent(t);
                        }

                        PrefabUtility.SaveAsPrefabAsset(root, path);
                    }
                    finally
                    {
                        PrefabUtility.UnloadPrefabContents(root);
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (!dryRun)
                {
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            }

            string totals = string.Join(", ", addedPerType.Select(kv => $"{kv.Value} {kv.Key.Name}"));
            Debug.Log($"[ReturnComponents] {(dryRun ? "PREVIEW — would add" : "added")} {totals} " +
                      $"across {prefabsChanged} prefab(s). Already present: {alreadyPresent}. " +
                      $"Failed to load: {failed}.\n" + string.Join("\n", log));
        }
    }
}
