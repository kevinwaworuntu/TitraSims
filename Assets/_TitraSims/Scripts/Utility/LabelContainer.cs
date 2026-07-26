using System;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace _TitraSims.Scripts.Utility
{
    [CreateAssetMenu(fileName = "LabelContainer", menuName = "TitraSims/Label Container")]
    public class LabelContainer : ScriptableObject
    {
        private const string AssetPath = "Assets/_TitraSims/Data/Config/LabelContainer.asset";

        private static LabelContainer _instance;

        public static LabelContainer Instance
        {
            get
            {
                if (_instance == null)
                {
#if UNITY_EDITOR
                    _instance = AssetDatabase.LoadAssetAtPath<LabelContainer>(AssetPath);
#endif
                    if (_instance == null)
                        Debug.LogError(
                            $"[LabelContainer] No asset found at '{AssetPath}'. " +
                            "Create one via Assets/Create/TitraSims/Label Container and place it there.");
                }

                return _instance;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Preload()
        {
            // Force the lazy load so Instance is ready before the first scene runs.
            _ = Instance;
        }

        [Serializable]
        public struct LabelEntry
        {
            public string key;
            public Material material;
        }

        [SerializeField] private LabelEntry[] entries;

        private readonly Dictionary<string, Material> _lookup = new();

        private void OnEnable() => BuildLookup();

        private void OnValidate() => BuildLookup();

        private void BuildLookup()
        {
            _lookup.Clear();
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    _lookup[entry.key] = entry.material;
            }
        }

        public bool TryGet(string key, out Material material) => _lookup.TryGetValue(key, out material);

        public Material Get(string key) => _lookup.TryGetValue(key, out var material) ? material : null;

        /// <summary>All configured keys, read straight from the entries so it works in edit mode.</summary>
        public string[] Keys
        {
            get
            {
                if (entries == null) return Array.Empty<string>();
                var keys = new string[entries.Length];
                for (int i = 0; i < entries.Length; i++) keys[i] = entries[i].key;
                return keys;
            }
        }
    }
}