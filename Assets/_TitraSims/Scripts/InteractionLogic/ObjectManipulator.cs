using UnityEngine;

namespace InteractionLogic
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public class ObjectManipulator : MonoBehaviour
    {
        [Header("Permissions")]
        public bool canRotate = false;
        public bool canDrag   = false;
        public bool canScale  = true;

        [Header("Rotate")]
        [Tooltip("Degrees of world-space rotation per screen pixel of horizontal swipe")]
        [SerializeField, Range(0.05f, 2f)]
        private float _rotateSensitivity = 0.3f;

        [Tooltip("World-space axis to rotate around. Default is Y (vertical spin).")]
        public Vector3 rotateAxis = Vector3.up;

        [Header("Drag")]
        public bool lockToHorizontalPlane;

        [Tooltip("Freeze this world-space axis at its value when the drag started.")]
        public bool lockDragX;
        [Tooltip("Freeze this world-space axis at its value when the drag started.")]
        public bool lockDragY;
        [Tooltip("Freeze this world-space axis at its value when the drag started.")]
        public bool lockDragZ = true;

        [Header("Scale")]
        [Tooltip("Minimum and maximum scale as a multiplier of the object's original local scale")]
        public Vector2 scaleRange = new Vector2(0.3f, 3f);


        private Camera  _cam;
        private Vector3 _originalScale;

        // Drag-to-pointer state
        private Plane   _dragPlane;
        private Vector3 _dragOffset;
        private Vector3 _dragStartPosition;

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

        /// <summary>
        /// Call once when a drag gesture starts (before the first <see cref="ReceiveDragToPoint"/>).
        /// Establishes the world-space plane the object will be dragged along, and the offset
        /// between the object and the grab point so it doesn't jump to be centered on the pointer.
        /// </summary>
        public void BeginDrag(Vector2 screenPos)
        {
            if (!canDrag || _cam == null) return;

            Vector3 planeNormal = ResolveDragPlaneNormal();
            _dragPlane = new Plane(planeNormal, transform.position);

            _dragStartPosition = transform.position;
            _dragOffset        = RaycastDragPlane(screenPos, out Vector3 hit) ? transform.position - hit : Vector3.zero;
        }

        /// <summary>Moves the object so it tracks the given screen-space pointer/touch position.</summary>
        public void ReceiveDragToPoint(Vector2 screenPos)
        {
            if (!canDrag || _cam == null) return;

            if (RaycastDragPlane(screenPos, out Vector3 hit))
            {
                Vector3 target = hit + _dragOffset;
                if (lockDragX) target.x = _dragStartPosition.x;
                if (lockDragY) target.y = _dragStartPosition.y;
                if (lockDragZ) target.z = _dragStartPosition.z;
                transform.position = target;
            }
        }

        // Aligns the plane normal with a single locked axis so it's baked out of the raycast
        // itself; clamping it post-hoc against a camera-tilted plane is what caused diagonal drift.
        private Vector3 ResolveDragPlaneNormal()
        {
            if (lockToHorizontalPlane) return Vector3.up;

            if (lockDragX && !lockDragY && !lockDragZ) return Vector3.right;
            if (lockDragY && !lockDragX && !lockDragZ) return Vector3.up;
            if (lockDragZ && !lockDragX && !lockDragY) return Vector3.forward;

            return -_cam.transform.forward;
        }

        private bool RaycastDragPlane(Vector2 screenPos, out Vector3 point)
        {
            Ray ray = _cam.ScreenPointToRay(screenPos);
            if (_dragPlane.Raycast(ray, out float enter))
            {
                point = ray.GetPoint(enter);
                return true;
            }

            point = Vector3.zero;
            return false;
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
        
        public void SetCanDrag(bool value) => canDrag = value;
    }
}