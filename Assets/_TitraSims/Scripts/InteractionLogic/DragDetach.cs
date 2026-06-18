using System;
using UnityEngine;
using UnityEngine.Events;

namespace InteractionLogic
{
    /// <summary>
    /// Detects when an object is pulled far enough from its anchor to detach.
    /// Designed for lab interactions such as removing a burette stopper or
    /// pulling a pipette out of a flask.
    ///
    /// Behaviour:
    ///   • While grabbed, monitors world-space distance from <see cref="anchor"/>.
    ///   • Drives an optional <see cref="_tensionLine"/> LineRenderer (green → red).
    ///   • When distance reaches <see cref="_detachDistance"/>, fires
    ///     <see cref="OnDetach"/> and marks the object as free.
    ///   • If released before detaching and <see cref="snapBackOnRelease"/> is true,
    ///     the object lerps back to the anchor.
    ///   • Once detached the object is fully free — no further tension or snap-back.
    ///     Call <see cref="Reattach"/> to reset (e.g. on experiment restart).
    ///
    /// Composes with <see cref="SnapInteractable"/>: if the object is currently
    /// snapped, SnapInteractable unsnaps it first (restoring canDrag) before
    /// DragDetach begins building tension — both components subscribe to the same
    /// OnManipulatorGrabbed event.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class DragDetach : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Attachment")]
        [Tooltip("The fixed point this object is tethered to. Must be assigned.")]
        public Transform anchor;

        [Tooltip("World-space pull distance at which detach fires")]
        [SerializeField, Range(0.01f, 1f)] private float _detachDistance = 0.15f;

        [Header("Snap Back")]
        [Tooltip("If released before detaching, lerp the object back to the anchor")]
        public bool snapBackOnRelease = true;

        [SerializeField, Range(1f, 20f)] private float _snapBackSpeed = 6f;

        [Header("Tension Visual")]
        [Tooltip("Optional LineRenderer that shows a tether cord. "
               + "Assign a LineRenderer; the component sets its positions and colour. "
               + "The renderer must have positionCount ≥ 2.")]
        [SerializeField] private LineRenderer _tensionLine;

        [SerializeField] private Color _colorRelaxed = Color.green;
        [SerializeField] private Color _colorTense   = Color.red;

        [Header("Events")]
        [Tooltip("Fired the moment the object detaches. Inspector-wirable.")]
        public UnityEvent OnDetach;

        // ── Public C# events (code-only) ─────────────────────────────────────────

        /// Fired when detach occurs. Code-level alternative to <see cref="OnDetach"/>.
        public event Action OnDetached;

        /// Fired every frame while grabbed.
        /// Arg: normalised tension in [0, 1]  — 0 = at anchor, 1 = at detach threshold.
        /// Use to drive audio pitch, particle rate, or haptic feedback.
        public event Action<float> OnTensionChanged;

        // ── Public state ─────────────────────────────────────────────────────────

        public bool IsDetached      => _isDetached;
        public bool IsSnappingBack  => _isSnappingBack;

        /// Normalised pull distance from the last Update. 0 = at anchor, 1 = at threshold.
        public float CurrentTension { get; private set; }

        // ── Internal state ───────────────────────────────────────────────────────

