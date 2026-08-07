using UnityEngine;

namespace UI
{
    public class NextTahapanButton : TitraSims_Button
    {
        [UnityEngine.SerializeField] private float targetScaleModifier = 1.11f;
        [UnityEngine.SerializeField] private float inhaleTime          = 1f;
        [UnityEngine.SerializeField] private float exhaleTime          = 1f;

        private Vector3 _initialScale;
        
        protected override void Awake()
        {
            base.Awake();
            
            _initialScale = RT.localScale;
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            
            RT.localScale = _initialScale;
            AudioManager.Instance?.SFX.Play(SoundType.Gameplay_CanContinueTahapan);
            UIAnimator.ButtonPulse(RT, -1, targetScaleModifier, inhaleTime, exhaleTime);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            UIAnimator.StopPulse(RT);
        }

        protected override void OnClick() { }
    }
}
