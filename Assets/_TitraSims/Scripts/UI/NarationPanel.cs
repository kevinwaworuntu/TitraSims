using DG.Tweening;
using TMPro;
using UnityEngine;

namespace UI
{
    [RequireComponent(typeof(CanvasGroup))]
    public class NarationPanel : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI titleText;
        [SerializeField] private TextMeshProUGUI descriptionText;

        private RectTransform rt;
        private CanvasGroup cg;
        private Vector2 restPosition;
        private bool isVisible;
        private bool initialized;

        private void EnsureInitialized()
        {
            if (initialized) return;
            rt = GetComponent<RectTransform>();
            cg = GetComponent<CanvasGroup>();
            restPosition = rt.anchoredPosition;
            initialized = true;
        }

        public void SetTitleText(string text) => titleText.text = text;
        public void SetDescriptionText(string text) => descriptionText.text = text;

        public void Show()
        {
            EnsureInitialized();
            rt.anchoredPosition = restPosition;
            rt.DOKill();
            gameObject.SetActive(true);
            isVisible = true;
            UIAnimator.ShowPanel(rt, cg, UIAnimator.SlideDirection.Down, 30f, 0.35f);
        }

        public void Hide()
        {
            EnsureInitialized();
            rt.anchoredPosition = restPosition;
            rt.DOKill();
            UIAnimator.HidePanel(rt, cg, UIAnimator.SlideDirection.Down, 30f, 0.25f)
                .OnComplete(() =>
                {
                    isVisible = false;
                    gameObject.SetActive(false);
                });
        }

        public bool IsVisible()
        {
            return isVisible;
        }

        public void SetVisibility(bool visible)
        {
            if (visible) Show();
            else Hide();
        }
    }
}