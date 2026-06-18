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
    /// Detects a circular swirl gesture (one finger making a circular motion).
    /// Intended for Erlenmeyer flask mixing, but usable on any object.
    ///
    /// Algorithm — velocity-angle accumulation:
    ///   Rather than tracking position around a fixed center, this measures how
    ///   much the finger's <em>direction of travel</em> rotates each frame.
    ///   One full revolution of the travel direction (±360°) counts as one swirl.
    ///   This works for any circle size and any starting position.
    ///
    /// Fires on every completion, so continuous stirring fires once per revolution.
    ///
    /// Works independently of GestureController — it reads EnhancedTouch directly
    /// because it needs raw position data each frame, not higher-level gesture events.
    /// EnhancedTouch is ref-counted; both components calling Enable/Disable is safe.
    ///
    /// Add this component to the Erlenmeyer (or any relevant object). Subscribe to
    /// <see cref="OnSwirl"/> in the Inspector or <see cref="OnSwirlDetected"/> in code.
    /// </summary>
    public class SwirlGestureDetector : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Detection Thresholds")]
        [Tooltip("Cumulative velocity-angle rotation needed to count as one swirl")]
        [SerializeField, Range(90f, 720f)]  private float _completionAngle     = 360f;

        [Tooltip("The swirl must be completed within this many seconds or progress resets")]
        [SerializeField, Range(0.5f, 6f)]   private float _timeWindow          = 2.5f;

        [Tooltip("Minimum finger speed (screen pixels/sec) to register movement. "
               + "Filters accidental drift and stationary contact.")]
        [SerializeField, Range(20f, 400f)]  private float _minSpeedPixels      = 80f;

        [Tooltip("If the travel direction reverses by this many degrees, progress resets. "
               + "Prevents back-and-forth motion from accumulating into a false swirl.")]
        [SerializeField, Range(10f, 90f)]   private float _reverseResetDegrees = 45f;

        [Header("Events")]
        [Tooltip("Fired each time a swirl is completed. Inspector-wirable.")]
        public UnityEvent OnSwirl;

        // ── Public C# event (code-only, carries duration) ────────────────────────

        /// Fired each time a swirl is completed.
        /// Arg: how many seconds the swirl took — use to scale animation speed.
        public event Action<float> OnSwirlDetected;

        // ── Internal state ───────────────────────────────────────────────────────

        private bool    _tracking;
        private float   _elapsed;
        private float   _signedTotal;    // signed accumulation of velocity-angle deltas
        private Vector2 _prevPos;
        private float   _prevVelAngle;
        private bool    _hasVelBaseline; // true once we have a first velocity sample

        // Mouse fallback (editor / desktop)
        private bool    _mouseWasDown;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void OnEnable()  => EnhancedTouchSupport.Enable();
        private void OnDisable()
        {
            EnhancedTouchSupport.Disable();
            ResetTracking();
        }

        private void Update()
        {
            // Mirror GestureController's live-touch gathering (skip Ended/Canceled).
            Touch liveTouch = default;
            int   liveCount = 0;

            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                liveTouch = t;
                liveCount++;
            }

            // Mouse fallback when no real touches (editor / desktop).
            var mouse = Mouse.current;
            if (liveCount == 0 && mouse != null && mouse.leftButton.isPressed)
            {
                UpdateMouse(mouse);
                return;
            }

            _mouseWasDown = false;

            // Swirl requires exactly one finger — anything else cancels tracking.
            if (liveCount != 1)
            {
                ResetTracking();
                return;
            }

            if (!_tracking || liveTouch.phase == TouchPhase.Began)
            {
                StartTracking(liveTouch.screenPosition);
                return;
            }

            ProcessPosition(liveTouch.screenPosition);
        }

        // ── Core logic ───────────────────────────────────────────────────────────

        private void StartTracking(Vector2 pos)
        {
            _tracking        = true;
            _elapsed         = 0f;
            _signedTotal     = 0f;
            _prevPos         = pos;
            _hasVelBaseline  = false;
        }

        private void ResetTracking()
        {
            _tracking       = false;
            _elapsed        = 0f;
            _signedTotal    = 0f;
            _hasVelBaseline = false;
        }

        private void ProcessPosition(Vector2 pos)
        {
            _elapsed += Time.deltaTime;

            if (_elapsed > _timeWindow)
            {
                ResetTracking();
                return;
            }

            // Velocity in pixels/sec.
            Vector2 velocity = (pos - _prevPos) / Time.deltaTime;
            _prevPos = pos;

            // Skip frames where the finger is nearly stationary.
            if (velocity.sqrMagnitude < _minSpeedPixels * _minSpeedPixels)
                return;

            float velAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;

            // First valid velocity sample — record baseline and wait for the next frame.
            if (!_hasVelBaseline)
            {
                _prevVelAngle   = velAngle;
                _hasVelBaseline = true;
                return;
            }

            // Signed delta: positive = CCW, negative = CW (screen space).
            // DeltaAngle returns the shortest path, range (-180, 180].
            float delta = Mathf.DeltaAngle(_prevVelAngle, velAngle);
            _prevVelAngle = velAngle;

            // Reversal check: if the delta strongly opposes the accumulated direction,
            // the user changed their circular motion — reset rather than subtract slowly.
            if (_signedTotal != 0f
                && Mathf.Sign(delta) != Mathf.Sign(_signedTotal)
                && Mathf.Abs(delta) >= _reverseResetDegrees)
            {
                ResetTracking();
                return;
            }

            _signedTotal += delta;

            // Check for completion.
            if (Mathf.Abs(_signedTotal) >= _completionAngle)
            {
                float duration = _elapsed;

                OnSwirl.Invoke();
                OnSwirlDetected?.Invoke(duration);

                // Reset progress but keep tracking so continuous stirring
                // fires once per revolution without requiring a new touch-down.
                _signedTotal    = 0f;
                _elapsed        = 0f;
                _hasVelBaseline = false;
            }
        }

        // ── Mouse fallback ───────────────────────────────────────────────────────

        private void UpdateMouse(Mouse mouse)
        {
            Vector2 pos = mouse.position.ReadValue();

            if (!_mouseWasDown)
            {
                StartTracking(pos);
                _mouseWasDown = true;
                return;
            }

            ProcessPosition(pos);
        }

        // ── Editor ───────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        [Header("Debug")]
        [SerializeField] private bool _showDebugGizmo = false;

        private void OnGUI()
        {
            if (!_showDebugGizmo || !_tracking) return;

            float pct = Mathf.Abs(_signedTotal) / _completionAngle;
            string dir = _signedTotal >= 0f ? "CCW" : "CW";

            GUI.Label(new Rect(10, 10, 300, 20),
                $"Swirl: {pct * 100f:F0}%  {dir}  t={_elapsed:F1}s");
        }
#endif
    }
}
