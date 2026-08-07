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
        [Tooltip("Restrict the free axes to this object's local horizontal plane (its parent's X/Z).")]
        public bool lockToHorizontalPlane;

        [Tooltip("Freeze this local axis (relative to this object's parent) at its value when the drag started.")]
        public bool lockDragX;
        [Tooltip("Freeze this local axis (relative to this object's parent) at its value when the drag started.")]
        public bool lockDragY;
        [Tooltip("Freeze this local axis (relative to this object's parent) at its value when the drag started.")]
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

        // When exactly one local axis is free, dragging is constrained to that axis's line
        // instead of a plane — see TryGetSingleFreeAxis().
        private bool    _isAxisConstrained;
        private Vector3 _freeAxisDir;

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
        /// Establishes the local-space plane (or axis line) the object will be dragged along, and
        /// the offset between the object and the grab point so it doesn't jump to be centered on
        /// the pointer. Everything is computed in the parent's local space — see <see cref="ToLocalRay"/>.
        /// </summary>
        public void BeginDrag(Vector2 screenPos)
        {
            if (!canDrag || _cam == null) return;

            _dragStartPosition = transform.localPosition;
            _isAxisConstrained = TryGetSingleFreeAxis(out _freeAxisDir);
            Ray localRay       = ToLocalRay(_cam.ScreenPointToRay(screenPos));

            if (_isAxisConstrained)
            {
                ClosestPointOnAxis(_dragStartPosition, _freeAxisDir, localRay, out Vector3 anchor);
                _dragOffset = transform.localPosition - anchor;
                return;
            }

            Vector3 planeNormal = ResolveDragPlaneNormal();
            _dragPlane = new Plane(planeNormal, _dragStartPosition);
            _dragOffset = RaycastDragPlane(localRay, out Vector3 hit) ? transform.localPosition - hit : Vector3.zero;
        }

        /// <summary>Moves the object so it tracks the given screen-space pointer/touch position.</summary>
        public void ReceiveDragToPoint(Vector2 screenPos)
        {
            if (!canDrag || _cam == null) return;

            Ray localRay = ToLocalRay(_cam.ScreenPointToRay(screenPos));

            if (_isAxisConstrained)
            {
                // Every point returned by ClosestPointOnAxis lies exactly on the free axis's
                // line through _dragStartPosition, so this is pure single-axis movement by
                // construction — no camera-angle coupling to clamp away afterward.
                if (ClosestPointOnAxis(_dragStartPosition, _freeAxisDir, localRay, out Vector3 anchor))
                {
                    transform.localPosition = anchor + _dragOffset;
                }
                return;
            }

            if (RaycastDragPlane(localRay, out Vector3 hit))
            {
                Vector3 target = hit + _dragOffset;
                if (lockDragX) target.x = _dragStartPosition.x;
                if (lockDragY) target.y = _dragStartPosition.y;
                if (lockDragZ) target.z = _dragStartPosition.z;
                transform.localPosition = target;
            }
        }

        /// <summary>
        /// Re-expresses a world-space ray in this object's parent's local space, so all drag
        /// math (axis lock, plane raycast) operates on the object's own local X/Y/Z rather than
        /// the global world axes. A straight line stays straight under this change of coordinates
        /// (even with non-uniform parent scale), so intersecting it with a local-axis-aligned
        /// plane or line afterward is still exact. Falls back to the world ray unchanged when
        /// there's no parent, since local space and world space are then identical.
        /// </summary>
        private Ray ToLocalRay(Ray worldRay)
        {
            Transform parent = transform.parent;
            if (parent == null) return worldRay;

            Vector3 origin    = parent.InverseTransformPoint(worldRay.origin);
            Vector3 direction = parent.InverseTransformDirection(worldRay.direction).normalized;
            return new Ray(origin, direction);
        }

        private void GetEffectiveLocks(out bool lockX, out bool lockY, out bool lockZ)
        {
            lockX = lockDragX;
            lockY = lockToHorizontalPlane || lockDragY;
            lockZ = lockDragZ;
        }

        /// <summary>Returns true and the local-space direction when exactly one axis is free.</summary>
        private bool TryGetSingleFreeAxis(out Vector3 axisDir)
        {
            GetEffectiveLocks(out bool lockX, out bool lockY, out bool lockZ);

            if (!lockX && lockY && lockZ) { axisDir = Vector3.right;   return true; }
            if (!lockY && lockX && lockZ) { axisDir = Vector3.up;      return true; }
            if (!lockZ && lockX && lockY) { axisDir = Vector3.forward; return true; }

            axisDir = Vector3.zero;
            return false;
        }

        // Aligns the plane normal with a single locked local axis so it's baked out of the
        // raycast itself; clamping it post-hoc against a camera-tilted plane is what caused
        // diagonal drift. Only reached when zero or 2+ axes are locked — the single-free-axis
        // case is handled by ClosestPointOnAxis instead, since a plane can't represent pure
        // single-axis movement without a lossy, camera-angle-dependent clamp.
        private Vector3 ResolveDragPlaneNormal()
        {
            GetEffectiveLocks(out bool lockX, out bool lockY, out bool lockZ);

            if (lockX && !lockY && !lockZ) return Vector3.right;
            if (lockY && !lockX && !lockZ) return Vector3.up;
            if (lockZ && !lockX && !lockY) return Vector3.forward;

            // Zero or 3 axes locked: fall back to a plane facing the camera, expressed in
            // local space so it's still consistent with everything else in this class.
            Transform parent = transform.parent;
            Vector3 camForward = _cam.transform.forward;
            return parent != null ? -parent.InverseTransformDirection(camForward).normalized : -camForward;
        }

        private bool RaycastDragPlane(Ray localRay, out Vector3 point)
        {
            if (_dragPlane.Raycast(localRay, out float enter))
            {
                point = localRay.GetPoint(enter);
                return true;
            }

            point = Vector3.zero;
            return false;
        }

        /// <summary>
        /// Closest point on the infinite line (axisPoint, axisDir) to the given ray — the same
        /// technique transform gizmos use for single-axis dragging. Returns false when the ray is
        /// nearly parallel to the axis (e.g. camera looking straight along it), an ill-conditioned
        /// case where the caller should skip the update rather than snap to an unstable point.
        /// </summary>
        private static bool ClosestPointOnAxis(Vector3 axisPoint, Vector3 axisDir, Ray ray, out Vector3 point)
        {
            Vector3 r = ray.origin - axisPoint;
            float b = Vector3.Dot(ray.direction, axisDir);
            float d = Vector3.Dot(ray.direction, r);
            float e = Vector3.Dot(axisDir, r);

            float denom = 1f - b * b; // both directions are unit-length
            if (Mathf.Abs(denom) < 1e-5f)
            {
                point = axisPoint;
                return false;
            }

            float t2 = (e - b * d) / denom;
            point = axisPoint + axisDir * t2;
            return true;
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