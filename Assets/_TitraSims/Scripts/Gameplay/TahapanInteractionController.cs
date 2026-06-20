using System;
using System.Collections;
using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Gameplay
{
    [RequireComponent(typeof(TahapanInteractionUI))]
    public class TahapanInteractionController : MonoBehaviour
    {
        [Serializable]
        private struct TahapanInteractionMappingStruct
        {
            public TahapanInteractionData InteractionData;
            [FormerlySerializedAs("IsNeedPlayerInputToContinue")] public bool IsAutoContinue;
            public bool IsContinueByOtherEvent;
            public UnityEvent UniqueEvent;
        }
        
        [Serializable]
        private struct TahapanInteractionDoneConditionsStruct
        {
            public bool IsNeedPlayerInputToContinue;
            public UnityEvent DoneCondition;
        }

        [SerializeField] private TahapanInteractionMappingStruct[] interactionDataActionMappings;

        [SerializeField] private Animator animator;

        private int currentInteractionIndex;
        private bool isPlaying;
        private TahapanInteractionPlayer interactionPlayer;

        public event Action<TahapanInteractionData, bool> OnInteractionEnter;
        public event Action OnStartWaitingForPlayerInputToContinue;
        public event Action OnInteractionComplete;
        public event Action OnFinishPlayingInteraction;

        void Awake()
        {
            interactionPlayer = new TahapanInteractionPlayer();
            if (!GameManager.Instance)
            {
                return;
            }
            if (!GameManager.Instance.AnimationConfig)
            {
                return;
            }
            interactionPlayer.Initialize(this, animator, GameManager.Instance.AnimationConfig);
        }

        private void OnEnable()
        {
            ResetState();
        }
        
        public void StartInteraction()
        {
            EnterInteractionState(currentInteractionIndex);
        }
        
        public void ContinueInteraction()
        {
            EnterInteractionState(currentInteractionIndex);
        }

        public void RestartInteraction()
        {
            isPlaying = false;
            EnterInteractionState(currentInteractionIndex);
        }
        
        public void PlayerInteractToFinishInteraction()
        {
            if (!isPlaying)
            {
                return;
            }
            if (interactionDataActionMappings[currentInteractionIndex].IsAutoContinue)
            {
                return;
            }
            ExitInteractionState();
        }

        private void EnterInteractionState(int targetIndex)
        {
            if (!CanEnterState(targetIndex))
            {
                return;
            }

            currentInteractionIndex = targetIndex;
            isPlaying = true;

            var mapping = interactionDataActionMappings[currentInteractionIndex];
            OnInteractionEnter?.Invoke(mapping.InteractionData, HasAnimationClip());

            ExecuteInteractionState();
        }

        private void ExecuteInteractionState()
        {
            var mapping = interactionDataActionMappings[currentInteractionIndex];
            mapping.UniqueEvent?.Invoke();
            interactionPlayer.Play(mapping.InteractionData, () =>
            {
                if (mapping.IsContinueByOtherEvent)
                {
                    return;
                }
                if (!mapping.IsAutoContinue)
                {
                    OnStartWaitingForPlayerInputToContinue?.Invoke();
                    return;
                }

                StartCoroutine(EndTahapanDelay());
                IEnumerator EndTahapanDelay()
                {
                    float delay = GameManager.Instance?.AnimationConfig != null ? GameManager.Instance.AnimationConfig.InteractionEndDelay : 0f;
                    yield return new WaitForSeconds(delay);
                    ExitInteractionState();
                }
            });
        }

        private void ExitInteractionState()
        {
            isPlaying = false;
            currentInteractionIndex++;
            OnFinishPlayingInteraction?.Invoke();
            if (currentInteractionIndex >= interactionDataActionMappings.Length) OnInteractionComplete?.Invoke();
        }

        private bool CanEnterState(int targetIndex)
        {
            if (targetIndex >= interactionDataActionMappings.Length)
            {
                return false;
            }
            if (targetIndex < 0)
            {
                return false;
            }
            if (!interactionDataActionMappings[targetIndex].InteractionData)
            {
                return false;
            }

            if (isPlaying)
            {
                return false;
            }
            return true;
        }

        private bool HasAnimationClip()
        {
            return interactionDataActionMappings[currentInteractionIndex].InteractionData.AnimationClip;
        }

        public void PlayAnimation()
        {
            if (currentInteractionIndex >= interactionDataActionMappings.Length)
                return;
            interactionPlayer.Play(interactionDataActionMappings[currentInteractionIndex].InteractionData);
        }

        public void StopAnimation() => interactionPlayer.Stop();

        public bool IsPlayingLastIndex()
        {
            return interactionDataActionMappings.Length - 1 == currentInteractionIndex;
        }
        
        private void ResetState()
        {
            currentInteractionIndex = 0;
            isPlaying = false;
        }
    }
}