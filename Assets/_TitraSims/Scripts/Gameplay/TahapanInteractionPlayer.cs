using UnityEngine;
using System;
using System.Collections;
using Config;

namespace Gameplay
{
    public class TahapanInteractionPlayer
    {
        private Animator animator;
        private AudioSource audioSource;
        private AnimationConfig animationConfig;
        private MonoBehaviour coroutineRunner;

        private bool isFinished;
       
        // Spare duration before auto finish (2.5 seconds)
        const float minPlayDuration = 0f;
        
        public void Initialize(MonoBehaviour coroutineRunner, Animator animator = null, AudioSource audioSource = null, AnimationConfig animationConfig = null)
        {
            this.coroutineRunner = coroutineRunner;
            this.animator = animator;
            this.audioSource = audioSource;
            this.animationConfig = animationConfig;
        }

        public void Play(TahapanInteractionData data, Action onFinishedCallback = null)
        {
            if (!data)
            {
                return;
            }

            isFinished = false;
            if (UIManager.Instance)
            {
                if (string.IsNullOrEmpty(data.Title) && string.IsNullOrEmpty(data.Description))
                {
                    UIManager.Instance.NarationPanel.gameObject.SetActive(false);
                }
                else
                {
                    UIManager.Instance.NarationPanel.SetTitleText(data.Title);
                    UIManager.Instance.NarationPanel.SetDescriptionText(data.Description);     
                }
            }

            bool isAnimationFinished = data.AnimationClip == null;
            bool isAudioClipFinished = data.AudioNaration == null;
            
            if (isAnimationFinished && isAudioClipFinished)
            {
                coroutineRunner.StartCoroutine(WaitForDuration(minPlayDuration, () => 
                    { 
                        onFinishedCallback?.Invoke(); 
                    }
                ));
                return;
            }

            if (animationConfig)
            {
                if (animationConfig.GenericAnimController)
                {
                    animationConfig.GenericAnimController[animationConfig.GetAnimGenericClipEntryName()] = data.AnimationClip;
                }
                if (data.AnimationClip != null)
                {
                    if (animator != null)
                    {
                        animator.SetTrigger(animationConfig.StopAnimationParamName);
                        animator.SetTrigger(animationConfig.PlayAnimationParamName);
                        coroutineRunner.StartCoroutine(WaitForDuration(Mathf.Max(data.AnimationClip.length, minPlayDuration), () => // This is only valid if animation speed is constant
                            { 
                                isAnimationFinished = true;
                                TryFinish(isAnimationFinished, isAudioClipFinished, () =>
                                {
                                    onFinishedCallback?.Invoke();
                                }); 
                            }
                        ));
                    }
                }
            }
            if (data.AudioNaration != null)
            {
                if(audioSource) audioSource.PlayOneShot(data.AudioNaration);
                coroutineRunner.StartCoroutine(WaitForDuration(Mathf.Max(data.AudioNaration.length, minPlayDuration), () => 
                    { 
                        isAudioClipFinished = true;
                        TryFinish(isAnimationFinished, isAudioClipFinished, () =>
                        {
                            onFinishedCallback?.Invoke();
                        }); 
                    }
                ));
            }
        }

        public void Stop()
        {
            if (animator != null)
            {
                if (animationConfig != null)
                {
                    animator.SetTrigger(animationConfig.StopAnimationParamName);
                }
            }
            if (audioSource != null)
            {
                audioSource.Stop();
            }
        }

        private void TryFinish(bool isAnimationFinished, bool isAudioClipFinished, Action onFinishedCallback)
        {
            if (isFinished)
            {
                return;
            }
            if (!isAnimationFinished || !isAudioClipFinished) return;

            isFinished = true;
            onFinishedCallback?.Invoke();
        }

        private IEnumerator WaitForDuration(float duration, Action onFinishedCallback)
        {
            yield return new WaitForSeconds(duration);
            onFinishedCallback?.Invoke();
        }
    }
}