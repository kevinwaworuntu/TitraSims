using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace InteractionLogic
{
    /// <summary>
    /// Fires when the player taps on this object's collider.
    ///
    /// Algorithm:
    ///   On TouchPhase.Began (or mouse button down), casts a ray from Camera.main
    ///   through the tap position. If it hits this GameObject's collider, fires the events.
    ///   Fires on touch-down, not release.
    /// </summary>
    public class TapDetector : MonoBehaviour
    {
        [Header("Events")]
        [Tooltip("Fired each time this object's collider is tapped.")]
        public UnityEvent OnTap;

        /// Fired each time this object's collider is tapped.
        public event Action OnTapDetected;

        private void OnEnable()  => EnhancedTouchSupport.Enable();
        private void OnDisable() => EnhancedTouchSupport.Disable();

        private void Update()
        {
            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                if (t.phase != TouchPhase.Began) continue;

                TryFireTap(t.screenPosition);
                return;
            }

            // Mouse fallback (editor / desktop).
            var mouse = Mouse.current;
            if (Touch.activeTouches.Count == 0 && mouse != null && mouse.leftButton.wasPressedThisFrame)
                TryFireTap(mouse.position.ReadValue());
        }

        private void TryFireTap(Vector2 screenPos)
        {
            var cam = Camera.main;
            if (cam == null) return;

            Ray ray = cam.ScreenPointToRay(screenPos);
            if (Physics.Raycast(ray, out RaycastHit hit) && hit.collider.gameObject == gameObject)
            {
                OnTap?.Invoke();
                OnTapDetected?.Invoke();
            }
        }
    }
}