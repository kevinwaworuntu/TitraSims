namespace UI
{
    public class NextTahapanButton : TitraSims_Button
    {
        [UnityEngine.SerializeField] private float targetScaleModifier = 1.08f;
        [UnityEngine.SerializeField] private float inhaleTime          = 1f;
        [UnityEngine.SerializeField] private float exhaleTime          = 1f;

        protected override void OnEnable()
        {
            base.OnEnable();
            
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
