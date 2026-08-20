using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// Add alongside <see cref="ObjectManipulator"/> to make an object smoothly
    /// return to its original rotation when released (untouched).
    ///
    /// Behaviour:
    ///   • Records the local rotation present when the component is enabled.
    ///   • While being manipulated (grabbed), any slerp is cancelled so the user's
    ///     rotate gesture is respected.
    ///   • On release, the rotation slerps back to the recorded origin rotation.
    ///
    /// This mirrors <see cref="ScaleReturnInteractable"/>, but affects only
    /// rotation — no position, scale, or snapping.
    ///
    /// Inert on objects that also carry an enabled <see cref="SnapInteractable"/>, which already
    /// returns/snaps rotation itself; adding both is harmless but only one drives.
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class RotationReturnInteractable : MonoBehaviour
    {
        [Header("Rotation Return Settings")]
        [Tooltip("Slerp speed toward the origin rotation (higher = snappier)")]
        [SerializeField, Range(1f, 30f)] private float _returnSpeed = 10f;

        // ── Internal state ───────────────────────────────────────────────────────

        private ObjectManipulator _manipulator;
        private SnapInteractable  _snap;            // optional — may be null
        private bool              _isBeingManipulated;

        private bool       _isLerping;
        private Quaternion _originLocalRotation;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _manipulator = GetComponent<ObjectManipulator>();
            _snap        = GetComponent<SnapInteractable>();
        }

        private void OnEnable()
        {
            _originLocalRotation = transform.localRotation;

            if (GestureController.Instance == null)
            {
                Debug.LogWarning("[RotationReturnInteractable] No GestureController found in scene.", this);
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
            if (_isLerping) ApplyRotationLerp();
        }

        // ── Gesture callbacks ────────────────────────────────────────────────────

        private void HandleGrabbed(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = true;
            _isLerping = false;   // cancel any in-flight return so the user can rotate freely
        }

        private void HandleReleased(ObjectManipulator m)
        {
            if (m != _manipulator) return;
            _isBeingManipulated = false;

            // SnapInteractable already drives rotation on release — toward the snap zone's
            // rotation when it snaps, toward the origin rotation when it doesn't. Both of us
            // writing rotation each frame would fight, and worse, pull a snapped object out of
            // its zone. Stand down and let it own rotation whenever it's active.
            if (_snap != null && _snap.enabled) return;

            _isLerping = true;
        }

        // ── Rotation return ──────────────────────────────────────────────────────

        private void ApplyRotationLerp()
        {
            transform.localRotation = Quaternion.Slerp(
                transform.localRotation, _originLocalRotation, _returnSpeed * Time.deltaTime);

            // Settle once close enough to avoid infinite asymptotic approach.
            if (Quaternion.Angle(transform.localRotation, _originLocalRotation) < 0.05f)
            {
                transform.localRotation = _originLocalRotation;
                _isLerping = false;
            }
        }

        [ContextMenu("Instant Return to Origin Rotation")]
        public void InstantReturnToOriginRotation()
        {
            transform.localRotation = _originLocalRotation;
            _isLerping = false;
        }
    }
}
