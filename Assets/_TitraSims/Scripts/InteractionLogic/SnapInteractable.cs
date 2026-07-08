using UnityEngine;
using UnityEngine.Events;

namespace InteractionLogic
{
    /// <summary>
    /// Add alongside <see cref="ObjectManipulator"/> to make an object snap to
    /// <see cref="SnapZone"/>s when released nearby.
    ///
    /// Behaviour:
    ///   • While being dragged, the nearest compatible zone within its radius
    ///     shows a highlight.
    ///   • On drag release, the object lerps to the nearest zone (if any).
    ///   • Picking up a snapped object (any touch contact) unsnaps it immediately
    ///     and re-enables free drag.
    ///
    /// Tag filtering:  set the same tag on this GameObject and on the target
    /// SnapZone's <c>acceptTag</c> field to restrict which zones accept it.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class SnapInteractable : MonoBehaviour
    {
        [Header("Snap Settings")]
        [Tooltip("Lerp speed toward the snap position (world units / second scale)")]
        [SerializeField, Range(1f, 30f)] private float _snapSpeed = 10f;

        [SerializeField] bool isBackToOriginPosition;

        [Header("Events")]
        public UnityEvent OnSnapped;
        public UnityEvent OnUnsnapped;

        // ── Public state ─────────────────────────────────────────────────────────

        public bool     IsSnapped    => _currentZone != null;
        public SnapZone CurrentZone  => _currentZone;

        // ── Internal state ───────────────────────────────────────────────────────

        private ObjectManipulator             _manipulator;
        private IAnimationPlaybackController[] _animationPlaybackControllers;
        private SnapZone          _currentZone;
        private SnapZone          _highlightedZone;
        private bool              _isBeingManipulated;

        // Lerp-to-snap / return-to-origin
        private bool       _isLerping;
        private Vector3    _lerpTargetPos;
        private Quaternion _lerpTargetRot;

        private Vector3    _originLocalPos;
        private Quaternion _originLocalRot;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _manipulator                  = GetComponent<ObjectManipulator>();
            _animationPlaybackControllers = GetComponents<IAnimationPlaybackController>();
        }

        private void OnEnable()
        {
            _originLocalPos = transform.localPosition;
            _originLocalRot = transform.localRotation;

            if (GestureController.Instance == null)
            {
                Debug.LogWarning("[SnapInteractable] No GestureController found in scene.", this);
                return;
            }
            GestureController.Instance.OnManipulatorGrabbed  += HandleGrabbed;
            GestureController.Instance.OnManipulatorReleased += HandleReleased;
        }

        private void OnDisable()
        {
            if (GestureController.Instance == null) return;
            GestureController.Instance.OnManipulatorGrabbed  -= HandleGrabbed;
            GestureController.Instance.OnManipulatorReleased -= HandleReleased;
        }

        private void OnDestroy()
        {
            if (GestureController.Instance == null) return;
            GestureController.Instance.OnManipulatorGrabbed  -= HandleGrabbed;
            GestureController.Instance.OnManipulatorReleased -= HandleReleased;
        }

        private void Update()
        {
            if (_isLerping)          ApplySnapLerp();
            if (_isBeingManipulated) UpdateHighlight();
        }

        // ── Gesture callbacks ────────────────────────────────────────────────────

        private void HandleGrabbed(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = true;
            _isLerping = false;   // cancel any in-flight lerp (snap or return-to-origin)
            if (IsSnapped) Unsnap();
        }

        private void HandleReleased(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = false;

            ClearHighlight();
            TrySnapToNearest();

            if (!IsSnapped && isBackToOriginPosition)
                StartReturnToOrigin();
        }

        // ── Snap logic ───────────────────────────────────────────────────────────

        private void TrySnapToNearest() => FindNearestZoneInRange()?.TryAccept(this);

        /// Called by <see cref="SnapZone.TryAccept"/> — do not call directly.
        public void SnapTo(SnapZone zone)
        {
            _currentZone   = zone;
            _isLerping     = true;
            _lerpTargetPos = zone.SnapTargetPosition;
            _lerpTargetRot = zone.snapRotation ? zone.transform.rotation : transform.rotation;

            _manipulator.canDrag = false;   // locked in place while snapped
            OnSnapped.Invoke();
        }

        private void Unsnap()
        {
            _currentZone?.Release();
            _currentZone         = null;
            _isLerping           = false;
            _manipulator.canDrag = true;
            OnUnsnapped.Invoke();
        }

        private void StartReturnToOrigin()
        {
            _isLerping = true;
            Transform parentTransform = transform.parent;
            _lerpTargetPos = parentTransform != null ? parentTransform.TransformPoint(_originLocalPos) : _originLocalPos;
            _lerpTargetRot = parentTransform != null ? parentTransform.rotation * _originLocalRot      : _originLocalRot;
        }

        [ContextMenu( "Instant Return to Origin")]
        public void InstantReturnToOrigin()
        {
            // "Instant" — stop any in-progress playback rather than waiting for it to finish,
            // so it doesn't keep animating out of sync with the freshly-reset transform.
            foreach (var controller in _animationPlaybackControllers)
            {
                controller.StopPlayback();
            }

            transform.localPosition = _originLocalPos;
            transform.localRotation = _originLocalRot;
        }

        // ── Highlight (during drag) ──────────────────────────────────────────────

        private void UpdateHighlight()
        {
            SnapZone nearest = FindNearestZoneInRange();
            if (nearest == _highlightedZone) return;

            _highlightedZone?.SetHighlight(false);
            _highlightedZone = nearest;
            _highlightedZone?.SetHighlight(true);
        }

        private void ClearHighlight()
        {
            _highlightedZone?.SetHighlight(false);
            _highlightedZone = null;
        }

        // ── Helpers ──────────────────────────────────────────────────────────────

        private SnapZone FindNearestZoneInRange()
        {
            SnapZone nearest    = null;
            float   nearestDist = float.MaxValue;

            foreach (var zone in SnapZone.All)
            {
                if (!zone.IsInRange(this)) continue;
                float d = Vector3.Distance(transform.position, zone.SnapTargetPosition);
                if (d < nearestDist) { nearest = zone; nearestDist = d; }
            }

            return nearest;
        }

        // ── Lerp animation ───────────────────────────────────────────────────────

        private void ApplySnapLerp()
        {
            transform.position = Vector3.Lerp(
                transform.position, _lerpTargetPos, _snapSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, _lerpTargetRot, _snapSpeed * Time.deltaTime);

            // Settle once close enough to avoid infinite asymptotic approach.
            if (Vector3.Distance(transform.position, _lerpTargetPos) < 0.0005f)
            {
                transform.position = _lerpTargetPos;
                transform.rotation = _lerpTargetRot;
                _isLerping = false;
            }
        }
    }
}