using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// Add alongside <see cref="ObjectManipulator"/> to make an object smoothly
    /// return to its original scale when released (untouched).
    ///
    /// Behaviour:
    ///   • Records the local scale present when the component is enabled.
    ///   • While being manipulated (grabbed), any lerp is cancelled so the user's
    ///     pinch-to-scale is respected.
    ///   • On release, the scale lerps back to the recorded origin scale.
    ///
    /// This mirrors <see cref="SnapInteractable"/>'s return-to-origin, but affects
    /// only scale — no position, rotation, or snapping.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class ScaleReturnInteractable : MonoBehaviour
    {
        [Header("Scale Return Settings")]
        [Tooltip("Lerp speed toward the origin scale (higher = snappier)")]
        [SerializeField, Range(1f, 30f)] private float _returnSpeed = 10f;

        // ── Internal state ───────────────────────────────────────────────────────

        private ObjectManipulator _manipulator;
        private bool              _isBeingManipulated;

        private bool    _isLerping;
        private Vector3 _originLocalScale;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _manipulator = GetComponent<ObjectManipulator>();
        }

        private void OnEnable()
        {
            _originLocalScale = transform.localScale;

            if (GestureController.Instance == null)
            {
                Debug.LogWarning("[ScaleReturnInteractable] No GestureController found in scene.", this);
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
            if (_isLerping) ApplyScaleLerp();
        }

        // ── Gesture callbacks ────────────────────────────────────────────────────

        private void HandleGrabbed(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = true;
            _isLerping = false;   // cancel any in-flight return so the user can scale freely
        }

        private void HandleReleased(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = false;
            _isLerping = true;
        }

        // ── Scale return ─────────────────────────────────────────────────────────

        private void ApplyScaleLerp()
        {
            transform.localScale = Vector3.Lerp(
                transform.localScale, _originLocalScale, _returnSpeed * Time.deltaTime);

            // Settle once close enough to avoid infinite asymptotic approach.
            if (Vector3.Distance(transform.localScale, _originLocalScale) < 0.0005f)
            {
                transform.localScale = _originLocalScale;
                _isLerping = false;
            }
        }

        [ContextMenu("Instant Return to Origin Scale")]
        public void InstantReturnToOriginScale()
        {
            transform.localScale = _originLocalScale;
            _isLerping = false;
        }
    }
}