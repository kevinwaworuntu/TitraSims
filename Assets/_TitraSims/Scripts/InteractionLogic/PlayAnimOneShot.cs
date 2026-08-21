using System.Collections;
using Config;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionLogic
{
    public class PlayAnimOneShot : MonoBehaviour, IAnimationPlaybackController
    {
        [Header("References")]
        [SerializeField] private Animator        _animator;
        [SerializeField] private AnimationConfig _animationConfig; // Todo : get from game manager, once completed
        [SerializeField] private AnimationClip   _animationClip;

        [SerializeField] private UnityEvent OnComplete;

        private AnimatorOverrideController _runtimeOverride;
        private RuntimeAnimatorController  _originalController;
        private Coroutine                  _playRoutine;

        private void Awake()
        {
            if (_animator != null)
            {
                _originalController = _animator.runtimeAnimatorController;
            }
        }

        public void Play()
        {
            if (_animator == null || _animationConfig?.GenericAnimController == null || _animationClip == null)
            {
                return;
            }

            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
            }

            _animator.enabled = true;

            if (_animator.runtimeAnimatorController is AnimatorOverrideController existingOverride
                && existingOverride.runtimeAnimatorController == _animationConfig.GenericAnimController)
            {
                _runtimeOverride = existingOverride;
            }
            else
            {
                _runtimeOverride = new AnimatorOverrideController(_animationConfig.GenericAnimController);
                _animator.runtimeAnimatorController = _runtimeOverride;
            }

            _runtimeOverride[_animationConfig.GetCustomAnimGenericClipEntryName()] = _animationClip;

            _animator.Play(_animationConfig.GetCustomAnimEntryStateName(), 0, 0f);

            _playRoutine = StartCoroutine(WaitForCompletion());
        }

        private IEnumerator WaitForCompletion()
        {
            yield return new WaitForSeconds(_animationClip.length);

            _playRoutine = null;
            RestoreAnimator();
            OnComplete?.Invoke();
        }

        private void RestoreAnimator()
        {
            _runtimeOverride[_animationConfig.GetCustomAnimGenericClipEntryName()] = null;
            if (_animator != null)
            {
                _animator.runtimeAnimatorController = _originalController;
            }
        }

        /// <inheritdoc />
        public void StopPlayback()
        {
            if (_playRoutine != null)
            {
                StopCoroutine(_playRoutine);
                _playRoutine = null;
            }

            RestoreAnimator();
        }

        public void SetAnimationClip(AnimationClip clip)
        {
            _animationClip = clip;
        }
    }
}