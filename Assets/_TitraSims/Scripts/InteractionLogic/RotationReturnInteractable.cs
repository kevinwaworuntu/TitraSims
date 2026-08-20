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
    /// </summary>
    [RequireComponent(typeof(ObjectManipulator))]
    public class RotationReturnInteractable : MonoBehaviour
    {
        [Header("Rotation Return Settings")]
        [Tooltip("Slerp speed toward the origin rotation (higher = snappier)")]
        [SerializeField, Range(1f, 30f)] private float _returnSpeed = 10f;

        // ── Internal state ───────────────────────────────────────────────────────

        private ObjectManipulator _manipulator;
        private bool              _isBeingManipulated;

        private bool       _isLerping;
        private Quaternion _originLocalRotation;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _manipulator = GetComponent<ObjectManipulator>();
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
