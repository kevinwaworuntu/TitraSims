using Animation;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class CairanManipulator : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Renderer cairanRenderer;

        [Header("Notify")]
        [Tooltip("Only notifies with this name trigger the event. Leave empty to react to any notify.")]
        private string notifyFilter = "Erlenmeyer";

        [SerializeField] private UnityEvent OnNotifyHitEvent;

        private AnimNotify animNotify;
        private float originalFillValue;
        private Material cairanMatInstance;

        private static readonly int FillID = Shader.PropertyToID("_Fill");

        private void Awake()
        {
            if (!cairanRenderer)
                return;

            cairanMatInstance = cairanRenderer.material;
            originalFillValue = cairanMatInstance.GetFloat(FillID);
        }

        private void OnEnable()
        {
            animNotify = FindObjectOfType<AnimNotify>();
            if (animNotify)
            {
                animNotify.OnNotify += OnNotifyHit;
            }

            ResetFillValue();
        }

        private void OnDisable()
        {
            if (animNotify)
            {
                animNotify.OnNotify -= OnNotifyHit;
            }
        }

        public void OnNotifyHit(string notifyName)
        {
            if (!string.IsNullOrEmpty(notifyFilter) && notifyName != notifyFilter)
                return;

            OnNotifyHitEvent.Invoke();
        }

        public void SetFillValue(float value)
        {
            if (!cairanMatInstance)
                return;

            cairanMatInstance.SetFloat(FillID, value);
        }
        
        public void IncreaseFillValue(float value)
        {
            if (!cairanMatInstance)
                return;

            var init = cairanMatInstance.GetFloat(FillID);
            cairanMatInstance.SetFloat(FillID, value + init);
        }

        [ContextMenu("Test Fill Value")]
        void TestFillValue()
        {
            IncreaseFillValue(0.2f);
        }
        public void ResetFillValue()
        {
            SetFillValue(originalFillValue);
        }
    }
}
