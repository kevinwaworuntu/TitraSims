using System.Collections;
using Data;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class TimbanganInteractionManager : ARContentManager
    {
        [Header("Stage Config")]
        [SerializeField] private TimbanganStageConfig config;
        
        [Header("Balance References")]
        [SerializeField] private TimbanganObject timbanganObject;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip animClipBalance;

        [Header("Powder Fill")]
        [SerializeField] private Transform powderFillObject;

        private bool isCheckWeightToContinue;

        protected void OnDisable()
        {
            ResetPowderFill();
        }

        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (!timbanganObject)
                return;

            if (isCheckWeightToContinue && !IsCurrentWeightComplete())
                return;

            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }

        public void SetupBalanceObject()
        {
            timbanganObject.SetWeight(0);

            ContextualButtonController.Instance.GenerateContextualButton(1);
            ContextualButtonController.Instance.RegisterTextToButton(0, $"{config.StepAmountMg} mg");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                PlayAnimation(animClipBalance);
                PowderFillValue(1);
            });
        }

        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var btn in ContextualButtonController.Instance.GetContextualButtons())
                btn.SetEnabled(enabled);
        }

        private void PlayAnimation(AnimationClip clip)
        {
            var animationConfig = GameManager.Instance.AnimationConfig;
            if (!animationConfig || !clip || !animator)
                return;

            if (animationConfig.GenericAnimController)
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = clip;

            animator.SetTrigger(animationConfig.StopAnimationParamName);
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            SetButtonEnabledState(false);
            StartCoroutine(WaitForDuration(clip.length));
            IEnumerator WaitForDuration(float duration)
            {
                yield return new WaitForSeconds(duration);
                SetButtonEnabledState(true);
                OnStartWaitingForPlayerInputToContinueHandler();
            }
        }

        private bool IsCurrentWeightComplete()
        {
            return Mathf.FloorToInt(timbanganObject.GetCurrentWeight()) == Mathf.FloorToInt(config.TargetWeight);
        }

        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }

        private void PowderFillValue(float steps)
        {
            var targetScale = powderFillObject.localScale.z + config.ScaleModifier * steps;
            powderFillObject.localScale = Vector3.one * targetScale;
        }

        private void ResetPowderFill()
        {
            powderFillObject.localScale = Vector3.one * config.InitialScale;
        }
    }
}