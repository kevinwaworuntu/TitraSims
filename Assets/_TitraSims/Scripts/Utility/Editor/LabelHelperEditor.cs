using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace _TitraSims.Scripts.Utility.Editor
{
    [CustomEditor(typeof(LabelHelper))]
    public class LabelHelperEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Draw every serialized field except the key — we replace it with a dropdown.
            DrawPropertiesExcluding(serializedObject, "m_Script", "labelKey");

            var keyProp = serializedObject.FindProperty("labelKey");
            var container = LabelContainer.Instance;
            var keys = container != null ? container.Keys : Array.Empty<string>();

            if (keys.Length == 0)
            {
                EditorGUILayout.PropertyField(keyProp);
                EditorGUILayout.HelpBox(
                    "No LabelContainer keys found. Ensure a LabelContainer asset exists in a Resources folder " +
                    "and has entries.", MessageType.Info);
            }
            else
            {
                int index = Array.IndexOf(keys, keyProp.stringValue);
                bool missing = index < 0;
                if (missing) index = 0;

                index = EditorGUILayout.Popup("Label Key", index, keys);
                keyProp.stringValue = keys[index];

                if (missing && !string.IsNullOrEmpty(keyProp.stringValue))
                    EditorGUILayout.HelpBox(
                        "Previously selected key is no longer in the LabelContainer.", MessageType.Warning);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Update Label"))
            {
                var method = typeof(LabelHelper).GetMethod(
                    "UpdateLabel", BindingFlags.NonPublic | BindingFlags.Instance);
                foreach (var t in targets)
                    method?.Invoke(t, null);
            }
        }
    }
}