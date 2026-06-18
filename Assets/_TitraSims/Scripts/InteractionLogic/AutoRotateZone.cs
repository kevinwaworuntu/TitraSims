using System;
using System.Collections.Generic;
using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// Trigger zone that rotates any <see cref="ObjectManipulator"/> that enters it.
    /// Manual 1-finger rotation is suppressed while inside (restored on exit).
    ///
    /// Setup:
    ///   • Add to a GameObject that has a Collider with <b>Is Trigger = true</b>.
    ///   • Set <see cref="rotationAxis"/>, <see cref="rotationSpeed"/>, and
    ///     <see cref="rotationSpace"/> to control how objects spin.
    ///   • Set <see cref="acceptTag"/> to restrict which objects are affected.
    ///   • Subscribe to <see cref="OnObjectEntered"/> / <see cref="OnObjectExited"/>
    ///     in code to trigger animations or game-logic on enter/exit.
    ///
    /// The light-blue Gizmo circle shows the rotation plane in the Editor.
    /// </summary>
    public class AutoRotateZone : MonoBehaviour
    {
        // ── Inspector ────────────────────────────────────────────────────────────

        [Header("Rotation")]
        [Tooltip("Axis to rotate around, interpreted in the chosen Space")]
        public Vector3 rotationAxis = Vector3.up;

        [Tooltip("Degrees per second")]
        public float rotationSpeed = 90f;

        [Tooltip("World: axis is a fixed world-space direction. Self: axis is the object's local direction.")]
        public Space rotationSpace = Space.World;

        [Header("Filter")]
        [Tooltip("Only affect ObjectManipulators whose GameObject tag matches. Empty = any.")]
        public string acceptTag = "";

        [Tooltip("Disable 1-finger manual rotation while the object is inside the zone")]
        public bool suppressManualRotate = true;

        // ── Events ───────────────────────────────────────────────────────────────

        /// Fired when an ObjectManipulator enters the zone.
        public event Action<ObjectManipulator> OnObjectEntered;

        /// Fired when an ObjectManipulator exits the zone (or the zone is disabled).
        public event Action<ObjectManipulator> OnObjectExited;

        // ── Internal ─────────────────────────────────────────────────────────────

        private struct Occupant
        {
            public ObjectManipulator Manipulator;
            public bool              OriginalCanRotate;
        }

        private readonly List<Occupant> _occupants = new List<Occupant>();

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void OnDisable()
        {
            // Zone turned off mid-interaction: restore every object and clear the list.
            foreach (var occ in _occupants)
            {
                if (occ.Manipulator == null) continue;
                occ.Manipulator.canRotate = occ.OriginalCanRotate;
                OnObjectExited?.Invoke(occ.Manipulator);
            }
            _occupants.Clear();
        }

        private void Update()
        {
            if (_occupants.Count == 0) return;

            float angle = rotationSpeed * Time.deltaTime;

            for (int i = _occupants.Count - 1; i >= 0; i--)
            {
                var manip = _occupants[i].Manipulator;

                // Object was destroyed while inside — clean up silently.
                if (manip == null) { _occupants.RemoveAt(i); continue; }

                manip.transform.Rotate(rotationAxis, angle, rotationSpace);
            }
        }

        // ── Trigger callbacks ────────────────────────────────────────────────────

        private void OnTriggerEnter(Collider other)
        {
            var manip = other.GetComponentInParent<ObjectManipulator>();
            if (manip == null) return;
            if (!string.IsNullOrEmpty(acceptTag) && !manip.CompareTag(acceptTag)) return;

            // Prevent duplicates — a multi-collider object could fire Enter multiple times.
            foreach (var occ in _occupants)
                if (occ.Manipulator == manip) return;

            bool origCanRotate = manip.canRotate;
            if (suppressManualRotate) manip.canRotate = false;

            _occupants.Add(new Occupant
            {
                Manipulator       = manip,
                OriginalCanRotate = origCanRotate
            });

            OnObjectEntered?.Invoke(manip);
        }

        private void OnTriggerExit(Collider other)
        {
            var manip = other.GetComponentInParent<ObjectManipulator>();
            if (manip == null) return;

            for (int i = _occupants.Count - 1; i >= 0; i--)
            {
                if (_occupants[i].Manipulator != manip) continue;

                if (manip != null)
                    manip.canRotate = _occupants[i].OriginalCanRotate;

                _occupants.RemoveAt(i);
                OnObjectExited?.Invoke(manip);
                break;
            }
        }

        // ── Editor ───────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnValidate()
        {
            bool hasTrigger = false;
            foreach (var col in GetComponents<Collider>())
                if (col.isTrigger) { hasTrigger = true; break; }

            if (!hasTrigger)
                Debug.LogWarning($"[AutoRotateZone] '{name}': No trigger Collider found. " +
                                 "Add a Collider and enable Is Trigger.", this);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.75f);

            Vector3 center = transform.position;
            Vector3 axis   = rotationAxis.normalized;

            // Axis arrow
            Gizmos.DrawLine(center - axis * 0.12f, center + axis * 0.12f);
            Gizmos.DrawSphere(center + axis * 0.12f, 0.012f);

            // Circle in the rotation plane
            DrawGizmoCircle(center, axis, 0.1f);
        }

        private static void DrawGizmoCircle(Vector3 center, Vector3 normal, float radius)
        {
            // Find a tangent vector perpendicular to the normal.
            Vector3 tangent = Vector3.Cross(normal, Vector3.up);
            if (tangent.sqrMagnitude < 0.001f)
                tangent = Vector3.Cross(normal, Vector3.right);
            tangent.Normalize();

            const int Steps = 20;
            Vector3 prev = center + Quaternion.AngleAxis(0f, normal) * tangent * radius;
            for (int i = 1; i <= Steps; i++)
            {
                float   a    = i * (360f / Steps);
                Vector3 next = center + Quaternion.AngleAxis(a, normal) * tangent * radius;
                Gizmos.DrawLine(prev, next);
                prev = next;
            }
        }
#endif
    }
}
