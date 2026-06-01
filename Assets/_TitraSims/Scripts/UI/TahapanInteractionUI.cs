using Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class TahapanInteractionUI : MonoBehaviour
    {
        private TahapanInteractionController controller;
        private NarationPanel narationPanel;
        private Button playButton;
        private Button stopButton;

        private void Awake()
        {
            controller = GetComponent<TahapanInteractionController>();
        }

        private void Start()
        {
            narationPanel = UIManager.Instance?.GetPanelByType(PanelType.PanelNaration)?.GetComponent<NarationPanel>();
            playButton = UIManager.Instance?.PlayAnimationButton;
            stopButton = UIManager.Instance?.StopAnimationButton;
        }

        private void OnEnable()
        {
            controller.OnInteractionEnter += HandleEnter;
            controller.OnFinishPlayingInteraction += HandleExit;
            controller.OnInteractionComplete += HandleComplete;

            if (playButton) playButton.onClick.AddListener(controller.PlayAnimation);
            if (stopButton) stopButton.onClick.AddListener(controller.StopAnimation);
        }

        private void OnDisable()
        {
            if (controller == null) return;
            controller.OnInteractionEnter -= HandleEnter;
            controller.OnFinishPlayingInteraction -= HandleExit;
            controller.OnInteractionComplete -= HandleComplete;

            if (playButton) playButton.onClick.RemoveListener(controller.PlayAnimation);
            if (stopButton) stopButton.onClick.RemoveListener(controller.StopAnimation);
        }

        private void HandleEnter(TahapanInteractionData data, bool hasAnimation)
        {
            var hasNaration = !string.IsNullOrEmpty(data.Title) || !string.IsNullOrEmpty(data.Description);
            narationPanel.SetVisibility(hasNaration);
            narationPanel.SetTitleText(data.Title);
            narationPanel.SetDescriptionText(data.Description);
            
            playButton?.gameObject.SetActive(hasAnimation);
            stopButton?.gameObject.SetActive(hasAnimation);
        }

        private void HandleExit()
        {
            playButton?.gameObject.SetActive(false);
            stopButton?.gameObject.SetActive(false);
        }

        private void HandleComplete()
        {
            narationPanel.SetVisibility(false);
        }
    }
}