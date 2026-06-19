using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class InfoPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI TitleText;
        [SerializeField] private TextMeshProUGUI DescriptionText;

        private RectTransform rt;
        private CanvasGroup cg;
        private Vector2 restPosition;
        private bool initialized;

        private void EnsureInitialized()
        {
            if (initialized) return;
            rt = GetComponent<RectTransform>();
            cg = GetComponent<CanvasGroup>();
            restPosition = rt.anchoredPosition;
            initialized = true;
        }

        public void SetTitleText(string text) => TitleText.text = text;
        public void SetDescriptionText(string text) => DescriptionText.text = text;

        public void Show()
        {
            EnsureInitialized();
            rt.anchoredPosition = restPosition;
            rt.DOKill();
            gameObject.SetActive(true);
            UIAnimator.ShowPanel(rt, cg, UIAnimator.SlideDirection.Down, 30f, 0.35f);
        }

        public void Hide()
        {
            EnsureInitialized();
            rt.anchoredPosition = restPosition;
            rt.DOKill();
            UIAnimator.HidePanel(rt, cg, UIAnimator.SlideDirection.Down, 30f, 0.25f)
                .OnComplete(() => gameObject.SetActive(false));
        }
    }
}
