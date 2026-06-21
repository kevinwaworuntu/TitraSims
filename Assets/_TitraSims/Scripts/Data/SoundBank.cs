using System;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "SoundBank", menuName = "TitraSims/Sound Bank")]
    public class SoundBank : ScriptableObject
    {
        [Serializable]
        public struct SoundEntry
        {
            public string key;
            public SoundType soundType;
            public int priority;
            public AudioClip clip;
        }

        [SerializeField] private SoundEntry[] entries;

        private Dictionary<string, SoundEntry> _lookup = new();
        private Dictionary<SoundType, List<SoundEntry>> _typeLookup = new();

        private void OnEnable() => BuildLookup();

        private void OnValidate() => BuildLookup();

        private void BuildLookup()
        {
            _lookup.Clear();
            _typeLookup.Clear();
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.key))
                    _lookup[entry.key] = entry;

                if (!_typeLookup.TryGetValue(entry.soundType, out var list))
                {
                    list = new List<SoundEntry>();
                    _typeLookup[entry.soundType] = list;
                }
                list.Add(entry);
            }

            foreach (var list in _typeLookup.Values)
                list.Sort((a, b) => b.priority.CompareTo(a.priority));
        }

        // --- string key lookups ---

        public bool TryGet(string key, out SoundEntry entry) =>
            _lookup.TryGetValue(key, out entry);

        public AudioClip GetClip(string key)
        {
            _lookup.TryGetValue(key, out var entry);
            return entry.clip;
        }

        // --- SoundType lookups ---

        public bool TryGet(SoundType type, out SoundEntry entry)
        {
            if (_typeLookup.TryGetValue(type, out var list) && list.Count > 0)
            {
                entry = list[0];
                return true;
            }
            entry = default;
            return false;
        }

        public AudioClip GetClip(SoundType type)
        {
            if (!_typeLookup.TryGetValue(type, out var list) || list.Count == 0) return null;
            list.Sort((a, b) => b.priority.CompareTo(a.priority));
            return list[0].clip;
        }

        public AudioClip[] GetClips(SoundType type)
        {
            if (!_typeLookup.TryGetValue(type, out var list)) return Array.Empty<AudioClip>();
            var clips = new AudioClip[list.Count];
            for (int i = 0; i < list.Count; i++) clips[i] = list[i].clip;
            return clips;
        }
    }
}