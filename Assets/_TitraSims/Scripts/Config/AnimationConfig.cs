using UnityEngine;

namespace Config
{
    [CreateAssetMenu(fileName = "AnimationConfig", menuName = "AR Pharma/Create Animation Config")]
    public class AnimationConfig : ScriptableObject
    {
        public string PlayAnimationParamName = "PlayAnimation"; 
        public string StopAnimationParamName = "StopAnimation"; 
        public AnimatorOverrideController GenericAnimController;
        [SerializeField] private AnimationClip animGenericClipEntry; // ToDo : reference to object aja langsung jangan string

        public string GetAnimGenericClipEntryName()
        {
            return animGenericClipEntry == null ? "AnimClipGenericEntry" : animGenericClipEntry.name;
        }
    }
}