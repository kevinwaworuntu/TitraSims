using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "CairanErlenmeyerConfig", menuName = "AR Pharma/Cairan Erlenmeyer Config")]
    public class CairanErlenmeyerConfig : ScriptableObject
    {
        [SerializeField] private Color colorValue;
        [SerializeField] private float fillValue;
        public Color ColorValue => colorValue;
        public float FillValue => fillValue;
    }
}
