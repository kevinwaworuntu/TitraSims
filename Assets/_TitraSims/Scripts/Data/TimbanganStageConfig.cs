using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "TimbanganStageConfig", menuName = "AR Pharma/TimbanganStageConfig")]
    public class TimbanganStageConfig : ScriptableObject
    {
        [Header("Weight Settings")]
        [SerializeField] private float targetWeight;
        [SerializeField] private float stepAmountMg = 25f;

        [Header("Visual Constants")]
        [SerializeField] private float scaleModifier = 0.1225f;
        [SerializeField] private float initialScale = 0.02f;

        public float TargetWeight => targetWeight;
        public float StepAmountMg => stepAmountMg;
        public float ScaleModifier => scaleModifier;
        public float InitialScale => initialScale;
    }
}