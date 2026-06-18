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
    /// Gesture map:
    ///   1 finger  →  rotate focused object
    ///   2 fingers spreading/pinching  →  scale focused object  (or OnPinchUpdate if no focus)
    ///   2 fingers translating         →  drag  focused object  (or OnTwoFingerDragDelta if no focus)
    ///
    /// Setup: add to a persistent GameObject in the scene. Objects must have a
    /// Collider so the raycast can find them; put ObjectManipulator anywhere in
    /// their hierarchy.
    /// </summary>
    [DefaultExecutionOrder(-10)]   // run before ObjectManipulators and SnapInteractables
    public class GestureController : MonoBehaviour
    {
        public static GestureController Instance { get; private set; }

        // ── Public events ────────────────────────────────────────────────────────

        /// Fired on first touch contact with an ObjectManipulator (finger down or mouse down).
        /// Not re-fired when a second finger is added to an already-focused object.
        public event Action<ObjectManipulator> OnManipulatorGrabbed;

        /// Fired when all contact with a focused ObjectManipulator ends (all fingers lifted).
        public event Action<ObjectManipulator> OnManipulatorReleased;

        /// Fired once when a two-touch pinch begins. Arg: initial finger distance in pixels.
        public event Action<float> OnPinchBegin;

        /// Fired every frame during a pinch that has no focused object.
        /// Arg: current finger distance in pixels — compute your own delta.
        public event Action<float> OnPinchUpdate;

        /// Fired when a pinch gesture ends.
        public event Action OnPinchEnd;

        /// Fired every frame during a two-finger drag that has no focused object.
        /// Arg: screen-space delta in pixels.
        public event Action<Vector2> OnTwoFingerDragDelta;

        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Raycast")]
        [SerializeField] private LayerMask _interactableLayer = ~0;
        [SerializeField] private float _raycastDistance = 100f;

        [Header("Two-Touch Disambiguation")]
        [Tooltip("Fraction of initial distance that must change before committing to Pinch")]
        [SerializeField, Range(0.01f, 0.15f)] private float _pinchCommitRatio = 0.04f;

        [Tooltip("Screen pixels the midpoint must travel before committing to Drag")]
        [SerializeField, Range(2f, 20f)] private float _dragCommitPixels = 6f;

        // ── Internal types ───────────────────────────────────────────────────────

        private enum State          { Idle, Single, Two }
        private enum TwoGestureKind { Undecided, Pinch, Drag }

        // ── State ────────────────────────────────────────────────────────────────

        private State             _state   = State.Idle;
        private TwoGestureKind    _twoKind = TwoGestureKind.Undecided;
        private ObjectManipulator _focused;
        private Camera            _cam;

        // Single-touch
        private Vector2 _singlePrev;

        // Two-touch
        private float   _twoPrevDist;
        private Vector2 _twoPrevCenter;
        private float   _twoBaseDist;
        private Vector2 _twoBaseCenter;

        // Mouse fallback (editor / desktop)
        private Vector2 _mousePrev;
        private bool    _mouseWasDown;

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
            // Collect live touches — skip Ended/Canceled to avoid off-by-one-frame errors.
            Touch live0 = default, live1 = default;
            int   liveCount = 0;

            foreach (var t in Touch.activeTouches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                if      (liveCount == 0) live0 = t;
                else if (liveCount == 1) live1 = t;
                liveCount++;
            }

            // Mouse acts as a single synthetic touch when no real touches are present.
            var mouse = Mouse.current;
            if (liveCount == 0 && mouse != null && mouse.leftButton.isPressed)
            {
                UpdateMouseFallback(mouse);
                return;
            }

            _mouseWasDown = false;

            switch (liveCount)
            {
                case 0:  HandleIdle();            break;
                case 1:  HandleSingle(live0);     break;
                default: HandleTwo(live0, live1); break;
            }
        }

        // ── Touch handlers ───────────────────────────────────────────────────────

        private void HandleIdle()
        {
            if (_state == State.Idle) return;
            if (_state == State.Two)  OnPinchEnd?.Invoke();
            ResetState();
        }

        private void HandleSingle(Touch t)
        {
            // One finger lifted from a two-touch gesture — transition back to single.
            if (_state == State.Two)
            {
                OnPinchEnd?.Invoke();
                _twoKind = TwoGestureKind.Undecided;
                _state   = State.Idle;
                // _focused is preserved so the object stays tracked across the transition.
                // OnManipulatorGrabbed is NOT re-fired; the grab is still in progress.
            }

            if (_state == State.Idle)
            {
                // Track whether we already had a focused object (from a prior Two-touch phase).
                bool hadFocused = _focused != null;
                _focused    = Raycast(t.screenPosition);
                _singlePrev = t.screenPosition;
                _state      = State.Single;

                // Fire Grabbed only on genuinely fresh contact, not Two→Single handoffs.
                if (!hadFocused && _focused != null)
                    OnManipulatorGrabbed?.Invoke(_focused);
                return;
            }

            // Ongoing single touch — route horizontal delta as rotation.
            if (t.phase == TouchPhase.Moved)
                _focused?.ReceiveRotateDelta(t.screenPosition - _singlePrev);

            _singlePrev = t.screenPosition;
        }

        private void HandleTwo(Touch t0, Touch t1)
        {
            bool justEntered    = _state != State.Two;
            bool comingFromIdle = _state == State.Idle;

            if (justEntered)
            {
                Vector2 mid = (t0.screenPosition + t1.screenPosition) * 0.5f;

                // Inherit the focused object from single-touch if available;
                // otherwise raycast from the midpoint.
                if (_focused == null) _focused = Raycast(mid);

                _twoBaseDist   = _twoPrevDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
                _twoBaseCenter = _twoPrevCenter  = mid;
                _twoKind       = TwoGestureKind.Undecided;
                _state         = State.Two;

                // Fire Grabbed only if both fingers landed simultaneously (no prior Single grab).
                if (comingFromIdle && _focused != null)
                    OnManipulatorGrabbed?.Invoke(_focused);

                OnPinchBegin?.Invoke(_twoBaseDist);
                return;
            }

            float   curDist   = Vector2.Distance(t0.screenPosition, t1.screenPosition);
            Vector2 curCenter = (t0.screenPosition + t1.screenPosition) * 0.5f;

            // Commit to a gesture kind on first significant movement.
            if (_twoKind == TwoGestureKind.Undecided)
            {
                float distChange   = Mathf.Abs(curDist - _twoBaseDist) / _twoBaseDist;
                float centerTravel = Vector2.Distance(curCenter, _twoBaseCenter);

                if (distChange >= _pinchCommitRatio)
                    _twoKind = TwoGestureKind.Pinch;
                else if (centerTravel >= _dragCommitPixels)
                    _twoKind = TwoGestureKind.Drag;
            }

            switch (_twoKind)
            {
                case TwoGestureKind.Pinch:
                    float scaleFactor = _twoPrevDist > 0f ? curDist / _twoPrevDist : 1f;
                    if (_focused != null) _focused.ReceivePinchDelta(scaleFactor);
                    else                  OnPinchUpdate?.Invoke(curDist);
                    break;

                case TwoGestureKind.Drag:
                    Vector2 dragDelta = curCenter - _twoPrevCenter;
                    if (_focused != null) _focused.ReceiveDragDelta(dragDelta);
                    else                  OnTwoFingerDragDelta?.Invoke(dragDelta);
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
                if (_state == State.Two) OnPinchEnd?.Invoke();
                _twoKind      = TwoGestureKind.Undecided;
                _focused      = Raycast(pos);
                _mousePrev    = pos;
                _state        = State.Single;
                _mouseWasDown = true;
                if (_focused != null) OnManipulatorGrabbed?.Invoke(_focused);
                return;
            }

            Vector2 delta = pos - _mousePrev;
            if (delta.sqrMagnitude > 0f)
                _focused?.ReceiveRotateDelta(delta);

            _mousePrev = pos;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private ObjectManipulator Raycast(Vector2 screenPos)
        {
            if (_cam == null) return null;
            Ray ray = _cam.ScreenPointToRay(screenPos);
            return Physics.Raycast(ray, out RaycastHit hit, _raycastDistance, _interactableLayer)
                ? hit.collider.GetComponentInParent<ObjectManipulator>()
                : null;
        }

        /// Fires OnManipulatorReleased then clears all tracked state.
        private void ResetState()
        {
            if (_focused != null) OnManipulatorReleased?.Invoke(_focused);
            _focused = null;
            _state   = State.Idle;
            _twoKind = TwoGestureKind.Undecided;
        }
    }
}