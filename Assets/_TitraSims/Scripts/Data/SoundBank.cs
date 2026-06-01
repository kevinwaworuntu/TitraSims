using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "SoundBank", menuName = "AR Pharma/Sound Bank")]
    public class SoundBank : ScriptableObject
    {
        [Serializable]
        public struct SoundEntry
        {
            public string key;
            public AudioClip clip;
        }

        [SerializeField] private SoundEntry[] sfx;

        private Dictionary<string, AudioClip> _lookup;

        private void OnEnable() => BuildLookup();

        private void OnValidate() => BuildLookup();

        private void BuildLookup()
        {
            _lookup = new Dictionary<string, AudioClip>(sfx?.Length ?? 0);
            if (sfx == null) return;
            foreach (var entry in sfx)
            {
                if (!string.IsNullOrEmpty(entry.key) && entry.clip != null)
                    _lookup[entry.key] = entry.clip;
            }
        }

        public AudioClip Get(string key)
        {
            if (_lookup == null) BuildLookup();
            _lookup.TryGetValue(key, out var clip);
            return clip;
        }
    }
}