        private ObjectManipulator _manipulator;
        private bool _isDetached;
        private bool _isGrabbed;
        private bool _isSnappingBack;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _manipulator = GetComponent<ObjectManipulator>();
        }

        private void Start()
        {
            if (anchor == null)
            {
                Debug.LogWarning($"[DragDetach] '{name}': anchor is not assigned — component disabled.", this);
                enabled = false;
                return;
            }

            if (GestureController.Instance == null)
            {
                Debug.LogWarning("[DragDetach] No GestureController found in scene.", this);
                return;
            }

            GestureController.Instance.OnManipulatorGrabbed  += HandleGrabbed;
            GestureController.Instance.OnManipulatorReleased += HandleReleased;

            InitTensionLine();
        }

        private void OnDestroy()
        {
            if (GestureController.Instance == null) return;
            GestureController.Instance.OnManipulatorGrabbed  -= HandleGrabbed;
            GestureController.Instance.OnManipulatorReleased -= HandleReleased;
        }

        private void Update()
        {
            if (_isGrabbed && !_isDetached)
                CheckTension();

            if (_isSnappingBack)
                ApplySnapBack();
        }

        // ── Gesture callbacks ────────────────────────────────────────────────────

        private void HandleGrabbed(ObjectManipulator m)
        {
            if (m != _manipulator || _isDetached) return;

            _isGrabbed      = true;
            _isSnappingBack = false;    // interrupt any in-progress snap-back
        }

        private void HandleReleased(ObjectManipulator m)
        {
            if (m != _manipulator || _isDetached) return;

            _isGrabbed      = false;
            CurrentTension  = 0f;
            ClearTensionVisual();

            if (snapBackOnRelease)
                _isSnappingBack = true;
        }

        // ── Core logic ───────────────────────────────────────────────────────────

        private void CheckTension()
        {
            float dist    = Vector3.Distance(transform.position, anchor.position);
            float tension = Mathf.Clamp01(dist / _detachDistance);

            CurrentTension = tension;
            UpdateTensionVisual(tension);
            OnTensionChanged?.Invoke(tension);

            if (dist >= _detachDistance)
                ExecuteDetach();
        }

        private void ExecuteDetach()
        {
            _isDetached     = true;
            _isGrabbed      = false;
            _isSnappingBack = false;
            CurrentTension  = 0f;

            ClearTensionVisual();

            OnDetach.Invoke();
            OnDetached?.Invoke();
        }

        private void ApplySnapBack()
        {
            // anchor could theoretically be destroyed mid-snap — guard.
            if (anchor == null) { _isSnappingBack = false; return; }

            transform.position = Vector3.Lerp(
                transform.position, anchor.position, _snapBackSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, anchor.position) < 0.0005f)
            {
                transform.position = anchor.position;
                _isSnappingBack    = false;
            }
        }

        // ── Tension visual ───────────────────────────────────────────────────────

        private void InitTensionLine()
        {
            if (_tensionLine == null) return;
            _tensionLine.positionCount = 2;
            _tensionLine.enabled = false;
        }

        private void UpdateTensionVisual(float tension)
        {
            if (_tensionLine == null) return;

            _tensionLine.enabled = true;
            _tensionLine.SetPosition(0, anchor.position);
            _tensionLine.SetPosition(1, transform.position);

            Color c = Color.Lerp(_colorRelaxed, _colorTense, tension);
            _tensionLine.startColor = c;
            _tensionLine.endColor   = c;
        }

        private void ClearTensionVisual()
        {
            if (_tensionLine != null)
                _tensionLine.enabled = false;
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Resets the object to the anchor and clears detach state.
        /// Call this when resetting a lab experiment step.
        /// </summary>
        public void Reattach()
        {
            _isDetached     = false;
            _isGrabbed      = false;
            _isSnappingBack = false;
            CurrentTension  = 0f;

            if (anchor != null)
                transform.position = anchor.position;

            ClearTensionVisual();
        }

        // ── Editor ───────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (anchor == null)
                Debug.LogWarning($"[DragDetach] '{name}': anchor is not assigned.", this);
        }

        private void OnDrawGizmosSelected()
        {
            if (anchor == null) return;

            // Detach-threshold sphere centred on the anchor.
            Gizmos.color = new Color(1f, 0.25f, 0.1f, 0.35f);
            Gizmos.DrawWireSphere(anchor.position, _detachDistance);

            // Line showing the current stretch.
            Gizmos.color = Color.Lerp(Color.green, Color.red,
                Vector3.Distance(transform.position, anchor.position) / _detachDistance);
            Gizmos.DrawLine(anchor.position, transform.position);
        }
#endif
    }
}
