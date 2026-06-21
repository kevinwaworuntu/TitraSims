using DG.Tweening;
using UnityEngine.UI;

namespace UI
{
    public class TitraSims_Button : Button
    {
        protected virtual SoundType ClickSoundType => SoundType.UI_ButtonClick;

        protected UnityEngine.RectTransform RT { get; private set; }

        protected override void Awake()
        {
            base.Awake();
            RT = GetComponent<UnityEngine.RectTransform>();
            onClick.AddListener(HandleClick);
        }

        private void HandleClick()
        {
            AudioManager.Instance?.SFX.Play(ClickSoundType);
            UIAnimator.ButtonPress(RT).OnComplete(() => OnClick());
        }

        protected virtual void OnClick()
        {
            
        }
    }
}