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
        private AnimatorOverrideController _runtimeOverride;
        private Coroutine sampelWeightRoutine;

        private static readonly int SideColorID = Shader.PropertyToID("_Side_Color");
        private static readonly int TopColorID = Shader.PropertyToID("_TopColor");

        protected override void Awake()
        {
            base.Awake();
            titrasiMatInstance = titrasiRenderer.material;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            
            isPlaying = false;
            currentWeight = 0;
            sampelWeightRoutine = null;

            ResetBuretteFill();
            RestartErlenmeyerVisual();

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
            sampelWeightRoutine = null;

            if (animator)
                animator.runtimeAnimatorController = null;
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
            isPlaying = false;
            currentWeight = 0;

            // The interaction is replayed from the top, so drop the state the overshot attempt
            // left behind: the pending weight label, the "waiting for the right weight" gate
            // (the mapping's UniqueEvent arms it again) and the tetesan clip still sitting in
            // the override controller.
            if (sampelWeightRoutine != null)
            {
                StopCoroutine(sampelWeightRoutine);
                sampelWeightRoutine = null;
            }
            if (textSampelWeight) textSampelWeight.gameObject.SetActive(false);
            SetIsCheckWeightToContinue(false);
            RestoreGenericAnimClip();

            RestartErlenmeyerVisual();
            ResetBuretteFill();
            ContextualButtonController.Instance.DestroyButtons();
            tahapanInteractionController.RestartInteraction();
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
            ContextualButtonController.Instance.RegisterAction(0, () => TryPlayTetesan(0.1f, 1));
            ContextualButtonController.Instance.RegisterTextToButton(1, "1 ml");
            ContextualButtonController.Instance.RegisterAction(1, () => TryPlayTetesan(1f, 10));
        }

        // TitraSims_Button defers OnClick behind the press tween, so a tap that lands just before
        // SetButtonEnabledState(false) still fires. Adding the volume before the isPlaying guard let
        // those taps inflate currentWeight with no tetesan played and no target check ever running.
        private void TryPlayTetesan(float milliliter, int totalTetes)
        {
            if (isPlaying)
                return;

            // Snap to tenths: every step is a multiple of 0.1 ml, but accumulating 0.1f drifts far
            // enough to miss floor/ceil windows that sit only 0.1 apart (TBA 8 is 6.8 - 6.9).
            currentWeight = Mathf.Round((currentWeight + milliliter) * 10f) / 10f;
            TetesanSequenceExecutor(totalTetes);
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

            if (animator.runtimeAnimatorController != _runtimeOverride)
                _runtimeOverride = animator.runtimeAnimatorController as AnimatorOverrideController;

            if (_runtimeOverride)
                _runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = animClipOpenKeran;

            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            if (totalTetes == 10)
            {
                if (_runtimeOverride)
                    _runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = animClipTetesBanyak;

                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(10);
                yield return new WaitForSeconds(animClipTetesBanyak.length);
            }
            else if (totalTetes == 1)
            {
                if (_runtimeOverride)
                    _runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = animClipTetes;

                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(1);
                yield return new WaitForSeconds(animClipTetes.length);
            }

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
            if (sampelWeightRoutine != null) StopCoroutine(sampelWeightRoutine);
            sampelWeightRoutine = StartCoroutine(DelaySetSampelWeight());
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