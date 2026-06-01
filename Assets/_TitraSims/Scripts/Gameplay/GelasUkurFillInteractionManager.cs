using System.Collections;
using System;
using Animation;
using Data;
using UI;
using UnityEngine;
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
            public int targetVolume;
        }

        private enum LarutanType
        {
            Larutan1,
            Larutan2
        }

        [Header("Stage Config")]
        [SerializeField] private GelasUkurStageConfig config;

        [Header("Animation")]
        [SerializeField] private AnimationClip animClipLarutan1_1ml;
        [SerializeField] private AnimationClip animClipLarutan1_10ml;
        [SerializeField] private AnimationClip animClipLarutan2_1ml;
        [SerializeField] private AnimationClip animClipLarutan2_10ml;
        [SerializeField] private Animator animator;
        [SerializeField] private AnimNotify animNotify;

        [FormerlySerializedAs("container1")]
        [Header("Liquid Containers")]
        [SerializeField] private LiquidContainer larutan1Container;
        [FormerlySerializedAs("Larutan2Container")]
        [SerializeField] private LiquidContainer larutan2Container;

        private int currentWeight;
        private bool isCheckWeightToContinue;
        private LarutanType currentLarutanType = LarutanType.Larutan1;
        private int targetML;

        private LiquidContainer CurrentContainer =>
            currentLarutanType == LarutanType.Larutan1 ? larutan1Container : larutan2Container;

        protected override void OnEnable()
        {
            base.OnEnable();

            if (!animNotify)
                animNotify = transform.GetComponentInChildren<AnimNotify>();

            if (animNotify)
                animNotify.OnNotify += OnStartUpdateVisualFill;
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            targetML = 0;
            if (animNotify)
                animNotify.OnNotify -= OnStartUpdateVisualFill;

            ResetLarutanContainer(larutan1Container);
            ResetLarutanContainer(larutan2Container);
        }

        public void CreatePenambahanLarutan1Button()
        {
            currentLarutanType = LarutanType.Larutan1;
            SetupLarutan();
        }

        public void CreatePenambahanLarutan2Button()
        {
            currentLarutanType = LarutanType.Larutan2;
            SetupLarutan();
        }

        private void SetupLarutan()
        {
            ContextualButtonController btnController = ContextualButtonController.Instance;
            btnController.GenerateContextualButton(2);

            AnimationClip clip1ml = currentLarutanType == LarutanType.Larutan1 ? animClipLarutan1_1ml : animClipLarutan2_1ml;
            AnimationClip clip10ml = currentLarutanType == LarutanType.Larutan1 ? animClipLarutan1_10ml : animClipLarutan2_10ml;

            btnController.RegisterTextToButton(0, "1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 1;
                targetML = 1;
                PlayAnimation(clip1ml);
            });

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
            return currentWeight == CurrentContainer.targetVolume;
        }

        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > CurrentContainer.targetVolume;
        }

        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var btn in ContextualButtonController.Instance.GetContextualButtons())
                btn.SetEnabled(enabled);
        }

        private void RestartCurrentInteraction()
        {
            currentWeight = 0;
            ResetLarutanContainer(CurrentContainer);
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
        }

        // ReSharper disable Unity.PerformanceAnalysis
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
                if (IsCurrentWeightComplete())
                    OnStartWaitingForPlayerInputToContinueHandler();
                else if (IsCurrentWeightExceedTarget())
                    RestartCurrentInteraction();
            }
        }

        // ReSharper disable Unity.PerformanceAnalysis
        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (isCheckWeightToContinue && !IsCurrentWeightComplete())
                return;

            currentWeight = 0;
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
        }

        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }

        private void UpdateVisualFill(LiquidContainer container, float ml)
        {
            Vector3 scale = container.fillObject.localScale;
            scale.z += config.FillObjectScaleModifier * ml;
            container.fillObject.localScale = scale;

            Vector3 pos = container.meniskusObject.localPosition;
            pos.y += config.MeniskusPositionModifier * ml;
            container.meniskusObject.localPosition = pos;
        }

        private void ResetLarutanContainer(LiquidContainer container)
        {
            if (!config || !container.fillObject || !container.meniskusObject)
                return;

            Vector3 scale = container.fillObject.localScale;
            scale.z = config.FillObjectInitialScale;
            container.fillObject.localScale = scale;

            Vector3 pos = container.meniskusObject.localPosition;
            pos.y = config.MeniskusInitPos;
            container.meniskusObject.localPosition = pos;
        }

        private void OnStartUpdateVisualFill(string value)
        {
            UpdateVisualFill(CurrentContainer, targetML);
        }
    }
}