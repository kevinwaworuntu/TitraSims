using UnityEngine;

namespace InteractionLogic
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ObjectManipulator : MonoBehaviour
    {
        [Header("Permissions")]
        public bool canRotate = true;
        public bool canDrag   = true;
        public bool canScale  = true;

        [Header("Rotate")]
        [Tooltip("Degrees of world-space rotation per screen pixel of horizontal swipe")]
        [SerializeField, Range(0.05f, 2f)]
        private float _rotateSensitivity = 0.3f;

        [Tooltip("World-space axis to rotate around. Default is Y (vertical spin).")]
        public Vector3 rotateAxis = Vector3.up;

        [Header("Drag")]
        [Tooltip("World units moved per screen pixel")]
        [SerializeField, Range(0.0001f, 0.05f)] 
        private float _dragSensitivity = 0.005f;
        public bool lockToHorizontalPlane;

        [Header("Scale")]
        [Tooltip("Minimum and maximum scale as a multiplier of the object's original local scale")]
        public Vector2 scaleRange = new Vector2(0.3f, 3f);


        private Camera  _cam;
        private Vector3 _originalScale;

        private void Awake()
        {
            _cam           = Camera.main;
            _originalScale = transform.localScale;
        }
        
        // ── Called by GestureController ──────────────────────────────────────────

        /// <summary>1-finger horizontal swipe → rotate around <see cref="rotateAxis"/>.</summary>
        public void ReceiveRotateDelta(Vector2 screenDelta)
        {
            if (!canRotate) return;
            float angle = -screenDelta.x * _rotateSensitivity;
            transform.Rotate(rotateAxis, angle, Space.World);
        }

        /// <summary>2-finger translation → move in world space.</summary>
        public void ReceiveDragDelta(Vector2 screenDelta)
        {
            if (!canDrag || _cam == null) return;

            Vector3 worldDelta;

            if (lockToHorizontalPlane)
            {
                // Project camera axes onto XZ so movement stays flat on the AR surface.
                Vector3 right   = Vector3.ProjectOnPlane(_cam.transform.right,   Vector3.up).normalized;
                Vector3 forward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
                worldDelta = (right * screenDelta.x + forward * screenDelta.y) * _dragSensitivity;
            }
            else
            {
                worldDelta = (_cam.transform.right * screenDelta.x + _cam.transform.up    * screenDelta.y) * _dragSensitivity;
            }

            transform.position += new Vector3(worldDelta.x, worldDelta.y, 0f);
        }

        /// <summary>2-finger pinch → scale clamped to <see cref="scaleRange"/> × original scale.</summary>
        public void ReceivePinchDelta(float scaleFactor)
        {
            if (!canScale) return;

            // Work in "normalised" space (ratio vs original) so scaleRange is always
            // relative to the object's initial size, not its current runtime size.
            float current    = transform.localScale.x / _originalScale.x;
            float normalised = Mathf.Clamp(current * scaleFactor, scaleRange.x, scaleRange.y);
            transform.localScale = _originalScale * normalised;
        }
    }
}