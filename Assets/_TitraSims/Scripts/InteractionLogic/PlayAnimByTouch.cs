using Config;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace InteractionLogic
{
    /// <summary>
    /// Plays an animation clip by normalized progression while this object's collider
    /// is being touched/held, and pauses (freezes at the current frame) on release.
    ///
    /// Same override-controller setup as PlayAnimByProgression, but progression advances
    /// continuously at <see cref="progressionSpeed"/> percent/sec for as long as the touch
    /// that started on this object is held, instead of being driven by discrete external calls.
    /// </summary>
    public class PlayAnimByTouch : MonoBehaviour, IAnimationPlaybackController
    {
        [Header("References")]
        [SerializeField] private Animator        _animator;
        [SerializeField] private AnimationConfig _animationConfig; // Todo : get from game manager, once completed
        [SerializeField] private AnimationClip   _animationClip;

        [Header("Touch")]
        [Tooltip("Percentage of progression (0-100) advanced per second while held.")]
        [SerializeField] private float progressionSpeed = 1;
        [SerializeField] private int loopCountNeeded = 4;
        [SerializeField] private UnityEvent OnTouch;
        [SerializeField] private UnityEvent OnUntouch;
        [SerializeField] private UnityEvent OnLoopCountComplete;

        private AnimatorOverrideController _runtimeOverride;
        private float currentProgression = 0;
        private int _loopCount;
        private bool _isTouching;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();

            if (_animator != null)
            {
                _animator.enabled = true;
            }

            if (_animator == null || _animationConfig?.GenericAnimController == null)
            {
                return;
            }

            if (_animator.runtimeAnimatorController is AnimatorOverrideController existingOverride)
            {
                _runtimeOverride = existingOverride;
            }
            else
            {
                _runtimeOverride = new AnimatorOverrideController(_animationConfig.GenericAnimController);
                _animator.runtimeAnimatorController = _runtimeOverride;
            }
        }
        
        private void Update()
        {
            DetectTouch();

            if (_isTouching)
            {
                PlayAnimation();
            }

            if (_runtimeOverride != _animator.runtimeAnimatorController) // Todo : Optimize later
            {
                _runtimeOverride = _animator.runtimeAnimatorController as AnimatorOverrideController;
            }
        }

        public void Reset()
        {
            currentProgression = 0;
            _loopCount = 0;
        }
        
        private void DetectTouch()
        {
            Touch liveTouch = default;
            int liveCount = 0;

            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                liveTouch = t;
                liveCount++;
            }

            var mouse = Mouse.current;
            if (liveCount == 0 && mouse != null && mouse.leftButton.isPressed)
            {
                if (!_isTouching)
                {
                    TryBeginTouch(mouse.position.ReadValue());
                }
                return;
            }

            if (liveCount == 0)
            {
                SetTouching(false);
                return;
            }

            if (!_isTouching || liveTouch.phase == TouchPhase.Began)
            {
                TryBeginTouch(liveTouch.screenPosition);
            }
        }

        private void TryBeginTouch(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            bool hitSelf = Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject;
            if (hitSelf)
            {
                SetTouching(true);
            }
        }

        private void SetTouching(bool touching)
        {
            if (_isTouching == touching) return;

            _isTouching = touching;

            // Animator keeps advancing the current state on its own once played, so
            // explicitly stop it on release instead of just skipping further Play() calls.
            if (_animator != null)
            {
                _animator.speed = touching ? 1f : 0f;
            }

            if (touching)
            {
                OnTouch?.Invoke();
            }
            else
            {
                OnUntouch?.Invoke();
            }
        }

        private void PlayAnimation()
        {
            if (_animator == null)
            {
                return;
            }

            var targetStateName = _animationConfig.GetCustomAnimEntryStateName();
            // if (_runtimeOverride[targetStateName] == null)
            // {
            //     _runtimeOverride[targetStateName] = _animationClip;
            // }
            
            _runtimeOverride[_animationConfig.GetCustomAnimGenericClipEntryName()] = _animationClip;

            currentProgression += progressionSpeed * Time.deltaTime;
            _animator.Play(targetStateName, 0, (currentProgression % 100) / 100f);

            if (currentProgression >= 100f)
            {
                currentProgression -= 100f;
                _loopCount++;
                if (_loopCount >= loopCountNeeded)
                {
                    enabled = false;
                    OnLoopCountComplete?.Invoke();
                }
            }
        }

        /// <inheritdoc />
        public void StopPlayback()
        {
            SetTouching(false);
            currentProgression = 0f;
            _loopCount = 0;
            enabled = false;
            
            // if (_animator != null)
            // {
            //     _animator.enabled = false;
            // }
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            _isTouching = false;
            if (_animator != null)
            {
                _animator.speed = 1f;
                _runtimeOverride[_animationConfig.GetCustomAnimGenericClipEntryName()] = null;
            }
        }
    }
}