using UnityEngine;
using System;
using System.Collections;
using Config;

namespace Gameplay
{
    public class TahapanInteractionPlayer
    {
        private Animator animator;
        private AnimationConfig animationConfig;
        private MonoBehaviour coroutineRunner;
        private AnimatorOverrideController runtimeOverride;

        private bool isFinished;
        private Coroutine animCoroutine;
        private Coroutine audioCoroutine;

        const float minPlayDuration = 0f;

        public void Initialize(MonoBehaviour coroutineRunner, Animator animator = null, AnimationConfig animationConfig = null)
        {
            this.coroutineRunner = coroutineRunner;
            this.animator = animator;
            this.animationConfig = animationConfig;

            if (animationConfig?.GenericAnimController != null && animator != null)
            {
                runtimeOverride = new AnimatorOverrideController(animationConfig.GenericAnimController);
                animator.runtimeAnimatorController = runtimeOverride;
            }
        }

        public void Play(TahapanInteractionData data, Action onFinishedCallback = null)
        {
            if (!data)
                return;

            StopActiveCoroutines();
            isFinished = false;

            bool isAnimationFinished = data.AnimationClip == null;
            bool isAudioClipFinished = data.AudioNaration == null;

            if (isAnimationFinished && isAudioClipFinished)
            {
                animCoroutine = coroutineRunner.StartCoroutine(
                    WaitForDuration(minPlayDuration, () => onFinishedCallback?.Invoke()));
                return;
            }

            if (animationConfig != null && data.AnimationClip != null && animator != null)
            {
                if (runtimeOverride != null)
                    runtimeOverride[animationConfig.GetAnimGenericClipEntryName()] = data.AnimationClip;

                animator.SetTrigger(animationConfig.StopAnimationParamName);
                animator.SetTrigger(animationConfig.PlayAnimationParamName);
                animCoroutine = coroutineRunner.StartCoroutine(
                    WaitForDuration(Mathf.Max(data.AnimationClip.length, minPlayDuration), () =>
                    {
                        isAnimationFinished = true;
                        TryFinish(isAnimationFinished, isAudioClipFinished, onFinishedCallback);
                    }));
            }

            if (data.AudioNaration != null)
            {
                AudioManager.Instance?.PlayNarration(data.AudioNaration);
                audioCoroutine = coroutineRunner.StartCoroutine(
                    WaitForDuration(Mathf.Max(data.AudioNaration.length, minPlayDuration), () =>
                    {
                        isAudioClipFinished = true;
                        TryFinish(isAnimationFinished, isAudioClipFinished, onFinishedCallback);
                    }));
            }
        }

        public void Stop()
        {
            StopActiveCoroutines();
            if (animator != null && animationConfig != null)
                animator.SetTrigger(animationConfig.StopAnimationParamName);
            AudioManager.Instance?.StopNarration();
        }

        private void StopActiveCoroutines()
        {
            if (animCoroutine != null)
            {
                coroutineRunner.StopCoroutine(animCoroutine);
                animCoroutine = null;
            }
            if (audioCoroutine != null)
            {
                coroutineRunner.StopCoroutine(audioCoroutine);
                audioCoroutine = null;
            }
        }

        private void TryFinish(bool isAnimationFinished, bool isAudioClipFinished, Action onFinishedCallback)
        {
            if (isFinished || !isAnimationFinished || !isAudioClipFinished)
                return;

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