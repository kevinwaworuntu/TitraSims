using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "BuretteStageConfig", menuName = "AR Pharma/BuretteStageConfig")]
    public class BuretteStageConfig : ScriptableObject
    {
        [Header("Visual Constants")]
        [SerializeField] private float fillObjectScaleModifier = 0.1790f;
        [SerializeField] private float fillObjectInitialScale = 90.93853f;
        [SerializeField] private float meniskusPositionModifier = 0.00107706f;
        [SerializeField] private float meniskusInitPos = 0.578676f;

        [Header("Color")]
        [SerializeField] private Color initialColor = Color.white;
        [SerializeField] private Color targetColor = Color.white;

        [Header("Per-Step Targets")]
        [SerializeField] private float[] weights;
        [SerializeField] private float[] floorTargets;
        [SerializeField] private float[] ceilTargets;

        public float FillObjectScaleModifier => fillObjectScaleModifier;
        public float FillObjectInitialScale => fillObjectInitialScale;
        public float MeniskusPositionModifier => meniskusPositionModifier;
        public float MeniskusInitPos => meniskusInitPos;
        public Color InitialColor => initialColor;
        public Color TargetColor => targetColor;
        public float[] Weights => weights;
        public float[] FloorTargets => floorTargets;
        public float[] CeilTargets => ceilTargets;
    }
}