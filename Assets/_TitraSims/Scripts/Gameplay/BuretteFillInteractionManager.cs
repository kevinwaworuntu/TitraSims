using System.Collections;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Gameplay
{
    public class BuretteFillInteractionManager : ARContentManager
    {
        [Header("Target Configuration")]
        [SerializeField] private float[] weights;
        [SerializeField] private float[] floorTargets;
        [SerializeField] private float[] ceilTargets;
        
        [Header("Animation References")]
        [SerializeField] private AnimationClip animClipOpenKeran;
        [SerializeField] private AnimationClip animClipTetes;
        [SerializeField] private AnimationClip animClipTetesBanyak;
        [SerializeField] private Animator animator;
        
        [Header("Titration Visuals")]
        [SerializeField] private Renderer titrasiRenderer;
        [SerializeField] private Color initialColor;
        [SerializeField] private Color targetColor;
        [SerializeField] private TextMeshProUGUI textSampelWeight;
        
        [FormerlySerializedAs("burretFillObject")]
        [Header("BURETTE FILL SETTINGS")]
        [SerializeField] private Transform buretteFillObject;
        [FormerlySerializedAs("burretMeniskusObject")] [SerializeField] private Transform buretteMeniskusObject;
        private float fillObjectScaleModifier = 0.1790f;
        private float fillObjectInitialScale = 90.93853f;
        private float meniskusPositionModifier = 0.00107706f;
        private float meniskusInitPos = 0.578676f;
        
        private bool isPlaying = false;
        private Material titrasiMatInstance;
        protected bool isCheckWeightToContinue;
        private float currentWeight = 0;
        private int currentIndex; 
        
        private static readonly int SideColorID = Shader.PropertyToID("_Side_Color");
        private static readonly int TopColorID = Shader.PropertyToID("_TopColor");
        
        private void Awake()
        {
            titrasiMatInstance = titrasiRenderer.material;
        }

        private void OnEnable()
        {
            base.OnEnable();
            
            ResetBuretteFill();
            RestartErlenmeyerVisual();
        }
        
        protected override void OnStartWaitingForPlayerInputToContinueHandler()
        {
            if(isCheckWeightToContinue)
            {
                if (!IsCurrentWeightComplete())
                {
                    return;
                }
                //Extra safety
                titrasiMatInstance.SetColor(SideColorID, targetColor);
                titrasiMatInstance.SetColor(TopColorID, targetColor);
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
            return currentWeight >= floorTargets[currentIndex] && currentWeight <= ceilTargets[currentIndex];
        }
        
        private bool IsCurrentWeightExceedTarget()
        {
            return currentWeight > ceilTargets[currentIndex];
        }
        
        private void SetButtonEnabledState(bool enabled)
        {
            foreach (var contextualButton in ContextualButtonController.Instance.GetContextualButtons())
            {
                contextualButton.SetEnabled(enabled);
            }
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
            titrasiMatInstance.SetColor(SideColorID, initialColor);
            titrasiMatInstance.SetColor(TopColorID, initialColor);
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
            {
                return;
            }
            StartCoroutine(TetesanSequence(totalTetes));
        }
        
        private IEnumerator TetesanSequence(int totalTetes)
        {
            isPlaying = true;
            SetButtonEnabledState(false);
            var animationConfig = GameManager.Instance.AnimationConfig;
            
            if (animationConfig.GenericAnimController)
            {
                animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipOpenKeran;
            }
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(animClipOpenKeran.length);

            if (totalTetes == 10)
            {
                if (animationConfig.GenericAnimController)
                {
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipTetesBanyak;
                }
                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(10);
                yield return new WaitForSeconds(animClipTetesBanyak.length);
            }
            else if (totalTetes == 1){
                if (animationConfig.GenericAnimController)
                {
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = animClipTetes;
                }
                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                BuretteFillValue(1);
                yield return new WaitForSeconds(animClipTetes.length);
            }
            
            animator.SetTrigger(animationConfig.PlayAnimationParamName);
            yield return new WaitForSeconds(totalTetes == 1 ? animClipTetes.length : animClipTetesBanyak.length );

            SetButtonEnabledState(true);
            isPlaying = false;
            if (IsCurrentWeightComplete())
            {
                StartCoroutine(DelaySetTargetColor());
                IEnumerator DelaySetTargetColor()
                {
                    yield return new WaitForSeconds(1);
                    titrasiMatInstance.SetColor(SideColorID, targetColor);
                    titrasiMatInstance.SetColor(TopColorID, targetColor);
                }
                OnStartWaitingForPlayerInputToContinueHandler();
            }
            else
            {
                if (IsCurrentWeightExceedTarget())
                {
                    RestartCurrentInteraction();
                }
            }
        }

        public void SetSampelWeightTextVisible()
        {
            StartCoroutine(DelaySetSampelWeight());
            IEnumerator DelaySetSampelWeight() // Wait for current index updated
            {
                yield return new WaitForSeconds(1f);
                textSampelWeight.gameObject.SetActive(true);
                textSampelWeight.SetText($"Bobot penimbangan: {weights[currentIndex]} mg");
           
            }
        }

        private void BuretteFillValue(float targetMl)
        {
            var targetScaleZ = buretteFillObject.transform.localScale.z - fillObjectScaleModifier * targetMl;
            buretteFillObject.transform.localScale = new Vector3(buretteFillObject.transform.localScale.x, buretteFillObject.transform.localScale.y, targetScaleZ);
            
            var targetY = buretteMeniskusObject.transform.localPosition.y - meniskusPositionModifier * targetMl;
            buretteMeniskusObject.transform.localPosition = new Vector3(buretteMeniskusObject.transform.localPosition.x, targetY, buretteMeniskusObject.transform.localPosition.z);
        }

        public void ResetBuretteFill()
        {
            buretteFillObject.transform.localScale = new Vector3(buretteFillObject.transform.localScale.x, buretteFillObject.transform.localScale.y, fillObjectInitialScale);
            buretteMeniskusObject.transform.localPosition = new Vector3(buretteMeniskusObject.transform.localPosition.x, meniskusInitPos, buretteMeniskusObject.transform.localPosition.z);
        }
    }
}