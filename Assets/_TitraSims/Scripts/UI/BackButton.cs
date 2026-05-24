using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class BackButton : MonoBehaviour
    {
        private Button button;
        private CanvasGroup canvasGroup;
        private ContextualButtonController contextualButtonController;
       
        private void Awake()
        {
            button = GetComponent<Button>();
            canvasGroup = GetComponent<CanvasGroup>();
            contextualButtonController = FindAnyObjectByType<ContextualButtonController>();
        }

        private void Start()
        {
            SetVisibility(false);
        }

        private void OnEnable()
        {
            button?.onClick.AddListener(OnClicked);
            UIManager.Instance.OnPanelTypeChanged += OnCurrentPanelChanged;
        }

        private void OnDisable()
        {
            button?.onClick.RemoveAllListeners();
            UIManager.Instance.OnPanelTypeChanged -= OnCurrentPanelChanged;
        }

        private void OnClicked()
        {
            var uiInstance = UIManager.Instance;
            if (!uiInstance)
            {
                return;
            }

            var currentPanelType = uiInstance.CurrentActivePanelType;
            switch (currentPanelType)
            {
                case PanelType.PanelScanAR :
                    uiInstance.GetPanelByType(PanelType.PanelNaration)?.SetActive(false);
                    contextualButtonController.DestroyButtons();
                    GameManager.Instance?.BackFrromCurrentTahap();
                    GameManager.Instance?.SetARCameraActive(false);
                    break;
            }
            uiInstance.GoBack();
        }

        private void OnCurrentPanelChanged(PanelType panelType)
        {
            switch (panelType)
            {
                case PanelType.PanelHomePage :
                    SetVisibility(false);
                    break;   
                default:
                    SetVisibility(true);
                    break;
            }
        }
        
        private void SetVisibility(bool visible)
        {
            canvasGroup.alpha = visible ? 1 : 0;
            button.interactable = visible;
        }
    }
}