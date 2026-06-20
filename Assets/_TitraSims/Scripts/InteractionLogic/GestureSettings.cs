using UnityEngine;

namespace InteractionLogic
{
    /// <summary>
    /// ScriptableObject asset that controls how many fingers are required for each gesture.
    /// Create via: Assets > Create > TitraSims > Gesture Settings
    /// Assign the asset to GestureController._settings in the Inspector.
    /// Leave unassigned to keep the original defaults (rotate=1, drag=2, scale=2).
    /// </summary>
    [CreateAssetMenu(fileName = "GestureSettings", menuName = "TitraSims/Gesture Settings")]
    public class GestureSettings : ScriptableObject
    {
        [Header("Finger Count per Gesture")]

        [Tooltip("Number of fingers required to rotate a focused object.")]
        [Range(1, 3)]
        public int rotateFingersNeeded = 1;

        [Tooltip("Number of fingers required to drag a focused object. Set to 1 for single-finger drag.")]
        [Range(1, 3)]
        public int dragFingersNeeded = 2;

        [Tooltip("Number of fingers required to pinch-scale a focused object. Must be at least 2 (pinch needs two points).")]
        [Range(2, 3)]
        public int scaleFingersNeeded = 2;
    }
}