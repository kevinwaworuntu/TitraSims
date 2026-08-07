using System.Collections;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;

namespace Gameplay
{
    public class PenambahanIndikator : ARContentManager
    {
        [Header("Drop Settings")]
        [SerializeField] private int requiredDropCount = 3;

        [Header("References")]
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip animClipPipetPress;

        [Header("Events")]
        [SerializeField] private UnityEvent onDropCompleted;

        private int currentDropCount;
        private bool isCheckDropToContinue;

        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (isCheckDropToContinue && currentDropCount < requiredDropCount)
                return;

            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckDropToContinue(false);
        }

        public void SetupPipetObject()
        {
            currentDropCount = 0;
            SetIsCheckDropToContinue(true);

            ContextualButtonController.Instance.GenerateContextualButton(1);
            ContextualButtonController.Instance.RegisterTextToButton(0, "Teteskan");
            ContextualButtonController.Instance.RegisterAction(0, OnDropButtonClicked);
        }

        private void OnDropButtonClicked()
        {
            currentDropCount++;
            PlayPipetAnimation();

            if (currentDropCount >= requiredDropCount)
                onDropCompleted?.Invoke();
        }

        private void PlayPipetAnimation()
        {
            if (!animator || !animClipPipetPress)
                return;

            animator.SetTrigger("Play");
            StartCoroutine(WaitForAnimation(animClipPipetPress.length));
        }

        private System.Collections.IEnumerator WaitForAnimation(float duration)
        {
            SetButtonEnabledState(false);
            yield return new WaitForSeconds(duration);
            SetButtonEnabledState(true);
            OnStartWaitingForPlayerInputToContinueHandler();
        }

        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var btn in ContextualButtonController.Instance.GetContextualButtons())
                btn.SetEnabled(enabled);
        }

        public void SetIsCheckDropToContinue(bool value)
        {
            isCheckDropToContinue = value;
        }
    }
}