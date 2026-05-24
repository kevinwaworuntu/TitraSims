using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "GelasUkurStageConfig", menuName = "AR Pharma/GelasUkurStageConfig")]
    public class GelasUkurStageConfig : ScriptableObject
    {
        [Header("Visual Constants")]
        [SerializeField] private float fillObjectScaleModifier = 0.10698485f;
        [SerializeField] private float fillObjectInitialScale = 0f;
        [SerializeField] private float meniskusPositionModifier = 0.000640f;
        [SerializeField] private float meniskusInitPos = -0.00339f;

        public float FillObjectScaleModifier => fillObjectScaleModifier;
        public float FillObjectInitialScale => fillObjectInitialScale;
        public float MeniskusPositionModifier => meniskusPositionModifier;
        public float MeniskusInitPos => meniskusInitPos;
    }
}