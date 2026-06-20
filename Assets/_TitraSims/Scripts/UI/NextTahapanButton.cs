using UnityEngine;

public class NextTahapanButton : MonoBehaviour
{
   [SerializeField] private float targetScaleModifier = 1.08f;
   [SerializeField] private float inhaleTime = 1f;
   [SerializeField] private float exhaleTime = 1f;
   
   private void OnEnable()
   {
      UIAnimator.ButtonPulse(GetComponent<RectTransform>(), -1, targetScaleModifier, inhaleTime, exhaleTime);
   }

   private void OnDisable()
   {
      UIAnimator.StopPulse(GetComponent<RectTransform>());
   }
}
