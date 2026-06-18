using System.Collections.Generic;
using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// Marks a world-space pose as a valid snap destination.
    /// Place on an empty GameObject at the exact position/rotation the snapped
    /// object should occupy.
    ///
    /// Registers itself in a static list so SnapInteractable can poll cheaply
    /// without FindObjectsByType each frame.
    ///
    /// Setup:
    ///   • Set <see cref="acceptTag"/> to restrict which objects can snap here
    ///     (leave empty to accept any SnapInteractable).
    ///   • Assign <see cref="_highlightVisual"/> — shown while a compatible object
    ///     is being dragged within range.
    ///   • Assign <see cref="_occupiedVisual"/> — shown while an object is snapped.
    ///   • The yellow Gizmo sphere shows the snap radius in the Editor.
    /// </summary>
    public class SnapZone : MonoBehaviour
    {
        [Header("Filter")]
        [Tooltip("Only accept SnapInteractables whose GameObject tag matches. Empty = accept any.")]
        public string acceptTag = "";

        [Header("Snap")]
        [SerializeField] private float _snapRadius = 0.1f;

        [Tooltip("Rotate the snapped object to match this zone's orientation")]
        public bool snapRotation = true;

        [Header("Visuals")]
        [SerializeField] private GameObject _highlightVisual;   // shown while candidate is in range
        [SerializeField] private GameObject _occupiedVisual;    // shown while an object is snapped

        // ── Static registry ──────────────────────────────────────────────────────
        // Avoids FindObjectsByType every frame. Entries are kept current via
        // OnEnable / OnDisable, which run correctly on scene load/unload.
        public static readonly List<SnapZone> All = new List<SnapZone>();

        // ── Public state ─────────────────────────────────────────────────────────

        public float SnapRadius => _snapRadius;
        public bool  IsOccupied => _snappedObject != null;

        // ── Internal state ───────────────────────────────────────────────────────

        private SnapInteractable _snappedObject;

        // ── Unity lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _highlightVisual?.SetActive(false);
            _occupiedVisual?.SetActive(false);
        }

        private void OnEnable()  => All.Add(this);
        private void OnDisable() => All.Remove(this);

        // ── Queried by SnapInteractable ──────────────────────────────────────────

        /// Returns true when <paramref name="candidate"/> is compatible with this
        /// zone and within snap radius.
        public bool IsInRange(SnapInteractable candidate)
        {
            // A zone occupied by someone else is not available.
            if (IsOccupied && _snappedObject != candidate) return false;

            if (!string.IsNullOrEmpty(acceptTag) && !candidate.CompareTag(acceptTag)) return false;

            return Vector3.Distance(candidate.transform.position, transform.position) <= _snapRadius;
        }

        /// Toggle the highlight visual (called by SnapInteractable during drag).
        public void SetHighlight(bool on) => _highlightVisual?.SetActive(on);

        // ── Called by SnapInteractable ───────────────────────────────────────────

        /// Attempts to accept <paramref name="candidate"/>. Returns true on success.
        /// Calls <see cref="SnapInteractable.SnapTo"/> which starts the lerp.
        public bool TryAccept(SnapInteractable candidate)
        {
            if (!IsInRange(candidate)) return false;

            _snappedObject = candidate;
            _highlightVisual?.SetActive(false);
            _occupiedVisual?.SetActive(true);
            candidate.SnapTo(this);
            return true;
        }

        /// Called by the currently snapped SnapInteractable when it is picked up.
        public void Release()
        {
            _snappedObject = null;
            _occupiedVisual?.SetActive(false);
        }

        // ── Editor ───────────────────────────────────────────────────────────────

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsOccupied ? new Color(0.2f, 1f, 0.2f, 0.6f)
                                      : new Color(1f, 0.9f, 0.1f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, _snapRadius);
        }
#endif
    }
}