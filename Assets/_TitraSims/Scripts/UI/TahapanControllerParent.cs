using System;
using UnityEngine;

namespace UI
{
    public class TahapanControllerParent : MonoBehaviour
    {
        [SerializeField] private int soundEffectDrawMaxCount = 2;
        private int _soundEffectDrawCount;
        
        protected void OnEnable()
        {
            var targetClip = AudioManager.Instance?.SFX.GetRandomClipInType(SoundType.UI_DrawListEach);
            UIAnimator.DrawList(transform, onItemAppear: () =>
            {
                if (_soundEffectDrawCount > soundEffectDrawMaxCount)
                {
                    return;
                }
                AudioManager.Instance?.SFX.Play(targetClip);
                _soundEffectDrawCount++;
            });
        }

        private void OnDisable()
        {
            _soundEffectDrawCount = 0;
        }
    }
}