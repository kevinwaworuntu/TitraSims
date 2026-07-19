using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace InteractionLogic
{
    public class PlayAnimByTouchOneShot : MonoBehaviour, IAnimationPlaybackController
    {
        [Header("References")]
        [SerializeField] private PlayAnimOneShot _playAnimOneShot;

        [Header("Touch")]
        [SerializeField] private UnityEvent OnTouch;
        [SerializeField] private UnityEvent OnUntouch;

        private bool _isTouching;

        private void OnEnable()
        {
            EnhancedTouchSupport.Enable();
        }

        private void Update()
        {
            DetectTouch();
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

            if (touching)
            {
                _playAnimOneShot?.Play();
                OnTouch?.Invoke();
            }
            else
            {
                OnUntouch?.Invoke();
            }
        }

        /// <inheritdoc />
        public void StopPlayback()
        {
            _isTouching = false;
            _playAnimOneShot?.StopPlayback();
        }

        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            _isTouching = false;
        }
    }
}