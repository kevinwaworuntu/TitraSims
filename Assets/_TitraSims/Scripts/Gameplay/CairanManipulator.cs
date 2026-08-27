using Animation;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;

namespace Gameplay
{
    public class CairanManipulator : MonoBehaviour
    {
        [Header("Target")] [SerializeField] private Renderer cairanRenderer;

        [Header("Notify")] [Tooltip("Only notifies with this name trigger the event. Leave empty to react to any notify.")]
        private string notifyFilter = "Erlenmeyer";

        [SerializeField] private UnityEvent OnNotifyHitEvent;

        [Header("Fill Tween")] [SerializeField]
        private float fillTweenDuration = 0.35f;

        [SerializeField] private Ease fillTweenEase = Ease.OutQuad;

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
            if (animNotify) animNotify.OnNotify += OnNotifyHit;

            ResetFillValue();
        }

        private void OnDisable()
        {
            if (animNotify) animNotify.OnNotify -= OnNotifyHit;


            if (cairanMatInstance) cairanMatInstance.DOKill();
        }

        public void OnNotifyHit(string notifyName)
        {
            if (!string.IsNullOrEmpty(notifyFilter) && notifyName != notifyFilter)
                return;

            OnNotifyHitEvent.Invoke();
        }

        public void SetFillValue(float value)
        {
            TweenFillTo(value);
        }

        public void IncreaseFillValue(float value)
        {
            TweenFillTo(cairanMatInstance.GetFloat(FillID) + value);
        }

        private void TweenFillTo(float value)
        {
            if (!cairanMatInstance) return;
            cairanMatInstance.DOKill();
            cairanMatInstance.DOFloat(value, FillID, fillTweenDuration).SetEase(fillTweenEase);
        }

        [ContextMenu("Test Fill Value")]
        private void TestFillValue()
        {
            IncreaseFillValue(0.01f);
        }

        public void ResetFillValue()
        {
            if (!cairanMatInstance)
                return;
            
            cairanMatInstance.DOKill();
            cairanMatInstance.SetFloat(FillID, originalFillValue);
        }
    }
}