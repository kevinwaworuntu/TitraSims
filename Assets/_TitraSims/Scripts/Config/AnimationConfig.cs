using System.Collections.Generic;
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

        public AnimationClip GetAnimGenericEntryClip()
        {
            return animGenericClipEntry;
        }
        public string GetAnimGenericClipEntryName()
        {
            return animGenericClipEntry == null ? "AnimClipGenericEntry" : animGenericClipEntry.name;
        }
        public bool IsAnimGenericClipEntryNameValid()
        {
            return IsClipNameRegisteredInOverrideController(GetAnimGenericClipEntryName());
        }
        public string GetCustomAnimGenericClipEntryName()
        {
            return customAnimClipEntry == null ? "CustomAnimGenericEntry" : customAnimClipEntry.name;
        }
        public bool IsCustomAnimGenericClipEntryNameValid()
        {
            return IsClipNameRegisteredInOverrideController(GetCustomAnimGenericClipEntryName());
        }

        private bool IsClipNameRegisteredInOverrideController(string clipName)
        {
            if (GenericAnimController == null || string.IsNullOrEmpty(clipName))
            {
                return false;
            }

            var overrides = new List<KeyValuePair<AnimationClip, AnimationClip>>(GenericAnimController.overridesCount);
            GenericAnimController.GetOverrides(overrides);

            foreach (var pair in overrides)
            {
                if (pair.Key != null && pair.Key.name == clipName)
                {
                    return true;
                }
            }

            return false;
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