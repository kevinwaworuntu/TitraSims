using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace InteractionLogic
{
    /// <summary>
    /// Singleton. Reads all touch/mouse input each frame, classifies gestures,
    /// and dispatches to the focused ObjectManipulator or fires public events
    /// for other systems (CameraZoom, SwirlDetector, SnapInteractable, etc.).
    ///
    /// Default gesture map (matches GestureSettings defaults):
    ///   1 finger  →  rotate focused object
    ///   2 fingers spreading/pinching  →  scale focused object  (or OnPinchUpdate if no focus)
    ///   2 fingers translating         →  drag  focused object  (or OnTwoFingerDragDelta if no focus)
    ///
    /// Assign a GestureSettings asset to _settings to override per-gesture finger counts.
    /// </summary>
    [DefaultExecutionOrder(-10)]
    public class GestureController : MonoBehaviour
    {
        public static GestureController Instance { get; private set; }

        // ── Public events ────────────────────────────────────────────────────────

        /// Fired on first touch contact with an ObjectManipulator.
        public event Action<ObjectManipulator> OnManipulatorGrabbed;

        /// Fired when all contact with a focused ObjectManipulator ends.
        public event Action<ObjectManipulator> OnManipulatorReleased;

        /// Fired once when a multi-touch gesture begins. Arg: initial finger distance in pixels.
        public event Action<float> OnPinchBegin;

        /// Fired every frame during a pinch that has no focused object.
        public event Action<float> OnPinchUpdate;

        /// Fired when a pinch gesture ends.
        public event Action OnPinchEnd;

        /// Fired every frame during a drag gesture that has no focused object.
        public event Action<Vector2> OnTwoFingerDragDelta;

        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Gesture Settings")]
        [Tooltip("Optional asset to configure per-gesture finger counts. Leave empty for defaults (rotate=1, drag=2, scale=2).")]
        [SerializeField] private GestureSettings _settings;

        [Header("Raycast")]
        [SerializeField] private LayerMask _interactableLayer = ~0;
        [SerializeField] private float _raycastDistance = 100f;

        [Header("Two-Touch Disambiguation")]
        [Tooltip("Fraction of initial distance that must change before committing to Pinch")]
        [SerializeField, Range(0.01f, 0.15f)] private float _pinchCommitRatio = 0.04f;

        [Tooltip("Screen pixels the midpoint must travel before committing to Drag")]
        [SerializeField, Range(2f, 20f)] private float _dragCommitPixels = 6f;

        // ── Internal types ───────────────────────────────────────────────────────

        private enum State          { Idle, SingleTouch, Multi }
        private enum TwoGestureKind { Undecided, Pinch, Drag }

        // ── State ────────────────────────────────────────────────────────────────

        private State             _state         = State.Idle;
        private TwoGestureKind    _twoKind       = TwoGestureKind.Undecided;
        private ObjectManipulator _focused;
        private Camera            _cam;
        private int               _prevLiveCount;

        // Single-touch prev position (shared between rotate, single-finger drag, and mouse)
        private Vector2 _singlePrev;

        // Multi-touch
        private float   _twoPrevDist;
        private Vector2 _twoPrevCenter;
        private float   _twoBaseDist;
        private Vector2 _twoBaseCenter;
        private bool    _dragJustCommitted;

        // Mouse fallback
        private bool _mouseWasDown;

        // ── Settings accessors with defaults ─────────────────────────────────────

        private int RotateCount => _settings != null ? _settings.rotateFingersNeeded : 1;
        private int DragCount   => _settings != null ? _settings.dragFingersNeeded   : 2;
        private int ScaleCount  => _settings != null ? _settings.scaleFingersNeeded  : 2;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        private void Start()     => _cam = Camera.main;
        private void OnEnable()  => EnhancedTouchSupport.Enable();
        private void OnDisable() => EnhancedTouchSupport.Disable();

        private void Update()
        {
            Touch live0 = default, live1 = default, live2 = default;
            int   liveCount = 0;

            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                if      (liveCount == 0) live0 = t;
                else if (liveCount == 1) live1 = t;
                else if (liveCount == 2) live2 = t;
                liveCount++;
            }

            var mouse = Mouse.current;
            if (liveCount == 0 && mouse != null && mouse.leftButton.isPressed)
            {
                UpdateMouseFallback(mouse);
                return;
            }
            _mouseWasDown = false;

            DispatchGesture(liveCount, live0, live1, live2);
            _prevLiveCount = liveCount;
        }

        // ── Gesture dispatch ─────────────────────────────────────────────────────

        private void DispatchGesture(int count, Touch t0, Touch t1, Touch t2)
        {
            if (count == 0) { HandleIdle(); return; }

            int rotC  = RotateCount;
            int dragC = DragCount;
            int scaC  = ScaleCount;

            // Scale requires ≥2 touches (pinch needs two distinct points).
            bool canScale  = count == scaC && count >= 2;
            bool canDrag   = count == dragC;
            bool canRotate = count == rotC && !canScale && !canDrag;

            if (canScale || canDrag)
            {
                if (count >= 2)
                {
                    // Multi-touch path: handles pinch / drag disambiguation.
                    if (_state == State.SingleTouch)
                        EnterMultiFromSingle(count, t0, t1, t2);
                    else
                        HandleMulti(count, t0, t1, t2);
                }
                else
                {
                    // Single-finger drag (dragC == 1).
                    HandleSingleDrag(t0);
                }
            }
            else if (canRotate)
            {
                if (_state == State.Multi)
                    EnterSingleFromMulti(t0);
                else
                    HandleRotate(t0);
            }
            else
            {
                // Finger count doesn't match any gesture.
                HandleIdle();
            }
        }

        // ── Idle ─────────────────────────────────────────────────────────────────

        private void HandleIdle()
        {
            if (_state == State.Idle) return;
            if (_state == State.Multi) OnPinchEnd?.Invoke();
            ResetState();
        }

        // ── Rotate ───────────────────────────────────────────────────────────────

        private void HandleRotate(Touch t)
        {
            if (_state == State.Idle)
            {
                bool hadFocused = _focused != null;
                _focused    = Raycast(t.screenPosition);
                _singlePrev = t.screenPosition;
                _state      = State.SingleTouch;

                if (!hadFocused && _focused != null)
                    OnManipulatorGrabbed?.Invoke(_focused);
                return;
            }

            if (t.phase == TouchPhase.Moved)
                _focused?.ReceiveRotateDelta(t.screenPosition - _singlePrev);

            _singlePrev = t.screenPosition;
        }

        private void EnterSingleFromMulti(Touch t)
        {
            OnPinchEnd?.Invoke();
            _twoKind    = TwoGestureKind.Undecided;
            _state      = State.Idle;
            // _focused is preserved; treat this as continuing the grab.
            _singlePrev = t.screenPosition;
            _state      = State.SingleTouch;
        }

        // ── Single-finger drag ───────────────────────────────────────────────────

        private void HandleSingleDrag(Touch t)
        {
            // Transition from multi-touch (e.g. going from 2 fingers → 1 finger drag).
            if (_state == State.Multi)
            {
                OnPinchEnd?.Invoke();
                _twoKind = TwoGestureKind.Undecided;
                _state   = State.Idle;
            }

            if (_state == State.Idle)
            {
                bool hadFocused = _focused != null;
                _focused    = Raycast(t.screenPosition);
                _singlePrev = t.screenPosition;
                _state      = State.SingleTouch;

                if (!hadFocused && _focused != null)
                    OnManipulatorGrabbed?.Invoke(_focused);

                _focused?.BeginDrag(t.screenPosition);
                return;
            }

            if (t.phase == TouchPhase.Moved)
            {
                if (_focused != null) _focused.ReceiveDragToPoint(t.screenPosition);
                else                  OnTwoFingerDragDelta?.Invoke(t.screenPosition - _singlePrev);
            }

            _singlePrev = t.screenPosition;
        }

        // ── Multi-touch (drag / scale) ───────────────────────────────────────────

        private void EnterMultiFromSingle(int count, Touch t0, Touch t1, Touch t2)
        {
            // Inherit focus from single-touch; do NOT re-fire OnManipulatorGrabbed.
            Vector2 mid = GetCenter(t0, t1, t2, count);
            _twoBaseDist   = _twoPrevDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            _twoBaseCenter = _twoPrevCenter = mid;
            _twoKind       = TwoGestureKind.Undecided;
            _state         = State.Multi;
            OnPinchBegin?.Invoke(_twoBaseDist);
        }

        private void HandleMulti(int count, Touch t0, Touch t1, Touch t2)
        {
            bool justEntered    = _state != State.Multi;
            bool comingFromIdle = _state == State.Idle;

            if (justEntered)
            {
                Vector2 mid = GetCenter(t0, t1, t2, count);
                if (_focused == null) _focused = Raycast(mid);

                _twoBaseDist   = _twoPrevDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
                _twoBaseCenter = _twoPrevCenter = mid;
                _twoKind       = TwoGestureKind.Undecided;
                _state         = State.Multi;

                if (comingFromIdle && _focused != null)
                    OnManipulatorGrabbed?.Invoke(_focused);

                OnPinchBegin?.Invoke(_twoBaseDist);
                return;
            }

            // Finger count changed while already in multi-touch — re-baseline.
            if (count != _prevLiveCount)
            {
                _twoKind       = TwoGestureKind.Undecided;
                Vector2 mid    = GetCenter(t0, t1, t2, count);
                _twoBaseDist   = _twoPrevDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
                _twoBaseCenter = _twoPrevCenter = mid;
                return;
            }

            float   curDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            Vector2 curCenter = GetCenter(t0, t1, t2, count);

            int dragC = DragCount;
            int scaC  = ScaleCount;

            if (_twoKind == TwoGestureKind.Undecided)
            {
                bool bothMatch = count == dragC && count == scaC;

                if (bothMatch)
                {
                    // Disambiguate: pinch vs drag.
                    float distChange   = Mathf.Abs(curDist - _twoBaseDist) / Mathf.Max(_twoBaseDist, 0.001f);
                    float centerTravel = Vector2.Distance(curCenter, _twoBaseCenter);

                    if (distChange >= _pinchCommitRatio)
                        _twoKind = TwoGestureKind.Pinch;
                    else if (centerTravel >= _dragCommitPixels)
                    {
                        _twoKind           = TwoGestureKind.Drag;
                        _dragJustCommitted = true;
                    }
                }
                else
                {
                    // No ambiguity — assign directly.
                    _twoKind = (count == scaC) ? TwoGestureKind.Pinch : TwoGestureKind.Drag;
                    if (_twoKind == TwoGestureKind.Drag) _dragJustCommitted = true;
                }
            }

            switch (_twoKind)
            {
                case TwoGestureKind.Pinch:
                    float scaleFactor = _twoPrevDist > 0f ? curDist / _twoPrevDist : 1f;
                    if (_focused != null) _focused.ReceivePinchDelta(scaleFactor);
                    else                  OnPinchUpdate?.Invoke(curDist);
                    break;

                case TwoGestureKind.Drag:
                    if (_dragJustCommitted)
                    {
                        _focused?.BeginDrag(curCenter);
                        _dragJustCommitted = false;
                    }

                    if (_focused != null) _focused.ReceiveDragToPoint(curCenter);
                    else                  OnTwoFingerDragDelta?.Invoke(curCenter - _twoPrevCenter);
                    break;
            }

            _twoPrevDist   = curDist;
            _twoPrevCenter = curCenter;
        }

        // ── Mouse fallback (editor / desktop) ───────────────────────────────────

        private void UpdateMouseFallback(Mouse mouse)
        {
            Vector2 pos = mouse.position.ReadValue();

            if (!_mouseWasDown)
            {
                if (_state == State.Multi) OnPinchEnd?.Invoke();
                _twoKind      = TwoGestureKind.Undecided;
                _focused      = Raycast(pos);
                _singlePrev   = pos;
                _state        = State.SingleTouch;
                _mouseWasDown = true;
                if (_focused != null) OnManipulatorGrabbed?.Invoke(_focused);
                return;
            }

            Vector2 delta = pos - _singlePrev;
            if (delta.sqrMagnitude > 0f)
                _focused?.ReceiveRotateDelta(delta);

            _singlePrev = pos;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private static Vector2 GetCenter(Touch t0, Touch t1, Touch t2, int count)
        {
            if (count >= 3)
                return (t0.screenPosition + t1.screenPosition + t2.screenPosition) / 3f;
            return (t0.screenPosition + t1.screenPosition) * 0.5f;
        }

        private ObjectManipulator Raycast(Vector2 screenPos)
        {
            if (_cam == null) return null;
            Ray ray = _cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _interactableLayer)
                ? hit.collider.GetComponentInParent<ObjectManipulator>()
                : null;
        }

        private void ResetState()
        {
            if (_focused != null) OnManipulatorReleased?.Invoke(_focused);
            _focused           = null;
            _state             = State.Idle;
            _twoKind           = TwoGestureKind.Undecided;
            _dragJustCommitted = false;
        }
    }
}