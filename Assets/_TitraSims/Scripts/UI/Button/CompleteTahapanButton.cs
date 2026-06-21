using DG.Tweening;

namespace UI
{
    public class CompleteTahapanButton : TitraSims_Button
    {
        [UnityEngine.SerializeField] private float interval = 0.5f;
        [UnityEngine.SerializeField] private float angle    = 1f;
        [UnityEngine.SerializeField] private float duration = 0.5f;

        private Sequence _wobble;

        protected override void OnEnable()
        {
            base.OnEnable();

            AudioManager.Instance?.SFX.Play(SoundType.Gameplay_CompleteTahapan);
            _wobble = UIAnimator.Wobble(RT, -1, interval, angle, duration);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            _wobble?.Kill();
            UIAnimator.StopWobble(RT, 0);
        }

        protected override void OnClick()
        {
            GameManager.Instance?.CompleteCurrentTahap();
        }
    }
}
