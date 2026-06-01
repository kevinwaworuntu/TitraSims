using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "AR Pharma/Create Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        public string PlayAnimationParamName = "PlayAnimation"; 
        public string StopAnimationParamName = "StopAnimation"; 
        public AnimatorOverrideController GenericAnimController;
        [SerializeField] private AnimationClip animGenericClipEntry;

        [Header("Interaction Timing")]
        [Tooltip("Seconds to wait after an auto-completing interaction before advancing to the next step.")]
        public float InteractionEndDelay = 2f;

        public string GetAnimGenericClipEntryName()
        {
            return animGenericClipEntry == null ? "AnimClipGenericEntry" : animGenericClipEntry.name;
        }
    }
}