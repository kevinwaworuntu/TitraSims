using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BackButton : TitraSims_Button
    {
        private CanvasGroup canvasGroup;
        private ContextualButtonController contextualButtonController;

        protected override void Awake()
        {
            base.Awake();
            canvasGroup = GetComponent<CanvasGroup>();
            contextualButtonController = FindAnyObjectByType<ContextualButtonController>();
        }

        private void Start() => SetVisibility(false);

        protected override void OnEnable()
        {
            base.OnEnable();
            if (UIManager.Instance) UIManager.Instance.OnPanelTypeChanged += OnCurrentPanelChanged;
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            if (UIManager.Instance) UIManager.Instance.OnPanelTypeChanged -= OnCurrentPanelChanged;
        }

        protected override void OnClick()
        {
            var uiInstance = UIManager.Instance;
            if (!uiInstance) return;

            if (uiInstance.CurrentActivePanelType == PanelType.PanelScanAR)
            {
                uiInstance.GetPanelByType(PanelType.PanelNaration)?.SetActive(false);
                contextualButtonController.DestroyButtons();
                uiInstance.ForceHideInfoPanel();
                GameManager.Instance?.BackFromCurrentTahap();
                GameManager.Instance?.SetARCameraActive(false);
            }

            uiInstance.GoBack();
        }

        private void OnCurrentPanelChanged(PanelType panelType)
        {
            SetVisibility(panelType != PanelType.PanelHomePage);
        }

        private void SetVisibility(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            interactable = visible;
        }
    }
}
