using System.Collections;
using Data;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class BuretteFillInteractionManager : ARContentManager
    {
        [Header("Stage Config")]
        [SerializeField] private BuretteStageConfig config;

        [Header("Animation References")]
        [SerializeField] private AnimationClip animClipOpenKeran;
        [SerializeField] private AnimationClip animClipTetes;
        [SerializeField] private AnimationClip animClipTetesBanyak;
        [SerializeField] private Animator animator;

        [Header("Titration Visuals")]
        [SerializeField] private Renderer titrasiRenderer;
        [SerializeField] private TextMeshProUGUI textSampelWeight;

        [FormerlySerializedAs("burretFillObject")]
        [Header("Burette Fill")]
        [SerializeField] private Transform buretteFillObject;
        [FormerlySerializedAs("burretMeniskusObject")]
        [SerializeField] private Transform buretteMeniskusObject;

        private bool isPlaying;
        private Material titrasiMatInstance;
        protected bool isCheckWeightToContinue;
        private float currentWeight;
        private int currentIndex;

        private static readonly int SideColorID = Shader.PropertyToID("_Side_Color");
        private static readonly int TopColorID = Shader.PropertyToID("_TopColor");

        protected void Awake()
        {
            titrasiMatInstance = titrasiRenderer.material;
        }

        protected void OnEnable()
        {
            base.OnEnable();
            ResetBuretteFill();
            RestartErlenmeyerVisual();
        }

        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if (isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                    return;

                titrasiMatInstance.SetColor(SideColorID, config.TargetColor);
                titrasiMatInstance.SetColor(TopColorID, config.TargetColor);
            }
            currentWeight = 0;
            ContextualButtonController.Instance.DestroyButtons();
            base.OnStartWaitingForPlayerInputToContinueHandler();
            SetIsCheckWeightToContinue(false);
            textSampelWeight.gameObject.SetActive(false);
        }

        public void SetCurrentWeightIndex(int value)
        {
            currentIndex = value;
        }

        private bool IsCurrentWeightComplete()
        {
            return currentWeight >= config.FloorTargets[currentIndex] && currentWeight <= config.CeilTargets[currentIndex];
        }

        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > config.CeilTargets[currentIndex];
        }

        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var btn in ContextualButtonController.Instance.GetContextualButtons())
                btn.SetEnabled(enabled);
        }

        public void SetIsCheckWeightToContinue(bool value)
        {
            isCheckWeightToContinue = value;
        }

        private void RestartCurrentInteraction()
        {
            currentWeight = 0;
            RestartErlenmeyerVisual();
            ResetBuretteFill();
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
        }

        public void RestartErlenmeyerVisual()
        {
            if (!config)
            {
                return;
            }
            titrasiMatInstance.SetColor(SideColorID, config.InitialColor);
            titrasiMatInstance.SetColor(TopColorID, config.InitialColor);
        }

        public void CreateButtonInteraction()
        {
            SetSampelWeightTextVisible();
            RestartErlenmeyerVisual();
            ResetBuretteFill();

            ContextualButtonController.Instance.GenerateContextualButton(2);
            ContextualButtonController.Instance.RegisterTextToButton(0, "0.1 ml");
            ContextualButtonController.Instance.RegisterAction(0, () =>
            {
                currentWeight += 0.1f;
                TetesanSequenceExecutor(1);
            });
            ContextualButtonController.Instance.RegisterTextToButton(1, "1 ml");
            ContextualButtonController.Instance.RegisterAction(1, () =>
            {
                currentWeight += 1f;
                TetesanSequenceExecutor(10);
            });
        }

        public void TetesanSequenceExecutor(int totalTetes)
        {
            if (isPlaying)
                return;
            StartCoroutine(TetesanSequence(totalTetes));
        }

        private IEnumerator TetesanSequence(int totalTetes)
        {
            isPlaying = true;
            SetButtonEnabledState(false);
            var animationConfig = GameManager.Instance.AnimationConfig;

            if (animationConfig.GenericAnimController)
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipOpenKeran;

            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            if (totalTetes == 10)
            {
                if (animationConfig.GenericAnimController)
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipTetesBanyak;

                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(10);
                yield return new WaitForSeconds(animClipTetesBanyak.length);
            }
            else if (totalTetes == 1)
            {
                if (animationConfig.GenericAnimController)
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipTetes;

                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(1);
                yield return new WaitForSeconds(animClipTetes.length);
            }

            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(totalTetes == 1 ? animClipTetes.length : animClipTetesBanyak.length);

            SetButtonEnabledState(true);
            isPlaying = false;

            if (IsCurrentWeightComplete())
            {
                StartCoroutine(DelaySetTargetColor());
                IEnumerator DelaySetTargetColor()
                {
                    yield return new WaitForSeconds(1);
                    titrasiMatInstance.SetColor(SideColorID, config.TargetColor);
                    titrasiMatInstance.SetColor(TopColorID, config.TargetColor);
                }
                OnStartWaitingForPlayerInputToContinueHandler();
            }
            else if (IsCurrentWeightExceedTarget())
            {
                RestartCurrentInteraction();
            }
        }

        public void SetSampelWeightTextVisible()
        {
            StartCoroutine(DelaySetSampelWeight());
            IEnumerator DelaySetSampelWeight()
            {
                yield return new WaitForSeconds(1f);
                textSampelWeight.gameObject.SetActive(true);
                textSampelWeight.SetText($"Bobot penimbangan: {config.Weights[currentIndex]} mg");
            }
        }

        private void BuretteFillValue(float targetMl)
        {
            if (!config)
            {
                return;
            }
            var scale = buretteFillObject.localScale;
            scale.z -= config.FillObjectScaleModifier * targetMl;
            buretteFillObject.localScale = scale;

            var pos = buretteMeniskusObject.localPosition;
            pos.y -= config.MeniskusPositionModifier * targetMl;
            buretteMeniskusObject.localPosition = pos;
        }

        public void ResetBuretteFill()
        {
            if (!config)
            {
                return;
            }
            var scale = buretteFillObject.localScale;
            scale.z = config.FillObjectInitialScale;
            buretteFillObject.localScale = scale;

            var pos = buretteMeniskusObject.localPosition;
            pos.y = config.MeniskusInitPos;
            buretteMeniskusObject.localPosition = pos;
        }
    }
}