using System.Collections;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Events;
using System.Diagnostics;

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
        private bool isPlaying;
        private AnimatorOverrideController _runtimeOverride;

        protected override void OnEnable()
        {
            base.OnEnable();
            
            isPlaying = false;
            currentDropCount = 0;

            var animationConfig = GameManager.Instance.AnimationConfig;
            if (animationConfig?.GenericAnimController != null && animator)
            {
                _runtimeOverride = new AnimatorOverrideController(animationConfig.GenericAnimController);
                animator.runtimeAnimatorController = _runtimeOverride;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            isPlaying = false;

            if (animator)
                animator.runtimeAnimatorController = null;
        }

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
            if (isPlaying)
                return;

            currentDropCount++;
            PlayPipetAnimation(animClipPipetPress);

            if (currentDropCount >= requiredDropCount)
                onDropCompleted?.Invoke();
        }

        private void PlayPipetAnimation(AnimationClip clip)
        {
            if (animator.runtimeAnimatorController != _runtimeOverride)
                _runtimeOverride = animator.runtimeAnimatorController as AnimatorOverrideController;

            var animationConfig = GameManager.Instance.AnimationConfig;
            if (!animationConfig || !clip || !animator)
                return;

            if (_runtimeOverride)
                _runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = clip;

            isPlaying = true;
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            SetButtonEnabledState(false);
            StartCoroutine(WaitForDuration(clip.length));
            IEnumerator WaitForDuration(float duration)
            {
                yield return new WaitForSeconds(duration);
                isPlaying = false;
                SetButtonEnabledState(true);
                RestoreGenericAnimClip();
                OnStartWaitingForPlayerInputToContinueHandler();
            }
        }

        private void RestoreGenericAnimClip()
        {
            var animationConfig = GameManager.Instance?.AnimationConfig;
            if (!animationConfig || !animator) return;

            if (animator.runtimeAnimatorController != _runtimeOverride)
                _runtimeOverride = animator.runtimeAnimatorController as AnimatorOverrideController;

            if (!_runtimeOverride || !animationConfig.IsAnimGenericClipEntryNameValid()) return;

            animator.SetTrigger(animationConfig.StopAnimationParamName);
            _runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = animationConfig.GetAnimGenericEntryClip();
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