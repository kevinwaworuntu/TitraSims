using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class NarationButton : MonoBehaviour
    {
        private Button button;
        private RectTransform rt;

        private void Awake()
        {
            button = GetComponent<Button>();
            rt = GetComponent<RectTransform>();
        }

        private void OnEnable()
        {
            if (button) button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            if (button) button.onClick.RemoveListener(OnButtonClick);
        }

        private void OnButtonClick()
        {
            UIAnimator.ButtonPress(rt).OnComplete(() =>
            {
                if (UIManager.Instance) UIManager.Instance.ToggleNarationPanel();
            });
        }
    }
}