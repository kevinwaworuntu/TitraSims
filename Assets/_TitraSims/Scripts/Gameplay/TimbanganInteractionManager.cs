using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class TimbanganInteractionManager : ARContentManager
    {
        [FormerlySerializedAs("balanceObject")]
        [Header("Balance Settings")]
        [SerializeField] private TimbanganObject timbanganObject;
        [SerializeField] private float targetWeight;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationClip animClipBalance;
        
        [Header("POWDER FILL SETTINGS")]
        [SerializeField] private Transform powderFillObject;
        private float scaleModifier = 0.1225f;
        private float initialScale = 0.02f;
        
        private bool isCheckWeightToContinue;
        
        protected void OnDisable()
        {
            ResetPowderFill();
        }
        
        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (!timbanganObject)
            {
                return;
            }
            if(isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                {
                    return;
                }
            }
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }

        public void SetupBalanceObject()
        {
            timbanganObject.SetWeight(0);
          
            ContextualButtonController.Instance.GenerateContextualButton(1);
            
            ContextualButtonController.Instance.RegisterTextToButton(0, "25 mg");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                PlayAnimation(animClipBalance);
                PowderFillValue(1);
            });
        }
        
        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
        }
        
        private void PlayAnimation(AnimationClip clip)
        {
            var animationConfig = GameManager.Instance.AnimationConfig;
            if (!animationConfig)
            {
                return;
            }
            if (animationConfig.GenericAnimController)
            {
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = clip;
            }
            if (!clip)
            {
                return;
            }
            if (!animator)
            {
                return;
            }
            animator.SetTrigger(animationConfig.StopAnimationParamName);
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            SetButtonEnabledState(false);
            StartCoroutine(WaitForDuration(clip.length)); // This is only valid if animation speed is constant
            IEnumerator WaitForDuration(float duration)
            {
                yield return new WaitForSeconds(duration);
                SetButtonEnabledState(true);
                OnStartWaitingForPlayerInputToContinueHandler();
            }
        }
        
        private bool IsCurrentWeightComplete()
        {
            return Mathf.FloorToInt(timbanganObject.GetCurrentWeight()) == Mathf.FloorToInt(targetWeight);
        }
        
        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }
        
        private void PowderFillValue(float targetMl)
        {
            var targetScale = powderFillObject.transform.localScale.z + scaleModifier * targetMl;
            powderFillObject.transform.localScale = Vector3.one * targetScale;
        }

        private void ResetPowderFill()
        {
            powderFillObject.transform.localScale = new Vector3(initialScale, initialScale, initialScale);
        }
    }
}