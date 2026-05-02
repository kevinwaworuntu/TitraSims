using UnityEngine;

namespace Gameplay
{
    [CreateAssetMenu(fileName = "_TahapanInteractionData_", menuName = "AR Pharma/TahapanInteractionData")]
    public class TahapanInteractionData : ScriptableObject
    {
        [SerializeField] private string title;
        [SerializeField, TextArea(2,3)] private string description;
        [SerializeField] private AudioClip audioNaration;
        [SerializeField] private AnimationClip animationClip;
        
        public string Title => title;
        public string Description => description;
        public AudioClip AudioNaration => audioNaration;
        public AnimationClip AnimationClip => animationClip;
    }
}