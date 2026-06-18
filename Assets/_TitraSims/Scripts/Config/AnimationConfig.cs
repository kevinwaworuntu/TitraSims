using UnityEngine;
using UnityEngine.Serialization;

namespace Config
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "AR Pharma/Create Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        public string PlayAnimationParamName = "PlayAnimation"; 
        public string StopAnimationParamName = "StopAnimation"; 
        public AnimatorOverrideController GenericAnimController;
        [SerializeField] private AnimationClip animGenericClipEntry;
        [SerializeField] private AnimationClip customAnimClipEntry;
        [SerializeField] private string animGenericClipEntryStateName = "AnimEntry";
        [SerializeField] private string customAnimEntryStateName = "CustomAnim";


        [Header("Interaction Timing")]
        [Tooltip("Seconds to wait after an auto-completing interaction before advancing to the next step.")]
        public float InteractionEndDelay = 2f;

        public string GetAnimGenericClipEntryName()
        {
            return animGenericClipEntry == null ? "AnimClipGenericEntry" : animGenericClipEntry.name;
        }
        public string GetCustomAnimGenericClipEntryName()
        {
            return customAnimClipEntry == null ? "CustomAnimGenericEntry" : customAnimClipEntry.name;
        }
        public string GetAnimGenericClipEntryStateName()
        {
            return animGenericClipEntryStateName;
        }
        public string GetCustomAnimEntryStateName()
        {
            return customAnimEntryStateName;
        }
    }
}