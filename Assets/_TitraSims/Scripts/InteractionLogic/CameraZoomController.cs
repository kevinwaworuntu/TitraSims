using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// Zooms the view when the user pinches on empty space (no ObjectManipulator focused).
    /// Pinching on a focused object still scales that object — the camera is unaffected.
    ///
    /// Two modes:
    ///   <see cref="ZoomMode.FieldOfView"/>  — changes Camera.main.fieldOfView.
    ///      ⚠ NOT suitable for Vuforia AR: Vuforia sets FOV to match the device lens;
    ///        overriding it breaks the virtual-to-real alignment.
    ///
    ///   <see cref="ZoomMode.ContentScale"/> — scales a target Transform instead.
    ///      AR-safe: makes the AR content appear larger/smaller without touching
    ///      the camera. Assign <see cref="contentRoot"/> to the root of your AR
    ///      content (the ImageTarget's child, not the ImageTarget itself).
    ///
    /// Setup: add to any persistent GameObject alongside GestureController.
    /// </summary>
    [DisallowMultipleComponent]
    public class CameraZoomController : MonoBehaviour
    {
        // ── Types ────────────────────────────────────────────────────────────────

        public enum ZoomMode
        {
            /// <summary>Changes Camera.main.fieldOfView. Not suitable for Vuforia AR.</summary>
            FieldOfView,
            /// <summary>Scales <see cref="contentRoot"/>. AR-safe.</summary>
            ContentScale,
        }

        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Mode")]
        public ZoomMode mode = ZoomMode.ContentScale;

        [Header("Content Scale  (AR-safe)")]
        [Tooltip("Root of AR content to scale. Typically the child of your Vuforia ImageTarget.")]
        public Transform contentRoot;

        [Tooltip("Minimum and maximum absolute local scale on each axis")]
        public Vector2 scaleRange = new Vector2(0.4f, 2.5f);

        [Header("Field of View  (non-AR only)")]
        [Tooltip("FOV bounds in degrees")]
        public Vector2 fovRange = new Vector2(20f, 70f);

        [Header("Feel")]
        [Tooltip("Lerp speed toward the zoom target (higher = snappier)")]
        [SerializeField, Range(1f, 20f)] private float _smoothSpeed = 8f;

        // ── Internal state ───────────────────────────────────────────────────────

        private Camera  _cam;

        // Baseline values captured when each new pinch begins
        private float   _basePinchDist;
        private float   _baseFov;
        private float   _baseContentScale;  // uniform X component of contentRoot.localScale

        // Lerp targets — initialised to current state; never drift unless a pinch changes them
        private float   _targetFov;
        private float   _targetContentScale;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Start()
        {
            _cam = Camera.main;

            // Initialise targets to current state so Update() lerp has no effect at rest.
            _targetFov          = _cam != null ? _cam.fieldOfView : 60f;
            _targetContentScale = contentRoot != null ? contentRoot.localScale.x : 1f;

            if (GestureController.Instance == null)
            {
                Debug.LogWarning("[CameraZoomController] No GestureController found in scene.", this);
                return;
            }

            GestureController.Instance.OnPinchBegin  += HandlePinchBegin;
            GestureController.Instance.OnPinchUpdate += HandlePinchUpdate;
        }

        private void OnDestroy()
        {
            if (GestureController.Instance == null) return;
            GestureController.Instance.OnPinchBegin  -= HandlePinchBegin;
            GestureController.Instance.OnPinchUpdate -= HandlePinchUpdate;
        }

        private void Update()
        {
            switch (mode)
            {
                case ZoomMode.FieldOfView when _cam != null:
                    _cam.fieldOfView = Mathf.Lerp(
                        _cam.fieldOfView, _targetFov, _smoothSpeed * Time.deltaTime);
                    break;

                case ZoomMode.ContentScale when contentRoot != null:
                    float cur = contentRoot.localScale.x;
                    float next = Mathf.Lerp(cur, _targetContentScale, _smoothSpeed * Time.deltaTime);
                    contentRoot.localScale = new Vector3(next, next, next);
                    break;
            }
        }

        // ── Pinch callbacks ──────────────────────────────────────────────────────

        private void HandlePinchBegin(float distance)
        {
            // Record the camera/content state at the moment this pinch gesture starts.
            // The next HandlePinchUpdate call computes zoom relative to this baseline.
            _basePinchDist     = distance;
            _baseFov           = _cam != null ? _cam.fieldOfView : _targetFov;
            _baseContentScale  = contentRoot != null ? contentRoot.localScale.x : _targetContentScale;
        }

        private void HandlePinchUpdate(float distance)
        {
            // OnPinchUpdate only fires when there is no focused ObjectManipulator,
            // so this never fights with per-object pinch-scaling.
            if (_basePinchDist <= 0f) return;

            // ratio > 1  →  fingers spread  →  zoom in
            // ratio < 1  →  fingers pinch   →  zoom out
            float ratio = distance / _basePinchDist;

            switch (mode)
            {
                case ZoomMode.FieldOfView:
                    // Dividing by ratio: spread (ratio > 1) shrinks FOV → zooms in.
                    _targetFov = Mathf.Clamp(_baseFov / ratio, fovRange.x, fovRange.y);
                    break;

                case ZoomMode.ContentScale:
                    // Multiplying by ratio: spread (ratio > 1) grows scale → zooms in.
                    _targetContentScale = Mathf.Clamp(
                        _baseContentScale * ratio, scaleRange.x, scaleRange.y);
                    break;
            }
        }

        // ── Editor ───────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (mode == ZoomMode.ContentScale && contentRoot == null)
                Debug.LogWarning("[CameraZoomController] Mode is ContentScale but contentRoot is not assigned.", this);

            if (mode == ZoomMode.FieldOfView)
                Debug.LogWarning("[CameraZoomController] FieldOfView mode is not suitable for Vuforia AR. " +
                                 "Consider ContentScale mode instead.", this);
        }
#endif
    }
}
