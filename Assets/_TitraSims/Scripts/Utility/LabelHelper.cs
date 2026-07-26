using UnityEngine;

namespace _TitraSims.Scripts.Utility
{
    public class LabelHelper : MonoBehaviour
    {
        private LabelContainer labelContainer;
        [SerializeField] private string labelKey;
        [SerializeField] private string labelGameObjectName = "label";

        [ContextMenu("Update Label")]
        private void UpdateLabel()
        {
         
            var container = labelContainer != null ? labelContainer : LabelContainer.Instance;
            if (container == null)
            {
                Debug.LogWarning($"[LabelHelper] No LabelContainer available for '{name}'.", this);
                return;
            }

            if (!container.TryGet(labelKey, out var material) || material == null)
            {
                Debug.LogWarning($"[LabelHelper] Key '{labelKey}' not found in LabelContainer.", this);
                return;
            }

            var label = FindLabel(transform);
            if (label == null)
            {
                Debug.LogWarning(
                    $"[LabelHelper] No child named '{labelGameObjectName}' with a Renderer found under '{name}'.", this);
                return;
            }

            label.sharedMaterial = material;
        }

        private Renderer FindLabel(Transform root)
        {
            foreach (Transform child in root)
            {
                if (child.name == labelGameObjectName && child.TryGetComponent(out Renderer renderer))
                    return renderer;

                var nested = FindLabel(child);
                if (nested != null) return nested;
            }

            return null;
        }
    }
}