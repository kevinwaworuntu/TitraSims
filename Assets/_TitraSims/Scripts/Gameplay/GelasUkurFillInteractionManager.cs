using System.Collections;
using UI;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class GelasUkurFillInteractionManager : ARContentManager
    {
        [Serializable]
        private struct LiquidContainer
        {
            public Transform fillObject;
            public Transform meniskusObject;
        }
        private enum LarutanType
        {
            Larutan1,
            Larutan2
        }
        
        [Header("TARGET VOLUME")]
        public int currentTargetWeight;
        public int targetWeightLarutan1 = 40;
        public int targetWeightLarutan2 = 80;
   
        
        [Header("ANIMATION")]
        [SerializeField] private AnimationClip animClipLarutan1_1ml;
        [SerializeField] private AnimationClip animClipLarutan1_10ml;
        
        [SerializeField] private AnimationClip animClipLarutan2_1ml;
        [SerializeField] private AnimationClip animClipLarutan2_10ml;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimNotify animNotify;
        
        [FormerlySerializedAs("container1")]
        [Header("Liquid Containers")]
        [SerializeField] private LiquidContainer larutan1Container;
        [SerializeField] private LiquidContainer Larutan2Container;
        
        private float fillObjectScaleModifier = 0.10698485f;
        private float fillObjectInitialScale = 0f;
        private float meniskusPositionModifier = 0.000640f;
        private float meniskusInitPos = -0.00339f;
        
        private int currentWeight;
        private bool isCheckWeightToContinue;
        private LarutanType currentLarutanType = LarutanType.Larutan1;
        private int targetML;


        protected override void OnEnable()
        {
            base.OnEnable();

            if (!animNotify)
            {
                animNotify = transform.GetComponentInChildren<AnimNotify>();
            }
            if (animNotify)
            {
                animNotify.OnNotifyHit += OnStartUpdateVisualFill;
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            targetML = 0;
            if (animNotify)
            {
                animNotify.OnNotifyHit -= OnStartUpdateVisualFill;
            }
            
            ResetLarutanContainer(larutan1Container);
            ResetLarutanContainer(Larutan2Container);
        }
        
        public void CreatePenambahanLarutan1Button()
        {
            currentLarutanType = LarutanType.Larutan1;
            currentTargetWeight = targetWeightLarutan1;
            SetupLarutan();
        }
        
        public void CreatePenambahanLarutan2Button()
        {
            currentLarutanType = LarutanType.Larutan2;
            currentTargetWeight = targetWeightLarutan2;
            SetupLarutan();
        }
        
        private void SetupLarutan()
        {
            ContextualButtonController btnController = ContextualButtonController.Instance;
            btnController.GenerateContextualButton(2);
            
            AnimationClip clip1ml = currentLarutanType == LarutanType.Larutan1 ? animClipLarutan1_1ml : animClipLarutan2_1ml;
            AnimationClip clip10ml = currentLarutanType == LarutanType.Larutan1 ? animClipLarutan1_10ml : animClipLarutan2_10ml;;

            // Register 1ml Button
            btnController.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                targetML = 1;
                PlayAnimation(clip1ml);
            });

            // Register 10ml Button
            btnController.RegisterTextToButton(1, "10 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 10;
                targetML = 10;
                PlayAnimation(clip10ml);
            });
        }
        
        private bool IsCurrentWeightComplete()
        {
            return currentWeight == currentTargetWeight;
        }
        
        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > currentTargetWeight;
        }

        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
        }
        
        private void RestartCurrentInteraction()
        {
            currentWeight = 0; 
            ResetLarutanContainer(currentLarutanType == LarutanType.Larutan1 ? larutan1Container : Larutan2Container);
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
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
                if (IsCurrentWeightComplete())
                {
                    OnStartWaitingForPlayerInputToContinueHandler();
                }
                if (IsCurrentWeightExceedTarget())
                {
                   RestartCurrentInteraction();
                }
            }
        }

        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if(isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                {
                    return;
                }
            }
            currentWeight = 0;
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }
        
        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }
        
        private void UpdateVisualFill(LiquidContainer container, float targetMl)
        {
            Vector3 scale = container.fillObject.localScale;
            scale.z += fillObjectScaleModifier * targetMl;
            container.fillObject.localScale = scale;
            
            Vector3 pos = container.meniskusObject.localPosition;
            pos.y += meniskusPositionModifier * targetMl;
            container.meniskusObject.localPosition = pos;
        }
        
        private void ResetLarutanContainer(LiquidContainer container)
        {
            if (!container.fillObject || !container.meniskusObject)
            {
                return;
            }
            
            Vector3 scale = container.fillObject.localScale;
            scale.z = 0;
            container.fillObject.localScale = scale;

            Vector3 pos = container.meniskusObject.localPosition;
            pos.y = meniskusInitPos;
            container.meniskusObject.localPosition = pos;
        }
        
        private void OnStartUpdateVisualFill()
        {
            UpdateVisualFill(currentLarutanType == LarutanType.Larutan1 ? larutan1Container : Larutan2Container, targetML);
        }
    }
}